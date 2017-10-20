using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using LuaInterface;
using System.IO;
using LuaFramework;
using LitJson;
using System.Collections;
using System.Collections.Generic;



class LRUCompare : IComparer<AvatarData>
{
    public int Compare(AvatarData a, AvatarData b)
    {
        return (int)(a.time - b.time);
    }
}

class AvatarData
{
    public float time;
    public Sprite sprite;
}

//头像的跨平台管理器
public class AvatarManager : MonoBehaviour
{
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _OpenCamera(string persist, string file_name);
    [DllImport("__Internal")]
    private static extern void _OpenPhoto(string persist, string file_name);
#endif
    // 同一时间理论上只能开一个头像获取的，所以回调直接保存在这里
    static LuaFunction imgSelectCallback;

    void RemoveAllAvtar()
    {
        if (Directory.Exists(Util.DataPath))
        {
            string[] all_jpg = Directory.GetFiles(Util.DataPath, "*.jpg", SearchOption.TopDirectoryOnly);
            foreach (string file in all_jpg)
            {
                File.Delete(file);
            }
        }
    }

    void Awake()
    {
        RemoveAllAvtar();
        DontDestroyOnLoad(gameObject);
    }

    static string img_path
    {
        get { return Util.DataPath; }
    }

    public static void OpenCamera(string file_name_for_save, LuaFunction func)
    {
        imgSelectCallback = func;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("TakeCamera", img_path, file_name_for_save);
                }
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            _OpenCamera(img_path, file_name_for_save);
#endif
        }
    }

    public static void OpenPhoto(string file_name_for_save, LuaFunction func)
    {
        imgSelectCallback = func;
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("TakePhoto", img_path, file_name_for_save);
                }
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            _OpenPhoto(img_path, file_name_for_save);
#endif
        }
    }

    // 照片保存以后会发消息到这个函数
    void avatar_message(string file_full_path)
    {
        if (imgSelectCallback != null)
        {
            StartCoroutine(LoadTexture(file_full_path, imgSelectCallback));
        }
    }

    IEnumerator UploadCro(string file_full_path, string full_url, LuaFunction call_back)
    {
        string path = "file://" + file_full_path;
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            path = "file:///" + file_full_path;
        }
        // 读文件
        WWW www1 = new WWW(path);
        yield return www1;

        if (www1.error != null)
        {
            Debug.LogError(www1.error);
            yield break;
        }

        WWWForm body = new WWWForm();
        // 这个test.jpg是没个卵用的名字。
        body.AddBinaryData("fileUpload", www1.bytes, "test.jpg", "image/jpg");
        // 上传
        WWW www2 = new WWW(full_url, body);
        yield return www2;

        if (www2.error != null)
        {
            Debug.LogError(www2.error);
            yield break;
        }

        JsonData data = JsonMapper.ToObject(www2.text);

        if (data == null || (int)data["error_code"] != 0)
        {
            Debug.LogError(www2.text);
            yield break;
        }

        Texture2D tex = www1.texture;
        Sprite ret = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        SaveToAvatarCache(file_full_path, ret);

        call_back.Call(ret);
    }

    public void UploadAvatar(string file_name, string url, LuaFunction call_back)
    {
        string file_full_path = img_path + file_name;
        string full_url = AppConst.AvatarServer + url;

        StartCoroutine(UploadCro(file_full_path, full_url, call_back));
    }

    string GetAvatarUrl(string www_text)
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
            files_txt_path = AppConst.AvatarServer + "/" + (string)data["content"];
        }

        return files_txt_path;
    }

    IEnumerator Downloadcro(string file_full_path, string full_url, LuaFunction call_back, bool no_sec_www)
    {
        WWW www = new WWW(full_url);
        yield return www;

        if (www.error != null)
        {
            Debug.LogError(www.error);
            yield break;
        }

        if (!no_sec_www)
        {
            // 第二次
            string download_url = GetAvatarUrl(www.text);
            if (download_url == null)
            {
                yield break;
            }

            www = new WWW(download_url);
            yield return www;

            if (www.error != null)
            {
                yield break;
            }
        }
        File.WriteAllBytes(file_full_path, www.bytes);
        www.Dispose();
        // 加载下载后的纹理
        StartCoroutine(LoadTexture(file_full_path, call_back));
    }

    Dictionary<string, AvatarData> g_avatar_cache = new Dictionary<string, AvatarData>();

    // 使用缓存的头像或者去下载
    public void TryDownloadAvatar(string file_name, string url, LuaFunction call_back, bool no_sec_www)
    {
        string file_full_path = img_path + file_name;

        if (g_avatar_cache.ContainsKey(file_full_path) && g_avatar_cache[file_full_path] != null)
        {
            g_avatar_cache[file_full_path].time = Time.time;
            call_back.Call(g_avatar_cache[file_full_path].sprite);
            return;
        }

        // 不要尝试从本地读取了，因为这样不能及时更新别人的头像，但是每次都拉也不合理，有待改进
        //if (File.Exists(file_full_path))
        //{
        //    StartCoroutine(LoadTexture(file_full_path, call_back));
        //}
        //else
        //{
        //    StartCoroutine(Downloadcro(file_full_path, url, call_back, no_sec_www));
        //}
        // 改成无论何时都直接去下载，保持更新
        StartCoroutine(Downloadcro(file_full_path, url, call_back, no_sec_www));
    }

    // 这个只用于自己的头像
    public void DownloadAvatarNoCache(string file_name, string url, LuaFunction call_back, bool no_sec_www)
    {
        string file_full_path = img_path + file_name;

        // 不要尝试从本地读取了，因为这样不能及时更新别人的头像，但是每次都拉也不合理，有待改进
        if (File.Exists(file_full_path))
        {
            StartCoroutine(LoadTexture(file_full_path, call_back));
        }
        else
        {
            StartCoroutine(Downloadcro(file_full_path, url, call_back, no_sec_www));
        }
        //// 改成无论何时都直接去下载，保持更新
        //StartCoroutine(Downloadcro(file_full_path, url, call_back, no_sec_www));
    }

    public void ClearCache()
    {
        foreach(var pair in g_avatar_cache)
        {
            Destroy(pair.Value.sprite);
        }
        g_avatar_cache.Clear();
    }

    public bool IsAvatarExsit(string file_name)
    {
        string file_full_path = img_path + file_name;
        return File.Exists(file_full_path);
    }

    // 使用缓存的头像
    public void CheckAvatar(string file_name, LuaFunction call_back)
    {
        string file_full_path = img_path + file_name;

        StartCoroutine(LoadTexture(file_full_path, call_back));
    }

    void SaveToAvatarCache(string path, Sprite sprite)
    {
        // cache大小，设定100个吧, 超过了就把最古老的删除
        string key = null;
        while (g_avatar_cache.Count > 100)
        {
            float min_time = Time.time + 9999;
            foreach (var data in g_avatar_cache)
            {
                if (data.Value.time < min_time)
                {
                    min_time = data.Value.time;
                    key = data.Key;
                }
            }
            if (key != null)
            {
                g_avatar_cache.Remove(key);
            }
            else
            {
                g_avatar_cache.Clear();
            }
        }

        g_avatar_cache[path] = new AvatarData();
        g_avatar_cache[path].time = Time.time;
        g_avatar_cache[path].sprite = sprite;

    }

    IEnumerator LoadTexture(string file_full_path, LuaFunction call_back)
    {
        //注解1
        string path = "file://" + file_full_path;
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            path = "file:///" + file_full_path;
        }

        WWW www = new WWW(path);
        yield return www;

        if (www.error != null)
        {
            Debug.LogWarning(www.error);
            yield break;
        }

        Texture2D tex = www.texture;
        Sprite ret = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        www.Dispose();

        SaveToAvatarCache(file_full_path, ret);

        call_back.Call(ret);
    }

    // 截屏
    IEnumerator ScreenShot(Texture2D tex, int x, int y, int width, int height, LuaFunction call_back)
    {
        yield return new WaitForEndOfFrame();
        tex.ReadPixels(new Rect(x, y, width, height), 0, 0);
        tex.Apply();
        call_back.Call(tex);
    }

    // 截屏
    public void SaveTex(int x, int y, int width, int height, LuaFunction call_back)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        StartCoroutine(ScreenShot(tex, x, y, width, height, call_back));
    }

    // 将一个tex完全转换成sprite
    public Sprite ChangeTex2DToSprite(Texture2D tex)
    {
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    // quality 1 - 100
    public void WriteTex2DToJPG(Texture2D tex, int quality, string file_name)
    {
        string file_full_path = img_path + file_name;
        File.WriteAllBytes(file_full_path, tex.EncodeToJPG(quality));
    }
}
