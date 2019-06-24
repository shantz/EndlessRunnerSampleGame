package com.unity.trashdash;

import android.Manifest;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.util.Log;

import com.wahoofitness.connector.HardwareConnector;
import com.wahoofitness.connector.HardwareConnectorEnums;
import com.wahoofitness.connector.HardwareConnectorTypes;
import com.wahoofitness.connector.capabilities.Capability;
import com.wahoofitness.connector.capabilities.Heartrate;
import com.wahoofitness.connector.conn.connections.SensorConnection;
import com.wahoofitness.connector.conn.connections.params.ConnectionParams;
import com.wahoofitness.connector.listeners.discovery.DiscoveryListener;

import com.unity3d.player.UnityPlayerActivity;

import com.unity.trashdash.HeartbeatListener;

public class MainActivity extends UnityPlayerActivity implements HardwareConnector.Listener, DiscoveryListener, SensorConnection.Listener, Heartrate.Listener {

    private HardwareConnector mHardwareConnector ;
    private SensorConnection mSensorConnection;
    private HeartbeatListener mListener;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        mHardwareConnector = new HardwareConnector ( this,this );
    }

    public void startDiscovery(HeartbeatListener listener) {
    	mListener = listener;
        Log.e("TEST", "startDiscovery");
        com.wahoofitness.common.log.Logger.setLogLevel ( android.util.Log.VERBOSE );
        mHardwareConnector.startDiscovery(MainActivity.this, HardwareConnectorTypes.NetworkType.BTLE);
    }

    @Override
    public void onDestroy() {
        super.onDestroy();

        if (mSensorConnection != null) {
            mSensorConnection.disconnect();
        }
        mHardwareConnector.shutdown();
    }

    @Override
    public void onHardwareConnectorStateChanged(HardwareConnectorTypes.NetworkType networkType, HardwareConnectorEnums.HardwareConnectorState hardwareConnectorState) {
        Log.e("TEST", "onHardwareConnectorStateChanged");
    }

    @Override
    public void onFirmwareUpdateRequired(SensorConnection sensorConnection, String s, String s1) {
        Log.e("TEST", "onFirmwareUpdateRequired");
    }

    @Override
    public void onDeviceDiscovered(ConnectionParams connectionParams) {
        Log.e("TEST", "onDeviceDiscovered");
        mHardwareConnector.stopDiscovery(this);

        mSensorConnection = mHardwareConnector.requestSensorConnection(connectionParams, this);
    }

    @Override
    public void onDiscoveredDeviceLost(ConnectionParams connectionParams) {
        Log.e("TEST", "onDiscoveredDeviceLost");
      
    }

    @Override
    public void onDiscoveredDeviceRssiChanged(ConnectionParams connectionParams, int i) {
        Log.e("TEST", "onDiscoveredDeviceRssiChanged");
    }

    @Override
    public void onNewCapabilityDetected(SensorConnection sensorConnection, Capability.CapabilityType capabilityType) {
        if (capabilityType == Capability.CapabilityType.Heartrate) {
        	Log.e("TEST", "Capability.CapabilityType.Heartrate");
            Heartrate heartrate = (Heartrate) sensorConnection.getCurrentCapability (Capability.CapabilityType.Heartrate );
            heartrate.addListener (this);

            mListener.onReady();
        }
    }

    @Override
    public void onSensorConnectionStateChanged(SensorConnection sensorConnection, HardwareConnectorEnums.SensorConnectionState sensorConnectionState) {
        if (sensorConnectionState == HardwareConnectorEnums.SensorConnectionState.DISCONNECTED) {
            mSensorConnection = null;
            mListener.onDisconnected();
        }
    }

    @Override
    public void onSensorConnectionError(SensorConnection sensorConnection, HardwareConnectorEnums.SensorConnectionError sensorConnectionError) {
      Log.e("TEST", "onSensorConnectionError");
    }

    @Override
    public void onHeartrateData(Heartrate.Data data) {
        final double value =  data.getHeartrate().asEventsPerMinute();
        Log.e("TEST", String.valueOf(value));
        mListener.onHeartbeat(value);
    }

    @Override
    public void onHeartrateDataReset() {

    }
}
