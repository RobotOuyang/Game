using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using UnityEditor.XCodeEditor;
using System.Collections.Generic;
using System.Diagnostics;
using LuaFramework;
using System.Text.RegularExpressions;

public class Packager {
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();
    static List<AssetBundleBuild> maps = new List<AssetBundleBuild>();

    ///-----------------------------------------------------------
    static string[] exts = { ".txt", ".xml", ".lua", ".assetbundle", ".json" };
    static bool CanCopy(string ext) {   //能不能复制
        foreach (string e in exts) {
            if (ext.Equals(e)) return true;
        }
        return false;
    }

    static void CopyPath(string from_path, string to_path)
    {
        if (Directory.Exists(to_path))
        {
            Directory.Delete(to_path, true);
        }

        string[] files = Directory.GetFiles(from_path, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            string to_str = files[i].Replace("\\", "/").Replace(from_path, to_path);
            if (!Directory.Exists(Directory.GetParent(to_str).FullName))
            {
                Directory.CreateDirectory(Directory.GetParent(to_str).FullName);
            }
            File.Copy(files[i], to_str, true);
        }
    }
    
    /// <summary>
    /// 不同的平台打的assetbundle不能通用，因此热更新的时候也要有不同的目录对应。
    /// 为了方便自动处理，每个平台打的assetbundle都会放到自己独特的文件夹下，便于脚本上传
    /// 而 steamingassets本身存放的是最后一次打包的平台
    /// </summary>
    [MenuItem("LuaFramework/Build iPhone Resource", false, 100)]
    public static void BuildiPhoneResource() {
        // 麻痹 luac, luajit不支持arm64， ios的lua就强制不用字节码了。
        AppConst.LuaByteMode = false;
        BuildAssetResource(BuildTarget.iOS);

        // 拷贝
        string src = AppDataPath + "/" + AppConst.AssetDir;
        string des = AppDataPath + "/" + AppConst.AssetDir_ios;

        CopyPath(src, des);
    }

    [MenuItem("LuaFramework/Build Android Resource", false, 101)]
    public static void BuildAndroidResource() {
        BuildAssetResource(BuildTarget.Android);
        // 拷贝
        string src = AppDataPath + "/" + AppConst.AssetDir;
        string des = AppDataPath + "/" + AppConst.AssetDir_andorid;

        CopyPath(src, des);
    }

    [MenuItem("LuaFramework/Build Android OnlyLua", false, 201)]
    public static void BuildAndroidLua()
    {
        // 拷贝
        string src = AppDataPath + "/" + AppConst.AssetDir;
        string des = AppDataPath + "/" + AppConst.AssetDir_andorid;

        CopyPath(des, src);

        BuildAssetResource(BuildTarget.Android, true);

        CopyPath(src, des);
    }

    [MenuItem("LuaFramework/Build iPhone OnlyLua", false, 201)]
    public static void BuildiPhoneLua()
    {
        // 麻痹 luac, luajit不支持arm64， ios的lua就强制不用字节码了。
        AppConst.LuaByteMode = false;
        // 拷贝
        string src = AppDataPath + "/" + AppConst.AssetDir;
        string des = AppDataPath + "/" + AppConst.AssetDir_ios;

        CopyPath(des, src);
        BuildAssetResource(BuildTarget.iOS, true);
        CopyPath(src, des);

    }

    [MenuItem("LuaFramework/Build Windows Resource", false, 102)]
    public static void BuildWindowsResource() {
        BuildAssetResource(BuildTarget.StandaloneWindows);
        // 拷贝到目录，以方便热更上传
        string src = AppDataPath + "/" + AppConst.AssetDir;
        string des = AppDataPath + "/" + AppConst.AssetDir_win;

        if (Directory.Exists(des))
        {
            Directory.Delete(des, true);
        }

        string[] files = Directory.GetFiles(src, "*.*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            string to_str = files[i].Replace("\\", "/").Replace(src, des);
            if (!Directory.Exists(Directory.GetParent(to_str).FullName))
            {
                Directory.CreateDirectory(Directory.GetParent(to_str).FullName);
            }
            File.Copy(files[i], to_str, true);
        }
    }

    /// <summary>
    /// 生成绑定素材
    /// </summary>
    public static void BuildAssetResource(BuildTarget target, bool only_lua = false) {
        if (Directory.Exists(Util.DataPath)) {
            Directory.Delete(Util.DataPath, true);
        }
        string streamPath = Application.streamingAssetsPath;
        if (!only_lua && Directory.Exists(streamPath))
        { 
            Directory.Delete(streamPath, true);
        }
        if (!Directory.Exists(streamPath))
        {
            Directory.CreateDirectory(streamPath);
        }
        // 只打包lua的时候，要保留原来的streamingassets, streamingassets.manifest
        if (only_lua)
        {
            File.Copy(streamPath + "/StreamingAssets", streamPath + "/_StreamingAssets", true);
            File.Copy(streamPath + "/StreamingAssets.manifest", streamPath + "/_StreamingAssets.manifest", true);
        }
        AssetDatabase.Refresh();

        maps.Clear();
        if (AppConst.LuaBundleMode) {
            HandleLuaBundle();
        } else {
            HandleLuaFile();
        }
        // 将所有的图片生成一个prefab放在prefab/_
        // HandleTexturePrefeb();
        if (!only_lua)
        {
            HandleTextureBundle();
            HandlePrefebBundle();
            HandleOtherResourceBundle();
            HandleSceneBundle();
        }

        string resPath = "Assets/" + AppConst.AssetDir;
        BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.StrictMode;
        BuildPipeline.BuildAssetBundles(resPath, maps.ToArray(), options, target);
        string streamDir = Application.dataPath + "/" + AppConst.LuaTempDir;
        if (Directory.Exists(streamDir)) Directory.Delete(streamDir, true);
        if (only_lua)
        {
            File.Copy(streamPath + "/_StreamingAssets", streamPath + "/StreamingAssets", true);
            File.Delete(streamPath + "/_StreamingAssets");
            File.Copy(streamPath + "/_StreamingAssets.manifest", streamPath + "/StreamingAssets.manifest", true);
            File.Delete(streamPath + "/_StreamingAssets.manifest");
        }
        BuildFileIndex();
        AssetDatabase.Refresh();

    }

    static void AddBuildMap(string bundleName, string pattern, string path) {
        string[] files = Directory.GetFiles(path, pattern);
        if (files.Length == 0) return;

        for (int i = 0; i < files.Length; i++) {
            files[i] = files[i].Replace('\\', '/');
        }
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = bundleName;
        build.assetNames = files;
        maps.Add(build);
    }

