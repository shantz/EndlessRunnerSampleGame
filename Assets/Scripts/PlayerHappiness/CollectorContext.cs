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
    
    struct FastFrameInfo
    {
        public float timestamp;
        public int startFloats;
        public int lengthFloats;
        
        public int startInts;
        public int lengthInts;
        
        public int startVector2s;
        public int lengthVector2s;
        
        public int startVector3s;
        public int lengthVector3s;
        
        public int startQuaternions;
        public int lengthQuaternions;
        
        public int startStrings;
        public int lengthStrings;

        public FastFrameInfo(FastCollectorContext context, float timestamp)
        {
            this.timestamp = timestamp;
            
            startFloats = context.floats.Count;
            startInts = context.ints.Count;
            startVector2s = context.vector2s.Count;
            startVector3s = context.vector3s.Count;
            startQuaternions = context.quaternions.Count;
            startStrings = context.strings.Count;
            lengthFloats = 0;
            lengthInts = 0;
            lengthVector2s = 0;
            lengthVector3s = 0;
            lengthQuaternions = 0;
            lengthStrings = 0;
        }
    }
    
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

    struct FastFrame : IFrame {
        readonly FastCollectorContext fastCollectorContext;
        readonly int index;

        public FastFrame(FastCollectorContext fastCollectorContext, int index)
        {
            this.fastCollectorContext = fastCollectorContext;
            this.index = index;
        }

        public void Dispose()
        {
            
        }

        public void Write(string name, string value)
        {
            fastCollectorContext.strings.Add(new FrameData<string>
            {
                name = name,
                value = value
            });

            var frame = fastCollectorContext.Frames[index];
            frame.lengthStrings++;
            fastCollectorContext.Frames[index] = frame;
        }

        public void Write(string name, float value)
        {
            fastCollectorContext.floats.Add(new FrameData<float>
            {
                name = name,
                value = value
            });

            var frame = fastCollectorContext.Frames[index];
            frame.lengthFloats++;
            fastCollectorContext.Frames[index] = frame;
        }

        public void Write(string name, int value)
        {
            fastCollectorContext.ints.Add(new FrameData<int>
            {
                name = name,
                value = value
            });

            var frame = fastCollectorContext.Frames[index];
            frame.lengthInts++;
            fastCollectorContext.Frames[index] = frame;
        }

        public void Write(string name, Vector2 value)
        {
            fastCollectorContext.vector2s.Add(new FrameData<Vector2>
            {
                name = name,
                value = value
            });

            var frame = fastCollectorContext.Frames[index];
            frame.lengthVector2s++;
            fastCollectorContext.Frames[index] = frame;
        }

        public void Write(string name, Vector3 value)
        {
            fastCollectorContext.vector3s.Add(new FrameData<Vector3>
            {
                name = name,
                value = value
            });

            var frame = fastCollectorContext.Frames[index];
            frame.lengthVector3s++;
            fastCollectorContext.Frames[index] = frame;
        }

        public void Write(string name, Quaternion value)
        {
            fastCollectorContext.quaternions.Add(new FrameData<Quaternion>
            {
                name = name,
                value = value
            });

            var frame = fastCollectorContext.Frames[index];
            frame.lengthQuaternions++;
            fastCollectorContext.Frames[index] = frame;
        }
    }
}
