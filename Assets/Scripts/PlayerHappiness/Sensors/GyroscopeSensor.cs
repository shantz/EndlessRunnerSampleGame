using System.Collections;
using UnityEngine;

namespace PlayerHappiness.Sensors
{
    class GyroscopeSensor : ISensor
    {
        ICollectorContext m_Context;
        bool m_Active;
        
        public GyroscopeSensor(float updateInterval)
        {
            Input.gyro.enabled = true;
            //Input.gyro.updateInterval = updateInterval;
        }
        
        public string name => "gyroscope";
        public bool useFrames => true;
        public int[] projectedValues => new[] { /* f */ 0,  /* i */ 0,  /* s */ 0,  /* v2 */ 0,  /* v3 */ 4,  /* q */ 1 };
        
        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }

        IEnumerator CollectFrame()
        {
            while (m_Active)
            {
                using (var frame = m_Context.DoFrame())
                {
                    frame.Write("rr", Input.gyro.rotationRate);
                    frame.Write("g", Input.gyro.gravity);
                    frame.Write("ua", Input.gyro.userAcceleration);
                    frame.Write("rru", Input.gyro.rotationRateUnbiased);
                    frame.Write("a", Input.gyro.attitude);
                }

                yield return null;
            }
        }

        public void Start()
        {
            m_Active = true;
            //Input.gyro.enabled = true;
            CoroutineHandler.StartStaticCoroutine(CollectFrame());
        }

        public CustomYieldInstruction Stop()
        {
            m_Active = false;
            //Input.gyro.enabled = false;
            return null;
        }
    }
}
