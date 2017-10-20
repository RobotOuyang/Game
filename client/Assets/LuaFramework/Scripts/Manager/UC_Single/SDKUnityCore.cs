using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/**
 SDK的操作接口类， 主要提供三大功能：
    1. SDK 初始化
    2. 支付
    3. 退出SDK
 */
namespace UC_Single
{
    public class SDKUnityCore : MonoBehaviour
    {

        // 对应android端当前的Activity
        public static AndroidJavaObject mContext;

        static SDKUnityCore()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                mContext = jc.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }
        /**
         * 初始化
         *     json： 初始化参数  (见 Demo.cs)
         *     操作结果通过SDKUnityCallbackListener回调。   
         */

        public static void init(String json)
        {
            if (Application.platform != RuntimePlatform.Android)
                return;
            mContext.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                using (AndroidJavaClass jc = new AndroidJavaClass("cn.uc.paysdk.SDKDemoUnityCore"))
                {
                    // calling Android SDK 's initSDK Method
                    jc.CallStatic("initSDK", mContext, json);
                }
            }));
        }

        /**
         * 支付
         *     json： 支付参数  (见 Demo.cs)
         *     操作结果通过SDKUnityCallbackListener回调。   
         *     
         */
        public static void pay(String json)
        {
            if (Application.platform != RuntimePlatform.Android)
                return;
            mContext.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                using (AndroidJavaClass jc = new AndroidJavaClass("cn.uc.paysdk.SDKDemoUnityCore"))
                {
                    object[] args = new object[2] { mContext, json };
                    // calling Android SDK 's pay Method
                    jc.CallStatic("pay", args);
                }
            }));
        }

        /**
         * 退出SDK
         *     释放SDK使用的资源
         *  
         */
        public static void exitSDK()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;
            mContext.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                using (AndroidJavaClass jc = new AndroidJavaClass("cn.uc.paysdk.SDKDemoUnityCore"))
                {
                    jc.CallStatic("exitSDK", mContext);
                }
            }));
        }

        // Use this for initialization
        void Start()
        {

        }



        // Update is called once per frame
        void Update()
        {

        }
    }
}
