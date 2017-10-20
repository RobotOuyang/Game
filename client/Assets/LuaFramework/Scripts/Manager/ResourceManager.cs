#define USE_OBJ_CACHE
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using UnityEngine.UI;
using LuaInterface;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;


public class AssetBundleInfo {
    public AssetBundle m_AssetBundle;
    public int m_DependenceCount;     // 这个表示被其他bundle引用的次数

#if USE_OBJ_CACHE
    public Dictionary<string, UObject> cache;
#endif

    public AssetBundleInfo(AssetBundle assetBundle) {
        m_AssetBundle = assetBundle;
        m_DependenceCount = 0;

#if USE_OBJ_CACHE
        cache = new Dictionary<string, UObject>();
#endif
    }
}

class LoadAssetRequest
{
    public Type assetType;
    public string[] assetNames;

    public LuaFunction luaFunc;
    public Action<UObject[]> sharpFunc;
}

class LoadRequestInfo
{
    public List<LoadAssetRequest> request_list = new List<LoadAssetRequest>();
    public int dep_times = 0;
}

namespace LuaFramework {

    public class ResourceManager : Manager {
        string m_BaseDownloadingURL;
        string m_ResDownloadingURL;  //备用的资源加载地址，在解压前的界面从这里load

        AssetBundleManifest m_AssetBundleManifest = null;
        Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();
        Dictionary<string, AssetBundleInfo> m_LoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
        Dictionary<string, LoadRequestInfo> m_LoadRequests = new Dictionary<string, LoadRequestInfo>();

        // Load AssetBundleManifest.
        //public void Initialize(string manifestName, Action initOK) {
        //    m_BaseDownloadingURL = Util.GetRelativePath();
        //    m_ResDownloadingURL = Util.GetStreamingPath();

        //    LoadAsset<AssetBundleManifest>(manifestName, new string[] { "AssetBundleManifest" }, delegate(UObject[] objs) {
        //        if (objs.Length > 0) {
        //            m_AssetBundleManifest = objs[0] as AssetBundleManifest;
        //        }
        //        if (initOK != null) initOK();
        //    });
        //}
        // Load AssetBundleManifest.
        public void Initialize(string manifestName, Action initOK)
        {
            m_BaseDownloadingURL = Util.GetRelativePath();
            m_ResDownloadingURL = Util.GetStreamingPath();

#if UNITY_IOS
		StartCoroutine (InitConfuse(manifestName, initOK));
#else
            LoadAsset<AssetBundleManifest>(manifestName, new string[] { "AssetBundleManifest" }, delegate (UObject[] objs) {
                if (objs.Length > 0)
                {
                    m_AssetBundleManifest = objs[0] as AssetBundleManifest;
                }
                if (initOK != null) initOK();
            });
#endif
        }

        /// <summary>
        /// 如果是prefab根目录下的，会被放到prefabs bundle里
        /// </summary>
        /// <param name="abName">bundle的名字，一般就是路径</param>
        /// <param name="assetName"></param>
        /// <param name="func"></param>
        public void LoadPrefab(string path, Action<UObject[]> func)
        {
            if (AppConst.EidtorNotUseBundle)
            {
                LoadAssetInEditor<GameObject>(new string[] { AppConst.PrefabPath + path + ".prefab" }, func);
            }
            else
            {
                int index = path.LastIndexOf('/');
                string abName = "_prefabs";
                string assetName = path;
                if (index >= 0)
                {
                    // 这里要跳过一个/ 符号
                    abName = path.Substring(0, index);
                    assetName = path.Substring(index + 1, path.Length - index - 1);
                }

                LoadPrefab(abName, new string[] { assetName }, func);
            }
        }

