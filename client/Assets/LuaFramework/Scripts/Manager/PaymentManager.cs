using UnityEngine;
using System.Runtime.InteropServices;
using LuaInterface;
using LuaFramework;
using LitJson;
using System.Collections.Generic;
using UC;
using XiaoMi;
using Umeng;
//SDK以及支付的跨平台管理器
public class PaymentManager : MonoBehaviour
{
#if UNITY_IOS
    // [DllImport("__Internal")]
    // private static extern void _OpenAlipay(string signed_order, string scheme);
    [DllImport("__Internal")]
    private static extern void _OpenWXLogin();
    [DllImport("__Internal")]
    private static extern void _OpenWXPay(string signed_order);
    [DllImport("__Internal")]
    private static extern void _AppStoreBuy(string name, string user_name);
    [DllImport("__Internal")]
    private static extern void _FinishAppPay(string hash_code);
    [DllImport("__Internal")]
    private static extern void _Bpay(string json);
    [DllImport("__Internal")]
    private static extern int _IsWXInstalled();
    [DllImport("__Internal")]
    private static extern void _WXShareUrl(string url, string title, string desc, string file_name, bool is_timeline);
    [DllImport("__Internal")]
    private static extern void _WXSharePicture(string file, int width, int height, bool is_timeline);
#endif
    static LuaFunction alipayCallback;
    static LuaFunction wxloginCallback;
    static LuaFunction wxpayCallback;
    static LuaFunction appstoreCallback;
    static LuaFunction shareCallback;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    //友盟
    public static void GA_Pay (double cash, string source, double coin)
    {
        if (source == "appstore_lhdb")
        {
            print("友盟付费分析appstore_lhdb");
            GA.Pay(cash, GA.PaySource.appstore_lhdb, coin);
        }
        if (source == "appstore_bsfb")
        {
            print("友盟付费分析appstore_bsfb");
            GA.Pay(cash, GA.PaySource.appstore_bsfb, coin);
        }
        
    }
    public static bool HasWXInstalled()
    {
        int is_wx_installed = 1;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    is_wx_installed = jo.Call<int>("IsWXInstalled");
                }
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            is_wx_installed = _IsWXInstalled();
#endif
        }
        return is_wx_installed != 0;
    }

    public static void OpenWXLogin(LuaFunction func)
    {
        wxloginCallback = func;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("OpenWXLogin");
                }
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            _OpenWXLogin();
#endif
        }
    }
    public static void OpenAlipay(string order, LuaFunction func)
    {
       alipayCallback = func;
       if (Application.platform == RuntimePlatform.Android)
       {
           using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
           {
               using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
               {
                   jo.Call("OpenAliPay", order);
               }
           }
       }
       else if (Application.platform == RuntimePlatform.IPhonePlayer)
       {
           #if UNITY_IOS
                   // _OpenAlipay(order, "宗倜");
           #endif
       }
    }

    void alipay_message(string result)
    {
        if (alipayCallback != null)
        {
            alipayCallback.Call(result);
        }
    }
    void wx_login_message(string res)
    {
        if (wxloginCallback != null)
        {
            wxloginCallback.Call(res);
        }
    }
    //贝支付
    public static void BPay(string order)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("Pay", order);
                }
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            //ios贝支付???
            _Bpay(order);
#endif
        }
    }
    public static void OpenWXPay(string order, LuaFunction func)
    {
        wxpayCallback = func;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("OpenWXPay", order);
                }
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            _OpenWXPay(order);
#endif
        }
    }

    void wx_pay_message(string res)
    {
        if (wxpayCallback != null)
        {
            wxpayCallback.Call(res);
        }
    }

    public static void OpenAppStorePay(string item_name, string player_id, LuaFunction func)
    {
        appstoreCallback = func;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            _AppStoreBuy(item_name, player_id);
#endif
        }
        else
        {
            appstoreCallback.Call("");
        }
    }

    public void WXShareUrl(string url, string title, string desc, string file_name, bool is_timeline, LuaFunction call_back)
    {
        shareCallback = call_back;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("WXShareUrl", url, title, desc, Util.DataPath + file_name, is_timeline);
                }
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            _WXShareUrl(url, title, desc, Util.DataPath + file_name, is_timeline);
#endif
        }
    }

    void wx_share_message(string id)
    {
        if (shareCallback != null)
        {
            shareCallback.Call(id);
        }
    }

    // 图片地址，缩略图的宽高， 是否发朋友圈， 图片不能太大，否则坑壁微信会无声无息的失败
    public void WXSharePic(string file_name, int width, int height, bool is_timeline, LuaFunction call_back)
    {
        shareCallback = call_back;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("WXSharePicture", Util.DataPath + file_name, width, height, is_timeline);
                }
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            _WXSharePicture(Util.DataPath + file_name, width, height, is_timeline);
#endif
        }
    }

    public static void FinishAppStorePay(string hash_str)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            _FinishAppPay(hash_str);
