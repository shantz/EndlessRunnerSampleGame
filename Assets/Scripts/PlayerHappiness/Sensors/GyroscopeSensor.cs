using System.Collections;
using UnityEngine;

namespace PlayerHappiness.Sensors
{
    public class GyroscopeSensor : ISensor
    {
        ICollectorContext m_Context;
        
        public GyroscopeSensor(float updateInterval)
        {
            Input.gyro.updateInterval = updateInterval;
        }
        
        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }

        IEnumerator CollectFrame()
        {
            while (Input.gyro.enabled)
            {
                using (var frame = m_Context.DoFrame())
                {
                    frame.Write("rotationRate", Input.gyro.rotationRate);
                    frame.Write("gravity", Input.gyro.gravity);
                    frame.Write("userAcceleration", Input.gyro.userAcceleration);
                    frame.Write("rotationRateUnbiased", Input.gyro.rotationRateUnbiased);
                    frame.Write("attitude", Input.gyro.attitude);
                }

                yield return new WaitForSeconds(Input.gyro.updateInterval);
            }
        }

        public void Start()
        {
            Input.gyro.enabled = true;
            CoroutineHandler.StartStaticCoroutine(CollectFrame());
        }

        public void Stop()
        {
            Input.gyro.enabled = false;
        }
    }
}
