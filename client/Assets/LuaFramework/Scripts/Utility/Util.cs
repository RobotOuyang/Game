using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LuaFramework {
    public class Util {
#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern string _GetIdfa();
        [DllImport("__Internal")]
        private static extern string _GetVersion();
#endif

        private static List<string> luaPaths = new List<string>();

        public static int Int(object o) {
            return Convert.ToInt32(o);
        }

        public static float Float(object o) {
            return (float)Math.Round(Convert.ToSingle(o), 2);
        }

        public static long Long(object o) {
            return Convert.ToInt64(o);
        }

        public static int Random(int min, int max) {
            return UnityEngine.Random.Range(min, max);
        }

        public static float Random(float min, float max) {
            return UnityEngine.Random.Range(min, max);
        }

        public static string Uid(string uid) {
            int position = uid.LastIndexOf('_');
            return uid.Remove(0, position + 1);
        }

        // 不考虑Z坐标, 暂时不考虑rect的旋转
        public static bool WorldPosInRect(Vector3 pos, RectTransform rect)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return pos.x >= corners[0].x && pos.x <= corners[3].x && pos.y >= corners[0].y && pos.y <= corners[1].y;
        }

        public static long GetTime() {
            TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            return (long)ts.TotalMilliseconds;
        }

        public static long GetUnixTime()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public static DateTime GetDateTime(long unix)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return startTime.AddSeconds(unix);
        }

        public static void ShakeScreen()
        {
#if !UNITY_STANDALONE_WIN
            Handheld.Vibrate();
#endif
        }

        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        public static string md5(string source) {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            string destString = "";
            for (int i = 0; i < md5Data.Length; i++) {
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            destString = destString.PadLeft(32, '0');
            return destString;
        }

        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string md5file(string file) {
            try {
                FileStream fs = new FileStream(file, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fs);
                fs.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++) {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            } catch (Exception ex) {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }

        /// <summary>
        /// 清理内存
        /// </summary>
        public static void ClearMemory() {
            GC.Collect();
            LuaManager mgr = AppFacade.Instance.GetManager<LuaManager>(ManagerName.Lua);
            if (mgr != null) mgr.LuaGC();
        }

        private static string fast_get_DataPath = null;
        /// <summary>
        /// 取得数据存放目录
        /// </summary>
        public static string DataPath {
            get {
                if (fast_get_DataPath != null)
                {
                    return fast_get_DataPath;
                }
                string game = AppConst.AppName.ToLower();
                if (Application.isMobilePlatform) {
                    fast_get_DataPath = Application.persistentDataPath + "/" + game + "/";
                    return fast_get_DataPath;
                }
                if (Application.platform == RuntimePlatform.OSXEditor) {
                    int i = Application.dataPath.LastIndexOf('/');
                    fast_get_DataPath = Application.dataPath.Substring(0, i + 1) + game + "/";
                    return fast_get_DataPath;
                }
                return fast_get_DataPath = "c:/" + game + "/";
            }
        }

        // 将某个url替换成ip后访问，解决某些运营商dns访问问题
        public static WWW CreateHostWWW(string url, string ip)
        {
            string http_dns_host = url.Substring(url.IndexOf("//") + 2);
            http_dns_host = http_dns_host.Substring(0, http_dns_host.IndexOf("/"));
            Dictionary<string, string> http_dnsheaders = new Dictionary<string, string>();
            http_dnsheaders.Add("Host", http_dns_host);
            return new WWW(url.Replace(http_dns_host, ip), null, http_dnsheaders);
        }

        // 暂时standalone平台只支持windows的热更新
        public static string GetRelativePath()
        {
#if UNITY_EDITOR
            //return "file:///" + System.Environment.CurrentDirectory.Replace("\\", "/") + "/Assets/" + AppConst.AssetDir + "/";
            return "file:///" + DataPath;
#elif UNITY_STANDALONE_WIN
            return "file:///" + DataPath;
#elif UNITY_IPHONE || UNITY_ANDROID
            return "file://" + DataPath;
#else
            return Application.streamingAssetsPath + "/";
#endif
        }

        // 文件是否已经在缓存
        public static bool IsFileInDataPath(string file_name)
        {
            return File.Exists(DataPath + file_name);
        }
        
        public static string GetStreamingPath()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return "file:///" + Application.streamingAssetsPath + "/";
#elif UNITY_ANDROID
            return Application.streamingAssetsPath + "/";
#else
            return "file://" + Application.streamingAssetsPath + "/";
#endif
        }

        /// <summary>
        /// 取得行文本
        /// </summary>
        public static string GetFileText(string path) {
            return File.ReadAllText(path);
        }

        /// <summary>
        /// 网络可用
        /// </summary>
        public static bool NetAvailable {
            get {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }

        /// <summary>
        /// 是否是无线
        /// </summary>
        public static bool IsWifi {
            get {
                return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            }
        }

        public static int GetLuaObjectCount()
        {
            LuaInterface.ObjectTranslator value = LuaInterface.ObjectTranslator.Get((IntPtr)0);
            return value.objectsBackMap.Count;
        }

        /// <summary>
        /// 应用程序内容路径
        /// </summary>
        public static string AppContentPath() {
            return Application.streamingAssetsPath + "/";
        }

        public static void Log(string str) {
            Debug.Log(str);
        }

        public static void LogWarning(string str) {
            Debug.LogWarning(str);
        }

        public static void LogError(string str) {
            Debug.LogError(str);
        }

        /// <summary>
        /// 防止初学者不按步骤来操作
        /// </summary>
        /// <returns></returns>
        public static int CheckRuntimeFile() {
            if (!Application.isEditor) return 0;
            string streamDir = Application.dataPath + "/StreamingAssets/";
            if (!Directory.Exists(streamDir)) {
                return -1;
            } else {
                string[] files = Directory.GetFiles(streamDir);
                if (files.Length == 0) return -1;

                if (!File.Exists(streamDir + "files.txt")) {
                    return -1;
                }
            }
            string sourceDir = AppConst.FrameworkRoot + "/ToLua/Source/Generate/";
            if (!Directory.Exists(sourceDir)) {
                return -2;
            } else {
                string[] files = Directory.GetFiles(sourceDir);
                if (files.Length == 0) return -2;
            }
            return 0;
        }

        /// <summary>
        /// 执行Lua方法
        /// </summary>
        public static object[] CallMethod(string module, string func, params object[] args) {
            LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>(ManagerName.Lua);
            if (luaMgr == null) return null;
            return luaMgr.CallFunction(module + "." + func, args);
        }

        /// <summary>
        /// 检查运行环境
        /// </summary>
        public static bool CheckEnvironment() {
#if UNITY_EDITOR
            int resultId = Util.CheckRuntimeFile();
            if (resultId == -1) {
                Debug.LogError("没有找到框架所需要的资源，单击Game菜单下Build xxx Resource生成！！");
                EditorApplication.isPlaying = false;
                return false;
            } else if (resultId == -2) {
                Debug.LogError("没有找到Wrap脚本缓存，单击Lua菜单下Gen Lua Wrap Files生成脚本！！");
                EditorApplication.isPlaying = false;
                return false;
            }
#endif
            return true;
        }

        public static string GetAddressIP()  
        {  
            string AddressIP = string.Empty;  
#if UNITY_IPHONE
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();  
            for (int i = 0; i < adapters.Length; i++)  
            {  
                if (adapters[i].Supports(NetworkInterfaceComponent.IPv4))  
                {  
                    UnicastIPAddressInformationCollection uniCast = adapters[i].GetIPProperties().UnicastAddresses;  
                    if (uniCast.Count > 0)  
                    {  
                        for (int j = 0; j < uniCast.Count; j++)  
                        {  
                            if (uniCast[j].Address.AddressFamily == AddressFamily.InterNetwork)  
                            {  
                                AddressIP = uniCast[j].Address.ToString();  
                            }  
                        }  
                    }  
                }  
            }  
#endif
#if UNITY_STANDALONE_WIN || UNITY_ANDROID
            for (int i = 0; i < Dns.GetHostEntry(Dns.GetHostName()).AddressList.Length; i++)  
            {  
                if (Dns.GetHostEntry(Dns.GetHostName()).AddressList[i].AddressFamily.ToString() == "InterNetwork")  
                {  
                    AddressIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[i].ToString();  
                }  
            } 
#endif
            return AddressIP;  
        }

        public static string GetMacAddress()  
        {
            string physicalAddress = "";
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                physicalAddress = File.ReadAllText("/sys/class/net/wlan0/address");
                if (physicalAddress != null)
                {
                    physicalAddress = physicalAddress.Trim();
                }
            }
            catch(Exception e)
            {

            }
#endif
            return physicalAddress;
        }

        public static string GetDeviceType()
        {
            return SystemInfo.deviceModel;
        } 

        public static string GetOperatingSystem()
        {
            return SystemInfo.operatingSystem;
        }

        public static string GetDeviceUniqueIdentifier()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public static string GetSystemVersion()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IPHONE
                return _GetVersion().Replace(",", "_");
#endif
            }
            return Application.version; 
        }

        public static string GetIdfa()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IPHONE
                return _GetIdfa();
