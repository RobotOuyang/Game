using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace UC_Single
{
    public class Demo : MonoBehaviour
    {

        private static int WIDHT = 300;
        private static int HEIGHT = 150;
        private static int TEXT_SIZE = 25;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }


        private String mAmount = "2";
        private String mPayCode = "30000897356904";

        void OnGUI()
        {
            GUI.skin.button.fontSize = TEXT_SIZE;
            GUI.skin.textField.fontSize = TEXT_SIZE;
            if (mAmount.Length == 0)
            {
                mAmount = "金额";
            }
            if (mPayCode.Length == 0)
            {
                mPayCode = "paycode";
            }
            mAmount = GUILayout.TextField(mAmount, GUILayout.Width(WIDHT), GUILayout.Height(HEIGHT));
            mPayCode = GUILayout.TextField(mPayCode, GUILayout.Width(WIDHT), GUILayout.Height(HEIGHT));
            if (mPayCode == "paycode")
            {
                mPayCode = "";
            }
            if (GUILayout.Button("初始化SDK", GUILayout.Width(WIDHT), GUILayout.Height(HEIGHT)))
            {
                Debug.Log("Start InitSDK!");
                if (Application.platform != RuntimePlatform.Android)
                    return;
#if UNITY_ANDROID

			// your appId , private_key and public_key
			string appId = "300008973569";
			string appKey = "044B0F69808C6151552A90ACF757A323";

			Dictionary<string,string>  dic = new Dictionary<string,string>();
			dic.Add(SDKUnityProtocolKeys.APP_ID,appId);
			dic.Add(SDKUnityProtocolKeys.APP_KEY, appKey);

			string str = UC_Single.Json.Serialize(dic);
            Debug.Log("Start pay in android!");
            SDKUnityCore.init (str);

#endif

            }
            if (GUILayout.Button("支付", GUILayout.Width(WIDHT), GUILayout.Height(HEIGHT)))
            {
                Debug.Log("Start Pay!");
                if (Application.platform != RuntimePlatform.Android)
                    return;
#if UNITY_ANDROID

			string app_name = "unity demo";
            string cp_order_id = GetTimeStamp();
            string product_name = "product_name";
            string notifyUrl = "";

			Dictionary<string,string>  pay = new Dictionary<string,string>();
            pay.Add(SDKUnityProtocolKeys.CP_ORDER_ID, cp_order_id);
			pay.Add(SDKUnityProtocolKeys.APP_NAME,app_name);
            pay.Add(SDKUnityProtocolKeys.PRODUCT_NAME,product_name);
            pay.Add(SDKUnityProtocolKeys.AMOUNT,mAmount);
			pay.Add(SDKUnityProtocolKeys.NOTIFY_URL,notifyUrl);
            pay.Add(SDKUnityProtocolKeys.PAY_CODE, mPayCode);
            string str = UC_Single.Json.Serialize(pay);

			SDKUnityCore.pay(str);
#endif
            }

            if (GUILayout.Button("退出SDK", GUILayout.Width(WIDHT), GUILayout.Height(HEIGHT)))
            {
                Debug.Log("Exit!");
                if (Application.platform != RuntimePlatform.Android)
                    return;
#if UNITY_ANDROID
			SDKUnityCore.exitSDK();
#endif
            }

        }
    }
}