        // 直接放在prefab目录下的预制体，会放在_prefabs这个bundle里
        public void LoadPrefab(string path, LuaFunction func)
        {
            if (AppConst.EidtorNotUseBundle)
            {
                LoadAssetInEditor<GameObject>(new string[] { AppConst.PrefabPath + path + ".prefab" }, null, func);
            }
            else
            {
                int index = path.LastIndexOf('/');
                string abName = "_prefabs";
                string assetName = path;
                if (index >= 0)
                {
                    // 这里要跳过一个/ 符号
                    abName = path.Substring(0, index);
                    assetName = path.Substring(index + 1, path.Length - index - 1);
                }
                LoadPrefab(abName, new string[] { assetName }, func);
            }
        }

        // 加一个优化，不频繁的Instantiate
        Dictionary<GameObject, List<GameObject>> m_Reuse_Instantiated = new Dictionary<GameObject, List<GameObject>>();
        Dictionary<GameObject, GameObject> m_serach_Prefab = new Dictionary<GameObject, GameObject>();

        public int GetCacheCount()
        {
            int count = 0;
            foreach (var var1 in m_Reuse_Instantiated)
            {
                count += var1.Value.Count;
            }
            return count;
        }

        // 这里默认加的child是横竖延展类型的了
        public RectTransform AddPrefabChild(GameObject parent, GameObject prefab, bool in_pool)
        {
            if (parent == null) return null;

            List<GameObject> collect = null;
            GameObject go = null;

            m_Reuse_Instantiated.TryGetValue(prefab, out collect);
            if (in_pool && collect != null && collect.Count > 0)
            {
                go = collect[collect.Count - 1];
                collect.RemoveAt(collect.Count - 1);
                go.SetActive(true);
            }

            if (go == null)
            {
                go = Instantiate(prefab) as GameObject;
                if (in_pool)
                {
                    m_serach_Prefab[go] = prefab;
                }
            }

            RectTransform rect = go.GetComponent<RectTransform>();

            rect.SetParent(parent.transform);

            rect.localScale = Vector3.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localPosition = Vector3.zero;

            return rect;
        }

        public RectTransform AddPrefabChild(GameObject parent, GameObject prefab)
        {
            return AddPrefabChild(parent, prefab, false);
        }

        public void ClearAllCache()
        {
            // AvatarManager.ClearCache();
            m_Reuse_Instantiated.Clear();
            m_serach_Prefab.Clear();
        }

        public void CollectObject(GameObject parent)
        {
            if (parent == null) return;

            GameObject prefab = null;

            m_serach_Prefab.TryGetValue(parent, out prefab);

            if (prefab != null)
            {
                List<GameObject> collect = null;
                m_Reuse_Instantiated.TryGetValue(prefab, out collect);

                if (collect == null)
                {
                    collect = new List<GameObject>();
                }
                collect.Add(parent);
                m_Reuse_Instantiated[prefab] = collect;
                parent.SetActive(false);
            }
            else
            {
                // 找不到自己的prefab, 就只能销毁了
                Destroy(parent);
            }
        }

        // 这个不直接销毁子类，而是设置为不活跃的，然后收集起来
        public void CollectAllchild(GameObject parent)
        {
            if (parent == null) return;

            for (int i = parent.transform.childCount - 1; i >= 0; i--)
            {
                CollectObject(parent.transform.GetChild(i).gameObject);
            }
            parent.transform.DetachChildren();
        }

        public void RemoveAllchild(GameObject parent)
        {
            if (parent == null) return;

            for (int i = parent.transform.childCount - 1; i >= 0; i--)
            {
                Transform trans = parent.transform.GetChild(i);
                Destroy(trans.gameObject);
            }
            parent.transform.DetachChildren();
        }

        public Button AddButton(GameObject obj)
        {
            if (obj == null) return null;

            MaskableGraphic img = obj.GetComponent<MaskableGraphic>();
            img.raycastTarget = true;

            return obj.AddComponent<Button>();
        }

        public void SetImgAlpha(Image img, float val)
        {
            if (img == null)
            {
                return;
            }
            Color color = img.color;
            color.a = val;
            img.color = color;
        }

