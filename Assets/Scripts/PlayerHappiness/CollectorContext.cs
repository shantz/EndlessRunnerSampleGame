using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlayerHappiness
{
    class CollectorContext : ICollectorContext
    {
        float m_StartTime;
        
        public List<FrameInfo> Frames;
        public Dictionary<string, byte[]> Media;
        public Dictionary<string, string> MediaFile;
        
        public CollectorContext(float startTime)
        {
            Media = new Dictionary<string, byte[]>();
            MediaFile = new Dictionary<string, string>();
            Frames = new List<FrameInfo>(4 * 60 * 1000 / 16);
            m_StartTime = startTime;
        }
        
        public IFrame DoFrame()
        {
            FrameInfo frameInfo = new FrameInfo(Time.realtimeSinceStartup - m_StartTime, 32);
            Frames.Add(frameInfo);
            return new Frame(frameInfo);
        }

        public void SetMetdataFile(string name, string path)
        {
            MediaFile[name] = path;
        }

    }
}
