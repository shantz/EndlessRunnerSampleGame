using System.Collections;
using UnityEngine;

namespace PlayerHappiness.Sensors
{
    class TouchSensor : ISensor
    {
        ICollectorContext m_Context;
        bool isActive;
        
        public string name => "touch";
        public bool useFrames => true;
        public int[] projectedValues => new[] { /* f */ 1,  /* i */ 0,  /* s */ 0,  /* v2 */ 1,  /* v3 */ 0,  /* q */ 0 };

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
                            frame.Write($"p", touch.pressure);
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

        public CustomYieldInstruction Stop()
        {
            isActive = false;
            return null;
        }
    }
}
