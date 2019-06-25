using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

namespace PlayerHappiness.Sensors
{
    class BrainSensor : ISensor
    {
        private OSCReciever reciever;
        
        public static int currentFrame = 0;

        public int port = 8338;
        bool isActive;
        ICollectorContext m_Context;

        public string name => "eeg";

        public BrainSensor()
        {
            reciever = new OSCReciever();
            if (!Application.isEditor)
            {
                reciever.Open(port);
                isActive = true;

                CoroutineHandler.StartStaticCoroutine(Update());
            }
        }
        
        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }

        public void Start()
        {
            if (Application.isEditor)
            {
                reciever.Open(port);
                isActive = true;

                CoroutineHandler.StartStaticCoroutine(Update());
            }
        }

        public CustomYieldInstruction Stop()
        {
            if (Application.isEditor)
            {
                reciever.Close();
                isActive = false;
            }
            
            return null;
        }
        
        IEnumerator Update () 
        {
            while (isActive)
            {
                if (reciever.hasWaitingMessages())
                {
                    OSCMessage msg = reciever.getLastMessage("Muse-2FCA/notch_filtered_eeg");

                    if (msg != null)
                    {
                        if (m_Context != null)
                        {
                            using (var frame = m_Context.DoFrame())
                            {
                                frame.Write("d", (float)msg.Data[0]);
                                frame.Write("t", (float)msg.Data[1]);
                                frame.Write("a", (float)msg.Data[2]);
                                frame.Write("b", (float)msg.Data[3]);
                            }
                        }
                        
                        currentFrame++;
                    }
                }
                
                yield return null;
            }
        }
    }
}
