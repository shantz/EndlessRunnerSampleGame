using System;
using UnityEngine;

namespace PlayerHappiness {
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