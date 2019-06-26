using UnityEngine;

class HeartRateBridge : MonoBehaviour {
#if UNITY_IOS
    public PlayerHappiness.Sensors.HeartbeatSensor.HeartbeatSensorListener listener;

    void HeartRateEvent(string message) {
        if (listener == null) {
            return;
        }

        if (message.StartsWith("CONNECTED")) {
            listener.onReady();
        } else if (message.StartsWith("DISCONNECTED")) {
            listener.onDisconnected();
        } else if (message.StartsWith("HEARTRATE")) {
            var split = message.Split(' ');
            if (split.Length > 1) {
                try
                {
                    var rate = System.Convert.ToInt32(split[1]);
                    listener.onHeartbeat((double)rate);
                }
                catch (System.Exception e) { }
            }
        }
    }

    void OnDestroy() {
        if (listener != null) {
            
        }
    }
#endif    
}