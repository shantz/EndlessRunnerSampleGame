package com.unity.trashdash;

public interface HeartbeatListener {
    void onReady();
    void onDisconnected();
    void onHeartbeat(double rate);
}
