using UnityEngine;

namespace PlayerHappiness.Sensors
{
    public class HeartbeatSensor : ISensor
    {
        public volatile bool isActive = false;
        ICollectorContext m_Context;

#if UNITY_ANDROID
        class HeartbeatSensorListener : AndroidJavaProxy
        {
            readonly HeartbeatSensor sensor;
            public HeartbeatSensorListener(HeartbeatSensor sensor) : base("com.unity.trashdash.HeartbeatListener")
            {
                this.sensor = sensor;
            }
            
            void onReady()
            {
                CoroutineHandler.RunOnMainThread(() => GameObject.Find("HearBeatSensor").SetActive(false));
            }
            
            void onHeartbeat(double rate)
            {
                if (sensor.isActive)
                {
                    using (var frame = this.sensor.m_Context.DoFrame())
                    {
                        frame.Write("r", (float)rate);
                    }
                }
            }

            void onDisconnected()
            {
                CoroutineHandler.RunOnMainThread(() => GameObject.Find("HearBeatSensor").SetActive(true));
            }
        }
        #endif

        public HeartbeatSensor()
        {
            GameObject.Find("HearBeatSensor").SetActive(true);
            
#if UNITY_ANDROID
            if (!Application.isEditor)
            {
                var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                activity.Call("startDiscovery", new HeartbeatSensorListener(this));
            }
#endif
        }
        
        public string name => "heartbeat";

        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }

        public void Start()
        {
            isActive = true;
        }

        public void Stop()
        {
            isActive = false;
        }
    }
}