    /// <summary>
    /// 处理Lua代码包
    /// </summary>
    static void HandleLuaBundle() {
        string streamDir = Application.dataPath + "/" + AppConst.LuaTempDir;
        if (!Directory.Exists(streamDir)) Directory.CreateDirectory(streamDir);

        string[] srcDirs = { CustomSettings.luaDir, CustomSettings.FrameworkPath + "/ToLua/Lua" };
        for (int i = 0; i < srcDirs.Length; i++) {
            if (AppConst.LuaByteMode) {
                string sourceDir = srcDirs[i];
                string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
                int len = sourceDir.Length;

                if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\') {
                    --len;
                }
                for (int j = 0; j < files.Length; j++) {
                    string str = files[j].Remove(0, len);
                    string dest = streamDir + str + ".bytes";
                    string dir = Path.GetDirectoryName(dest);
                    Directory.CreateDirectory(dir);
                    EncodeLuaFile(files[j], dest);
                }    
            } else {
                ToLuaMenu.CopyLuaBytesFiles(srcDirs[i], streamDir);
            }
        }
        string[] dirs = Directory.GetDirectories(streamDir, "*", SearchOption.AllDirectories);
        for (int i = 0; i < dirs.Length; i++) {
            string name = dirs[i].Replace(streamDir, string.Empty);
            name = name.Replace('\\', '_').Replace('/', '_');
            name = "lua/lua_" + name.ToLower() + AppConst.ExtName;

            string path = "Assets" + dirs[i].Replace(Application.dataPath, "");
            AddBuildMap(name, "*.bytes", path);
        }
        AddBuildMap("lua/lua" + AppConst.ExtName, "*.bytes", "Assets/" + AppConst.LuaTempDir);