        public void SetImgGray(Image img, float val)
        {
            if (img == null)
            {
                return;
            }
            Color color = img.color;
            color.r = color.g = color.b = val;
            img.color = color;
        }

        public RectTransform GetChildRect(GameObject parent)
        {
            if (parent == null) return null;

            if (parent.transform.childCount < 1)
            {
                return null;
            }
            return parent.transform.GetChild(0).GetComponent<RectTransform>();
        }

        public UObject CreateUIObject(string type_str)
        {
            Type t = Type.GetType(string.Format("UnityEngine.UI.{0},UnityEngine.UI", type_str));
            GameObject obj = new GameObject();
            return obj.AddComponent(t);
        }

        // 转移孩子
        public RectTransform ChangeRectParent(GameObject from, GameObject to)
        {
            if (from == null || from.transform.childCount < 1)
            {
                return null;
            }
 
            Transform go = from.transform.GetChild(0);
            RectTransform rect = go.GetComponent<RectTransform>();
            
            rect.SetParent(to.transform);
            rect.localScale = Vector3.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localPosition = Vector3.zero;
            RemoveAllchild(go.gameObject);
            return rect;
        }

        public void LoadTexture(string path, LuaFunction func)
        {
            if (AppConst.EidtorNotUseBundle)
            {
                // 从预制体读纹理，要求纹理生成了对应的预制体
                LoadAssetInEditor<GameObject>(new string[] { AppConst.PrefabPath + AppConst.TexPrefabPath + path + ".prefab" }, null, func);
            }
            else
            {
                int index = path.LastIndexOf('/');
                string abName = "";
                string assetName = path;
                if (index >= 0)
                {
                    // 这里要跳过一个/ 符号
                    abName = path.Substring(0, index);
                    assetName = path.Substring(index + 1, path.Length - index - 1);
                }
                LoadPrefab(AppConst.TexPrefabPath + abName, new string[] { assetName }, func);
            }
        }

        public void LoadTexture(string path, string[] texNames, LuaFunction func)
        {
            if (AppConst.EidtorNotUseBundle)
            {
                List<string> paths = new List<string>();
                string file_path = Application.dataPath + "/" + AppConst.PrefabPath + AppConst.TexPrefabPath + path + "/";
                foreach (string name in texNames)
                {
                    paths.Add(AppConst.PrefabPath + AppConst.TexPrefabPath + path + "/" + name + ".prefab");
                }
                if (paths.Count == 0)
                {
                    LoadAssetInEditor<GameObject>(Directory.GetFiles(file_path, "*.prefab", SearchOption.AllDirectories), null, func);
                }
                else
                {
                    LoadAssetInEditor<GameObject>(paths.ToArray(), null, func);
                }
            }
            else
            {
                LoadPrefab(AppConst.TexPrefabPath + path, texNames, func);
                // LoadAsset<Sprite>(path, texNames, null, func);
            }
        }

        // 由于声音文件的特殊性。这个paths要带后缀名，这个加载的不是预制
        public void LoadAudio(string bundle, string[] names, LuaFunction func)
        {
            if (AppConst.EidtorNotUseBundle)
            {
                List<string> path_list = new List<string>();
                if (bundle != null && bundle.Length > 0)
                {
                    bundle = bundle + "/";
                }
                foreach (string path in names)
                {
                    path_list.Add(string.Format("{2}/{0}{1}", bundle, path, AppConst.SoundPath));
                }
                if (path_list.Count == 0)
                {
                    LoadAssetInEditor<AudioClip>(Directory.GetFiles(Application.dataPath + "/" + AppConst.SoundPath + "/" + bundle, "*", SearchOption.AllDirectories), null, func);
                }
                else
                {
                    LoadAssetInEditor<AudioClip>(path_list.ToArray(), null, func);
                }
            }
            else
            {
                string abName = AppConst.SoundPath;
                if (bundle != null && bundle.Length > 0)
                {
                    abName = abName + "/" + bundle;
                }
                LoadAsset<AudioClip>(abName, names, null, func);
            }
        }


