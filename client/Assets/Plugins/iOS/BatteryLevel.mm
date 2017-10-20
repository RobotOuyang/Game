
extern "C" {
	float _GetBatteryLevel () {
		BOOL orig = [UIDevice currentDevice].batteryMonitoringEnabled;
		[UIDevice currentDevice].batteryMonitoringEnabled = YES;
		float val = [UIDevice currentDevice].batteryLevel;
		[UIDevice currentDevice].batteryMonitoringEnabled = orig;
		return val;
	}
}