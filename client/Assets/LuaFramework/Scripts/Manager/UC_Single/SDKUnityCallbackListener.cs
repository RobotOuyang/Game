using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * SDK的回调接口。注意，这个回调接口是被绑定在“Main Camera"对象上的
 * 回调一共有三个：1. 游戏SDK初始化回调 2.支付SDK初始化回调 3. 支付结果回调， 1是调用onSDKInitSuccessful，2，3则是调用onPaySuccessful,并根据LISTENER_TYPE判断是初始化回调还是支付回调
 * 
 */
namespace UC_Single
{
    public class SDKUnityCallbackListener : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /**
     * 游戏SDK初始化成功回调，SDK初始化成功时候都会回调该接口。
     */
        void onSDKInitSuccessful(string jsonString)
        {
            Debug.Log("Calling from android onSuccessful callback result:" + jsonString);
            //初始化成功回调
            showToast("初始化成功!");
        }

        /**
         * SDK成功回调，支付初始化成功，支付成功的时候都会回调该接口。
         */
        void onPaySuccessful(string jsonString)
        {
            Debug.Log("Calling from android onSuccessful callback result:" + jsonString);

            Dictionary<string, object> result = UC_Single.Json.Deserialize(jsonString) as Dictionary<string, object>;
            if (result.ContainsKey(Response.TYPE))
            {
                long type = (long)result[Response.TYPE];
                if (type == Response.LISTENER_TYPE_INIT)
                {
                    //初始化成功回调
                    showToast("支付初始化成功!");
                }
                if (type == Response.LISTENER_TYPE_PAY)
                {
                    //支付成功回调
                    showToast("支付成功!\r\n" + jsonString);
                    if (result.ContainsKey(Response.DATA))
                    {
                        // data是一个json字符串
                        string data = (string)result[Response.DATA];
                        Dictionary<string, object> billDetail = UC_Single.Json.Deserialize(data) as Dictionary<string, object>;
                        string productName = (string)billDetail[PayResponse.PRO_NAME];
                    }
                }
            }
        }

        /**
         * SDK错误回调，初始化失败，支付失败的时候会被调用
         *    支付失败回调参数示例如下
         *    {"message":"Pay Unsuccess","code":4104}
         */
        void onErrorResponse(string jsonString)
        {
            Debug.Log("Calling from android onErrorResponse callback result:" + jsonString);
            showToast(jsonString);
        }

        /**
    * 游戏SDK退出回调，用户点击退出界面的确认按钮的时候会回调该接口。
*/
        void onExitSDK(string jsonString)
        {
            Debug.Log("Calling from android onExitSDK callback result:" + jsonString);
            Application.Quit();
        }

        public void showToast(string msg)
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject mContext = jc.GetStatic<AndroidJavaObject>("currentActivity");
            mContext.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                using (AndroidJavaClass jcToast = new AndroidJavaClass("android.widget.Toast"))
                {
                    AndroidJavaObject toast = jcToast.CallStatic<AndroidJavaObject>("makeText", mContext, msg, 1);
                    toast.Call("show");
                }
            }));
        }
    }
}