        string pre_scene = null;
        public void LoadSceneBundle(string path, Action<UObject[]> func)
        {
            int index = path.LastIndexOf('/');
            string abName = path;
            string assetName = path;
            if (index >= 0)
            {
                assetName = path.Substring(index + 1, path.Length - index - 1);
            }
            if (pre_scene != null)
            {
                UnloadAssetBundle(pre_scene);
            }
            pre_scene = AppConst.ScenePath + abName;

            LoadAsset<GameObject>(AppConst.ScenePath + abName + AppConst.ExtName, new string[] { assetName }, func);
        }

        public string cur_scene = "appenter";
        IEnumerator LoadScene_async_cro(string path, LuaFunction progress_func, LuaFunction finish_func)
        {
            var load = SceneManager.LoadSceneAsync(path);
            while (load != null && !load.isDone)
            {
                if (progress_func != null)
                {
                    progress_func.Call(load.progress);
                }
                yield return null;
            }
            cur_scene = path;
            if (finish_func != null)
            {
                LuaHelper.GetPanelManager().ClearAllEffect();
                ClearAllCache();
                // 下面这两个顺序不能变，因为ClearMemory有gc
                Util.CallMethod("Game", "AfterSceneLoad");
                Util.ClearMemory();

                finish_func.Call(load.progress);
            }
        }

        public void LoadSceneAsync(string path, LuaFunction progress_func, LuaFunction finish_func)
        {
            if (AppConst.EidtorNotUseBundle)
            {
                // 场景直接load，但是在编辑器运行是要加入到场景打包列表里，打包的时候再去掉（不去掉会增大包体）
                StartCoroutine(LoadScene_async_cro(path, progress_func, finish_func));
            }
            else
            {
                LoadSceneBundle(path, delegate (UObject[] objs)
                {
                    StartCoroutine(LoadScene_async_cro(path, progress_func, finish_func));
                });
            }
        }

        IEnumerator LoadScene_sync_cro(string path, LuaFunction progress_func, LuaFunction finish_func)
        {
            LuaHelper.GetPanelManager().ClearAllEffect();
            ClearAllCache();
            // 下面这两个顺序不能变，因为ClearMemory有gc
            Util.CallMethod("Game", "AfterSceneLoad");
            Resources.UnloadUnusedAssets();
            Util.ClearMemory();

            SceneManager.LoadScene(path);
            cur_scene = path;
            // 加这个return，逻辑跟异步保持一致，不依赖顺序
            yield return null;

            if (finish_func != null)
            {
                finish_func.Call(100);
            }
        }

        public void LoadSceneSync(string path, LuaFunction progress_func, LuaFunction finish_func)
        {
            if (AppConst.EidtorNotUseBundle)
            {
                // 场景直接load，但是在编辑器运行是要加入到场景打包列表里，打包的时候再去掉（不去掉会增大包体）
                StartCoroutine(LoadScene_sync_cro(path, progress_func, finish_func));
            }
            else
            {
                LoadSceneBundle(path, delegate (UObject[] objs)
                {
                    StartCoroutine(LoadScene_sync_cro(path, progress_func, finish_func));
                });
            }
        }

        public void LoadPrefab(string abName, string[] assetNames, Action<UObject[]> func) {

            LoadPrefab(abName, assetNames, func, null);
        }

        public void LoadPrefab(string abName, string[] assetNames, LuaFunction func) {
            LoadPrefab(abName, assetNames, null, func);
        }

