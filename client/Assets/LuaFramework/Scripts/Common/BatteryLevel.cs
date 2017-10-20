
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;

public class BatteryLevel {

#if UNITY_IOS
	[DllImport ("__Internal")]
	static extern float _GetBatteryLevel ();
#endif
    public static float GetBatteryLevel () {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                string CapacityString = File.ReadAllText("/sys/class/power_supply/battery/capacity");
                return int.Parse(CapacityString);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to read battery power; " + e.Message);
            }
            return 0;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
           return 100 * _GetBatteryLevel();
#endif
        }

        return 100;
    }
}