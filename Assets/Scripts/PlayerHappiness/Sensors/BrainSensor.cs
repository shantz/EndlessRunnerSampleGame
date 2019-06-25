using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

namespace PlayerHappiness.Sensors
{
    public class BrainSensor : ISensor
    {
        private OSCReciever reciever;

        public static bool s_Connected = false;

        public int port = 8338;
        bool isActive;
        ICollectorContext m_Context;

        public string name => "brain";

        public BrainSensor()
        {
            reciever = new OSCReciever();
        }
        
        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }

        public void Start()
        {
            s_Connected = false;
            isActive = true;

            CoroutineHandler.StartStaticCoroutine(Update());
            
            reciever.Open(port);
        }

        public CustomYieldInstruction Stop()
        {
            reciever.Close();
            s_Connected = false;
            isActive = false;
            return null;
        }
        
        IEnumerator Update () 
        {
            while (isActive)
            {
                if (reciever.hasWaitingMessages())
                {
                    s_Connected = true;
                    
                    OSCMessage msg = reciever.getNextMessage();
                    
                    if (msg.Address == "Muse-2FCA/notch_filtered_eeg")
                    {
                        using (var frame = m_Context.DoFrame())
                        {
                            frame.Write("delta", (float)msg.Data[0]);
                            frame.Write("theta", (float)msg.Data[1]);
                            frame.Write("alpha", (float)msg.Data[2]);
                            frame.Write("beta", (float)msg.Data[3]);
                            frame.Write("gamma", (float)msg.Data[4]);
                            frame.Write("mu", (float)msg.Data[5]);
                        }
                    }
                }

                yield return null;
            }
        }
    }
}