        public void LoadPrefab(string abName, string[] assetNames, Action<UObject[]> func1,  LuaFunction func2)
        {
            if (AppConst.EidtorNotUseBundle)
            {
                List<string> paths = new List<string>();
                string file_path = Application.dataPath + "/" + AppConst.PrefabPath + abName + "/";
                foreach (string name in assetNames)
                {
                    paths.Add(AppConst.PrefabPath + abName + "/" + name + ".prefab");
                }
                if (paths.Count == 0)
                {
                    LoadAssetInEditor<GameObject>(Directory.GetFiles(file_path, "*.prefab", SearchOption.AllDirectories), func1, func2);
                }
                else
                {
                    LoadAssetInEditor<GameObject>(paths.ToArray(), func1, func2);
                }
            }
            else
            {
                LoadAsset<GameObject>(AppConst.PrefabPath + abName + AppConst.ExtName, assetNames, func1, func2);
            }
        }

        string GetRealAssetPath(string abName) {
            if (abName.Equals(AppConst.AssetDir)) {
                return abName;
            }
            abName = abName.ToLower();
            if (!abName.EndsWith(AppConst.ExtName)) {
                abName += AppConst.ExtName;
            }
            if (abName.Contains("/")) {
                return abName;
            }
            string[] paths = m_AssetBundleManifest.GetAllAssetBundles();
            for (int i = 0; i < paths.Length; i++) {
                int index = paths[i].LastIndexOf('/');
                string path = paths[i].Remove(0, index + 1);
                if (path.Equals(abName)) {
                    return paths[i];
                }
            }
            // 找不到那就不load，不报错了，没关系的
            //Debug.LogError("GetRealAssetPath Error:>>" + abName);
            return null;
        }

        // 通过这个协程把编辑器的资源加载也强制异步，保持逻辑统一
        IEnumerator LoadAssetEidtor(List<UObject> obj_lists, Action<UObject[]> func1 = null, LuaFunction func2 = null)
        {
            yield return null;

            if (func1 != null)
            {
                func1(obj_lists.ToArray());
            }
            if (func2 != null)
            {
                func2.Call((object)obj_lists.ToArray());
            }
        }

        // 在编辑器里加载资源，不从assetbundle里读取，改为直接从路径读取, 特么这个函数在运行时对paths格式的要求非常严格
        void LoadAssetInEditor<T>(string[] paths, Action<UObject[]> func1 = null, LuaFunction func2 = null) where T:UObject
        {
#if UNITY_EDITOR
            List<UObject> obj_lists = new List<UObject>();
            foreach (string path in paths)
            {
                T obj = null;
                if (path.Contains("Assets"))
                {
                    obj = AssetDatabase.LoadAssetAtPath<T>(path.Substring(path.IndexOf("Assets")));
                }
                else
                {
                    obj = AssetDatabase.LoadAssetAtPath<T>("assets/" + path);
                }
                obj_lists.Add(obj);
            }
            StartCoroutine(LoadAssetEidtor(obj_lists, func1, func2));
#endif
        }

        /// <summary>
        /// 载入素材
        /// </summary>
        void LoadAsset<T>(string abName, string[] assetNames, Action<UObject[]> action = null, LuaFunction func = null) where T : UObject {
            // 如果abName含路径会直接返回abName，否则就会在全路径下搜索名字一致的第一个遇到的
            abName = GetRealAssetPath(abName);

            LoadAssetRequest request = new LoadAssetRequest();
            request.assetType = typeof(T);
            request.assetNames = assetNames;
            request.luaFunc = func;
            request.sharpFunc = action;

            LoadRequestInfo requests = null;
            if (!m_LoadRequests.TryGetValue(abName, out requests)) {
                requests = new LoadRequestInfo();
                requests.dep_times = 1;
                requests.request_list.Add(request);
                m_LoadRequests.Add(abName, requests);
                StartCoroutine(OnLoadAsset<T>(abName, requests));
            } else {
                requests.dep_times++;
                requests.request_list.Add(request);
            }
        }

        public GameObject LoadGoFromBundle(AssetBundle ab, string name)
        {
            if (ab == null) return null;
            return ab.LoadAsset<GameObject>(name);
        }