#endif
        }
    }

    static List<string> m_payment_list = new List<string>();
    // 收到来自app的支付回调可能是在应用刚启动时，这时候可能lua还没初始化。要保存起来
    void appstore_message(string res)
    {
        if (appstoreCallback != null)
        {
            appstoreCallback.Call(res);
        }
        else
        {
            m_payment_list.Add(res);
        }
    }

    // 当lua初始化好以后，再来处理这些未处理的订单
    public static void CheckOldAppPay(LuaFunction func)
    {
        appstoreCallback = func;
        foreach (string payment in m_payment_list)
        {
            func.Call(payment);
        }
        m_payment_list.Clear();
    }


    static LuaFunction yyb_callback;
    static LuaFunction yyb_pay_callback;

    public static void YsdkGuestLogin(LuaFunction _yyb_callback)
    {
        yyb_callback = _yyb_callback;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("YsdkGuestLogin");
                }
            }
        }
    }

    public static void OpenYsdkPay(string money, LuaFunction _yyb_callback)
    {
        yyb_pay_callback = _yyb_callback;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("OpenYsdkPay", "1", money, "ysdkExt");
                }
            }
        }
    }

    public static string GetYsdkLoginInfo()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    return jo.Call<string>("GetYsdkLoginInfo");
                }
            }
        }
        return "";
    }

    void yyb_login_suc(string res)
    {
        if (yyb_callback != null)
        {
            // Debug.Log(GetYsdkLoginInfo());
            yyb_callback.Call(GetYsdkLoginInfo());
        }
    }

    void yyb_pay_message(string res)
    {
        if (yyb_pay_callback != null)
        {
            yyb_pay_callback.Call(res);
        }
    }

    #region 百度
    static LuaFunction baidu_pay_callback;
    static LuaFunction baidu_login_callback;
    public static void OpenBaiduLogin(LuaFunction callback)
    {
        baidu_login_callback = callback;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("BaiduLogin");
                }
            }
        }
    }

    public static void OpenBaiduPay(string orderid, string product_name, long price, string extinfo, LuaFunction callback)
    {
        baidu_pay_callback = callback;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("BaiduPay", orderid, product_name, price, extinfo, "");
                }
            }
        }
    }

    void baidu_message(string res)
    {
        if (res.Equals("login_suc"))
        {
            if (baidu_login_callback != null)
            {
                baidu_login_callback.Call();
            }
        }
    }
    #endregion

    #region UC久游,阿里游戏
    static LuaFunction UClogincallback;
    static LuaFunction UCLogoutcallback;
    static bool is_uc_init = false;
    static void InitUcGame(int game_id)
    {
        GameParamInfo gameParamInfo = new GameParamInfo();
        gameParamInfo.GameId = game_id;
        gameParamInfo.EnablePayHistory = true;
        gameParamInfo.EnableUserChange = false;
        gameParamInfo.Orientation = UCOrientation.LANDSCAPE;

        SDKParams sdkParams = new SDKParams();
        sdkParams.Add(SDKParamKey.DEBUG_MODE, true);
        sdkParams.Add(SDKParamKey.GAME_PARAMS, gameParamInfo);
        UCGameSdk.initSDK(sdkParams);
    }

    public static void UCLogin(LuaFunction call_back)
    {
        UClogincallback = call_back;
        if (is_uc_init)
        {
            UCGameSdk.login();
        }
        else
        {
            InitUcGame(102273);
        }
    }

    public static void UCLogout(LuaFunction call_back)
    {
        UCLogoutcallback = call_back;
        UCGameSdk.logout();
    }

    public static void UCPay(string order_json)
    {
        JsonData data = JsonMapper.ToObject(order_json);

        SDKParams sdkParams = new SDKParams();
        sdkParams.Add(SDKParamKey.ACCOUNT_ID, "123456");
        sdkParams.Add(SDKParamKey.CP_ORDER_ID, "123456789");
        sdkParams.Add(SDKParamKey.AMOUNT, "0.01");
        sdkParams.Add(SDKParamKey.SERVER_ID, "0");
        sdkParams.Add(SDKParamKey.ROLE_ID, "角色ID");
        sdkParams.Add(SDKParamKey.ROLE_NAME, "角色名");
        sdkParams.Add(SDKParamKey.GRADE, "1");
        sdkParams.Add(SDKParamKey.NOTIFY_URL, "http://www.taobao.com");
        sdkParams.Add(SDKParamKey.CALLBACK_INFO, "游戏支付回调信息");
        sdkParams.Add(SDKParamKey.SIGN_TYPE, "MD5");
        UCGameSdk.pay(sdkParams);
    }

    public static void UCSubmitRoleData(string role_id, string role_name, long role_lvl, long role_time)
    {
        SDKParams sdkParams = new SDKParams();
        sdkParams.Add(SDKParamKey.STRING_ROLE_ID, role_id);
        sdkParams.Add(SDKParamKey.STRING_ROLE_NAME, role_name);
        sdkParams.Add(SDKParamKey.LONG_ROLE_LEVEL, role_lvl);
        sdkParams.Add(SDKParamKey.LONG_ROLE_CTIME, role_time);
        sdkParams.Add(SDKParamKey.STRING_ZONE_ID, "0");
        sdkParams.Add(SDKParamKey.STRING_ZONE_NAME, "default");

        UCGameSdk.submitRoleData(sdkParams);
    }


    static void onUCInitSucc()
    {
        is_uc_init = true;
        UCGameSdk.login();
    }

    /// <summary>
    /// 初始化失败游戏
    /// </summary>
    /// <param name="msg">Message.</param>
    void onUCInitFailed(string msg)
    {
        Debug.Log("初始化失败：" + msg);
    }

    /// <summary>
    /// 登录成功
    /// </summary>
    /// <param name="sid">Sid.</param>
    void onUCLoginSucc(string sid)
    {
        Debug.Log("账号登录成功 sid:" + sid);
        UCConfig.sid = sid;
        if (UClogincallback != null)
        {
            UClogincallback.Call(sid);
        }
        //此处仅供调试参考

    }

    /// <summary>
    /// 登录界面退出，返回到游戏画面
    /// </summary>
    /// <param name="msg">Message.</param>
    void onUCLoginFailed(string msg)
    {
        Debug.Log("账号登录失败：" + msg);
    }

    /// <summary>
    /// 当前登录用户已退出，应将游戏切换到未登录的状态。
    /// </summary>
    void onUCLogoutSucc()
    {
        if (UCLogoutcallback != null)
        {
            UCLogoutcallback.Call();
        }
    }

    /// <summary>
    /// 登录失败
    /// </summary>
    /// <param name="msg">Message.</param>
    void onUCLogoutFailed(string msg)
    {
        Debug.Log("账号退出失败：" + msg);
    }

    /// <summary>
    /// 退出游戏成功
    /// </summary>
    void onUCExitSucc()
    {
        Debug.Log("退出游戏成功");
        Application.Quit();
    }

    /// <summary>
    /// 用户取消退出游戏
    /// </summary>
    /// <param name="msg">Message.</param>
    void onUCExitCanceled(string msg)
    {
        Debug.Log("退出游戏失败：" + msg);
    }

    /// <summary>
    /// 创建订单成功
    /// </summary>
    /// <param name="orderInfo">Order info.</param>
    void onUCCreateOrderSucc(string orderInfo)
    {
        JsonData json = JsonMapper.ToObject(orderInfo);
        string orderId = (string)json["orderId"];
        //int payWayId = (int)json["payWayId"];
        //string payWayName = (string)json["payWayName"];
        //float orderAmount = 0;

        //JsonData jdAmount = (JsonData)json["orderAmount"];
        //switch (jdAmount.GetJsonType())
        //{
        //  case JsonType.Int:
        //      orderAmount = (float)(int)jdAmount;
        //      break;
        //  case JsonType.Double:
        //      orderAmount = (float)(double)jdAmount;
        //      break;
        //  case JsonType.String:
        //      try
        //      {
        //          orderAmount = (float)Convert.ToDouble((string)jdAmount);
        //      }
        //      catch (Exception e)
        //      {
        //          log("order amount is not a valid number");
        //      }
        //      break;
        //  default:
        //      log("order amount is not a valid json number");
        //      break;
        //}
        Debug.Log("支付订单成功：订单号=" + orderId);
    }

    /// <summary>
    /// 用户取消订单支付
    /// </summary>
    /// <param name="orderInfo">Order info.</param>
    void onUCPayUserExit(string orderInfo)
    {
        JsonData json = JsonMapper.ToObject(orderInfo);
        string orderId = (string)json["orderId"];
        Debug.Log("用户取消支付：订单号=" + orderId);
    }
    #endregion

    #region 小米



    static LuaFunction XiaoMilogincallback;
    static LuaFunction XiaoMipaycallback;
    public static void XiaoMiLogin(LuaFunction call_back)
    {
        XiaoMilogincallback = call_back;
        XiaoMiGameSdk.login();
    }

    public static void XiaoMiLogout(LuaFunction call_back)
    {
        XiaoMiGameSdk.logout();
        call_back.Call();
    }

    public static void XiaoMiPay(string cp_order_id, string product_code, string count, string player_id, LuaFunction callback)
    {
        XiaoMipaycallback = callback;

        MiSDKParams sdkParams = new MiSDKParams();
        sdkParams.Add(MiSDKParamKey.CP_ORDER_ID, cp_order_id);
        sdkParams.Add(MiSDKParamKey.PRODUCT_CODE, product_code);
        sdkParams.Add(MiSDKParamKey.COUNT, count);
        sdkParams.Add(MiSDKParamKey.CP_USER_INFO, player_id);
        XiaoMiGameSdk.pay(sdkParams);
    }

    /// <summary>
    /// 登录成功
    /// </summary>
    /// <param name="uidAndSession">uidAndSession.</param>
    public void onXiaoMiLoginSucc(string uidAndSession)
    {
        JsonData json = JsonMapper.ToObject(uidAndSession);
        string uid = (string)json["uid"];
        string session = (string)json["session"];
        if (XiaoMilogincallback != null)
        {
            XiaoMilogincallback.Call(uid, session);
        }
        //此处仅供调试参考
    }
    #endregion

    #region 天天游戏中心
    static LuaFunction TianTianlogincallback;
    static LuaFunction TianTianlogoutcallback;
    static LuaFunction TianTianpaycallback;
    static bool is_tiantian_init = false;
    public static void TianTianLogin(LuaFunction call_back)
    {
        TianTianlogincallback = call_back;
        if (!is_tiantian_init)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    is_tiantian_init = true;
                    jo.Call("UcSdkInit");
                }
            }
        }
        else
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("UcSdkLogin");
                }
            }
        }
    }

    void tiantian_login(string msg)
    {
        if (TianTianlogincallback != null)
        {
            TianTianlogincallback.Call(msg);
        }
    }

    void tiantian_logout()
    {
        if (TianTianlogoutcallback != null)
        {
            TianTianlogoutcallback.Call();
        }
    }

    public static void TianTianLogout(LuaFunction call_back)
    {
        TianTianlogoutcallback = call_back;
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                jo.Call("UcSdkLogout");
            }
        }
    }

    public static void TianTianPay(double price, string name, string order_id, string cp_info, LuaFunction call_back)
    {
        TianTianpaycallback = call_back;
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                jo.Call("UcSdkPay", price, name, order_id, cp_info, "http://114.55.142.14:87/gamehall_tiantian_finish/");
            }
        }
    }
    #endregion

    #region UC,单机游戏

    static LuaFunction UCSinglePaycallback;
    static LuaFunction UCSingleInitCallback;
    static bool _uc_single_inited = false;
    public static void InitUcSingle(LuaFunction call_back)
    {
        UCSingleInitCallback = call_back;
        if (_uc_single_inited)
        {
            UCSingleInitCallback.Call();
            return;
        }
        string appId = "300008973569";
        string appKey = "044B0F69808C6151552A90ACF757A323";

        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic.Add(UC_Single.SDKUnityProtocolKeys.APP_ID, appId);
        dic.Add(UC_Single.SDKUnityProtocolKeys.APP_KEY, appKey);

        string str = UC_Single.Json.Serialize(dic);
        UC_Single.SDKUnityCore.init(str);
    }

    public static void UcSinglePay(string order_id, string product_name, string amount, string ext_info, LuaFunction call_back)
    {
        UCSinglePaycallback = call_back;
        string app_name = "";
        if (AppConst.Channel == "uc-single" || AppConst.Channel == "uc-single_wdj" || AppConst.Channel == "uc-single_uc")
        {
            app_name = "天天连环夺宝";
        }
        else if (AppConst.Channel == "uc_dbyxds" || AppConst.Channel == "uc_dbyxds_wdj" || AppConst.Channel == "uc_dbyxds_uc")
        {
            app_name = "夺宝游戏大师";
        }
        Dictionary<string, string> pay = new Dictionary<string, string>();
        pay.Add(UC_Single.SDKUnityProtocolKeys.CP_ORDER_ID, order_id);
        pay.Add(UC_Single.SDKUnityProtocolKeys.APP_NAME, app_name);
        pay.Add(UC_Single.SDKUnityProtocolKeys.ATTACH_INFO, ext_info);
        pay.Add(UC_Single.SDKUnityProtocolKeys.PRODUCT_NAME, product_name);
        pay.Add(UC_Single.SDKUnityProtocolKeys.AMOUNT, amount);
        string str = UC_Single.Json.Serialize(pay);

        UC_Single.SDKUnityCore.pay(str);
    }

    public static void UcExit()
    {
        UC_Single.SDKUnityCore.exitSDK();
    }

    void on_ucsingle_init_suc()
    {
        _uc_single_inited = true;
        if (UCSingleInitCallback != null)
        {
            UCSingleInitCallback.Call();
        }
    }

    void on_uc_single_exit()
    {
        Application.Quit();
    }

    void on_uc_single_pay_suc()
    {
        if (UCSinglePaycallback != null)
        {
            UCSinglePaycallback.Call();
        }
    }
    #endregion

    #region VIVO

    static LuaFunction VivoPayCallback;
    public static void OpenVivoPay(string transNo, string vivoSignature, string productName, string productDes, string price, int PaymentType, LuaFunction callback)
    {
        VivoPayCallback = callback;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("VivoPay", transNo, vivoSignature, productName, "游戏内金币", price, PaymentType);
                }
            }
        }
    }

    void on_vivo_pay_suc(string str)
    {
        if (VivoPayCallback != null)
        {
            VivoPayCallback.Call(str);
        }
    }

    #endregion

    #region 联想爱贝

    public static void OpenLenovoPay(string player_id, string order_id, int ipay_id, float price, string product_name)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("StartLenovoIPay", ipay_id, player_id, order_id, price, product_name);
                }
            }
        }
    }

    #endregion

    #region 百度单机

    public static void BaiduSinglePay(string propsId, string price, string propsName, string cp_info, int type)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("BaiduPay", propsId, price, propsName, cp_info, type);
                }
            }
        }
    }

    public static void BaiduExit()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("BaiduExit");
                }
            }
        }
    }

    #endregion

    #region 华为
    static LuaFunction HuaweiLoginCallback; 
    public static void HuaweiLogin( bool need_auth, LuaFunction call_back)
    {
        HuaweiLoginCallback = call_back;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("HuaweiLogin", need_auth);
                }
            }
        }
    }
    void huawei_login_message( string token )
    {
        if (HuaweiLoginCallback != null)
        {
            HuaweiLoginCallback.Call(token);
        }
    }
    static LuaFunction HuaweiTTLoginCallback;
    public static void HuaweiTTLogin(int need_auth, LuaFunction call_back)
    {
        HuaweiTTLoginCallback = call_back;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("HuaweittLogin", need_auth);
                }
            }
        }
    }
    void huawei_TT_login_message( string send_json)
    {
        JsonData json = JsonMapper.ToObject(send_json);
        string playerId = (string)json["playerId"];
        string result = (string)json["result"];
        if (HuaweiTTLoginCallback != null)
        {
            HuaweiTTLoginCallback.Call(playerId, result);
        }
    }
    #endregion

    #region 华为


    public static void HuaweiPay(string price, string productName, string productDesc, string requestId, string player_id)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("HuaweiPay", string.Format("{0:.00}", double.Parse(price)), productName, productDesc, requestId, player_id);
                }
            }
        }
    }
    #endregion
}
