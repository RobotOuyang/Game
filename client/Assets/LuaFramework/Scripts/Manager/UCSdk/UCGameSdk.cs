using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SDK调用入口类
/// </summary>
namespace UC
{
    public class UCGameSdk : MonoBehaviour
    {
        /// <summary>
        /// 调用android java端接口类
        /// </summary>
        private const string SDK_JAVA_CLASS = "cn.uc.gamesdk.unity3d.UCGameSdk";

        /// <summary>
        /// 初始化SDK
        /// </summary>
        public static void initSDK(SDKParams sdkParams)
        {
            bool debugMode = false;
            if (sdkParams.ContainsKey(SDKParamKey.DEBUG_MODE))
            {
                debugMode = (bool)sdkParams[SDKParamKey.DEBUG_MODE];
            }

            string gameId = string.Empty;
            if (sdkParams.ContainsKey(SDKParamKey.GAME_ID))
            {
                gameId = (string)sdkParams[SDKParamKey.GAME_ID];
            }

            GameParamInfo gameInfo = new GameParamInfo();
            if (sdkParams.ContainsKey(SDKParamKey.GAME_PARAMS))
            {
                gameInfo = (GameParamInfo)sdkParams[SDKParamKey.GAME_PARAMS];
            }

            int orientation;
            switch (gameInfo.Orientation)
            {
                case UCOrientation.PORTRAIT:
                    orientation = 0;
                    break;
                case UCOrientation.LANDSCAPE:
                    orientation = 1;
                    break;
                default:
                    orientation = 0;
                    break;
            }
            callSdkApi("initSDK", debugMode, gameInfo.GameId, gameInfo.EnablePayHistory, gameInfo.EnableUserChange, orientation);
        }

        /// <summary>
        /// 调用SDK的用户登录 
        /// </summary>
        public static void login()
        {
            callSdkApi("login");
        }

        /// <summary>
        /// 退出当前登录的账号
        /// </summary>
        public static void logout()
        {
            callSdkApi("logout");
        }

        /// <summary>
        /// 设置玩家选择的游戏分区及角色信息 
        /// </summary>
        public static void submitRoleData(SDKParams sdkParams)
        {
            string zoneId = string.Empty;
            if (sdkParams.ContainsKey(SDKParamKey.STRING_ZONE_ID))
            {
                zoneId = (string)sdkParams[SDKParamKey.STRING_ZONE_ID];
            }

            string zoneName = string.Empty;
            if (sdkParams.ContainsKey(SDKParamKey.STRING_ZONE_NAME))
            {
                zoneName = (string)sdkParams[SDKParamKey.STRING_ZONE_NAME];
            }

            string roleId = string.Empty;
            if (sdkParams.ContainsKey(SDKParamKey.STRING_ROLE_ID))
            {
                roleId = (string)sdkParams[SDKParamKey.STRING_ROLE_ID];
            }

            string roleName = string.Empty;
            if (sdkParams.ContainsKey(SDKParamKey.STRING_ROLE_NAME))
            {
                roleName = (string)sdkParams[SDKParamKey.STRING_ROLE_NAME];
            }

            long roleLevel = 0;
            if (sdkParams.ContainsKey(SDKParamKey.LONG_ROLE_LEVEL))
            {
                roleLevel = (long)sdkParams[SDKParamKey.LONG_ROLE_LEVEL];
            }

            long roleCTime = 0;
            if (sdkParams.ContainsKey(SDKParamKey.LONG_ROLE_CTIME))
            {
                roleCTime = (long)sdkParams[SDKParamKey.LONG_ROLE_CTIME];
            }

            callSdkApi("submitRoleData", zoneId, zoneName, roleId, roleName, roleLevel, roleCTime);
        }

        /// <summary>
        /// 支付
        /// </summary>
        public static void pay(SDKParams sdkParams)
        {
            string accountId = null;
            if (sdkParams.ContainsKey(SDKParamKey.ACCOUNT_ID))
            {
                accountId = (string)sdkParams[SDKParamKey.ACCOUNT_ID];
            }

            string cpOrderId = null;
            if (sdkParams.ContainsKey(SDKParamKey.CP_ORDER_ID))
            {
                cpOrderId = (string)sdkParams[SDKParamKey.CP_ORDER_ID];
            }

            string amount = null;
            if (sdkParams.ContainsKey(SDKParamKey.AMOUNT))
            {
                amount = (string)sdkParams[SDKParamKey.AMOUNT];
            }

            string serverId = null;
            if (sdkParams.ContainsKey(SDKParamKey.SERVER_ID))
            {
                serverId = (string)sdkParams[SDKParamKey.SERVER_ID];
            }

            string roleId = null;
            if (sdkParams.ContainsKey(SDKParamKey.ROLE_ID))
            {
                roleId = (string)sdkParams[SDKParamKey.ROLE_ID];
            }

            string roleName = null;
            if (sdkParams.ContainsKey(SDKParamKey.ROLE_NAME))
            {
                roleName = (string)sdkParams[SDKParamKey.ROLE_NAME];
            }

            string grade = null;
            if (sdkParams.ContainsKey(SDKParamKey.GRADE))
            {
                grade = (string)sdkParams[SDKParamKey.GRADE];
            }

            string callbackInfo = null;
            if (sdkParams.ContainsKey(SDKParamKey.CALLBACK_INFO))
            {
                callbackInfo = (string)sdkParams[SDKParamKey.CALLBACK_INFO];
            }

            string notifyUrl = null;
            if (sdkParams.ContainsKey(SDKParamKey.NOTIFY_URL))
            {
                notifyUrl = (string)sdkParams[SDKParamKey.NOTIFY_URL];
            }

            string signType = null;
            if (sdkParams.ContainsKey(SDKParamKey.SIGN_TYPE))
            {
                signType = (string)sdkParams[SDKParamKey.SIGN_TYPE];
            }

            string sign = null;
            if (sdkParams.ContainsKey(SDKParamKey.SIGN))
            {
                sign = (string)sdkParams[SDKParamKey.SIGN];
            }

            callSdkApi("pay", accountId, cpOrderId, amount, serverId, roleId, roleName, grade, callbackInfo, notifyUrl, signType, sign);
        }

        /// <summary>
        /// 退出SDK，游戏退出前必须调用此方法，以清理SDK占用的系统资源。如果游戏退出时不调用该方法，可能会引起程序错误。
        /// </summary>
        public static void exitSDK()
        {
            callSdkApi("exitSDK");
        }

        private static void callSdkApi(string apiName, params object[] args)
        {
            using (AndroidJavaClass cls = new AndroidJavaClass(SDK_JAVA_CLASS))
            {
                cls.CallStatic(apiName, args);
            }
        }
    }
}
