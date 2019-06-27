using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerHappiness {
    class FastCollectorContext : ICollectorContext
    {
        float m_StartTime;
        
        public List<FastFrameInfo> Frames;
        public Dictionary<string, string> MediaFile;
        
        public List<FrameData<float>> floats;
        public List<FrameData<int>> ints;
        public List<FrameData<Vector2>> vector2s;
        public List<FrameData<Vector3>> vector3s;
        public List<FrameData<Quaternion>> quaternions;
        public List<FrameData<string>> strings;
        
        public FastCollectorContext(float startTime, bool useFrames, int[] projection)
        {
            MediaFile = new Dictionary<string, string>();

            if (useFrames)
            {
                int projectedTime = 4 * 60 * 1000 / 16;
                
                Frames = new List<FastFrameInfo>(projectedTime);

                floats = new List<FrameData<float>>(projectedTime * projection[0]);
                ints = new List<FrameData<int>>(projectedTime * projection[1]);
                strings = new List<FrameData<string>>(projectedTime * projection[2]);
                vector2s = new List<FrameData<Vector2>>(projectedTime * projection[3]);
                vector3s = new List<FrameData<Vector3>>(projectedTime * projection[4]);
                quaternions = new List<FrameData<Quaternion>>(projectedTime * projection[5]);
            }

            m_StartTime = startTime;
        }
        
        public IFrame DoFrame()
        {
            Frames.Add(new FastFrameInfo(this, Time.realtimeSinceStartup - m_StartTime));
            return new FastFrame(this, Frames.Count - 1);
        }

        public void SetMetdataFile(string name, string path)
        {
            MediaFile[name] = path;
        }

    }
}