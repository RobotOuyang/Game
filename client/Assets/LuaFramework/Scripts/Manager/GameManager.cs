using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;
using System.Text;
using LuaInterface;

public class LoadingMessage
{
    public string message;
    public float value;

    public LoadingMessage(string msg, float val)
    {
        message = msg;
        value = val;
    }
}

namespace LuaFramework {
    public class GameManager : Manager {
        private List<string> downloadFiles = new List<string>();

#if UNITY_IOS
        [DllImport("__Internal")]   // 用于保存ios的keychain的
        static extern string _GetKeyChain(string main_key, string data_key);
        [DllImport("__Internal")]
        static extern void _SaveKeyChain(string main_key, string data_key, string value);
#endif

        bool IsIgnore(string file_name)
        {
            foreach(string path in AppConst.PatchIgnorePaths)
            {
                if (file_name.StartsWith(path))
                {
                    if (Directory.Exists(Util.DataPath + path))
                    {
                        // 目录已经有了，就不忽视了。
                        return false;
                    }
                    return true; // 只有当文件属于被忽视目录，并且该目录不存在时，忽视此文件。
                }
            }
            return false;
        }

        public void SetPatchIgnorePaths(string[] PatchIgnorePaths)
        {
            AppConst.PatchIgnorePaths = PatchIgnorePaths;
        }

