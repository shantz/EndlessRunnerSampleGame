#import <Foundation/Foundation.h>
#import <WFConnector/WFConnector.h>

@interface WFManager : NSObject <WFDiscoveryManagerDelegate, WFSensorConnectionDelegate>
{
}

@property (nonatomic,strong) WFDiscoveryManager* discoveryManager;
@property (nonatomic,strong) NSMutableArray* discoveredDevices;
@property (nonatomic,strong) WFSensorConnection* sensorConnection;
@property (strong, nonatomic) NSTimer* dataUpdateTimer;

@end

@implementation WFManager

- (instancetype)init {
    return self;
}

- (void)startDiscovery {
    if(self.discoveredDevices.count || !self.discoveredDevices) {
        self.discoveredDevices = [NSMutableArray new];
    }

    // Setup the discovery manager
    if(self.discoveryManager==nil) {
        self.discoveryManager = [WFDiscoveryManager new];
        self.discoveryManager.delegate = self;
    }

    // Start discovery on both ANT+ and BTLE, only networks that are ready will start.
    [self.discoveryManager discoverSensorTypes:@[@(WF_SENSORTYPE_HEARTRATE)] onNetwork:WF_NETWORKTYPE_BTLE];
}

- (void) cancelDiscovery
{
    NSLog(@"cancelDiscovery");

    [self.discoveryManager cancelDiscovery];
}

- (void) shutdown
{
    if (self.sensorConnection != nil) {
        [self.sensorConnection disconnect];
    }
    [self cancelDiscovery];
}

- (void) connect:(WFDeviceInformation*)deviceInformation
{
    NSArray* connectionParams = [deviceInformation connecitonParamsForAllSupportSensorTypes];

    for (WFConnectionParams* params in connectionParams) {
        NSError* error = nil;

        self.sensorConnection = [[WFHardwareConnector sharedConnector] requestSensorConnection:params
                                                                                 withProximity:WF_PROXIMITY_RANGE_DISABLED
                                                                                         error:&error];
        self.sensorConnection.delegate = self;

        if(error) {
            NSLog(@"ERROR: Failed to create sensor connection! \n%@", error);
            dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(1.0 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
                [self connect:deviceInformation];
            });
        } else {
            self.dataUpdateTimer = [NSTimer scheduledTimerWithTimeInterval:1.0 target:self selector:@selector(updateData) userInfo:nil repeats:YES];
            [self cancelDiscovery];
        }
    }

}

-(void) updateData
{
    WFHeartrateConnection* connection = (WFHeartrateConnection*)self.sensorConnection;
    if (!connection.hasData) {
        return;
    }

    WFHeartrateData* data = [connection getHeartrateData];
    NSString *str = [NSString stringWithFormat:@"HEARTRATE %d", data.computedHeartrate];
    UnitySendMessage("HearBeatSensor", "HeartRateEvent", [str cStringUsingEncoding:NSUTF8StringEncoding]);
}

#pragma mark -
#pragma mark  WFDiscoveryManagerDelegate

- (void)discoveryManager:(WFDiscoveryManager*)discoveryManager didDiscoverDevice:(WFDeviceInformation*)deviceInformation
{
    if([self.discoveredDevices containsObject:deviceInformation]==NO)
    {
        [self.discoveredDevices addObject:deviceInformation];
        [self connect:deviceInformation];

        NSLog(@"Got device - %@", deviceInformation.name);
    }
}

- (void)discoveryManager:(WFDiscoveryManager*)discoveryManager didLooseDevice:(WFDeviceInformation*)deviceInformation
{
    NSInteger index = [self.discoveredDevices indexOfObject:deviceInformation];

    if(index!=NSNotFound)
    {
        [self.discoveredDevices removeObject:deviceInformation];
    }
}

#pragma mark -
#pragma mark WFSensorConnectionDelegate

- (void)connectionDidTimeout:(WFSensorConnection*)connectionInfo
{

}

- (void)connection:(WFSensorConnection*)connectionInfo stateChanged:(WFSensorConnectionStatus_t)connState
{
    if (connState == WF_SENSOR_CONNECTION_STATUS_CONNECTED) {
        UnitySendMessage("HearBeatSensor", "HeartRateEvent", "CONNECTED");
    } else if (connState == WF_SENSOR_CONNECTION_STATUS_INTERRUPTED || connState == WF_SENSOR_CONNECTION_STATUS_DISCONNECTING) {
        self.sensorConnection = nil;
        UnitySendMessage("HearBeatSensor", "HeartRateEvent", "DISCONNECTED");
    }
}


- (void) connection:(WFSensorConnection*)connectionInfo rejectedByDeviceNamed:(NSString*) deviceName appAlreadyConnected:(NSString*) appName
{
}

@end

extern "C" {
    static WFManager *sharedManager = NULL;

    void trash_dash_init_heartbeat() {
        if (sharedManager == NULL) {
            sharedManager = [[WFManager alloc] init];
            [sharedManager startDiscovery];
        }
    }

    void trash_dash_destroy_heartbeat() {
        if (sharedManager != NULL) {
            [sharedManager shutdown];
        }
    }
}
