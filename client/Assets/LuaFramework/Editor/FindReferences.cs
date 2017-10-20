using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class FindReferences
{
    public static List<List<string>> path_list = new List<List<string>>();

    public static int GetStartIndex(string path)
    {
        path = path.Replace("/", "\\");
        for (int index = 0; index < path_list.Count; index++)
        {
            foreach (string file_path in path_list[index])
            {
                if (path.IndexOf(file_path) != -1)
                {
                    return index;
                }
            }
        }
        return path_list.Count;
    }

    static private bool IgnorePath(string path, int start_index)
    {
        for (int index = start_index; index < path_list.Count; index++)
        {
            foreach (string file_path in path_list[index])
            {
                if (path.IndexOf(file_path) != -1)
                {
                    return false;
                }
            }
        }
        return true;
    }

    [MenuItem("Assets/Find References", false, 10)]
    static private void Find()
    {
        path_list.Add(new List<string>() { "Textures\\share", "Textures\\spade_ace", "Prefabs\\SpadeAce", "Prefabs\\textureprefab\\spade_ace" });
        path_list.Add(new List<string>() { "Textures\\massive_battle", "Prefabs\\MassiveBattle", "Prefabs\\textureprefab\\massive_battle" });
        path_list.Add(new List<string>() { "Textures\\golden_shark", "Prefabs\\GoldenShark", "Prefabs\\textureprefab\\golden_shark", "NewGamePanel\\golden_shark_main_panel" });
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!Directory.Exists(path))
        {
            if (!string.IsNullOrEmpty(path))
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset", ".controller", ".anim" };
                string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                    .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
                int startIndex = 0;
                int match_count = 0;
                EditorApplication.update = delegate ()
                {
                    string file = files[startIndex];

                    bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);

                    if (Regex.IsMatch(File.ReadAllText(file), guid))
                    {
                        match_count += 1;
                        Debug.Log(file, AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(file)));
                    }

                    startIndex++;
                    if (isCancel || startIndex >= files.Length)
                    {
                        EditorUtility.ClearProgressBar();
                        EditorApplication.update = null;
                        startIndex = 0;
                        Debug.Log(string.Format("匹配结束, 共找到 {0} 个引用", match_count));
                    }
                };
            }
        }
        else
        {
            int start_index = GetStartIndex(path);
            List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset", ".controller", ".anim", ".fbx" };
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())
                    && IgnorePath(s, start_index)).ToArray();
            DirectoryInfo direction = new DirectoryInfo(path);
            FileInfo[] files_info = direction.GetFiles("*", SearchOption.AllDirectories).Where(s => !s.Name.EndsWith(".meta")).ToArray();
            for (int i = 0; i < files_info.Length; i++)
            {
                string path_name = path + "/" + files_info[i].Name;
                string guid = AssetDatabase.AssetPathToGUID(path_name);
                int startIndex = 0;
                for (startIndex = 0; startIndex < files.Length; startIndex++)
                {
                    string file = files[startIndex];
                    bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);
                    if (Regex.IsMatch(File.ReadAllText(file), guid))
                    {
                        Debug.Log(file.Substring(31) + "引用了" + files_info[i].Name);
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }

    class ResMatch
    {
        public int count;
        public string ref_file;
        public string file;
    }

    [MenuItem("LuaFramework/Find No Ref resource", false)]
    static private void FindAll()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset", ".controller", ".anim", ".fbx" };
        List<string> withExtensions = new List<string>() { ".png", ".jpg", ".mat" };

        string[] allfiles = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
        string[] files =   allfiles.Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        string[] single_fils = allfiles.Where(s => withExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        int startIndex = 0;
        Dictionary<string, ResMatch> result = new Dictionary<string, ResMatch>();

        // 所有被引用的纪录
        EditorApplication.update = delegate ()
        {
            string file = GetRelativeAssetsPath(files[startIndex]);

            bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);
            string[] depends = AssetDatabase.GetDependencies(file);

            foreach (string depend in depends)
            {
                if (depend.Equals(file))
                {
                    continue;
                }
                ResMatch res;
                if (result.TryGetValue(depend, out res))
                {
                    res.count++;
                }
                else
                {
                    res = new ResMatch();
                    res.count = 1;
                    res.ref_file = file;
                    result[depend] = res;
                }
            }

            startIndex++;
            if (isCancel || startIndex >= files.Length)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;

                for (int i = 0; i < single_fils.Length; i++)
                {
                    if (!single_fils[i].Contains("Textures")) continue;

                    string path = GetRelativeAssetsPath(single_fils[i]);
                    ResMatch res;
                    if (result.TryGetValue(path, out res))
                    {
                        res.file = path;
                    }
                    else
                    {
                        res = new ResMatch();
                        res.count = 0;
                        res.file = path;
                        result[path] = res;
                    }
                }

                Debug.Log(string.Format("匹配开始 ====================>"));
                foreach (var val in result)
                {
                    if (val.Value.count <= 0 && !string.IsNullOrEmpty(val.Value.file))
                    {
                        Debug.Log(val.Value.file + "被引用： " + val.Value.ref_file, AssetDatabase.LoadAssetAtPath<Object>(val.Value.file));
                        // File.Delete(val.Value.file);
                    }
                }
            }
        };
    }

    [MenuItem("Assets/Find References", true)]
    static private bool VFind()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return (!string.IsNullOrEmpty(path));
    }

    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }
}