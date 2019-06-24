using UnityEngine;

namespace PlayerHappiness
{
    public class Frame : IFrame
    {
        private FrameInfo m_FrameInfo;

        public Frame(FrameInfo frameInfo)
        {
            m_FrameInfo = frameInfo;
        }
        
        public void Dispose()
        {
            m_FrameInfo.floats.TrimExcess();
            m_FrameInfo.ints.TrimExcess();
            m_FrameInfo.vector2s.TrimExcess();
            m_FrameInfo.vector3s.TrimExcess();
        }

        public void Write(string name, float value)
        {
            m_FrameInfo.floats.Add(new FrameData<float>
            {
                name = name,
                value = value
                
            });
        }

        public void Write(string name, int value)
        {
            m_FrameInfo.ints.Add(new FrameData<int>
            {
                name = name,
                value = value

            });
        }

        public void Write(string name, Vector2 value)
        {
            m_FrameInfo.vector2s.Add(new FrameData<Vector2>
            {
                name = name,
                value = value

            });
        }

        public void Write(string name, Vector3 value)
        {
            m_FrameInfo.vector3s.Add(new FrameData<Vector3>
            {
                name = name,
                value = value
                
            });
        }

        public void Write(string name, Quaternion value)
        {
            m_FrameInfo.quaternions.Add(new FrameData<Quaternion>
            {
                name = name,
                value = value
                
            });
        }
    }
}
