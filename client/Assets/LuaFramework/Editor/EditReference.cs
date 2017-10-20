// by kingbird
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class EditReference : EditorWindow
{
    [MenuItem("Tools/EditReference")]
    static void OpenEditor()
    {
        //创建窗口
        var window = GetWindow<EditReference>();
        window.Show();
    }

    class WindowData
    {
        public int id;
        public GUIContent content;
        public string path;
        public Rect rect;

        // 依赖关系线条, vector2的 x表示依赖的窗口， y表示依赖的索引
        public Dictionary<int, List<Vector2>> child_ref = new Dictionary<int, List<Vector2>>();
        // 儿子的content
        public List<GUIContent> child_content = new List<GUIContent>();
        // 儿子的名字查询表
        public Dictionary<string, int> child_name = new Dictionary<string, int>();
    }

    // 所有的窗口列表
    List<WindowData> all_windows = new List<WindowData>();
    // 窗口每一列已经使用的长度
    Dictionary<int, float> window_height = new Dictionary<int, float>();
    // 路径到窗口的索引
    public Dictionary<string, int> path_ref = new Dictionary<string, int>();
    // 窗口id的索引
    int window_index = 0;
    // 窗口的最大高度
    float max_height = 0;
    // 选中的资源
    List<Vector2> selected = new List<Vector2>();
    // 处理路径列表
    List<string> show_path_list = new List<string>();
    List<string> sub_path_list = new List<string>();
    List<string> ref_path_list = new List<string>();
    string last_open_path;


    //方案列表
    class Conf
    {
        public string conf_name;
        public List<string> show_path_list = new List<string>();
        public List<string> sub_path_list = new List<string>();
        public List<string> ref_path_list = new List<string>();
    }
    List<Conf> conf_list = new List<Conf>();

    void SaveOne(int index, string conf_name)
    {
        if (conf_list.Count >= index)
        {
            if (conf_list.Count == index)
            {
                conf_list.Add(new Conf());
            }
            conf_list[index].conf_name = conf_name;
            conf_list[index].show_path_list.Clear();
            conf_list[index].show_path_list.AddRange(show_path_list.GetRange(0, show_path_list.Count));
            conf_list[index].sub_path_list.Clear();
            conf_list[index].sub_path_list.AddRange(sub_path_list.GetRange(0, sub_path_list.Count));
            conf_list[index].ref_path_list.Clear();
            conf_list[index].ref_path_list.AddRange(ref_path_list.GetRange(0, ref_path_list.Count));
        }
    }

    void LoadOne(int index)
    {
        if (conf_list.Count > index)
        {
            show_path_list.Clear();
            show_path_list.AddRange(conf_list[index].show_path_list.GetRange(0, conf_list[index].show_path_list.Count));
            sub_path_list.Clear();
            sub_path_list.AddRange(conf_list[index].sub_path_list.GetRange(0, conf_list[index].sub_path_list.Count));
            ref_path_list.Clear();
            ref_path_list.AddRange(conf_list[index].ref_path_list.GetRange(0, conf_list[index].ref_path_list.Count));
        }
    }

    void SaveToFile(string path)
    {
        StringBuilder sb = new StringBuilder();
        foreach ( Conf conf in conf_list)
        {
            sb.AppendLine("name|" + conf.conf_name);
            foreach(string val in conf.show_path_list)
            {
                sb.AppendLine("show|" + val);
            }
            foreach (string val in conf.sub_path_list)
            {
                sb.AppendLine("sub|" + val);
            }
            foreach (string val in conf.ref_path_list)
            {
                sb.AppendLine("ref|" + val);
            }
        }
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    void LoadFromFile(string path)
    {
        string[] files = File.ReadAllLines(path);
        conf_list.Clear();
        Conf cur_conf = null;
        foreach (string val in files)
        {
            string[] temp = val.Split('|');
            if (temp[0] == "name")
            {
                cur_conf = new Conf();
                cur_conf.conf_name = temp[1];
                conf_list.Add(cur_conf);
            }
            else if (temp[0] == "show")
            {
                cur_conf.show_path_list.Add(temp[1]);
            }
            else if (temp[0] == "sub")
            {
                cur_conf.sub_path_list.Add(temp[1]);
            }
            else if (temp[0] == "ref")
            {
                cur_conf.ref_path_list.Add(temp[1]);
            }
        }
    }

    public void Awake()
    {
        last_open_path = Application.dataPath;
    }

    public void ReCaculate()
    {
        all_windows.Clear();
        window_height.Clear();
        path_ref.Clear();
        window_index = 0;
        max_height = 0;
        selected.Clear();
        highlight_id = -1;
        only_show_ref_lines = false;
        should_highlight_circle = false;
        foreach (string path in show_path_list)
        {
            BuildPathWindowData(path, 0, true);
        }
        // 调整依赖窗口的位置
        AdjustWindowPos();
    }

    GUIContent GetGUIContent(string path)
    {
        Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
        if (asset == null)
        {
            return null;  // meta 文件等无法加载之物
        }
        return new GUIContent(SelfName(asset.name), AssetDatabase.GetCachedIcon(path));
    }

    string FullPath(string asset_path)
    {
        return Application.dataPath + "/" + asset_path.Substring(7);
    }

    string AdjustPath(string path)
    {
        return path.Replace("\\", "/").ToLower();
    }

    string AssetsPath(string full_path)
    {
        full_path = AdjustPath(full_path);
        if (!full_path.Contains("assets/"))
        {
            return "";
        }
        return full_path.Substring(full_path.IndexOf("assets/"));
    }

    string ParentPath(string asset_path)
    {
        string full_path = Directory.GetParent(asset_path).FullName;
        return AssetsPath(full_path);
    }

    string SelfName(string any_file)
    {
        any_file = AdjustPath(any_file);
        any_file = any_file.Substring(any_file.LastIndexOf("/") + 1);
        if (any_file.Contains("."))
        {
            any_file = any_file.Substring(0, any_file.LastIndexOf("."));
        }
        return any_file;
    }

    // 处理被引用的文件，如果其父窗口不存在，还要创建父亲
    Vector2 BuildRef(string path, int index)
    {
        string parent_path = ParentPath(path);
        string self_path = SelfName(path);
        BuildPathWindowData(parent_path, index);
        int id = path_ref[parent_path];
        WindowData data = all_windows[id];
        if (!data.child_name.ContainsKey(self_path))
        {
            // 找不到？？ 有时候shader文件名和import名不一致，会出现这个情况
            return new Vector2(id, -1);
        }
        return new Vector2(id, data.child_name[self_path]);
    }

    Rect GetRect(int index, int count)
    {
        if (!window_height.ContainsKey(index))
        {
            window_height[index] = 0;
        }
        float height = count * 22 + 25;
        Rect rect = new Rect(300 * index + 300, window_height[index] + 20, 160, height);
        window_height[index] = window_height[index] + height + 20;

        if (window_height[index] > max_height)
        {
            max_height = window_height[index];
        }
        return rect;
    }

    // 递归创建窗口数据, index 表示递归的层数，影响展示
    void BuildPathWindowData(string path, int index, bool is_first = false)
    {
        if (!Directory.Exists(path)) return;
        // 这个路径已经被创建过，则返回
        if (path_ref.ContainsKey(path)) return;
        
        WindowData data = new WindowData();
        data.id = window_index++;
        path_ref[path] = data.id;
        data.path = path;
        data.content = GetGUIContent(path);
        data.content.text = path.Substring(7);

        // 处理自己的绘制数据
        int child_index = 0;
        List<string> file_list = new List<string>();
        foreach (string file in Directory.GetFiles(path))
        {
            GUIContent content = GetGUIContent(AdjustPath(file));
            if (content == null) continue;
            file_list.Add(file);
            data.child_name[content.text] = child_index++;
            data.child_content.Add(content);
        }
        data.rect = GetRect(index, child_index + 1);
        all_windows.Add(data);

        // 处理儿子的引用
        child_index = 0;
        foreach (string file in file_list)
        {
            string[] refs = AssetDatabase.GetDependencies(file);
            if (refs.Length > 0)
            {
                data.child_ref[child_index] = new List<Vector2>();
            }
            foreach (string ref_path in refs)
            {
                // 如果依赖跟自己同一个目录， 则不管
                if (ParentPath(ref_path).Equals(path) || ref_path.EndsWith(".cs")) continue;
                data.child_ref[child_index].Add(BuildRef(AdjustPath(ref_path), index + 1));
            }
            child_index++;
        }

        if (is_first)
        {
            // 然后处理所有子目录
            foreach (string file in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
            {
                Debug.Log(file);
                string sub_path = AdjustPath(file);
                bool is_sub = false;
                // 排除路径不考虑
                foreach (string sub_str in sub_path_list)
                {
                    if (sub_path.StartsWith(sub_str))
                    {
                        is_sub = true;
                        break;
                    }
                }
                if (!is_sub)
                {
                    BuildPathWindowData(sub_path, index);
                }
            }
        }
    }

    void AdjustWindowPos()
    {
        int max_column = 0;
        foreach (var pair in window_height)
        {
            if (pair.Key > max_column)
            {
                max_column = pair.Key;
            }
        }

        foreach ( string ref_path in ref_path_list)
        {
            max_column++;
            foreach (WindowData data in all_windows)
            {
                if (data.path.StartsWith(ref_path))
                {
                    data.rect = GetRect(max_column, data.child_content.Count + 1);
                }
            }
        }
    }

    Texture2D tex;
    void DrawNodeCurve(Vector3 startPos, Vector3 endPos, Color color, float width)
    {
        // Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
        // Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        if (tex == null)
        {
            tex = new Texture2D(2, 2);
            tex.SetPixels(new Color[] { new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 0.5f), new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 0.5f) });
            tex.Apply();
        }

        Handles.DrawBezier(startPos, endPos, startTan, endTan, color, tex, width);
    }

    // 有选择就只画到选择项
    bool only_show_ref_lines = false;
    void DrawAllLines()
    {
        foreach (WindowData data in all_windows)
        {
            Rect self_rect = data.rect;
            float self_x = self_rect.x + self_rect.width;
            int from_ref_index = -1;
            for (int i = 0; i < ref_path_list.Count; i++)
            {
                if (data.path.StartsWith(ref_path_list[i])) from_ref_index = i;
            }

            foreach (var ref_list in data.child_ref)
            {
                float self_index = ref_list.Key + 1;
                float self_y = self_rect.y + self_index * 22 + 30;
                foreach (Vector2 ref_pair in ref_list.Value)
                {
                    WindowData to_data = all_windows[(int)ref_pair.x];
                    Rect to_rect = to_data.rect;
                    float to_index = ref_pair.y + 1;
                    if (selected.Count == 0)
                    {
                        if (only_show_ref_lines)
                        {
                            int to_ref_index = -1;
                            for (int i = 0; i < ref_path_list.Count; i++)
                            {
                                if (to_data.path.StartsWith(ref_path_list[i])) to_ref_index = i;
                            }
                            if (from_ref_index < 0 && to_ref_index >= 0)
                            {
                                DrawNodeCurve(new Vector3(self_x, self_y, 0),
                                    new Vector3(to_rect.x, to_rect.y + to_index * 22 + 30, 0), Color.red, 2);
                            }
                            else if (from_ref_index < to_ref_index)
                            {
                                DrawNodeCurve(new Vector3(self_x, self_y, 0),
                                    new Vector3(to_rect.x, to_rect.y + to_index * 22 + 30, 0), Color.red, 2);
                            }
                        }
                        else
                        {
                            DrawNodeCurve(new Vector3(self_x, self_y, 0),
                                new Vector3(to_rect.x, to_rect.y + to_index * 22 + 30, 0), Color.green, 1);
                        }
                    }
                    else
                    {
                        foreach (Vector2 pos in selected)
                        {
                            if (pos.x == ref_pair.x && pos.y == ref_pair.y)
                            {
                                DrawNodeCurve(new Vector3(self_x, self_y, 0),
                                        new Vector3(to_rect.x, to_rect.y + to_index * 22 + 30, 0), Color.red, 2);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    // 画选择项的依赖
    void HighLightRef()
    {
        foreach (Vector2 select in selected)
        {
            if (select.x < 0 || select.x > all_windows.Count - 1)
            {
                continue;
            }
            WindowData data = all_windows[(int)select.x];
            Rect self_rect = data.rect;
            var ref_list = data.child_ref[(int)select.y];

            foreach (Vector2 ref_pair in ref_list)
            {
                Rect to_rect = all_windows[(int)ref_pair.x].rect;
                float to_index = ref_pair.y + 1;

                DrawNodeCurve(new Vector3(self_rect.x + self_rect.width, self_rect.y + (select.y + 1) * 22 + 30, 0),
                    new Vector3(to_rect.x, to_rect.y + to_index * 22 + 30, 0), Color.red, 2);
            }
        }
    }

    bool should_highlight_circle = false;
    bool has_circle_found = false;
    List<Vector2> dfs_stack = new List<Vector2>();
    void DrawOneLine(Vector2 from, Vector2 to)
    {
        WindowData data = all_windows[(int)from.x];
        Rect self_rect = data.rect;
        Rect to_rect = all_windows[(int)to.x].rect;
        float to_index = to.y + 1;

        DrawNodeCurve(new Vector3(self_rect.x + self_rect.width, self_rect.y + (from.y + 1) * 22 + 30, 0),
                new Vector3(to_rect.x, to_rect.y + to_index * 22 + 30, 0), Color.red, 2);
    }
    void dfs(Vector2 pos)
    {
        if (dfs_stack.Count > 2 && pos.x == dfs_stack[0].x)
        {
            for (int i = 0; i < dfs_stack.Count; i += 2)
            {
                DrawOneLine(dfs_stack[i], dfs_stack[i+1]);
            }
            has_circle_found = true;
            return;
        }
        // 如果出现循环，直接返回
        if (dfs_stack.Count > 2)
        {
            foreach (Vector2 val in dfs_stack)
            {
                if (pos.x == val.x)
                {
                    return;
                }
            }
        }
        WindowData data = all_windows[(int)pos.x];
        foreach (var ref_list in data.child_ref)
        {
            dfs_stack.Add(new Vector2(pos.x, ref_list.Key));
            foreach (Vector2 next in ref_list.Value)
            {
                dfs_stack.Add(next);
                dfs(next);
                dfs_stack.RemoveAt(dfs_stack.Count - 1);
            }
            dfs_stack.RemoveAt(dfs_stack.Count - 1);
        }
    }

    bool HighlightCircle()
    {
        if (!should_highlight_circle)
        {
            return false;
        }
        has_circle_found = false;
        // 反正窗口不多，直接以每个窗口为根节点dfs
        foreach (WindowData data in all_windows)
        {
            dfs_stack.Clear();
            dfs(new Vector2(data.id, -1));
        }
        return has_circle_found;
    }

    int highlight_id = -1;
    void DrawNodeWindow(int id)
    {
        GUIStyle style = "Label";
        GUIStyle style_path = new GUIStyle();
        style_path.normal.textColor = Color.yellow;
        if (id == highlight_id)
        {
            style_path.normal.textColor = Color.red;
        }
        WindowData data = all_windows[id];
        int child_index = 0;
        GUILayout.Label(data.content, style_path, GUILayout.Height(20));
        foreach (var content in data.child_content)
        {
            bool select_pos = false;
            foreach (Vector2 pos in selected)
            {
                if (id == pos.x && child_index == pos.y)
                {
                    select_pos = true;
                    if (GUILayout.Button(content, GUILayout.Height(20)))
                    {
                        selected.Remove(pos);
                        break;
                    }
                }
            }

            if (!select_pos)
            {
                if (GUILayout.Button(content, style, GUILayout.Height(20)))
                {
                    selected.Add(new Vector2(id, child_index));
                }
            }
            child_index++;
        }
        GUI.DragWindow();
    }

    Vector2 scroll_pos = Vector2.zero;
    void Search(string str)
    {
        bool found = false;
        foreach (WindowData data in all_windows)
        {
            if (data.path.Contains(str) && data.id > highlight_id)
            {
                scroll_pos = new Vector2(data.rect.x, data.rect.y - 100);
                highlight_id = data.id;
                found = true;
                break;
            }
        }
        if (!found)
        {
            highlight_id = -1;
            scroll_pos = new Vector2(0, 0);
        }
    }

    string text_serach = "";
    bool is_adding_conf = false;
    string add_conf_name = "方案";
    //绘制窗口时调用
    void OnGUI()
    {
        EditorGUIUtility.SetIconSize(Vector2.one * 16);
        // DrawNodeCurve(window1, window2);
        scroll_pos = GUILayout.BeginScrollView(scroll_pos);
        GUILayout.Label("", GUILayout.Width(window_height.Count * 300 + 800));
        GUILayout.Space(scroll_pos.y);
        GUILayout.BeginVertical(GUILayout.Width(200), GUILayout.Height(max_height));
        // 需要显示的目录
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("增加显示目录"))
            {
                last_open_path = EditorUtility.OpenFolderPanel("选择文件夹", last_open_path, "");
                show_path_list.Add(AssetsPath(last_open_path));
            }
            if (GUILayout.Button("删除"))
            {
                if (show_path_list.Count > 0)
                {
                    show_path_list.RemoveAt(show_path_list.Count - 1);
                }
            }
        }
        foreach ( string path in show_path_list)
        {
            GUILayout.Label(path);
        }
        GUILayout.Space(40);

        // 在需要显示的目录中，排除的目录
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("增加排除子目录"))
            {
                last_open_path = EditorUtility.OpenFolderPanel("选择文件夹", last_open_path, "");
                sub_path_list.Add(AssetsPath(last_open_path));
            }
            if (GUILayout.Button("删除"))
            {
                if (sub_path_list.Count > 0)
                {
                    sub_path_list.RemoveAt(sub_path_list.Count - 1);
                }
            }
        }
        foreach (string path in sub_path_list)
        {
            GUILayout.Label(path);
        }
        GUILayout.Space(40);

        // 增加依赖关系序列
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("增加逐级依赖"))
            {
                last_open_path = EditorUtility.OpenFolderPanel("选择文件夹", last_open_path, "");
                ref_path_list.Add(AssetsPath(last_open_path));
            }
            if (GUILayout.Button("删除"))
            {
                if (ref_path_list.Count > 0)
                {
                    ref_path_list.RemoveAt(ref_path_list.Count - 1);
                }
            }
        }
        GUILayout.Label("依赖范围从小到大");
        foreach (string path in ref_path_list)
        {
            GUILayout.Label(path);
        }
        if (GUILayout.Button("只显示错误依赖"))
        {
            only_show_ref_lines = !only_show_ref_lines;
        }

        GUILayout.Space(100);
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("保存方案"))
            {
                SaveToFile(EditorUtility.SaveFilePanel("保存方案", Application.dataPath, "", "refconf"));
            }
            if (GUILayout.Button("载入方案"))
            {
                LoadFromFile(EditorUtility.OpenFilePanel("载入方案", Application.dataPath, "refconf"));
            }
            if (GUILayout.Button("新增"))
            {
                if (!is_adding_conf)
                {
                    is_adding_conf = true;
                }
            }
        }
        int conf_index = 0;
        int remove_index = -1;
        foreach(Conf conf in conf_list)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(conf.conf_name, GUILayout.Width(120));
                if (GUILayout.Button("应用"))
                {
                    LoadOne(conf_index);
                }
                if (GUILayout.Button("删除"))
                {
                    remove_index = conf_index;
                }
            }
            conf_index++;
        }
        if (remove_index >= 0)
        {
            conf_list.RemoveAt(remove_index);
            conf_index--;
        }
        if (is_adding_conf)
        {
            using (new GUILayout.HorizontalScope())
            {
                add_conf_name = GUILayout.TextField(add_conf_name, GUILayout.Width(120));
                if (GUILayout.Button("保存"))
                {
                    SaveOne(conf_index, add_conf_name);
                    is_adding_conf = false;
                }
                if (GUILayout.Button("取消"))
                {
                    is_adding_conf = false;
                }
            }
        }

        GUILayout.Space(40);
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("搜索目录", GUILayout.Width(80)))
            {
                Search(text_serach);
            }
            text_serach = GUILayout.TextField(text_serach);
        }
        if (GUILayout.Button("显示"))
        {
            ReCaculate();
        }
        if (GUILayout.Button("高亮循环引用"))
        {
            should_highlight_circle = !should_highlight_circle;
        }
        GUILayout.EndVertical();
        BeginWindows();
        foreach ( WindowData window_data in all_windows)
        {
            window_data.rect = GUILayout.Window(window_data.id, window_data.rect, DrawNodeWindow, "");
        }
        EndWindows();

        if (!HighlightCircle())
        {
            HighLightRef();
            DrawAllLines();
        }
        GUILayout.EndScrollView();
    }
}
