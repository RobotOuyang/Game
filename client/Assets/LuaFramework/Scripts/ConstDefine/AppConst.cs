using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LuaFramework {
    public class AppConst {
        public static bool DebugMode = true;
        /// <summary>
        /// 如果开启更新模式，前提必须启动框架自带服务器端。
        /// 否则就需要自己将StreamingAssets里面的所有内容
        /// 复制到自己的Webserver上面，并修改下面的WebUrl。
        /// </summary>
        public const bool luaDebugMode = true;                    // 开了这个以后，编辑器里运行的时候会直接加载当前目录下的lua

        public static bool UpdateMode = true;                        //更新模式， 在windows编辑器关闭
        public static bool EditorUpdate = false;                    // 编辑器是不是热更新
        public static bool LuaByteMode = true;                       //Lua字节码模式-默认关闭 （尼玛啊这个字节码在arm64位上不识别，IOS上直接关闭了）
        public const bool LuaBundleMode = true;                    //Lua代码AssetBundle模式
        public const bool AtlasMode = true;                         // Textures目录是否打包成图集

        public const int TimerInterval = 1;
        public const int GameFrameRate = 30;                        //游戏帧频

        public static bool EidtorNotUseBundle = false;  // 编辑器模式下是否使用bundle

        //-------------------------------目录结构
        public const string PrefabPath = "prefabs/";                 // 预制体打包后的目录
        public const string TexturePath = "textures/";               // 纹理打包后的目录
        public const string ScenePath = "scenes/";               // 场景打包后的目录，有一个启动场景是不打包的
        public const string OtherResourcePath = "otherresource/";   // 其他杂七杂八资源的路径
        public const string TexPrefabPath = "textureprefab/";       // 纹理预制体目录
        public const string AudioPrefabPath = "audioprefab/";       // 音效预制体目录
        public const string SoundPath = "otherresource/sound";  // 声音的路径

        public const string AppName = "LuaFramework";               //程序主目录
        public const string LuaTempDir = "Lua/";                    //临时目录
        public const string AppPrefix = AppName + "_";              //应用程序前缀
        public const string ExtName = ".unity3d";                   //素材扩展名

        public const string AssetDir = "StreamingAssets";           //素材目录
        public const string AssetDir_win = "StreamingAssets_win";           //素材目录
        public const string AssetDir_andorid = "StreamingAssets_android";           //素材目录
        public const string AssetDir_ios = "StreamingAssets_ios";           //素材目录

        public static string Server_info_url = "http://www.vr-cat.com/server/";    // 获取服务器信息的url，不可变更
        public static string UserId = string.Empty;                 //用户ID
        public static int SocketPort = 10000;                           //Socket服务器端口
        public static string SocketAddress = "127.0.0.1";          //Socket服务器地址
        public static int GatePort = 3014;
        public static string AccountServer = "101.37.160.50:8000";      // 账号服务器，用于获取token
        public static string AvatarServer = "127.0.0.1:0";  // 头像下载
        public static string PatchServer = "127.0.0.1:0";  // 补丁下载
        public static string PatchPath = "/download/?remote=lhdb_";  // 下载补丁的路径
        public static string[] PatchIgnorePaths = { };    // 更新时如果不存在则忽略的路径，用于分包加载

        public static string PlatformName = "PC";   // 平台名字，这个如果不是游客登陆，要换成对应渠道的名字
        public static string Access_info = SystemInfo.deviceUniqueIdentifier;  // 用于游客登陆的特定字符串
        public static string Channel = "";

        public static string FrameworkRoot {
            get {
                return Application.dataPath + "/" + AppName;
            }
        }
    }
}