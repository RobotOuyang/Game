
namespace LuaFramework {
    public class Protocal {
        ///BUILD TABLE
        /// 其实这个表在lua里也有一份，这个是lua用来处理事件分发的。 在c#里图 方便就直接用了， 这四个的值再也不能更改
        public const int Connect = 101;     //连接服务器
        public const int Exception = 102;     //异常掉线
        public const int Disconnect = 103;     //正常断线
        public const int Message = 104; // 发送网络消息
        public const int MessageFailed = 105; // 发送消息失败
    }
}