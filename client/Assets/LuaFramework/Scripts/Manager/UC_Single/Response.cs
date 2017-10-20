using UnityEngine;
using System.Collections;

namespace UC_Single
{
    /**
	 * 
     * 定义支付成功回调参数中json数据的key值
     * 
	 */
    public class Response
    {

        /**
         * 回调类型
         */

        public static long LISTENER_TYPE_INIT = 100; // 初始化回调
        public static long LISTENER_TYPE_PAY = 101; // 支付回调
        public static long LISTENER_TYPE_EXIT = 102; // 退出回调


        /**
          以下属性是支付成功回调参数中json数据的key
         */
        public static string CODE = "code";         // SDK 状态吗，定义在 SDKStatus.cs 中
        public static string STATUS = "status";     // 操作状态，定义在 SDKStatus.cs 中
        public static string MESSAGE = "message";   // 响应消息
        public static string TYPE = "type";       // 回调类型
                                                  /*
                                                   data属性表示操作返回的结果数据，值是一个字符串。
                                                   *  注意：
                                                         当回调类型为初始化回调的时候，data域是一个空字符串
                                                         当回调类型为支付回调的时候， data域是一个json字符串。
                                                   * ========================================================
                                                   */
        public static string DATA = "data";
        public static string TRADEID = "tradeId";   // the OrderId of this operation (exist when the type is LISTENER_TYPE_PAY )

    }
}
