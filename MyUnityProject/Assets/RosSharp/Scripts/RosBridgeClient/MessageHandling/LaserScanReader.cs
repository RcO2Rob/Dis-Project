﻿

// /*
// © Siemens AG, 2018-2019
// Author: Berkay Alp Cakal (berkay_alp.cakal.ct@siemens.com)

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// <http://www.apache.org/licenses/LICENSE-2.0>.
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// */

using UnityEngine;
using System.Collections.Generic;

namespace RosSharp.RosBridgeClient
{
    public class LaserScanReader : MonoBehaviour
    {
        private Ray[] rays;
        private RaycastHit[] raycastHits;
        private Vector3[] directions;
        private LaserScanVisualizer[] laserScanVisualizers;

        public int samples = 360;
        public int update_rate = 1800;
        public float angle_min = 0;
        public float angle_max = 6.28f;
        public float angle_increment = 0.0174533f;
        public float time_increment = 0;
        public float scan_time = 0;
        public float range_min = 0.12f;
        public float range_max = 3.5f;
        public float[] ranges;
        public float[] intensities;

        public void Start()
        {
            directions = new Vector3[samples];
            ranges = new float[samples];
            intensities = new float[samples];
            rays = new Ray[samples];
            raycastHits = new RaycastHit[samples];
        }

        public float[] Scan()
        {
            MeasureDistance();

            laserScanVisualizers = GetComponents<LaserScanVisualizer>();
            if (laserScanVisualizers != null)
                foreach (LaserScanVisualizer laserScanVisualizer in laserScanVisualizers)
                    laserScanVisualizer.SetSensorData(gameObject.transform, directions, ranges, range_min, range_max);

            return ranges;
        }

        public (bool[], float) caculateMinDistance()
        {   
            
            bool[] direction = {false,false,false,false};
            float[] ranges = Scan();
            float value = 0;
            int totalAngles = ranges.Length;  // 获取数组长度
            //int minIndex = (int)((-45.0 + 90.0) / 180.0 * totalAngles);  
            //int maxIndex = (int)((45.0 + 90.0) / 180.0 * totalAngles);   

            for (int i = 0; i < ranges.Length; i++)
            {
                if(ranges[i]<0.2f && ranges[i] != 0){
                    if ((i >= 0 && i <= 45) || (i >= 316 && i <= 359)) {
                        value = ranges[i];
                        direction[0] = true; //forward
                    }
                    if ((i > 45 && i <= 135)) 
                    {
                        value = ranges[i];
                        direction[3] = true; //right
                    }
                    if ((i > 135 && i <= 225)) 
                    {
                        value = ranges[i];
                        direction[1] = true; //backward
                    }
                    if ((i > 225 && i < 316) ) 
                    {
                        value = ranges[i];
                        direction[2] = true; //left
                    }
                }
                
            }

            return (direction,value);  // 如果没有找到符合条件的距离，返回false
        }


        private void MeasureDistance()
        {
            rays = new Ray[samples];
            raycastHits = new RaycastHit[samples];
            ranges = new float[samples];
            

            for (int i = 0; i < samples; i++)
            {
                rays[i] = new Ray(transform.position, GetRayRotation(i) * transform.forward);
                directions[i] = Quaternion.Euler(-transform.rotation.eulerAngles) * rays[i].direction.Unity2Ros();
                //directions[i] = Quaternion.Euler(new Vector3(-transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, -transform.rotation.eulerAngles.z)) * rays[i].direction;


                raycastHits[i] = new RaycastHit();
                if (Physics.Raycast(rays[i], out raycastHits[i], range_max))
                    if (raycastHits[i].distance >= range_min && raycastHits[i].distance <= range_max)
                        ranges[i] = raycastHits[i].distance;
            }
        }

        private Quaternion GetRayRotation(int sample) {
            float eulerAngleInRadians = angle_min + (angle_increment * sample);
            float eulerAngleInDegrees = eulerAngleInRadians * 180 / Mathf.PI;

            return Quaternion.Euler(new Vector3(0, -eulerAngleInDegrees, 0));
        }
    }
}