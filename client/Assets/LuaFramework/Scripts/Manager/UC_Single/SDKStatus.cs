using UnityEngine;
using System.Collections;

/**
 * SDK的操作返回码
 */
namespace UC_Single
{
    public class SDKStatus
    {

        /**
         *  失败标识
         */
        public static int FAILURE = 0x0;

        /**
         *  成功标识
         */
        public static int SUCCESSFUL = 0x1;


        /**
         * 不支持的编码
         */
        public static int UNSUPPORT_ENCODING = -0x100;

        /**
         * 网络发生错误
         */
        public static int NETWORK_ERROR = -0x101;

        /**
         * 数据解析错误
         */
        public static int PARSE_DATA_ERROR = -0x102;

        /**
         * 签名过程发生错误
         */
        public static int SIGNATURE_ERROR = -0x102;

        /**
         * 验证签名过程发生错误
         */
        public static int VERIFY_ERROR = -0x103;

        /**
         * 不支持的签名方法
         */
        public static int UNSUPPORT_SIGNATURE_METHOD = -0x104;

        /**
         * 授权体为空
         */
        public static int AUTHORIZE_BODY_EMPTY = -0x105;

        /**
         * 授权支付方式为空
         */
        public static int AUTHORIZE_TYPE_EMPTY = -0x106;

        /**
         * 系统内部错误(服务器错误)
         */
        public static int SERVER_SYSTEM_ERROR = -0x1;

        /**
         * 无效的请求IP (服务器错误)
         */
        public static int SERVER_INVALID_REQUEST_IP = -0x2;

        /**
         * 无效的请求参数 (服务器错误)
         */
        public static int SERVER_INVALID_REQUEST_PARAM = -0x3;

        /**
         * 无效的接口服务名称 (服务器错误)
         */
        public static int SERVER_INVALID_SERVICE = -0x4;

        /**
         * 无效的接口版本 (服务器错误)
         */
        public static int SERVER_INVALID_VERSION = -0x5;

        /**
         * 无效APP (服务器错误)
         */
        public static int SERVER_INVALID_APP = -0x6;

        /**
         * 无效开通信息 (服务器错误)
         */
        public static int SERVER_INVALID_OPEN_INFO = -0x7;

        /**
         * 无效的接口安全模式(服务器错误)
         */
        public static int SERVER_INVALID_SECURE_MODE = -0x8;

        /**
         * 签名不正确(服务器错误)
         */
        public static int SERVER_INVALID_SIGN = -0x9;

        /**
         * 签名不正确(服务器错误)
         */
        public static int SERVER_PARTNER_ERROR = -0xA;

        public static int ERROR_CODE_UNINITIALIZED = 0xfff;
        public static int ERROR_CODE_PARAMS_INVALID = 0x1000;
        public static int ERROR_CODE_UNAUTHRIZE = 0x1001;
        public static int ERROR_CODE_AUTHRIZE_ERROR = 0x1002;
        public static int ERROR_CODE_PRODUCT_ID_INVALID = 0x1003;
        public static int ERROR_CODE_NULL_CALLBACK_METHOD = 0x1004;
        public static int ERROR_CODE_CONTEXT_IS_NULL = 0x1005;
        public static int ERROR_CODE_INTENT_IS_NULL = 0x1006;
        public static int ERROR_CODE_PARAMS_IS_NULL = 0x1007;
        public static int ERROR_CODE_PAY_FAIL = 0x1008;
        public static int ERROR_CODE_MCC_FILE_ERROR = 0x1009;
        public static int ERROR_CODE_SDK_RESOURCE_ERROR = 0x100A;
        public static int ERROR_CODE_SDK_INVALID_GAME_ID = 0x100B;

        // 重复订单
        public static int ERROR_CODE_DUPLICATE_TRADE_ID_ERROR = 0x100C;
    }
}