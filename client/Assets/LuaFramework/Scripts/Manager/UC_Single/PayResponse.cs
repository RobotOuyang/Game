using UnityEngine;
using System.Collections;


/**
 *  支付响应结构体，作为支付的回调参数返回给调用者
 */
namespace UC_Single
{
    public class PayResponse
    {

        /**
         * 
         *  以下是支付成功回调结果示例（json字符串格式） < void onSuccessful(string jsonString)>
         *  
         *  注意：
         *    以下示例结果是的data域是json字符串，如果要使用，需要以字符串的形式取出，然后转化为Json对象再进行访问。
         *    访问相关属性用到的key定义在 Response.cs
         * 
         *  {
               "data":"{"ORDER_FINISH_TIME":"20141216130109","ORDER_STATUS":"00","PRO_ID":"0dd935a8422346068174fd25336f8459","IMEI":"354356056492083","PAY_TYPE":"401","PRO_NAME":"印度GP?","ATTACH_INFO":"","CURRENCY_ID":"INR","TRADE_ID":"20141216130102170143","PRO_TYPE":"1","PAY_MONEY":"50.000","EXT_INFO":"?","APP_ID":"000000001"}",  //订单详情
               "message":"00", // 回调确认消息，CP收到回调时需要设置此项的值
               "code":1,       // SDKStatus ，定义在 SDKStatus.cs
               "type":101,     // 回调类型
               "tradeId":"20141216130102170143",  // 交易号
               "status":1      // 操作状态
            }

         *  
         *  
         *   以下是支付失败回调结果示例（json字符串格式）  < void onErrorResponse(string jsonString)>
         * 
         * 
         *   code: 值定义在 SDKStatus.cs 中
         *   message: 详细的错误信息
         * 
         *   {"message":"Pay Unsuccess","code":4104}
         *
         *
         */

        /**
         * 支付结果是json格式，key定义如下
         */
        public static string PRO_NAME = "PRO_NAME";         // 商品名称
        public static string CP_ORDER_ID = "CP_ORDER_ID";       // 游戏传入的订单号
        public static string TRADE_ID = "TRADE_ID";         // 支付系统生成的交易号，与订单号一一对应
        public static string PAY_TYPE = "PAY_TYPE";         // 支付类型（支付宝，微信。。。）
        public static string PAY_MONEY = "PAY_MONEY";           // 支付金额
        public static string ORDER_STATUS = "ORDER_STATUS"; // 订单状态 (  00  成功，01  失败)
        public static string ORDER_FINISH_TIME = "ORDER_FINISH_TIME";// 订单完成时间
        public static string ATTACH_INFO = "ATTACH_INFO";              // 支付附加信息

        /**
         * 支付状态的值
         */
        public static string PAY_STATUS_SUCCESS = "ok";
        public static string PAY_STATUS_ERROR = "error";

        /**
         * 订单错误码
         */
        public static int ERROR_CODE_PARAMS_INVALID = 1000; //非法参数
        public static int ERROR_CODE_UNAUTHRIZE = 1001; //未授权
        public static int ERROR_CODE_AUTHRIZE_ERROR = 1002; //授权错误
        public static int ERROR_CODE_PRODUCT_ID_INVALID = 1003; //商品id错误

    }
}