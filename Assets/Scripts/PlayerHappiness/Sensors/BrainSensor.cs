using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerHappiness.Sensors
{
    public class BrainSensor : ISensor
    {
        private LibmuseBridge m_Muse;
        ICollectorContext m_Context;
        string m_ConnectionBuffer;
        string m_DataBuffer;
        GameObject m_GameObject;
        BrainSensorBehaviour m_Component;

        public string name => "brain";

        class BrainSensorBehaviour : MonoBehaviour
        {
            public BrainSensor sensor;
            
            void receiveMuseList(string data) {
                sensor.receiveMuseList(data);
            }

            void receiveConnectionPackets(string data) {
                sensor.receiveConnectionPackets(data);
            }

            void receiveDataPackets(string data) {   
                sensor.receiveDataPackets(data);
            }

            void receiveArtifactPackets(string data) {
                sensor.receiveArtifactPackets(data);
            }
        }

        public BrainSensor()
        {
            m_Component = GameObject.Find("BrainSensor").AddComponent<BrainSensorBehaviour>();
            m_Component.sensor = this;
            
#if UNITY_IPHONE
            m_Muse = new LibmuseBridgeIos();
#elif UNITY_ANDROID
            m_Muse = new LibmuseBridgeAndroid();
#endif

            registerListeners();
            registerAllData();

            m_Muse.startListening();
        }

        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }
        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }
        
        void registerListeners() 
        {
            m_Muse.registerMuseListener(m_Component.name, "receiveMuseList");
            m_Muse.registerConnectionListener(m_Component.name, "receiveConnectionPackets");
            m_Muse.registerDataListener(m_Component.name, "receiveDataPackets");
            m_Muse.registerArtifactListener(m_Component.name, "receiveArtifactPackets");
        }

    void registerAllData() {
        // This will register for all the available data from muse headband
        // Comment out the ones you don't want
        /*m_Muse.listenForDataPacket("ACCELEROMETER");
        m_Muse.listenForDataPacket("GYRO");
        m_Muse.listenForDataPacket("EEG");
        m_Muse.listenForDataPacket("QUANTIZATION");
        m_Muse.listenForDataPacket("BATTERY");
        m_Muse.listenForDataPacket("DRL_REF");
        m_Muse.listenForDataPacket("ALPHA_ABSOLUTE");
        m_Muse.listenForDataPacket("BETA_ABSOLUTE");
        m_Muse.listenForDataPacket("DELTA_ABSOLUTE");
        m_Muse.listenForDataPacket("THETA_ABSOLUTE");
        m_Muse.listenForDataPacket("GAMMA_ABSOLUTE");
        m_Muse.listenForDataPacket("ALPHA_RELATIVE");
        m_Muse.listenForDataPacket("BETA_RELATIVE");
        m_Muse.listenForDataPacket("DELTA_RELATIVE");
        m_Muse.listenForDataPacket("THETA_RELATIVE");
        m_Muse.listenForDataPacket("GAMMA_RELATIVE");
        m_Muse.listenForDataPacket("ALPHA_SCORE");
        m_Muse.listenForDataPacket("BETA_SCORE");
        m_Muse.listenForDataPacket("DELTA_SCORE");
        m_Muse.listenForDataPacket("THETA_SCORE");
        m_Muse.listenForDataPacket("GAMMA_SCORE");
        m_Muse.listenForDataPacket("HSI_PRECISION");
        m_Muse.listenForDataPacket("ARTIFACTS");*/
    }
    
    //--------------------------------------
    // These listener methods update the buffer
    // The Update() per frame will display the data.

    void receiveMuseList(string data) {
        // Convert string to list of muses and populate the dropdown menu.
        List<string> muses = data.Split(' ').ToList<string>();
        Debug.Log ("Connecting to " + muses[0]);
        m_Muse.connect (muses[0]);
    }

    void receiveConnectionPackets(string data) {
        Debug.Log("Unity received connection packet: " + data);
        m_ConnectionBuffer = data;
        
        CoroutineHandler.RunOnMainThread(() => GameObject.Find("BrainSensor").SetActive(false));
    }

    void receiveDataPackets(string data) {   
        Debug.Log("Unity received data packet: " + data);
        m_DataBuffer = data;
    }

    void receiveArtifactPackets(string data) {
        Debug.Log("Unity received artifact packet: " + data);
        m_DataBuffer = data;
    }
    }
}
