using System.Collections;
using UnityEngine;

namespace PlayerHappiness.Sensors
{
    public class TouchSensor : ISensor
    {
        ICollectorContext m_Context;
        bool isActive;
        
        public string name => "touch";

        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }

        IEnumerator CollectFrame()
        {
            while (isActive)
            {
                if (Input.touchCount > 0)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        using (var frame = m_Context.DoFrame())
                        {
                            Touch touch = Input.GetTouch(i);
                            Vector2 pos = new Vector2(touch.position.x / Screen.width, touch.position.x / Screen.height);
                            frame.Write($"c", pos);
                            if (Input.touchPressureSupported)
                            {
                                frame.Write($"p", touch.pressure);
                            }
                            else
                            {
                                frame.Write($"p", 0.0f);
                            }
                        }
                    }
                }

                yield return null;
            }
        }

        public void Start()
        {
            isActive = true;
            CoroutineHandler.StartStaticCoroutine(CollectFrame());
        }

        public void Stop()
        {
            isActive = false;
        }
    }
}