        /// <summary>
        /// 初始化游戏管理器
        /// </summary>
        void Awake() {
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        void Init() {
            Caching.expirationDelay = 600;
            DontDestroyOnLoad(gameObject);  //防止销毁自己
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = AppConst.GameFrameRate;

#if DEBUG
            AppConst.DebugMode = true;
            Debug.logger.logEnabled = true;
#else
            AppConst.DebugMode = false;
            Debug.logger.logEnabled = false;
#endif

            // 设置平台，ios读取钥匙串
            if (Application.platform == RuntimePlatform.Android)
            {
                AppConst.PlatformName = "Android";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IOS
                AppConst.PlatformName = "IOS";
                // 因为每次ios卸载后，这个access_info可能会变，于是用keychain保存一下，在appid不变，不刷机，不重置手机的情况下，游客不会丢失
                if (string.IsNullOrEmpty(_GetKeyChain("gamehall", "access_info")))
                {
                    _SaveKeyChain("gamehall", "access_info", AppConst.Access_info);
                }
                else
                {
                    AppConst.Access_info = _GetKeyChain("gamehall", "access_info");
                }
#endif
            }

            ResManager.Initialize(AppConst.AssetDir, delegate ()
            {
                StartCoroutine(CheckExtractLuaFiles());
            });
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void CheckExtractResource(LuaFunction call_back)
        {
            bool isExists = Directory.Exists(Util.DataPath) && File.Exists(Util.DataPath + "files.txt");

            if (isExists) {
                if (call_back != null) call_back.Call(true, "正在检查更新", 1);
                return;   //文件已经解压过了，自己可添加检查文件列表逻辑
            }
            try
            {
                StartCoroutine( OnExtractResource(null, call_back) );    //启动释放协成 
            }
            catch(Exception e)
            {
                if (call_back != null) call_back.Call(false, "解压失败，请退出重试", 1);
                Debug.LogError(e.ToString());
            }
        }

        int extract_count = 0;
        float extract_finished = 0;

        IEnumerator ExtractOneFile(string infile, string outfile)
        {
            using (WWW www = new WWW(infile))
            {
                yield return www;

                if (www.isDone)
                {
                    File.WriteAllBytes(outfile, www.bytes);
                }
                extract_count = extract_count - 1;
                extract_finished++;
            }
        }

        IEnumerator CheckExtractLuaFiles()
        {
            bool isExists = Directory.Exists(Util.DataPath) && Directory.Exists(Util.DataPath + "lua") && File.Exists(Util.DataPath + "files.txt") &&
                File.Exists(Util.DataPath + "__unity56__");
            if (isExists)
            {
                OnResourceInited();
                yield break;
            }
            // 开始新的解压，防止有老版本不兼容的bundle，都删除了
            if (Directory.Exists(Util.DataPath))
            {
                Directory.Delete(Util.DataPath, true);
            }

            yield return StartCoroutine(OnExtractResource("lua/", null));    //启动释放协成
            if (File.Exists(Util.DataPath + "files.txt"))
            {
                File.Delete(Util.DataPath + "files.txt");
            }
            OnResourceInited();
        }

        IEnumerator OnExtractResource(string start_with, LuaFunction call_back)
        {
            string dataPath = Util.DataPath;  //数据目录
            string resPath = Util.AppContentPath(); //游戏包资源目录
            extract_count = 0;
            extract_finished = 0;

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            string infile = resPath + "files.txt";
            string outfile = dataPath + "files.txt";
            if (File.Exists(outfile)) File.Delete(outfile);

            if (Application.platform == RuntimePlatform.Android) {
                WWW www = new WWW(infile);
                yield return www;

                if (www.isDone)
                {
                    File.WriteAllBytes(outfile, www.bytes);
                }
                www.Dispose();
                yield return 0;
            } else File.Copy(infile, outfile, true);
            yield return new WaitForEndOfFrame();

            string message = "正在解压资源，该过程不会消耗流量";
            //释放所有文件到数据目录
            string[] files = File.ReadAllLines(outfile);
            float cur_count = 0;
            float extract_size = 0;
            byte[] files_txt_bytes = Util.GetUnixTimeBytes(Util.DataPath + "files.txt");
            foreach (var file in files) {
                string[] fs = file.Split('|');
                // 过滤一些特殊的文件
                if (start_with != null && !file.StartsWith(start_with))
                {
                    continue;
                }
                //infile = resPath + fs[0];  //
                infile = resPath + ResManager.GetConfusePath(fs[0]);
                if (IsIgnore(fs[0])) continue;
                outfile = dataPath + fs[0];
                long file_size = long.Parse(fs[2]);
                cur_count++;

                string dir = Path.GetDirectoryName(outfile);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                if (Application.platform == RuntimePlatform.Android) {
                    extract_count = extract_count + 1;
                    StartCoroutine(ExtractOneFile(infile, outfile));  // 使用阻塞式的会有一个问题，就是每个文件最少消耗一帧，这里用非阻塞的方式。
                    while (extract_count >= 50)
                    {
                        if (call_back != null ) call_back.Call(false, message, extract_finished / files.Length);
                        yield return 0; // 缓一缓，不要卡太久
                    }
                } else {
                    if (call_back != null) call_back.Call(false, message, cur_count / files.Length);
                    byte[] infile_bytes = File.ReadAllBytes(infile);
                    if (Util.FirstIsSecondPrefix(files_txt_bytes, infile_bytes)) 
                    {
                        File.WriteAllBytes(outfile, Util.RemoveUnixTimePrefix(Util.DataPath + "files.txt", infile_bytes));
                    }else{
                        File.Copy(infile, outfile, true);
                    }
                    extract_size = extract_size + file_size;
                    if (extract_size > 1000000)  // 不要每次拷贝都跳下一帧了，有些文件很小
                    {
                        extract_size = 0;
                        yield return 0;
                    }
                }
            }

            // 安卓更新的
            while(extract_count > 0)
            {
                if (call_back != null) call_back.Call(false, message, extract_finished / files.Length);
                yield return 0;
            }

            message = "解压完成，正在检查更新";
            if (call_back != null) call_back.Call(true, message, 1);
        }

        string GetPatchUrl(string www_text)
        {
            // 增加一个解析json的
            JsonData data = JsonMapper.ToObject(www_text);
            if (data == null)
            {
                Debug.LogError("服务器返回的json格式不正确");
                return null;
            }

            if ((int)data["error_code"] != 0)
            {
                Debug.LogError((string)data["error_msg"]);
                return null;
            }

            string files_txt_path;
            // 这些东西写在引擎里的一个弊端就是不能改名字了，否则要更新引擎，写在lua里的问题是读了这些东西后才热更，就需要重新载入lua
            JsonData type = data["content_type"];
            if ((string)type == "full")
            {
                files_txt_path = (string)data["content"];
            }
            else
            {
                files_txt_path = AppConst.PatchServer + "/" + (string)data["content"];
            }
            return files_txt_path;
        }

        bool NoNeedUpdate(string[] remote_files)
        {
            string[] files = File.ReadAllLines(Util.DataPath + "files.txt");
            string dataPath = Util.DataPath;  //数据目录

            //if (files.Length != remote_files.Length - 1)
            //{
            //    return false;
            //}
            for (int i = 0; i < files.Length; i++)
            {
                string[] keyValue = files[i].Split('|');
                string[] remote_value = remote_files[i].Split('|');
                //时间戳
                if (i == 0)
                {
                    // 本地文件比远端的更新
                    if (remote_value.Length <= 3 || long.Parse(remote_value[3]) < long.Parse(keyValue[3]))
                    {
                        return true;
                    }
                }

                if (keyValue[1] != remote_value[1] || !File.Exists(dataPath + keyValue[0]))
                {
                    return false;
                }
            }
            return true;
        }

        struct _file_info
        {
            public WWW www;
            public bool md5_ok;
            public double file_size;
            public _file_info(WWW _www, bool val, double size)
            {
                www = _www;
                md5_ok = val;
                file_size = size;
            }
        }

        // http dns
        string http_dns_host = "patch.vr-cat.com";
        string http_dns_ip = null;
        string http_dns_url = "http://203.107.1.1/126108/d?host=";

        IEnumerator WaitWWW(WWW www)
        {
            int count = 0;
            while(count <= 20 && !www.isDone)
            {
                count++;
                yield return new WaitForSeconds(0.1f);
            }
        }

        Dictionary<string, string> http_dnsheaders = new Dictionary<string, string>();
        IEnumerator HttpDnsWWW()
        {
            System.Net.IPAddress temp;
            if (System.Net.IPAddress.TryParse(http_dns_host, out temp))
            {
                http_dns_ip = http_dns_host;
                yield break;
            }
            string url = http_dns_url + http_dns_host;
            WWW httpdns = new WWW(url);
            int count = 0;
            while(count <= 20 && !httpdns.isDone)
            {
                count++;
                yield return new WaitForSeconds(0.1f);
            }
            if (httpdns.isDone && httpdns.error == null)
            {
                JsonData data = JsonMapper.ToObject(httpdns.text);
                if (data["ips"].Count == 0)
                {
                    http_dns_ip = http_dns_host;
                    yield break;
                }
                http_dns_ip = (string)data["ips"][0];
                http_dnsheaders.Clear();
                http_dnsheaders.Add("Host", http_dns_host);
            }
            else
            {
                http_dns_ip = http_dns_host;
            }
        }

        Dictionary<string, _file_info> downloading_files = new Dictionary<string, _file_info>();
        bool m_update_failed = false;
        IEnumerator DownloadOneFile(string url, string file_name, string localfile, string remoteMd5, double file_size)
        {
            bool aliyun_not_ok = true;
            downloading_files.Add(file_name, new _file_info(null, false, file_size));
            string files_txt_path = null; 
            WWW www = new WWW(url);
            while (aliyun_not_ok)
            {
                // yield return www;
                yield return StartCoroutine(WaitWWW(www));
                // 这里改成两步操作，第一步get的只返回一个url，然后再用这个url去下载。
                if (!www.isDone || www.error != null)
                {
                    Debug.LogError(file_name + www.error);
                    // yield return StartCoroutine((url));
                    yield return new WaitForSeconds(0.5f);
                    www = new WWW(url);
                    continue;
                }

                files_txt_path = GetPatchUrl(www.text);
                if (files_txt_path != null)
                {
                    aliyun_not_ok = false;
                    www.Dispose();
                    //OnUpdateFailed(file_name + files_txt_path);
                    //yield break;
                }
            }
            aliyun_not_ok = true;
            while (aliyun_not_ok)
            {
                www = new WWW(files_txt_path);
                yield return StartCoroutine(WaitWWW(www));
                if (www.isDone && www.error == null)
                {
                    aliyun_not_ok = false;
                }
                else
                {
                    if (http_dns_ip == null)
                    {
                        yield return StartCoroutine(HttpDnsWWW());
                    }
                    www = new WWW(files_txt_path.Replace(http_dns_host, http_dns_ip), null, http_dnsheaders);
                    int count = 0;
                    while (count <= 20 && !www.isDone)
                    {
                        count++;
                        yield return new WaitForSeconds(0.1f);
                    }
                    if (www.isDone && www.error == null)
                    {
                        aliyun_not_ok = false;
                    }
                }
            }
            downloading_files[file_name] = new _file_info(www, false, file_size);

            File.WriteAllBytes(localfile, www.bytes);
            www.Dispose();

            //  理论上这个文件下完了，再比对下md5，防止刚好放出新patch导致两个补丁各下了一半
            if (!File.Exists(localfile) || !remoteMd5.Equals(Util.md5file(localfile)))
            {
                if (!File.Exists(localfile))
                {
                    Debug.LogError(localfile + "not exsit");
                }
                else
                {
                    Debug.LogError(localfile + "localmd5:" + Util.md5file(localfile));
                }
                OnUpdateFailed(file_name);
                downloading_files[file_name] = new _file_info(www, false, 0);
                yield break;
            }
            else
            {
                // 到这里才算真正完成了
                downloading_files[file_name] = new _file_info(www, true, file_size);
            }
        }

        int NotFinishedCount()
        {
            int count = 0;
            foreach(var keyval in downloading_files)
            {
                if (keyval.Value.md5_ok == false)
                {
                    count += 1;
                }
            }
            return count;
        }

        double GetDownloadedSize()
        {
            double size = 0;
            foreach (var keyval in downloading_files)
            {
                if (keyval.Value.md5_ok)
                {
                    size = size + keyval.Value.file_size;
                }
                else if (keyval.Value.www != null)
                {
                    size = size + keyval.Value.www.progress * keyval.Value.file_size;
                }
            }
            return size;
        }

        public void CheckUpdate(string avoid_first_patch, LuaFunction call_back )
        {
            StartCoroutine(OnUpdateResource(avoid_first_patch, call_back));
        }

        string GetUpdateUrl()
        {
            string url = AppConst.PatchServer + AppConst.PatchPath;
            if (Application.platform == RuntimePlatform.Android)
            {
                url = url + "android/";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                url = url + "ios/";
            }
            else
            {
                url = url + "win/";
            }
            return url;
        }

        /// <summary>
        /// 启动更新, 可以指定一个特定的目录。
        /// </summary>
        IEnumerator OnUpdateResource(string avoid_first_patch, LuaFunction call_back) {
            downloading_files.Clear();
            m_update_failed = false;

            string dataPath = Util.DataPath;  //数据目录
            string url = GetUpdateUrl();
            string message = "正在更新资源";
            string listUrl = url + "files.txt";
            WWW www = new WWW(listUrl);
            yield return StartCoroutine(WaitWWW(www));

            Debug.Log(listUrl);
            // yield return www;

            if (!www.isDone || www.error != null)
            {
                call_back.Call("www_error", "连接错误,正在重试", 1);
                yield break;
            }
            // 这里改成两步操作，第一步get的只返回一个url，然后再用这个url去下载。
            string files_txt_path = GetPatchUrl(www.text);
            if (files_txt_path == null)
            {
                OnUpdateFailed("files.txt--->");
                call_back.Call("www_error", "连接错误,正在重试", 1);
                yield break;
            }
            if (avoid_first_patch == "yes" && Application.platform != RuntimePlatform.IPhonePlayer)
            {
                if (call_back != null) call_back.Call(true, message, 1);
                yield break;
            }
            http_dns_host = files_txt_path.Substring(files_txt_path.IndexOf("//") + 2);
            http_dns_host = http_dns_host.Substring(0, http_dns_host.IndexOf("/"));
            bool aliyun_not_ok = true;
            while (aliyun_not_ok)
            {
                www = new WWW(files_txt_path);
                yield return StartCoroutine(WaitWWW(www));
                if (www.isDone && www.error == null)
                {
                    aliyun_not_ok = false;
                }
                else
                {
                    if (http_dns_ip == null)
                    {
                        yield return StartCoroutine(HttpDnsWWW());
                    }
                    www = new WWW(files_txt_path.Replace(http_dns_host, http_dns_ip), null, http_dnsheaders);
                    int count = 0;
                    while (count <= 20 && !www.isDone)
                    {
                        count++;
                        yield return new WaitForSeconds(0.1f);
                    }
                    if (www.isDone && www.error == null)
                    {
                        aliyun_not_ok = false;
                    }
                }
            }
            if (!Directory.Exists(dataPath)) {
                Directory.CreateDirectory(dataPath);
            }

            string filesText = www.text;
            byte[] filesByte = www.bytes;
            bool is_updated = false;
            string[] files = filesText.Split('\n');
            double total_file_size = 0;
            //  做一个优化，快速对比files。txt，防止md5对比太慢
            if (call_back != null) call_back.Call(false, "正在校验", 1);
            if (NoNeedUpdate(files))
            {
                if (call_back != null) call_back.Call(true, "校验完成", 1);
                yield break;
            }

            Dictionary<string, string> need_update_file = new Dictionary<string, string>();
            // 计算一下需要更新的文件总大小
            for (int i = 0; i < files.Length; i++)
            {
                if (string.IsNullOrEmpty(files[i])) continue;
                string[] keyValue = files[i].Split('|');
                if (IsIgnore(keyValue[0])) continue; // 忽视的目录

                string localfile = (dataPath + keyValue[0]).Trim();

                string remoteMd5 = keyValue[1].Trim();

                bool canUpdate = !File.Exists(localfile);
                if (!canUpdate)
                {
                    string localMd5 = Util.md5file(localfile);
                    canUpdate = !remoteMd5.Equals(localMd5);
                }
                if (canUpdate)
                {
                    is_updated = true;
                    need_update_file.Add(localfile, files[i]);
                    total_file_size = total_file_size + long.Parse(keyValue[2]);
                }
            }
            yield return null;
            int wait_count = 0;
            // 这里不和本地的files.txt对比，而是计算每个文件的md5，可以让patch断点续传
            foreach (var keyvalue in need_update_file) {
                string[] keyValue = keyvalue.Value.Split('|');
                string f = keyValue[0];
                string localfile = (dataPath + f).Trim();
                string path = Path.GetDirectoryName(localfile);
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
                string fileUrl = url + f;
                string remoteMd5 = keyValue[1].Trim();
                long cur_file_size = long.Parse(keyValue[2]);
                wait_count = wait_count + 1;
                StartCoroutine(DownloadOneFile(fileUrl, f, localfile, remoteMd5, cur_file_size));

                // 同时只下载50个吧
                while (!m_update_failed && wait_count > 50)
                {
                    double load_size = GetDownloadedSize();

                    if (total_file_size > 1048576)
                    {
                        if (call_back != null) call_back.Call(false, string.Format("正在下载文件:{0:f1}M/{1:f1}M", load_size / 1048576, total_file_size / 1048576), (float)(load_size / total_file_size));
                    }
                    else
                    {
                        if (call_back != null) call_back.Call(false, string.Format("正在下载文件:{0:f1}K/{1:f1}K", load_size / 1024, total_file_size / 1024), (float)(load_size / total_file_size));
                    }
                    yield return 1;
                    wait_count = NotFinishedCount();
                }
                if (m_update_failed)
                {
                    yield break;
                }
            }

            //等待更新完成
            while(NotFinishedCount() > 0)
            {
                double load_size = GetDownloadedSize();
                if (m_update_failed)
                {
                    yield break;
                }
                if (total_file_size > 1048576)
                {
                    if (call_back != null) call_back.Call(false, string.Format("正在下载文件:{0:f1}M/{1:f1}M", load_size / 1048576, total_file_size / 1048576), (float)(load_size / total_file_size));
                }
                else
                {
                    if (call_back != null) call_back.Call(false, string.Format("正在下载文件:{0:f1}K/{1:f1}K", load_size / 1024, total_file_size / 1024), (float)(load_size / total_file_size));
                }
                yield return null;
            }

            File.WriteAllBytes(dataPath + "files.txt", filesByte);
            www.Dispose();
            yield return new WaitForEndOfFrame();

            if (is_updated)
            {
                Caching.CleanCache();
                // 重载StreamingAsset、因为它纪录了依赖，而更新后可能依赖项发了变化，有了新的bundle
                ResManager.UnloadAssetBundle(AppConst.AssetDir);
                ResManager.Initialize(AppConst.AssetDir, null);
                LuaManager.InitLuaBundle();  // 重新加载lua代码
            }

            if (call_back != null) call_back.Call(true, message, 1);
            downloading_files.Clear();
        }

        IEnumerator WaitForSpecificPath(double total_file_size, LuaFunction call_back)
        {
            double load_size = 0;
            //等待更新完成
            while (load_size < total_file_size && NotFinishedCount() > 0)
            {
                if (m_update_failed)
                {
                    if (call_back != null) call_back.Call(-1, total_file_size);
                    downloading_files.Clear();
                    yield break;
                }

                load_size = GetDownloadedSize();
                if (load_size < total_file_size)
                {
                    if (call_back != null) call_back.Call(load_size, total_file_size);
                }
                yield return null;
            }
            downloading_files.Clear();
            if (call_back != null) call_back.Call(total_file_size, total_file_size);
        }

        bool IsInPaths(string[] paths, string file_name)
        {
            foreach(string path in paths)
            {
                if (file_name.StartsWith(path))
                {
                    return true;
                }
            }
            return false;
        }

        public bool NeedUpdateSpecific(string specific_path)
        {
            string[] specific_paths = specific_path.Split('|');
            foreach (string path in specific_paths)
            {
                if (!Directory.Exists(Util.DataPath + path))
                {
                    return true;
                }
            }
            return false;
        }

        IEnumerator OnUpdateSpecificPath(string specific_path, LuaFunction call_back)
        {
            downloading_files.Clear();
            m_update_failed = false;

            string url = GetUpdateUrl();
            string[] specific_paths = specific_path.Split('|');

            string[] files = File.ReadAllLines(Util.DataPath + "files.txt");
            long total_file_size = 0;
            string dataPath = Util.DataPath;

            for (int i = 0; i < files.Length; i++)
            {
                if (string.IsNullOrEmpty(files[i])) continue;
                string[] keyValue = files[i].Split('|');

                if (IsInPaths(specific_paths, keyValue[0]))
                {
                    total_file_size = total_file_size + long.Parse(keyValue[2]); // 非指定目录的直接忽略
                }
            }

            int wait_count = 0;
            for (int i = 0; i < files.Length; i++)
            {
                if (string.IsNullOrEmpty(files[i])) continue;
                string[] keyValue = files[i].Split('|');
                string f = keyValue[0];

                if (!IsInPaths(specific_paths, keyValue[0]))
                {
                    continue; // 非指定目录的直接忽略
                }

                string localfile = (dataPath + f).Trim();
                string path = Path.GetDirectoryName(localfile);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fileUrl = url + f;
                string remoteMd5 = keyValue[1].Trim();
                long cur_file_size = long.Parse(keyValue[2]);
                wait_count = wait_count + 1;
                StartCoroutine(DownloadOneFile(fileUrl, f, localfile, remoteMd5, cur_file_size));

                // 同时只下载50个吧
                while (!m_update_failed && wait_count > 50)
                {
                    double load_size = GetDownloadedSize();
                    if (call_back != null) call_back.Call(load_size, total_file_size);
                    yield return null;
                    wait_count = NotFinishedCount();
                }

            }
            StartCoroutine(WaitForSpecificPath(total_file_size, call_back));
        }


        public double GetAllDownloadedSize(string specific_path)
        {
            string[] specific_paths = specific_path.Split('|');

            string[] files = File.ReadAllLines(Util.DataPath + "files.txt");
            long total_file_size = 0;

            for (int i = 0; i < files.Length; i++)
            {
                if (string.IsNullOrEmpty(files[i])) continue;
                string[] keyValue = files[i].Split('|');

                if (IsInPaths(specific_paths, keyValue[0]))
                {
                    total_file_size = total_file_size + long.Parse(keyValue[2]); // 非指定目录的直接忽略
                }
            }
            return total_file_size;
        }

        // 更新指定目录, 以|隔开的一系列目录， 这里就不下载files.txt了，因为这里一般是在游戏内部，并且不做md5比较
        public void UpdateSpecificPath(string specific_path, LuaFunction call_back)
        {
            StartCoroutine(OnUpdateSpecificPath(specific_path, call_back));
        }

        void OnUpdateFailed(string file) {
            string message = "更新失败,请退出重试!>" + file;
            m_update_failed = true;
            Debug.LogError(message);
            facade.SendMessageCommand(NotiConst.UPDATE_TIMEOUT, new LoadingMessage(message, 0));
        }

        /// <summary>
        /// 资源初始化结束
        /// </summary>
        void OnResourceInited() {
            LuaManager.InitStart();
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        void OnDestroy() {
            if (NetManager != null) {
                NetManager.Unload();
            }
            if (LuaManager != null) {
                LuaManager.Close();
            }
            Debug.Log("~GameManager was destroyed");
        }
    }
}