using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlayerHappiness
{
    public class CollectorContext : ICollectorContext
    {
        float m_StartTime;
        
        public List<FrameInfo> Frames;
        public Dictionary<string, byte[]> Media;
        
        public CollectorContext(float startTime)
        {
            Media = new Dictionary<string, byte[]>();
            Frames = new List<FrameInfo>(120 * 1000 / 16);
            m_StartTime = startTime;
        }
        
        public IFrame DoFrame()
        {
            FrameInfo frameInfo = new FrameInfo(Time.realtimeSinceStartup - m_StartTime, 32);
            Frames.Add(frameInfo);
            return new Frame(frameInfo);
        }

        public void SetMedia(string name, byte[] data)
        {
            Media[name] = data;
        }

	    public void SetMetdataFile(string name, string path)
	    {

	    }

    }
}
