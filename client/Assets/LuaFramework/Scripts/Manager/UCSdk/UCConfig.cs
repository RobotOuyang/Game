//此类用于对参数进行集中管理
namespace UC
{
    public static class UCConfig
    {

        //联调环境参数
        public static int cpId = 20087;
        public static int gameId = 119474;
        public static string serverId = "0";

        //注意：由cp服务器存储，cp后台自行计算sign，demo只供演示，接入时请去掉
        public static string apiKey = "f25e24a1cb03252b48938235149f0798";


        public static bool debugMode = true;

        //注意：demo测试模拟cp服务器与阿里游戏服务器校验账号过程，cp接入时请去掉，由cp后台处理
        public static string[] accountVerifySessionUrls = {
            "http://sdk.test4.9game.cn/cp/account.verifySession",
            "http://sdk.9game.cn/cp/account.verifySession"
    };

        public static UCOrientation orientation = UCOrientation.LANDSCAPE;
        public static bool enablePayHistory = true;
        public static bool enableUserChange = true;
        public static string sid = "";
        public static string accountId = "";

    }
}
