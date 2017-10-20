using UnityEngine;
using System.Collections;

/**
 * SDK接口调用参数的Key
 * 
 */
namespace UC_Single
{
    public class SDKUnityProtocolKeys
    {
        // 可选参数，服务器通知地址
        public static string NOTIFY_URL = "notify_url";
        // 可选参数，应用名称
        public static string APP_NAME = "app_name";
        // 可选参数， 透传信息（透传）
        public static string ATTACH_INFO = "attach_info";
        // 客户的唯一订单号
        public static string CP_ORDER_ID = "cp_order_id";
        // APP渠道号
        public static string CHANNEL_ID = "channel_id";
        // APP编号
        public static string APP_ID = "app_id";

        // 用户id
        public static string USER_ID = "user_id";
        // 订单金额
        public static string AMOUNT = "order_amount";
        // 产品名称
        public static string PRODUCT_NAME = "product_name";
        // 业务平台方 JY/PP
        public static string BIZ_ID = "biz_id";
        // 原始包名
        public static string ORIGINAL_PACKNAME = "original_packname";
        // 支付代码
        public static string PAY_CODE = "pay_code";
        //移动短信支付需要传appKey
        public static string APP_KEY = "app_key";
    }
}