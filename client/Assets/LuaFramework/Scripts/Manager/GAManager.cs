using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaFramework;
using Umeng;
public class GAManager : MonoBehaviour {

	// Use this for initialization
	void Awake() {
        print("打开友盟统计");
        GA.StartWithAppKeyAndChannelId("5962e64d9f06fd79fc001571", AppConst.Channel);
        print(AppConst.Channel);
        print(GA.PaySource.appstore_lhdb);
    }
	
}