        void CallbackRequestList(AssetBundleInfo bundleInfo, LoadRequestInfo list)
        {
            // 这里有个巨坑，在循环的途中， 回调函数里也有可能会动态增加request_list的大小。
            for (int i = 0; i < list.request_list.Count; i++)
            {
                string[] assetNames = list.request_list[i].assetNames;
                List<UObject> result = new List<UObject>();

                // assetbundle已经异步加载了，这个asset就同步加载，免得出现比较大的加载延迟
                AssetBundle ab = bundleInfo.m_AssetBundle;
#if USE_OBJ_CACHE
                Dictionary<string, UObject> cache = bundleInfo.cache;
#endif
                if (assetNames.Length == 0)
                {
                    assetNames = ab.GetAllAssetNames();
                }

                for (int j = 0; j < assetNames.Length; j++)
                {
                    if (!ab.isStreamedSceneAssetBundle)
                    {
#if USE_OBJ_CACHE
                        UObject obj;
                        if (cache.TryGetValue(assetNames[j], out obj))
                        {
                            result.Add(obj);
                        }
                        else
                        {
                            obj = ab.LoadAsset(assetNames[j], list.request_list[i].assetType);
                            cache.Add(assetNames[j], obj);
                            result.Add(obj);
                        }
#else
                        result.Add(ab.LoadAsset(assetNames[j], list.request_list[i].assetType));
#endif
                    }
                }

                if (list.request_list[i].sharpFunc != null)
                {
                    list.request_list[i].sharpFunc(result.ToArray());
                    list.request_list[i].sharpFunc = null;
                }
                if (list.request_list[i].luaFunc != null)
                {
                    if (assetNames.Length == 0)
                    {
                        list.request_list[i].luaFunc.Call(ab);
                    }
                    else
                    {
                        list.request_list[i].luaFunc.Call((object)result.ToArray());
                    }
                    list.request_list[i].luaFunc.Dispose();
                    list.request_list[i].luaFunc = null;
                }
            }
        }

        IEnumerator OnLoadAsset<T>(string abName, LoadRequestInfo list) where T : UObject {
            AssetBundleInfo bundleInfo = GetLoadedAssetBundle(abName);

            if (bundleInfo == null) {
                yield return StartCoroutine(OnLoadAssetBundle(abName, typeof(T)));
                bundleInfo = GetLoadedAssetBundle(abName);
                bundleInfo.m_DependenceCount += list.dep_times;

                if (bundleInfo == null) {
                    m_LoadRequests.Remove(abName);
                    Debug.LogError("OnLoadAsset--->>>" + abName);
                    yield break;
                }
            }
            else
            {
                bundleInfo.m_DependenceCount++;
                // 要给所有依赖项加引用计数, 这时候依赖肯定加载了
                string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                for (int i = 0; i < dependencies.Length; i++)
                {
                    m_LoadedAssetBundles[dependencies[i]].m_DependenceCount++;
                }
                // 强制变异步
                yield return null;
            }

            if (list.request_list.Count > 0)
            {
                CallbackRequestList(bundleInfo, list);
            }

            m_LoadRequests.Remove(abName);
        }

        // 多线程加载
        byte[] LoadOneAssetAsync(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            return null; 
        }