#endif
            }
            return SystemInfo.deviceUniqueIdentifier;
        }

        public static string GetFilesTextUnixTime(string files_txt_path)
        {
            string first_files_txt = File.ReadAllLines(files_txt_path)[0];
            string first_unix_time = first_files_txt.Split('|')[3];
            return first_unix_time + first_unix_time + first_unix_time;
        }

        public static bool FirstIsSecondPrefix(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1.Length > bytes2.Length)
            {
                return false;
            }
            for (int i = 0; i < bytes1.Length; i ++)
            {
                if (bytes1[i] != bytes2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static byte[] GetUnixTimeBytes(string files_txt_path)
        {
            string files_txt_unix_time = Util.GetFilesTextUnixTime(files_txt_path);
            return Encoding.Default.GetBytes(files_txt_unix_time);
        }

        public static byte[] AddUnixTimePrefix(string files_txt_path, byte[] origin_bytes)
        {
            byte[] files_txt_bytes = Util.GetUnixTimeBytes(files_txt_path);
            byte[] goal_bytes = new byte[origin_bytes.Length + files_txt_bytes.Length];
            Array.Copy(files_txt_bytes, 0, goal_bytes, 0, files_txt_bytes.Length);
            Array.Copy(origin_bytes, 0, goal_bytes, files_txt_bytes.Length, origin_bytes.Length);
            return goal_bytes;
        }

        public static byte[] RemoveUnixTimePrefix(string files_txt_path, byte[] origin_bytes)
        {
            byte[] files_txt_bytes = Util.GetUnixTimeBytes(files_txt_path);
            byte[] goal_bytes = new byte[origin_bytes.Length - files_txt_bytes.Length];
            Array.Copy(origin_bytes, files_txt_bytes.Length, goal_bytes, 0, goal_bytes.Length);
            return goal_bytes;
        }
    }
}
