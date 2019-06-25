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

        public string name => "eeg";

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
                while (reciever.hasWaitingMessages())
                {
                    if (!s_Connected)
                    {
                        Debug.Log("EEG is on");
                    }
                    
                    s_Connected = true;
                    
                    OSCMessage msg = reciever.getNextMessage();

                    if (msg.Address == "Muse-2FCA/notch_filtered_eeg")
                    {
                        using (var frame = m_Context.DoFrame())
                        {
                            frame.Write("d", (float)msg.Data[0]);
                            frame.Write("t", (float)msg.Data[1]);
                            frame.Write("a", (float)msg.Data[2]);
                            frame.Write("b", (float)msg.Data[3]);
                            //frame.Write("g", (float)msg.Data[4]);
                            //frame.Write("m", (float)msg.Data[5]);
                        }
                        break;
                    }
                }

                yield return null;
            }
        }
    }
}