        delegate byte[] LoadDelegate(string path);
        // 加载一个无需加载依赖的bundle
        IEnumerator LoadOneBundle(string abName)
        {
            string path = Util.DataPath + abName;
            //if (!File.Exists(path))
            //{
            //    path = Application.streamingAssetsPath + "/" + abName;
            //}
            if (!File.Exists(path))
            {
                path = Application.streamingAssetsPath + "/" + GetConfusePath(abName);
            }
            //// 异步读取到线程
            //LoadDelegate mydelegate = new LoadDelegate(LoadOneAssetAsync);
            //IAsyncResult result = mydelegate.BeginInvoke(path, null, null);

            //while (!result.IsCompleted)
            //{
            //    yield return null;
            //}

            //byte[] bytes = mydelegate.EndInvoke(result);
            //AssetBundle assetObj = AssetBundle.LoadFromMemory(bytes);
            AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(path);
            yield return req;
            AssetBundle assetObj = req.assetBundle;

            LoadRequestInfo loadInfo = m_LoadRequests[abName];
            if (assetObj != null)
            {
                AssetBundleInfo bundle_info = new AssetBundleInfo(assetObj);
                bundle_info.m_DependenceCount += loadInfo.dep_times;
                m_LoadedAssetBundles.Add(abName, bundle_info);
                if (loadInfo.request_list.Count > 0)
                {
                    // 如果有作为主体被加载，需要回调其他list，通知加载完成
                    CallbackRequestList(bundle_info, loadInfo);
                }
                m_LoadRequests.Remove(abName);
            }
            else
            {
                Debug.LogError("bundle is null :" + abName);
            }
        }

        IEnumerator OnLoadAssetBundle(string abName, Type type) {
            // 这个url在编辑器下是StreanmingAssets，而在打包后是热更新存储的persistentDatapath
            string url = m_BaseDownloadingURL + abName;
            //if (!Util.IsFileInDataPath(abName))
            //{
            //    url = m_ResDownloadingURL + abName;
            //}
            if (!Util.IsFileInDataPath(abName))
            {
                url = m_ResDownloadingURL + GetConfusePath(abName);
            }
            long seconds = Util.GetTime();

            WWW download = null;
            LoadRequestInfo loadInfo = null;
            if (type == typeof(AssetBundleManifest))
            { 
                download = new WWW(url);
            }
            // 因为前面的判断，自己肯定不是正在被加载的
            else
            {
                // 这个GetAllDependencies会递归获取到所有的依赖项
                string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                if (dependencies.Length > 0) {
                    m_Dependencies[abName] = dependencies;
                    AssetBundleInfo bundleInfo = null;
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        string depName = dependencies[i];
                        // 已经加载完成
                        if (m_LoadedAssetBundles.TryGetValue(depName, out bundleInfo))
                        {
                            bundleInfo.m_DependenceCount++;
                        }
                        // 还有可能正在被加载
                        else if (m_LoadRequests.TryGetValue(depName, out loadInfo))
                        {
                            loadInfo.dep_times++;
                        }
                        else
                        {
                            loadInfo = new LoadRequestInfo();
                            loadInfo.dep_times = 1;
                            m_LoadRequests.Add(depName, loadInfo);
                            // 不需要继续递归。因为依赖已经是递归获取的
                            StartCoroutine(LoadOneBundle(depName));
                        }
                    }
                    // 需要等待所有依赖全部加载完毕
                    bool children_loaded = false;
                    int wait_times = 0;
                    while (!children_loaded)
                    {
                        children_loaded = true;
                        for (int i = 0; i < dependencies.Length; i++)
                        {
                            if (m_LoadRequests.TryGetValue(dependencies[i], out loadInfo))
                            {
                                children_loaded = false;
                                break;
                            }
                        }
                        wait_times++;
                        if (wait_times >= 200)
                        {
                            Debug.LogError(abName + "怎么等待了一万年（200帧）？检查下是不是出错了！");
                            yield break;
                        }
                        yield return null;
                    }
                }
                download = new WWW(url);
            }
            yield return download;
            AssetBundle assetObj = download.assetBundle;
            Debug.Log("add bundle " + abName + " cost time " + (Util.GetTime() - seconds));
            if (assetObj != null)
            {
                AssetBundleInfo info = new AssetBundleInfo(assetObj);
                m_LoadedAssetBundles.Add(abName, info);
            }
            else
            {
                Debug.LogWarning("bundle is null :" + abName);
            }
        }

