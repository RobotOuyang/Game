using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using LuaFramework;

public class ABOptimize
{
    [MenuItem("Optimize/AB/ReplaceDefaultUI")]
    public static void ReplaceDefaultUI()
    {
        // 1.提取内置资源
        string defaultUIPath = AppDataPath + "/Textures/DefaultUI";
        if (!Directory.Exists(defaultUIPath)) 
            Directory.CreateDirectory(defaultUIPath);

        string prefabDir = AppDataPath + "/" + AppConst.PrefabPath;
        string texDir = AppDataPath + "/" + AppConst.TexturePath;
        string otherDir = AppDataPath + "/" + AppConst.OtherResourcePath;
        Dictionary<string, FileInfo> idDict = new Dictionary<string, FileInfo>();
        Dictionary<string, ABAsset> dict = new Dictionary<string, ABAsset>();
        Object[] UnityAssets = AssetDatabase.LoadAllAssetsAtPath("Resources/unity_builtin_extra");
        foreach (var asset in UnityAssets)
        {
            if (!dict.ContainsKey(asset.name))
                dict.Add(asset.name, new ABAsset());

            if (asset.GetType() == typeof(Texture2D))
            {
                dict[asset.name].tex = asset as Texture2D;
            }
            else if (asset.GetType() == typeof(Sprite))
            {
                dict[asset.name].sprite = asset as Sprite;
                FileInfo fi = new FileInfo();
                fi.oldFileId = GetFileID(asset);
                idDict.Add(asset.name, fi);
            }
            else if (asset.GetType() == typeof(Material))
            {
                dict[asset.name].mat = asset as Material;
                FileInfo fi = new FileInfo();
                fi.oldFileId = GetFileID(asset);
                idDict.Add(asset.name, fi);
            }
            else if (asset.GetType() == typeof(Shader))
            {
                dict[asset.name].shader = asset as Shader;
                FileInfo fi = new FileInfo();
                fi.oldFileId = GetFileID(asset);
                idDict.Add(asset.name, fi);
            }
            Debug.Log(asset.name + "\t\t\t" + asset.GetType() + "\t\t\t" + GetFileID(asset));
        }
        
        // 2.遍历Prefab，然后将内置资源fileID和guid进行替换
        List<string> files = new List<string>();
        files.AddRange(Directory.GetFiles(prefabDir, "*.*", SearchOption.AllDirectories));
        files.AddRange(Directory.GetFiles(texDir, "*.*", SearchOption.AllDirectories));
        files.AddRange(Directory.GetFiles(otherDir, "*.*", SearchOption.AllDirectories));
        for (int i = 0; i < files.Count; i++)
        {
            if (files[i].EndsWith(".meta"))
            {
                continue;
            }

            try
            {
                string content = File.ReadAllText(files[i]);
                int num = 0;
                foreach (var kvp in idDict)
                {
                    string oldStr = string.Format("fileID: {0}, guid: 0000000000000000f000000000000000, type: 0", kvp.Value.oldFileId);
                    if (content.Contains(oldStr))
                    {
                        num++;
                        FileInfo fi = HandleOneUI(dict[kvp.Key]);
                        string newStr = string.Format("fileID: {0}, guid: {1}, type: 2", fi.fileId, fi.guid);
                        content = content.Replace(oldStr, newStr);
                        Debug.Log(string.Format("old ：{0} \n new ：{1}", oldStr, newStr));
                    }
                }
                if (num > 0)
                {
                    Debug.Log("replace ：" + files[i]);
                    File.WriteAllText(files[i], content, System.Text.Encoding.UTF8);
                }
            }
            catch (IOException ex)
            {
                Debug.LogError(ex.Message);
            }
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    static FileInfo HandleOneUI(ABAsset asset)
    {
        FileInfo ret = new FileInfo();
        if (null != asset.tex)
        {
            string newTexName = asset.tex.name + "Texture";
            if (null == AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/DefaultUI/" + newTexName + ".asset"))
            {
                Texture2D newTex = new Texture2D(asset.tex.width, asset.tex.height, asset.tex.format, false);
                newTex.LoadRawTextureData(asset.tex.GetRawTextureData());
                newTex.filterMode = asset.tex.filterMode;
                newTex.anisoLevel = asset.tex.anisoLevel;
                newTex.mipMapBias = asset.tex.mipMapBias;
                newTex.wrapMode = asset.tex.wrapMode;
                newTex.name = newTexName;
                AssetDatabase.CreateAsset(newTex, "Assets/Textures/DefaultUI/" + newTexName + ".asset");
            }

            if (null != asset.sprite)
            {
                string newSpriteName = asset.sprite.name + "Sprite";
                if (null == AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/DefaultUI/" + newSpriteName + ".asset"))
                {
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/DefaultUI/" + newTexName + ".asset");
                    Sprite newSprite = Sprite.Create(tex, asset.sprite.rect, new Vector2(0.5f, 0.5f),
                        asset.sprite.pixelsPerUnit, 1, SpriteMeshType.Tight, asset.sprite.border);
                    newSprite.name = newSpriteName;
                    AssetDatabase.CreateAsset(newSprite, "Assets/Textures/DefaultUI/" + newSpriteName + ".asset");
                }

                string path = string.Format("Assets/Textures/DefaultUI/{0}.asset", newSpriteName);
                ret.fileId = GetFileID(AssetDatabase.LoadAssetAtPath<Sprite>(path));
                ret.guid = AssetDatabase.AssetPathToGUID(path);
            }
        }
        if (null != asset.mat)
        {
            if (null == AssetDatabase.LoadAssetAtPath<Material>("Assets/Textures/DefaultUI/" + asset.mat.name + ".asset"))
                AssetDatabase.CreateAsset(Object.Instantiate<Material>(asset.mat), "Assets/Textures/DefaultUI/" + asset.mat.name + ".asset");

            string path = string.Format("Assets/Textures/DefaultUI/{0}.asset", asset.mat.name);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            string filepath = "Assets/Textures/DefaultUI/" + mat.shader.name + ".asset";
            if (!Directory.Exists(Directory.GetParent(filepath).FullName))
            {
                Directory.CreateDirectory(Directory.GetParent(filepath).FullName);
            }
            if (!Directory.Exists(filepath))
            {
                AssetDatabase.CreateAsset(Object.Instantiate<Shader>(mat.shader), filepath);
            }
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(filepath);
            mat.shader = shader;

            ret.fileId = GetFileID(mat);
            ret.guid = AssetDatabase.AssetPathToGUID(path);
        }
        if (null != asset.shader)
        {
            string filepath = "Assets/Textures/DefaultUI/" + asset.shader.name + ".asset";
            if (null == AssetDatabase.LoadAssetAtPath<Shader>(filepath))
            {

                if (!Directory.Exists(Directory.GetParent(filepath).FullName))
                {
                    Directory.CreateDirectory(Directory.GetParent(filepath).FullName);
                }
                AssetDatabase.CreateAsset(Object.Instantiate<Shader>(asset.shader), filepath);
            }
            
            ret.fileId = GetFileID(AssetDatabase.LoadAssetAtPath<Shader>(filepath));
            ret.guid = AssetDatabase.AssetPathToGUID(filepath);
        }
        return ret;
    }

    public class ABAsset
    {
        public Texture2D tex;
        public Sprite sprite;
        public Material mat;
        public Shader shader;
    }

    public class FileInfo
    {
        public long oldFileId;
        public long fileId;
        public string guid;
    }

    static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }

    static long GetFileID(Object target)
    {
        PropertyInfo inspectorMode = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
        SerializedObject serializedObject = new SerializedObject(target);
        inspectorMode.SetValue(serializedObject, InspectorMode.Debug, null);
        SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");
        return localIdProp.longValue;
    }
}