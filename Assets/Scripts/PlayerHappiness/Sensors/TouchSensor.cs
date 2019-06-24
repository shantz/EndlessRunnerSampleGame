using System.Collections;
using UnityEngine;

namespace PlayerHappiness.Sensors
{
    public class TouchSensor : ISensor
    {
        ICollectorContext m_Context;

        
        public TouchSensor()
        {
            
        }
        
        
        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }

        IEnumerator CollectFrame()
        {
            if (Input.touchCount > 0)
            {
                using (var frame = m_Context.DoFrame())
                {
                    frame.Write("numberOfTouches", Input.touchCount);
                    for(int i = 0; i < Input.touchCount; i++)
                    {
                        Touch touch = Input.GetTouch(i);
                        if (Input.touchPressureSupported)
                        {
                            frame.Write($"touchPressure{i}", touch.pressure);
                        }
                    }
                }
            }
            yield return null;
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