        AssetBundleInfo GetLoadedAssetBundle(string abName) {
            AssetBundleInfo bundle = null;
            m_LoadedAssetBundles.TryGetValue(abName, out bundle);
            if (bundle == null) return null;

            // No dependencies are recorded, only the bundle itself is required.
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies))
                return bundle;

            // Make sure all dependencies are loaded
            foreach (var dependency in dependencies) {
                AssetBundleInfo dependentBundle;
                m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
                if (dependentBundle == null)
                {
                    Debug.LogError(abName + "dep: " + dependency + "not loaded");
                    return null;
                }
            }
            return bundle;
        }
        
        // TODO 这个资源管理还有很多坑，目前除了panel，scene的，其他的都不能及时卸载，以后再改
        public void UnloadAllBundle()
        {
            //m_Dependencies.Clear();
            //m_LoadedAssetBundles.Clear();
            //m_LoadRequests.Clear();
            //LuaHelper.GetPanelManager().ClearAllEffect();
            //ClearAllCache();
            //Util.ClearMemory();
        }

        public void UnloadAssetBundle(string abName) {
            abName = GetRealAssetPath(abName);
            if (abName == null)
            {
                return;
            }
            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + abName);
            UnloadAssetBundleInternal(abName, false);
            UnloadDependencies(abName);
            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + abName);
        }

        void UnloadDependencies(string abName) {
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies))
                return;

            // Loop dependencies.
            foreach (var dependency in dependencies) {
                UnloadAssetBundleInternal(dependency);
            }
            m_Dependencies.Remove(abName);
        }

        
        void UnloadAssetBundleInternal(string abName, bool is_dependence = true) {
            AssetBundleInfo bundle;
            if (!m_LoadedAssetBundles.TryGetValue(abName, out bundle)) return;
            --bundle.m_DependenceCount;

            if ( bundle.m_DependenceCount <= 0 ) {
                bundle.m_AssetBundle.Unload(false);
                m_LoadedAssetBundles.Remove(abName);
                Debug.Log(abName + " has been unloaded successfully");
            }
        }

        //／ 资源混淆还原
        Dictionary<string, string> confuse_dic = new Dictionary<string, string>();
        public string GetConfusePath(string local_path)
        {
            // 安卓平台可能无混淆
            if (confuse_dic.Count == 0)
            {
                return local_path;
            }
            string tail = null;
            if (local_path.EndsWith(".unity3d"))
            {
                tail = ".unity3d";
            }
            else if (local_path.EndsWith(".unity3d.manifest"))
            {
                tail = ".unity3d.manifest";
                local_path = local_path.Substring(0, local_path.Length - 9);
            }
            if (tail == null)
            {
                return local_path;
            }

            string[] words = local_path.Split('/');
            for (int i = 0; i < words.Length; i++)
            {
                if (confuse_dic.ContainsKey(words[i]))
                {
                    words[i] = confuse_dic[words[i]];
                }
            }
            return string.Join("/", words) + tail;
        }

        void ProcessConfuse(string content)
        {
            string[] lines = content.Split('\n');
            foreach (string line in lines)
            {
                if (!line.Contains("|"))
                    continue;
                string[] words = line.Split('|');
                confuse_dic[words[0]] = words[1];
            }
        }

        IEnumerator InitConfuse(string manifestName, Action initOK)
        {
            string resPath = Util.AppContentPath() + "confuse.txt"; //游戏包资源目录
            if (Application.platform == RuntimePlatform.Android)
            {
                WWW www = new WWW(resPath);
                yield return www;

                if (www.error != null)
                {
                    ProcessConfuse(www.text);
                }
            }
            else
            {
                if (File.Exists(resPath))
                {
                    ProcessConfuse(File.ReadAllText(resPath));
                }
            }
            LoadAsset<AssetBundleManifest>(manifestName, new string[] { "AssetBundleManifest" }, delegate (UObject[] objs) {
                if (objs.Length > 0)
                {
                    m_AssetBundleManifest = objs[0] as AssetBundleManifest;
                }
                if (initOK != null) initOK();
            });
        }
    }
}
