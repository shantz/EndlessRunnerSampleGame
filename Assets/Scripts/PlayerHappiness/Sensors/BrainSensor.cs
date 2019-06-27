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

        string[] datas = new[]
        {
            "Muse-2FCA/elements/alpha_absolute",
            "Muse-2FCA/elements/beta_absolute",
            "Muse-2FCA/elements/delta_absolute",
            "Muse-2FCA/elements/theta_absolute",
            "Muse-2FCA/elements/gamma_absolute",
            "Muse-2FCA/notch_filtered_eeg"
        };
        
        string[] frameValues = new[]
        {
            "a",
            "b",
            "d",
            "t",
            "g",
            "eeg"
        };
        
        OSCMessage[] messages;

        public int port = 8338;
        bool isActive;
        ICollectorContext m_Context;

        public string name => "muse";
        public bool useFrames => true;
        public int[] projectedValues => new[] { /* f */ 0,  /* i */ 0,  /* s */ 0,  /* v2 */ 0,  /* v3 */ 0,  /* q */ 6 };

        public BrainSensor()
        {
            messages = new OSCMessage[datas.Length];
            
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
                    reciever.getLastMessages(datas, messages);

                    if (m_Context != null)
                    {
                        using (var frame = m_Context.DoFrame())
                        {
                            for (int i = 0; i < messages.Length; i++)
                            {
                                if (messages[i] != null)
                                {
                                    frame.Write(frameValues[i], getData(messages[i]));
                                }
                                else
                                {
                                    frame.Write(frameValues[i], new Quaternion(0, 0, 0, 0));
                                }
                            }

                        }
                    }

                    currentFrame++;

                }

                yield return null;
            }
        }

        Quaternion getData(OSCMessage msg)
        {
            return new Quaternion((float)msg.Data[0], (float)msg.Data[1], (float)msg.Data[2], (float)msg.Data[3]);
        }
    }
}