        //-------------------------------处理非Lua文件----------------------------------
        string luaPath = AppDataPath + "/" + AppConst.AssetDir + "/lua/";
        for (int i = 0; i < srcDirs.Length; i++) {
            paths.Clear(); files.Clear();
            string luaDataPath = srcDirs[i].ToLower();
            Recursive(luaDataPath);
            foreach (string f in files) {
                if (f.EndsWith(".meta") || f.EndsWith(".lua") || f.EndsWith(".proto") ||
                    f.EndsWith(".py") || f.EndsWith(".bat") ) continue;
                string newfile = f.Replace(luaDataPath, "");
                string path = Path.GetDirectoryName(luaPath + newfile);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string destfile = path + "/" + Path.GetFileName(f);
                File.Copy(f, destfile, true);
            }
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 处理框架实例包
    /// </summary>
    static void HandlePrefebBundle() {
        string resPath = AppDataPath + "/" + AppConst.AssetDir + "/" + AppConst.PrefabPath;
        if (!Directory.Exists(resPath)) Directory.CreateDirectory(resPath);

        string streamDir = AppDataPath + "/" + AppConst.PrefabPath;
        string[] dirs = Directory.GetDirectories(streamDir, "*", SearchOption.AllDirectories);
        for (int i = 0; i < dirs.Length; i++)
        {
            string name = dirs[i].Replace(streamDir, string.Empty).ToLower();
            string fab_bundle_path = Directory.GetParent(resPath + name).FullName;
            if (!Directory.Exists(fab_bundle_path))
            {
                Directory.CreateDirectory(fab_bundle_path);
            }
            string path = "Assets/" + AppConst.PrefabPath + name;

            AddBuildMap(AppConst.PrefabPath + name + AppConst.ExtName, "*.*", path);
        }
        AddBuildMap(AppConst.PrefabPath + "_prefabs" + AppConst.ExtName, "*.*", "Assets/" + AppConst.PrefabPath);
    }


    static void Recxxx(Font font, Transform trans)
    {
        foreach(Transform xx in trans)
        {
            Text input = xx.GetComponent<Text>();
            if (input != null)
            {
                input.font = font;
            }

            if (xx.childCount > 0)
            {
                Recxxx(font, xx);
            }
        }
    } 

    // [MenuItem("LuaFramework/change", false)]
    static void DOALL()
    {
        string streamDir = AppDataPath + "/" + AppConst.PrefabPath;
        string[] dirs = Directory.GetFiles(streamDir, "*", SearchOption.AllDirectories);
        Font mFont = AssetDatabase.LoadAssetAtPath<Font>("assets/font/STXINGKA.TTF");
        for (int i = 0; i < dirs.Length; i++)
        {
            if (!dirs[i].EndsWith(".prefab"))
            {
                continue;
            }

            string assetPath = dirs[i].Substring(dirs[i].IndexOf("assets"));
            UnityEngine.Debug.Log(assetPath);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            Recxxx(mFont, obj.transform);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 处理Lua文件
    /// </summary>
    static void HandleLuaFile() {
        string resPath = AppDataPath + "/" + AppConst.AssetDir + "/";
        string luaPath = resPath + "/lua/";

        //----------复制Lua文件----------------
        if (!Directory.Exists(luaPath)) {
            Directory.CreateDirectory(luaPath); 
        }
        string[] luaPaths = { AppDataPath + "/" + AppConst.AppName + "/lua/", 
                              AppDataPath + "/" + AppConst.AppName + "/Tolua/Lua/" };

        for (int i = 0; i < luaPaths.Length; i++) {
            paths.Clear(); files.Clear();
            string luaDataPath = luaPaths[i];
            Recursive(luaDataPath);
            int n = 0;
            foreach (string f in files) {
                if (f.EndsWith(".meta")) continue;
                string newfile = f.Replace(luaDataPath, "");
                string newpath = luaPath + newfile;
                string path = Path.GetDirectoryName(newpath);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                if (File.Exists(newpath)) {
                    File.Delete(newpath);
                }
                if (AppConst.LuaByteMode) {
                    EncodeLuaFile(f, newpath);
                } else {
                    File.Copy(f, newpath, true);
                }
                UpdateProgress(n++, files.Count, newpath);
            } 
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 将所有的纹理打一个prefeb, 放在prefab/_TexturePrefab，用于在代码中可以动态加载 
    /// </summary>
    [MenuItem("Assets/Texture Gen Prefab", false, 1)]
    static void HandleTexturePrefab()
    {
        Object obj = Selection.activeObject;
        if (!AssetDatabase.GetAssetPath(obj).Contains("Assets/Textures/"))
        {
            return;
        }
        string path = AssetDatabase.GetAssetPath(obj).Replace("Assets/Textures/", "");
        path = path.Substring(0, path.LastIndexOf("/"));

        string streamDir = AppDataPath + "/" + AppConst.TexturePath + path;
        string prefabPath = AppDataPath + "/" + AppConst.PrefabPath + AppConst.TexPrefabPath + path;
        if (Directory.Exists(prefabPath))
        {
            Directory.Delete(prefabPath, true);
        }
        Directory.CreateDirectory(prefabPath);

        string[] files = Directory.GetFiles(streamDir, "*.*", SearchOption.TopDirectoryOnly);
        GameObject father = new GameObject("parent");
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta"))
            {
                continue;
            }
            // 给每个图片生成一个prefeb放在 _TexturePrefab，便于代码里动态创建, 这里就不要地鬼了，因为外面的dir已经递归
            string tex_fab_path = files[i].Replace("\\", "/").Replace(streamDir, prefabPath);
            tex_fab_path = tex_fab_path.Substring(0, tex_fab_path.LastIndexOf("/"));

            if (!Directory.Exists(tex_fab_path))
            {
                Directory.CreateDirectory(tex_fab_path);
            }
            string png_path = files[i].Replace("\\", "/").Replace(streamDir, prefabPath).Replace(".png", ".prefab").Replace(".jpg", ".prefab");
            string assetPath = files[i].Substring(files[i].IndexOf("assets"));
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                continue;
            }
            GameObject go = new GameObject(sprite.name);
            go.layer = LayerMask.NameToLayer("UI");

            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.SetNativeSize();
            img.raycastTarget = false;

            PrefabUtility.CreatePrefab(png_path.Substring(png_path.IndexOf("assets")), go);
            go.transform.SetParent(father.transform);
        }
        GameObject.DestroyImmediate(father);
        AssetDatabase.Refresh();
    }

    static string GetLuaString(string to_match)
    {
        Match mat = Regex.Match(to_match, ".*['\"](.*)['\"].*");
        if (mat.Success)
        {
            return mat.Groups[1].Value;
        }
        return null;
    }

    static void HandleMatch(Dictionary<string, List<string>> tex_to_build, string match_all, string match_file, string match_texs)
    {
        string streamDir = AppDataPath + "/" + AppConst.TexturePath;
        string prefabPath = AppDataPath + "/" + AppConst.PrefabPath + AppConst.TexPrefabPath;

        string tex_path = (streamDir + match_file).ToLower();
        List<string> tex_list;
        if (!tex_to_build.TryGetValue(tex_path, out tex_list))
        {
            tex_list = new List<string>();
            tex_to_build[tex_path] = tex_list;
        }
        else
        {
            if (tex_list.Count == 0)
            {
                // 上次已经是全部添加了
                UnityEngine.Debug.Log(match_all + " " + match_file + " " + match_texs + " add: " + tex_list.Count);
                return;
            }
        }

        string[] texs = match_texs.Split(',');
        bool not_add = true;
        foreach (string tex in texs)
        {
            string _tex = GetLuaString(tex);
            //即使是函数加了逗号
            if (_tex != null)
            {
                tex_list.Add(GetLuaString(tex));
                not_add = false;
            }
        }
        if (not_add)
        {
            //处理所有文件，就是保持tex_list为空
            tex_list.Clear();
        }

        UnityEngine.Debug.Log(match_all + " " + match_file + " " + match_texs + " add: " + tex_list.Count);
    }

    [MenuItem("LuaFramework/ClearProgressBar", false, 103)]
    static void Clear()
    {
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 将所有的纹理打一个prefeb, 放在prefab/_TexturePrefab，用于在代码中可以动态加载 
    /// </summary>
    [MenuItem("LuaFramework/ALL Texture Gen Prefab", false, 103)]
    static void HandleAllTexturePrefab()
    {
        string lua_path = AppDataPath + "/" + AppConst.AppName + "/lua/";
        string[] lua_files = Directory.GetFiles(lua_path, "*.lua", SearchOption.AllDirectories);
        string streamDir = (AppDataPath + "/" + AppConst.TexturePath).ToLower();
        string prefabPath = (AppDataPath + "/" + AppConst.PrefabPath + AppConst.TexPrefabPath).ToLower();
        Dictionary<string, List<string>> tex_to_build = new Dictionary<string, List<string>>();

        Regex rgx_load_res1 = new Regex("\\{\\s*type\\s*=\\s*['\"]texture['\"]\\s*,\\s*files\\s*=\\s*\\{\\s*['\"]([^{]*)['\"]\\s*,([^{}]*)\\}\\s*\\}", RegexOptions.IgnoreCase);
        Regex rgx_load_res2 = new Regex("\\{\\s*type\\s*=\\s*['\"]texture['\"]\\s*,\\s*files\\s*=\\s*\\{\\s*['\"]([^{]*)['\"]\\s*,\\s*\\{([^}]*)\\}\\s*\\}\\s*\\}", RegexOptions.IgnoreCase);

        Regex rgx_load_tex1 = new Regex("resMgr:LoadTexture\\s*\\(['\"]([^{}]*)['\"]\\s*,\\s*([^{}]*),", RegexOptions.IgnoreCase);
        Regex rgx_load_tex2 = new Regex("resMgr:LoadTexture\\s*\\(['\"]([^{}]*)['\"]\\s*,\\s*\\{([^{}]*)\\},", RegexOptions.IgnoreCase);

        // ArtNumber
        Regex artnumber = new Regex("ArtNumber\\.New\\s*\\(.*,\\s*['\"]([^,]*)['\"]\\s*,\\s*['\"]([^,()]*)['\"]\\s*(,|\\))", RegexOptions.IgnoreCase);

        for ( int i = 0; i < lua_files.Length; i++ )
        {
            string file_content = File.ReadAllText(lua_files[i]);
            MatchCollection collect1 = rgx_load_res1.Matches(file_content);
            MatchCollection collect2 = rgx_load_res2.Matches(file_content);
            MatchCollection collect3 = rgx_load_tex1.Matches(file_content);
            MatchCollection collect4 = rgx_load_tex2.Matches(file_content);
            MatchCollection collect5 = artnumber.Matches(file_content);

            foreach (Match match in collect1)
            {
                HandleMatch(tex_to_build, match.Groups[0].Value, match.Groups[1].Value, match.Groups[2].Value);
            }
            foreach (Match match in collect2)
            {
                HandleMatch(tex_to_build, match.Groups[0].Value, match.Groups[1].Value, match.Groups[2].Value);
            }
            foreach (Match match in collect3)
            {
                HandleMatch(tex_to_build, match.Groups[0].Value, match.Groups[1].Value, match.Groups[2].Value);
            }
            foreach (Match match in collect4)
            {
                HandleMatch(tex_to_build, match.Groups[0].Value, match.Groups[1].Value, match.Groups[2].Value);
            }
            foreach (Match match in collect5)
            {
                string tex_path = (streamDir + match.Groups[1]).ToLower();
                UnityEngine.Debug.Log(match.Groups[0] + " " + match.Groups[1].Value + " " + match.Groups[2].Value);
                List<string> tex_list;
                if (!tex_to_build.TryGetValue(tex_path, out tex_list))
                {
                    tex_list = new List<string>();
                    tex_to_build[tex_path] = tex_list;
                }
                else
                {
                    if (tex_list.Count == 0)
                    {
                        // 上次已经是全部添加了
                        continue;
                    }
                }
                tex_list.Add(match.Groups[2].Value + ".*");
            }

            EditorUtility.DisplayCancelableProgressBar("匹配lua文件中：", lua_files[i], (float)i / lua_files.Length);
        }

        float index = 0;
        foreach(var pair in tex_to_build)
        {
            string tex_fab_path = pair.Key.Replace(streamDir, prefabPath);
            string assetPath = pair.Key.Substring(pair.Key.IndexOf("assets"));
            if (!Directory.Exists(pair.Key))
            {
                continue;
            }
            if (!Directory.Exists(tex_fab_path))
            {
                Directory.CreateDirectory(tex_fab_path);
            }

            string[] all_files = Directory.GetFiles(pair.Key, "*", SearchOption.TopDirectoryOnly);

            foreach(string file in all_files)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(file.Substring(file.IndexOf("assets/")));
                string pre_fab_path = file.Replace("\\", "/").Replace(streamDir, prefabPath).Replace(".png", ".prefab").Replace(".jpg", ".prefab");
                if (sprite == null || File.Exists(pre_fab_path))
                {
                    continue;
                }
                if (pair.Value.Count > 0)
                {
                    foreach (string need_file in pair.Value)
                    {
                        if (Regex.IsMatch(sprite.name, need_file))
                        {
                            GameObject go = new GameObject(sprite.name);
                            go.layer = LayerMask.NameToLayer("UI");

                            Image img = go.AddComponent<Image>();
                            img.sprite = sprite;
                            img.SetNativeSize();
                            img.raycastTarget = false;

                            PrefabUtility.CreatePrefab(pre_fab_path.Substring(pre_fab_path.IndexOf("assets/")), go);
                            GameObject.DestroyImmediate(go);
                            break;
                        }
                    }
                }
                else
                {
                    GameObject go = new GameObject(sprite.name);
                    go.layer = LayerMask.NameToLayer("UI");

                    Image img = go.AddComponent<Image>();
                    img.sprite = sprite;
                    img.SetNativeSize();
                    img.raycastTarget = false;

                    PrefabUtility.CreatePrefab(pre_fab_path.Substring(pre_fab_path.IndexOf("assets/")), go);
                    GameObject.DestroyImmediate(go);
                }

            }
            EditorUtility.DisplayCancelableProgressBar("处理lua文件中：", pair.Key, index++ / tex_to_build.Count);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 将所有的声音打一个prefeb, 放在prefab/_AudioPrefab，用于在代码中可以动态加载 
    /// </summary>
    [MenuItem("LuaFramework/Audio Gen Prefab", false, 103)]
    static void HandleAudioPrefab()
    {
        string streamDir = AppDataPath + "/" + AppConst.SoundPath;
        string prefabPath = AppDataPath + "/" + AppConst.PrefabPath + AppConst.AudioPrefabPath;
        if (Directory.Exists(prefabPath))
        {
            Directory.Delete(prefabPath, true);
        }
        Directory.CreateDirectory(prefabPath);

        string[] files = Directory.GetFiles(streamDir, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta"))
            {
                continue;
            }
            // 给每个图片生成一个prefeb放在 _TexturePrefab，便于代码里动态创建, 这里就不要地鬼了，因为外面的dir已经递归
            string tex_fab_path = files[i].Replace("\\", "/").Replace(streamDir, prefabPath);
            tex_fab_path = tex_fab_path.Substring(0, tex_fab_path.LastIndexOf("/"));

            if (!Directory.Exists(tex_fab_path))
            {
                Directory.CreateDirectory(tex_fab_path);
            }
            string png_path = files[i].Replace("\\", "/").Replace(streamDir, prefabPath);
            png_path = png_path.Substring(0, png_path.LastIndexOf(".")) + ".prefab";

            string assetPath = files[i].Substring(files[i].IndexOf("assets"));
            AudioClip sprite = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            GameObject go = new GameObject(sprite.name);
            go.layer = LayerMask.NameToLayer("UI");

            AudioSource img = go.AddComponent<AudioSource>();
            img.clip = sprite;
            img.reverbZoneMix = 0;
            img.spatialBlend = 0.5f;

            PrefabUtility.CreatePrefab(png_path.Substring(png_path.IndexOf("assets")), go);
            GameObject.DestroyImmediate(go);
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 打包纹理文件到steaming asset
    /// </summary>
    static void HandleTextureBundle()
    {
        string resPath = AppDataPath + "/" + AppConst.AssetDir + "/" + AppConst.TexturePath;
        if (!Directory.Exists(resPath)) Directory.CreateDirectory(resPath);

        string texDir = AppDataPath + "/" + AppConst.TexturePath;

        string[] dirs = Directory.GetDirectories(texDir, "*", SearchOption.AllDirectories);
        for (int i = 0; i < dirs.Length; i++)
        {
            string name = dirs[i].Replace(texDir, string.Empty).ToLower();
            string tex_bundle_path = Directory.GetParent(resPath + name).FullName;
            if (!Directory.Exists(tex_bundle_path)) 
            {
                Directory.CreateDirectory(tex_bundle_path);
            }
            string path = "Assets/" + AppConst.TexturePath + name;

            AddBuildMap(AppConst.TexturePath + name + AppConst.ExtName, "*.*", path);
        }
        AddBuildMap(AppConst.TexturePath + "_textures" + AppConst.ExtName, "*.*", "Assets/" + AppConst.TexturePath);
    }

    /// <summary>
    /// 打包其他资源，如动画，动画控制器，等等
    /// </summary>
    static void HandleOtherResourceBundle()
    {
        string resPath = AppDataPath + "/" + AppConst.AssetDir + "/" + AppConst.OtherResourcePath;
        if (!Directory.Exists(resPath)) Directory.CreateDirectory(resPath);

        string texDir = AppDataPath + "/" + AppConst.OtherResourcePath;

        string[] dirs = Directory.GetDirectories(texDir, "*", SearchOption.AllDirectories);
        for (int i = 0; i < dirs.Length; i++)
        {
            string name = dirs[i].Replace(texDir, string.Empty).ToLower();
            string tex_bundle_path = Directory.GetParent(resPath + name).FullName;
            if (!Directory.Exists(tex_bundle_path))
            {
                Directory.CreateDirectory(tex_bundle_path);
            }
            string path = "Assets/" + AppConst.OtherResourcePath + name;

            AddBuildMap(AppConst.OtherResourcePath + name + AppConst.ExtName, "*.*", path);
        }
        AddBuildMap(AppConst.OtherResourcePath + "_others" + AppConst.ExtName, "*.*", "Assets/" + AppConst.OtherResourcePath);
    }

    /// <summary>
    /// 打包场景文件到steaming asset,  由于场景比较大，一个场景打一个
    /// </summary>
    static void HandleSceneBundle()
    {
        string resPath = AppDataPath + "/" + AppConst.AssetDir + "/" + AppConst.ScenePath;
        if (!Directory.Exists(resPath)) Directory.CreateDirectory(resPath);

        string texDir = AppDataPath + "/" + AppConst.ScenePath;

        string[] files = Directory.GetFiles(texDir, "*.unity", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            string name = files[i].Replace(texDir, string.Empty).ToLower();
            string tex_bundle_path = Directory.GetParent(resPath + name).FullName;
            if (!Directory.Exists(tex_bundle_path))
            {
                Directory.CreateDirectory(tex_bundle_path);
            }
            string path = "Assets/" + AppConst.ScenePath + name;

            AddBuildMap(AppConst.ScenePath + name.Replace(".unity", AppConst.ExtName), "*.unity", path);
        }
    }

    static void BuildFileIndex() {
        string resPath = AppDataPath + "/" + AppConst.AssetDir + "/";
        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "files.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        paths.Clear(); files.Clear();
        Recursive(resPath);

        long unix_time = Util.GetUnixTime();
        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < files.Count; i++) {
            string file = files[i];
            if (file.EndsWith(".meta") || file.Contains(".DS_Store")) continue;
            FileInfo info = new FileInfo(file);

            long file_size = info.Length;

            string md5 = Util.md5file(file);
            string value = file.Replace(resPath, string.Empty);
            sw.WriteLine(value + "|" + md5 + "|" + file_size + "|" + unix_time);
        }
        sw.Close(); fs.Close();
    }

    /// <summary>
    /// 数据目录
    /// </summary>
    static string AppDataPath {
        get { return Application.dataPath.ToLower(); }
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path) {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names) {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs) {
            // 跳过.svn目录
            if (dir.EndsWith(".svn")) continue;

            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

    static void UpdateProgress(int progress, int progressMax, string desc) {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }

    public static void EncodeLuaFile(string srcFile, string outFile) {
        if (!srcFile.ToLower().EndsWith(".lua")) {
            File.Copy(srcFile, outFile, true);
            return;
        }
        bool isWin = true;
        string luaexe = string.Empty;
        string args = string.Empty;
        string exedir = string.Empty;
        string currDir = Directory.GetCurrentDirectory();
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            isWin = true;
            luaexe = "luajit.exe";
            args = "-b " + srcFile + " " + outFile;
            exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit/";
        } else if (Application.platform == RuntimePlatform.OSXEditor) {
            isWin = false;
            luaexe = "./luac";
            args = "-o " + outFile + " " + srcFile;
            exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luavm/";
        }
        Directory.SetCurrentDirectory(exedir);
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = args;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.ErrorDialog = true;
        info.UseShellExecute = isWin;
        Util.Log(info.FileName + " " + info.Arguments);

        Process pro = Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    [MenuItem("LuaFramework/gen UGUI lua File")]
    public static void GenLhdbUGUILuaFile()
    {
        // 这个本来要加路径，但是连环夺宝的暂时放外面
        // GenUGUILuaFile("LHDB/");
        GenUGUILuaFile("");
    }

    [MenuItem("LuaFramework/gen LHDB UGUI lua File")]
    public static void GenLhdbLHDBUGUILuaFile()
    {
        // 这个本来要加路径，但是连环夺宝的暂时放外面
        // GenUGUILuaFile("LHDB/");
        GenUGUILuaFile("lhdb/");
    }


    [MenuItem("LuaFramework/gen MASSBAT UGUI lua File")]
    public static void GenLhdbMASSBATUGUILuaFile()
    {
        // 这个本来要加路径，但是连环夺宝的暂时放外面
        // GenUGUILuaFile("LHDB/");
        GenUGUILuaFile("massive_battle/");
    }

    [MenuItem("LuaFramework/gen GOLDENSHARK UGUI lua File")]
    public static void GenGOLDENSHARKUGUILuaFile()
    {
        // 这个本来要加路径，但是连环夺宝的暂时放外面
        // GenUGUILuaFile("LHDB/");
        GenUGUILuaFile("golden_shark/");
    }

    [MenuItem("LuaFramework/gen SLOTS UGUI lua File")]
    public static void GenLhdbSLOTSUGUILuaFile()
    {
        GenUGUILuaFile("slots/");
    }

    [MenuItem("LuaFramework/gen SPADEACE UGUI lua File")]
    public static void GenLhdbSPADEACEUGUILuaFile()
    {
        GenUGUILuaFile("spade_ace/");
    }

    [MenuItem("LuaFramework/gen ELIMNINATE UGUI lua File")]
    public static void GenLhdbELIUGUILuaFile()
    {
        // 这个本来要加路径，但是连环夺宝的暂时放外面
        // GenUGUILuaFile("LHDB/");
        GenUGUILuaFile("eliminate/");
    }

    [MenuItem("LuaFramework/gen ZJH UGUI lua File")]
    public static void GenZJHLuaFile()
    {
        // 这个本来要加路径，但是连环夺宝的暂时放外面
        // GenUGUILuaFile("LHDB/");
        GenUGUILuaFile("zjh/");
    }

    [MenuItem("LuaFramework/gen BULLFIGHT UGUI lua File")]
    public static void GenBULLFIGHTLuaFile()
    {
        // 这个本来要加路径，但是连环夺宝的暂时放外面
        // GenUGUILuaFile("LHDB/");
        GenUGUILuaFile("bull_fight/");
    }

    public static void GenUGUILuaFile(string path)
    {
        GameObject obj = Selection.activeGameObject;
        string view_path = AppDataPath + "/" + AppConst.AppName + "/lua/View/" + path;
        string ctrl_path = AppDataPath + "/" + AppConst.AppName + "/lua/Controller/" + path;
        string ctrl_name;

        if (obj == null)
        {
            UnityEngine.Debug.LogError("请选中一个panel预制体！");
            return;
        }
        int index = obj.name.IndexOf("Panel");

        if (index <= 0)
        {
            UnityEngine.Debug.LogError("预制体名必须以Panel结束！");
            return;
        }
        ctrl_name = obj.name.Substring(0, index) + "Ctrl";
        ctrl_path += ctrl_name + ".lua";
        view_path += obj.name + ".lua";

        // ———————代码生成———————强行覆盖xxxPanel.lua（就是为了禁止在这里面写自己的逻辑） ,  但是不覆盖 xxxCtrl.lua 因为后者是手写逻辑的。
        StringBuilder sb = new StringBuilder();
        if (File.Exists(ctrl_path))
        {
            UnityEngine.Debug.LogWarning(ctrl_path + "已经存在，未发生替换");
        }
        else
        {
            string prefab_path = AssetDatabase.GetAssetPath(obj);
            if (prefab_path.Length == 0)
            {
                UnityEngine.Debug.LogError("必须选中目录下的预制体，而不是场景中的obj！");
                return;
            }
            prefab_path = prefab_path.Replace("Assets/Prefabs/", string.Empty);
            prefab_path = prefab_path.Substring(0, prefab_path.LastIndexOf("."));

            // 生成View(Ctrl)文件      
            sb.AppendLine("require \"Common/define\"");
            sb.AppendLine("require \"View/" + path + obj.name + "\"");
            sb.AppendLine("");
            sb.AppendLine(ctrl_name + " = {}");
            sb.AppendLine("local this = " + ctrl_name);
            sb.AppendLine("");
            sb.AppendLine("local transform");
            sb.AppendLine("local gameObject");
            sb.AppendLine("local panel");
            sb.AppendLine("local luaBehaviour");
            sb.AppendLine("local coroutine = coroutine.new_local_cro()");
            sb.AppendLine("");
            sb.AppendLine("--构造函数--");
            sb.AppendLine("function " + ctrl_name + ".New()");
            sb.AppendLine("\tthis.isAwake = false");
            sb.AppendLine("\treturn this");
            sb.AppendLine("end");
            sb.AppendLine("");
            sb.AppendLine("--显示前调用--");
            sb.AppendLine("function " + ctrl_name + ".Awake()");
            sb.AppendLine("\tthis.isAwake = true");
            sb.AppendLine("\tcreatePanel('" + prefab_path + "', this.OnCreate)");
            sb.AppendLine("end");
            sb.AppendLine("");
            sb.AppendLine("--创建成功后--");
            sb.AppendLine("function " + ctrl_name + ".OnCreate(obj)");
            sb.AppendLine("\tif not this.isAwake then panelMgr:ClosePanel('" + obj.name + "') end");
            sb.AppendLine("\tgameObject = obj");
            sb.AppendLine("\tpanel = " + obj.name);
            sb.AppendLine("\ttransform = obj.transform");
            sb.AppendLine("\tluaBehaviour = transform:GetComponent('LuaBehaviour')");
            sb.AppendLine("end");
            sb.AppendLine("");
            // TODO 这里是不是要一个隐藏/显示的功能
            sb.AppendLine("--关闭界面,可以重入--");
            sb.AppendLine("function " + ctrl_name + ".Close()");
            sb.AppendLine("\tif not this.isAwake then");
            sb.AppendLine("\t\treturn");
            sb.AppendLine("\tend");
            sb.AppendLine("\tthis.isAwake = false");
            sb.AppendLine("\tcoroutine.stop_all()");
            sb.AppendLine("\tpanelMgr:ClosePanel('" + obj.name + "')");
            sb.AppendLine("end");
            sb.AppendLine("");
            File.WriteAllText(ctrl_path, sb.ToString(), new UTF8Encoding(false));
            sb.Clear();
        }

        // 生成Panel文件
        sb.AppendLine("local transform");
        sb.AppendLine("local gameObject");
        sb.AppendLine("");
        sb.AppendLine(obj.name + " = {}");
        sb.AppendLine("local this = " + obj.name + "");
        sb.AppendLine("");
        sb.AppendLine("--启动事件--");
        sb.AppendLine("function " + obj.name + ".Awake(obj)");
        sb.AppendLine("\tgameObject = obj");
        sb.AppendLine("\ttransform = obj.transform");
        sb.AppendLine("");
        sb.AppendLine("\tthis.InitPanel()");
        sb.AppendLine("\tlog(\"Awake lua--->> " + obj.name + "\")");
        sb.AppendLine("end");
        sb.AppendLine("");
        sb.AppendLine("--初始化面板--");
        sb.AppendLine("function " + obj.name + ".InitPanel()");

        // 初始化组件, 遍历。
        itemHaveNames.Clear();
        RecurseChildren(sb, obj.transform, "");

        //sb.AppendLine(" this.btnOpen = transform:FindChild(\"Open\").gameObject;");
        //sb.AppendLine(" this.gridParent = transform:FindChild('ScrollView/Grid');");

        sb.AppendLine("end");
        sb.AppendLine("");
        sb.AppendLine("--单击事件--");
        sb.AppendLine("function " + obj.name + ".OnDestroy()");
        sb.AppendLine("\tclearTableExceptFunc(this)");
        sb.AppendLine("\tgameObject = nil");
        sb.AppendLine("\ttransform = nil");
        sb.AppendLine("\tlog(\"OnDestroy---- >>> " + obj.name + "\")");
        sb.AppendLine("end");

        File.WriteAllText(view_path, sb.ToString(), new UTF8Encoding(false));
    }

    static Dictionary<string, int> itemHaveNames = new Dictionary<string, int>();
    static void CheckUIComponent(StringBuilder sb, Transform obj, string obj_name, string type_str, string com_name)
    {
        Component com = obj.GetComponent(type_str);
        if (com != null)
        {
            com_name = com_name.Replace(" ", "_");
            if (itemHaveNames.ContainsKey(com_name))
            {
                itemHaveNames[com_name] += 1;
                com_name = string.Format("{0}_{1}", com_name, itemHaveNames[com_name]);
            }
            else
            {
                itemHaveNames.Add(com_name, 0);
            }
            // + 太多了不好，不过反正是编辑器, 先注释起来
            sb.AppendLine("\t--this." + com_name + " = this." + obj_name + ":GetComponent(\"" + type_str + "\")");
        }
    }

    static void RecurseChildren(StringBuilder sb, Transform my_tran, string header)
    {
        foreach (Transform child_tran in my_tran)
        {
            int childs = child_tran.childCount;
            string child_name = string.Format("obj_{0}", child_tran.name).Replace(" ", "_").Replace("(", "_").Replace(")", "_");
            if (itemHaveNames.ContainsKey(child_name))
            {
                itemHaveNames[child_name] += 1;
                child_name = string.Format("{0}_{1}", child_name, itemHaveNames[child_name]);
            }
            else
            {
                itemHaveNames.Add(child_name, 0);
            }

            sb.AppendLine("-----" + header + child_tran.name + "-----");
            sb.AppendLine("\t--this." + child_name + " = transform:Find(\"" + header + child_tran.name + "\").gameObject");
            // 需要导出的组件放下面
            CheckUIComponent(sb, child_tran, child_name, "Image", "img_" + child_tran.name);
            CheckUIComponent(sb, child_tran, child_name, "Text", "txt_" + child_tran.name);
            CheckUIComponent(sb, child_tran, child_name, "InputField", "input_" + child_tran.name);
            CheckUIComponent(sb, child_tran, child_name, "Button", "btn_" + child_tran.name);
            CheckUIComponent(sb, child_tran, child_name, "Toggle", "tog_" + child_tran.name);
            CheckUIComponent(sb, child_tran, child_name, "DropDown", "drop_" + child_tran.name);

            if (childs > 0)
            {
                RecurseChildren(sb, child_tran, header + child_tran.name + "/");
            }
        }
    }

    // protobuf协议的定义放在  lua/protocol下， 生成的lua放在 /lua/protocol下
    [MenuItem("LuaFramework/Build Protobuf-lua-gen File")]
    public static void BuildProtobufFile() {
        string dir = AppDataPath + "/" + AppConst.AppName + "/lua/protocol_define";
        paths.Clear(); files.Clear(); Recursive(dir);

        string protoc = AppDataPath.Replace("assets", "") + "protobuf-2.4.1/src/protoc.exe";
        string protoc_gen_dir = "\"" + dir + "/protoc-gen-lua.bat\"";

        string gen_path = AppDataPath + "/" + AppConst.AppName + "/lua/protocol";
        if (Directory.Exists(gen_path))
        {
            Directory.Delete(gen_path, true);
        }
        Directory.CreateDirectory(gen_path);
        foreach (string f in files) {
            string name = Path.GetFileName(f);
            string ext = Path.GetExtension(f);
            if (!ext.Equals(".proto")) continue;

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = protoc;
            info.Arguments = " --lua_out=\"" + gen_path + "\" --plugin=protoc-gen-lua=" + protoc_gen_dir + " " + name;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = true;
            info.WorkingDirectory = dir;
            info.ErrorDialog = true;
            Util.Log(info.FileName + " " + info.Arguments);

            Process pro = Process.Start(info);
            pro.WaitForExit();
        }

        AssetDatabase.Refresh();
    }

    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
            {
                continue;
            }
            // 打包只要这个场景
            if (e.enabled && e.path.IndexOf("AppEnter") > 0)
            {
                names.Add(e.path);
            }
        }
        return names.ToArray();
    }

    // [MenuItem("LuaFramework/android auto")]
    public static void BuildForAndroid()
    {
        string package_name = "default";
        bool is_debug = true;
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith("-project:"))
            {
                package_name = arg.Split(':')[1];
            }
            else if (arg.StartsWith("-name:"))
            {
                PlayerSettings.productName = arg.Split(':')[1];
            }
            else if (arg.ToLower().StartsWith("-options:release"))
            {
                is_debug = false;
            }
            else if (arg.StartsWith("-chanel:"))
            {
                string[] appenters = File.ReadAllLines(AppConst.FrameworkRoot + "/AppEnterScene/AppEnter.unity");
                for (int i = 0; i < appenters.Length; i++)
                {
                    if (appenters[i].Contains("m_Chanel:"))
                    {
                        appenters[i] = "  m_Chanel: " + arg.Split(':')[1];
                        File.WriteAllLines(AppConst.FrameworkRoot + "/AppEnterScene/AppEnter.unity", appenters);
                        break;
                    }
                }
            }
        }

        PlayerSettings.Android.keystoreName = "user.keystore";
        PlayerSettings.Android.keystorePass = "123456";
        PlayerSettings.Android.keyaliasName = "mirror";
        PlayerSettings.keyaliasPass = "123456";

        string path = Application.dataPath.Replace("Assets", string.Empty) + package_name + ".apk";
        BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.Android, is_debug ? BuildOptions.Development | BuildOptions.AllowDebugging : BuildOptions.None);
    }

    static string PROVISIONING = "";
    static string CODE_SIGN = "";
    static string VERSION;
    public static void BuildForIOS()
    {
        string package_name = "default";
        bool is_debug = true;
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith("-project:"))
            {
                package_name = arg.Split(':')[1];
            }
            else if (arg.StartsWith("-provision:"))
            {
                PROVISIONING = arg.Split(':')[1];
            }
            else if (arg.StartsWith("-codesign:"))
            {
                CODE_SIGN = arg.Split(':')[1];
            }
            else if (arg.StartsWith("-version:"))
            {
                VERSION = arg.Split(':')[1];
            }
            else if (arg.ToLower().StartsWith("-options:release"))
            {
                is_debug = false;
            }
        }

        string path = Application.dataPath.Replace("Assets", string.Empty) + package_name;
        BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.iOS, is_debug ? BuildOptions.Development | BuildOptions.AllowDebugging : BuildOptions.None);
    }

    static string wx_scheme = "wx852782bbc02a58c3";
    static string delegate_name = "MirrorAppDelegate";
    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            string path = Path.GetFullPath(pathToBuiltProject);
            // Create a new project object from build target
            XCProject project = new XCProject(path);

            UnityEngine.Debug.Log("finish build :" + path);

            //在这里面把frameworks添加在你的xcode工程里面
            string[] files = Directory.GetFiles(Application.dataPath + "/XUPorter/Mod", "*.projmods", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                project.ApplyMod(file);
            }

            if (!string.IsNullOrEmpty(CODE_SIGN))
            {
                project.overwriteBuildSetting("CODE_SIGN_IDENTITY", CODE_SIGN, "Release");
                project.overwriteBuildSetting("CODE_SIGN_IDENTITY", CODE_SIGN, "Debug");
            }

            Confuse(path + "/Libraries/Plugins/iOS/");
            EditCode(path);
            EditPlist(path);
            project.Save();
            UnityEngine.Debug.Log("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            ConfuseMd5(path + "/Data/Raw/");
            ConfuseResource(path + "/Data/Raw/");
        }
    }

    static void EditPlist(string filePath)
    {
        XCPlist list = new XCPlist(filePath);
        string PlistAdd = @"  
            <key>CFBundleURLTypes</key>
            <array>
                <dict>
                    <key>CFBundleTypeRole</key>
                    <string>Editor</string>
                    <key>CFBundleURLName</key>
                    <string>weixin</string>
                    <key>CFBundleURLSchemes</key>
                    <array>
                        <string>" + wx_scheme + @"</string>
                    </array>
                </dict>
            </array>
            <key>NSPhotoLibraryUsageDescription</key>
            <string>亲，打开下相册好吗</string>
            <key>NSCameraUsageDescription</key>
            <string>亲，打开下相机好吗</string>";

        //在plist里面增加一行
        if (!string.IsNullOrEmpty(VERSION))
        {
            list.ReplaceKey("CFBundleShortVersionString", "<string>" + VERSION + "</string>");
            list.ReplaceKey("CFBundleVersion", "<string>" + VERSION + "</string>");
        }
        list.AddKey(PlistAdd);
        list.Save();
    }

    // Random.Range(0, 62)
    static char NumToChar(int num)
    {
        if (num < 26) return (char)(num + 'a');
        else if (num < 52) return (char)(num + 'A' - 26);
        else return (char)(num + '0' - 52);
    }

    static int index = 0; 
    static string GetRandomToken()
    {
        int len = Random.Range(1, 10);
        StringBuilder buider = new StringBuilder(len + 1);
        for (int i = 0; i < len; i++)
        {
            buider.Append(NumToChar(Random.Range(0, 62)));
        }

        return string.Format("{0}{1}_{2}", NumToChar(Random.Range(0, 52)), buider.ToString(), index++);
    }

    static string CreateControlCode()
    {
        string []control_val = 
@"if
for
".Split('\n');
        string rubbish_control_code = "";
        int times = Random.Range(0, 10);
        for (int i = 0; i <= times; i++)
        {
            int index = Random.Range(0, control_val.Length - 1);
            string tmp = GetRandomToken();
            rubbish_control_code = rubbish_control_code + "\n\tint " + tmp + "= " + Random.Range(0, 10000).ToString() + ";";
            if (control_val[index] == "if")
            {
                rubbish_control_code = rubbish_control_code + "\n\t" + control_val[index] + "(" + tmp + " <= " + Random.Range(0, 10000).ToString() + "){";
                rubbish_control_code = rubbish_control_code + "\n\t\t" + tmp + "= " + Random.Range(0, 10000).ToString() + ";\n\t}"; 
            }else if (control_val[index] == "for")
            {
                string i_val = GetRandomToken();
                rubbish_control_code = rubbish_control_code + "\n\tint " + i_val + "= " + "0;";
                rubbish_control_code = rubbish_control_code + "\n\t" + control_val[index] + "(" + i_val + " = 0;" + i_val + " <= " + tmp + "; " + i_val + "++){\n\t}";    
            }
        }
        return rubbish_control_code;
    }

    static string CreateRubbishCode()
    {
       string []rubbish_return_val = 
@"void
int
float
char
struct".Split('\n');          
        string rubbish_content = "\nextern \"C\" {\n";
        int times = Random.Range(50, 100);
        for (int i = 0; i <= times; i++)
        {
            int index = Random.Range(0, rubbish_return_val.Length - 1);
            string struct_tmp = GetRandomToken();
            string child_tmp = GetRandomToken();
            if (rubbish_return_val[index] == "struct" || rubbish_return_val[index] == "union" || rubbish_return_val[index] == "enum")
            {
                rubbish_content = rubbish_content + "\n" + rubbish_return_val[index] + " " + struct_tmp + "\n{\n\tint " + child_tmp + ";\n};";
                rubbish_content = rubbish_content + "\n" + rubbish_return_val[index] + " " + struct_tmp + " " + GetRandomToken() + "(){";
            }else
            {
                rubbish_content = rubbish_content + "\n" + rubbish_return_val[index] + " " + GetRandomToken() + "(){";
            }
            rubbish_content = rubbish_content + CreateControlCode();
            if (rubbish_return_val[index] == "void")
            {
                rubbish_content = rubbish_content + "\n}";
            } else if (rubbish_return_val[index] == "int")
            {
                rubbish_content = rubbish_content + "\n\treturn " + Random.Range(0, 10000).ToString() + ";\n}";  
            }else if (rubbish_return_val[index] == "float")
            {
            rubbish_content = rubbish_content + "\n\treturn " + (Random.Range(0, 10000) / 1000).ToString() + ";\n}";
            }else if (rubbish_return_val[index] == "char")
            {
            rubbish_content = rubbish_content + "\n\treturn '" + NumToChar(Random.Range(0, 52)) + "';\n}";
            }else if (rubbish_return_val[index] == "struct")
            {
                string local_tmp = GetRandomToken();
                rubbish_content = rubbish_content + "\n\t" + rubbish_return_val[index] + " " + struct_tmp + " " + local_tmp + ";";
                rubbish_content = rubbish_content + "\n\t" + local_tmp + "." + child_tmp + " = " + Random.Range(0, 10000).ToString() + ";"; 
                rubbish_content = rubbish_content + "\n\treturn " + local_tmp + ";\n}";
            }
        } 
        return rubbish_content;
    }

    static void GenOneMMFile(string file_path)
    {   
        string file_name = file_path + GetRandomToken() + ".mm";
        StringBuilder sb = new StringBuilder();     
        sb.AppendLine("@implementation " + GetRandomToken());
        sb.AppendLine("@end");
        sb.AppendLine("+ (void) " + GetRandomToken() + ": (NSString *)" + GetRandomToken());
        sb.AppendLine("{");
        sb.AppendLine("}");
        File.WriteAllText(file_name, sb.ToString(), new UTF8Encoding(false));
    }
    

    static void Confuse(string filepath)
    {
		string []conf_words = 
@"CreateNSString
MakeStringCopy
AppStorePay
validateProductIdentifiers
CheckReceipt
SaveTransactions
FinishTransaction
dictionaryWithJsonString
UnityAvtar
OpenTarget
GetSavePath
compressImage
SaveFileToDoc
objc_copyTextToClipboard
MakeNewAvtar
UniWebViewToolBar
UniWebSpinner
UniWebView
UniWebViewManager
UniWebViewMakeCString
UniWebViewMakeNSString
webViewName
webViewNameGetAlpha
webViewSetUserAgent
videoExitFullScreen
webView
webViewDidFinishLoad
webViewDidStartLoad
performJavaScript
goForwardWebViewName
goBackWebViewName
setWebViewBackgroundColorName
updateBackgroundWebViewName
removeWebViewName
checkOrientationSupport
MirrorKeyChain
key_chain_save
key_chain_load
key_chain_delete
getKeychainQuery".Split('\n');

        // 生成混淆宏定义文件
        StringBuilder buider = new StringBuilder();

       // wx_scheme = "wx" + Random.Range(100000, 999999);
        delegate_name = GetRandomToken();

        buider.AppendLine("#ifndef __CONFUSE_H__");
        buider.AppendLine("#define __CONFUSE_H__");

        for (int i = conf_words.Length - 1; i >= 0; i--)
        {
            int cur = Random.Range(0, i);
            string word = conf_words[cur];
            conf_words[cur] = conf_words[i];

            if (!string.IsNullOrEmpty(word))
            {
                buider.AppendLine(string.Format("#define {0} {1}", word, GetRandomToken()));
            }
        }
        buider.AppendLine(string.Format("#define {0} {1}", "MirrorAppDelegate", delegate_name));
        buider.AppendLine("#endif");

        string[] h_files = Directory.GetFiles(filepath, "*.h", SearchOption.AllDirectories);
        foreach (string file_name in h_files)
        {
            string content = File.ReadAllText(file_name);
            File.WriteAllText(file_name, "#import \"confuse.h\"\n" + content);
        }
        File.WriteAllText(filepath + "confuse.h", buider.ToString());

        string []mm_files = Directory.GetFiles(filepath, "*.mm", SearchOption.AllDirectories);
        foreach(string file_name in mm_files)
        {
            string content = File.ReadAllText(file_name);
            //if (file_name.EndsWith("MirrorAppDelegate.mm"))
            //{
            //    content = content.Replace("wx852782bbc02a58c3", wx_scheme);
            //}
            content = content + CreateRubbishCode();

            content = content + '}';
			File.WriteAllText(file_name, "#import \"confuse.h\"\n" + content);
        }
        int rubbish_file_num = Random.Range(500, 1000);
        for (int i = 1; i <= rubbish_file_num; i++)
        {
            GenOneMMFile(filepath);
        }
    }

    static void EditCode(string filePath)
    {
        //读取UnityAppController.mm文件
        XClass UnityAppController = new XClass(filePath + "/Classes/main.mm");
        // MirrorAppDelegate 继承了 UnityAppController
        UnityAppController.Replace("const char* AppControllerClassName = \"UnityAppController\";",
            string.Format("const char* AppControllerClassName = \"{0}\";", delegate_name));
    }

    static void ConfuseMd5(string file_path)
    {
        string[] files = Directory.GetFiles(file_path, "*.unity3d", SearchOption.AllDirectories);
        string files_txt_path = file_path + "files.txt";
        foreach (string file in files)
        {
            string local_path = file.Replace(file_path, "");
            string final_path = file_path + local_path;
            if (final_path.IndexOf("loading") >= 0)
            {
                continue;
            }
            string manifest_path = final_path + ".manifest";
            if (File.Exists(final_path)) {
                File.WriteAllBytes(final_path, Util.AddUnixTimePrefix(files_txt_path, File.ReadAllBytes(final_path)));
            }
            if (File.Exists(manifest_path))
            {
                File.WriteAllBytes(manifest_path, Util.AddUnixTimePrefix(files_txt_path, File.ReadAllBytes(manifest_path)));
            }
        }
    }

    static void ConfuseResource(string file_path)
    {
        Dictionary<string, string> confuse = new Dictionary<string, string>();
        string[] files = Directory.GetFiles(file_path, "*.unity3d", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string local_path = file.Replace(file_path, "");
            string[] paths = local_path.Split('/');
            for (int i = 0; i < paths.Length; i++)
            {
                if (confuse.ContainsKey(paths[i]))
                {
                    paths[i] = confuse[paths[i]];
                }
                else
                {
                    confuse[paths[i]] = GetRandomToken();
                    paths[i] = confuse[paths[i]];
                }
            }
            string final_path = file_path + string.Join("/", paths) + ".unity3d";
            UnityEngine.Debug.LogWarning(local_path + "|" + final_path);
            string to_path = final_path.Substring(0, final_path.LastIndexOf("/"));
            if (!Directory.Exists(to_path))
            {
                Directory.CreateDirectory(to_path);
            }
            File.Move(file, final_path);
            File.Move(file + ".manifest", final_path + ".manifest");
        }
        string[] dirs = Directory.GetDirectories(file_path, "*", SearchOption.AllDirectories);
        foreach (string dir in dirs)
        {
            if (Directory.Exists(dir) && Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length == 0)
            {
                Directory.Delete(dir, true);
            }
        }

        StringBuilder buider = new StringBuilder();
        foreach (KeyValuePair<string, string> e in confuse)
        {
            buider.AppendLine(e.Key + "|" + e.Value);
        }
        File.WriteAllText(file_path + "confuse.txt", buider.ToString());
    }
}