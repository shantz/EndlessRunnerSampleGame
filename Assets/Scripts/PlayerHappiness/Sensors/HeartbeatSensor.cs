using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;

namespace PlayerHappiness.Sensors
{
    class HeartbeatSensor : ISensor
    {
        public bool isActive = false;
        public static volatile int currentFrame = 0;
        public static volatile bool connected = false;
        public volatile float rate;
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
                connected = true;
            }
            
            void onHeartbeat(double rate)
            {
                currentFrame++;
                this.sensor.rate = (float)rate;
            }

            void onDisconnected()
            {
                connected = false;
            }
        }
#elif UNITY_IOS
        [DllImport("__Internal")]
        private static extern void trash_dash_init_heartbeat();
        [DllImport("__Internal")]
        private static extern void trash_dash_destroy_heartbeat();

        private HeartbeatSensorListener listener;

        public class HeartbeatSensorListener {
            readonly HeartbeatSensor sensor;
            public HeartbeatSensorListener(HeartbeatSensor sensor)
            {
                var go = new GameObject("HeartbeatSensor");
                go.AddComponent<HeartRateBridge>();
                go.GetComponent<HeartRateBridge>().listener = this;
                GameObject.DontDestroyOnLoad(go);

                this.sensor = sensor;
            }

            public void onReady()
            {
                connected = true;
                Debug.Log("Heartreate Sensor ready");
            }

            public void onHeartbeat(double rate) {
                currentFrame++;
                this.sensor.rate = (float)rate;
            }

            public void onDisconnected()
            {
                connected = false;
                Debug.Log("Heartreate Sensor disconnected");
            }

            public void onShutdown() {
                trash_dash_destroy_heartbeat();
            }
        }
#endif

        public HeartbeatSensor()
        {
#if UNITY_ANDROID
            if (!Application.isEditor)
            {
                var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                activity.Call("startDiscovery", new HeartbeatSensorListener(this));
            }
#elif UNITY_IOS
            if (!Application.isEditor)
            {
                listener = new HeartbeatSensorListener(this);
                trash_dash_init_heartbeat();
            }
#endif
        }
        
        public string name => "heartbeat";
        public bool useFrames => true;
        public int[] projectedValues => new[] { /* f */ 1,  /* i */ 0,  /* s */ 0,  /* v2 */ 0,  /* v3 */ 0,  /* q */ 0 };

        public void SetContext(ICollectorContext context)
        {
            m_Context = context;
        }

        public void Start()
        {
            isActive = true;

            CoroutineHandler.StartStaticCoroutine(Update());
        }

        public CustomYieldInstruction Stop()
        {
            isActive = false;
            return null;
        }

        IEnumerator Update()
        {
            while (isActive)
            {
                using (var frame = m_Context.DoFrame())
                {
                    frame.Write("r", rate);
                }
                
                yield return new WaitForSeconds(0.25f);
            }
        }
    }
}
