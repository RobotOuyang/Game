using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SDK调用入口类
/// </summary>
namespace XiaoMi
{
    public class XiaoMiGameSdk : MonoBehaviour
    {
        /// <summary>
        /// 调用android java端接口类
        /// </summary>
        private const string SDK_JAVA_CLASS = "com.sdk.migame.payment.MiGameSdk";

        /// <summary>
        /// 调用SDK的用户登录 
        /// </summary>
        public static void login()
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("MiGameLogin");
                }
            }
        }

        /// <summary>
        /// 检查系统登录状态
        /// </summary>
        public static void loginStatus()
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("GetMiLoginStatus");
                }
            }
        }

        /// <summary>
        /// 退出当前登录的账号
        /// </summary>
        public static void logout()
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("MiGameLogout");
                }
            }
        }

        /// <summary>
        /// 支付
        /// </summary>
        public static void pay(MiSDKParams sdkParams)
        {
            string cpOrderId = null;
            if (sdkParams.ContainsKey(MiSDKParamKey.CP_ORDER_ID))
            {
                cpOrderId = (string)sdkParams[MiSDKParamKey.CP_ORDER_ID];
            }

            string productCode = null;
            if (sdkParams.ContainsKey(MiSDKParamKey.PRODUCT_CODE))
            {
                productCode = (string)sdkParams[MiSDKParamKey.PRODUCT_CODE];
            }

            string count = null;
            if (sdkParams.ContainsKey(MiSDKParamKey.COUNT))
            {
                count = (string)sdkParams[MiSDKParamKey.COUNT];
            }

            string cpUserInfo = null;
            if (sdkParams.ContainsKey(MiSDKParamKey.CP_USER_INFO))
            {
                cpUserInfo = (string)sdkParams[MiSDKParamKey.CP_USER_INFO];
            }
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("MiGamePay", cpOrderId, productCode, count, cpUserInfo);
                }
            }
        }
    }
}
