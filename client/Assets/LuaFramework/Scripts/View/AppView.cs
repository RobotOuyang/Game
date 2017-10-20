using UnityEngine;
using UnityEngine.UI;
using LuaFramework;
using System.Collections.Generic;

public class AppView : View {

    ///<summary>
    /// 监听的消息
    ///</summary>
    List<string> MessageList {
        get {
            return new List<string>()
            {
                NotiConst.UPDATE_LOADING,
                NotiConst.UPDATE_MESSAGE,
                NotiConst.UPDATE_EXTRACT,
                NotiConst.UPDATE_PROGRESS,
                NotiConst.UPDATE_FINISH,
                NotiConst.UPDATE_TIMEOUT,
            };
        }
    }

    void Awake() {
        RemoveMessage(this, MessageList);
        RegisterMessage(this, MessageList);
    }

    GameObject loading_obj = null;
    Text loading_text = null;
    Slider loading_img = null;
    float loading_size_x = 0; 
    float max_width;

    public void OpenLoadingPanel(GameManager manager)
    {
        Transform Parent = GameObject.FindWithTag("GuiCanvas").transform;
        string name = "loading/LoadingPanel";

        ResManager.LoadPrefab(name, delegate (UnityEngine.Object[] objs)
        {
            if (objs.Length == 0) return;
            // Get the asset.
            GameObject prefab = objs[0] as GameObject;

            int obj_index = name.LastIndexOf('/');
            string obj_name = name;
            if (obj_index > 0)
            {
                obj_name = name.Substring(obj_index + 1, name.Length - obj_index - 1);
            }

            Transform trans = Parent.Find(obj_name);
            GameObject loading_obj = null;
            if (trans != null)
            {
                loading_obj = trans.gameObject;
            }
            // 如果场景里已经有这个ui 就不创建了。 这种应用于场景有个默认ui的情况，防止出现ui空白期
            if (loading_obj == null)
            {
                loading_obj = Instantiate(prefab) as GameObject;
            }

            RectTransform rect = loading_obj.GetComponent<RectTransform>();
            Transform text_obj = loading_obj.transform.Find("background/info");
            if (text_obj != null)
            {
                loading_text = text_obj.GetComponent<Text>();
            }

            loading_obj.name = obj_name;
            loading_obj.layer = LayerMask.NameToLayer("UI");

            rect.SetParent(Parent);
            rect.localScale = Vector3.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localPosition = Vector3.zero;

            Transform img_obj = loading_obj.transform.Find("background/Slider");
            if (img_obj != null)
            {
                loading_img = img_obj.GetComponent<Slider>();

                // 调节下滑动条框大小
                Transform loading_trans = img_obj.Find("Fill Area/Fill/loading");
                if (loading_trans != null)
                {
                    RectTransform loading_rect = loading_trans.GetComponent<RectTransform>();
                    RectTransform slider_rect = img_obj.GetComponent<RectTransform>();
                    if (loading_rect != null && slider_rect != null)
                    {
                        loading_rect.sizeDelta = new Vector2(slider_rect.rect.width, loading_rect.sizeDelta.y);
                    }
                }
            }
        });
    }
    /// <summary>
    /// 处理View消息
    /// </summary>
    /// <param name="message"></param>
    public override void OnMessage(IMessage message) {
        //string name = message.Name;
        //object body = message.Body;
        //switch (name) {
        //    case NotiConst.UPDATE_LOADING:
        //        OpenLoadingPanel(body as GameManager);
        //    break;
        //    case NotiConst.UPDATE_MESSAGE:      //更新消息
        //        UpdateMessage(body as LoadingMessage);
        //    break;
        //    case NotiConst.UPDATE_EXTRACT:      //更新解压
        //        UpdateExtract(body as LoadingMessage);
        //    break;
        //    case NotiConst.UPDATE_PROGRESS:     //更新下载进度
        //        percent = (float)body;
        //    break;
        //    case NotiConst.UPDATE_FINISH:     //更新完成
        //        this.message = body.ToString();
        //        break;
        //    case NotiConst.UPDATE_TIMEOUT:
        //        UpdateProgress(body as LoadingMessage);
        //        break;
        //    default:break;
        //}
    }

    string _message;
    string message
    {
        get { return _message; }
        set { _message = value; }
    }

    bool failed = false;
    void Update()
    {
        if (failed)
        {
            return;
        }

        if (loading_text != null)
        {
            loading_text.text = _message;
        }
        if (loading_img != null)
        {
            loading_img.value = percent;
        }
    }

    float percent
    {
        get; set;
    }

    public void UpdateMessage(LoadingMessage data) {
        this.message = data.message;
        this.percent = data.value;
    }

    public void UpdateExtract(LoadingMessage data) {
        this.message = data.message;
        this.percent = data.value;
    }

    public void UpdateDownload(LoadingMessage data) {
        this.message = data.message;
        this.percent = data.value;
    }

    //因为并行处理的原因， 这个消息后不再更新其他信息 
    public void UpdateProgress(LoadingMessage data) {
        failed = true;
        message = data.message;
        percent = data.value;
        loading_text.text = message;
        loading_img.value = percent;
    }

    //private int update_offset = 100;
    //private float wait_time = -999f;
    //void OnGUI() {
    //    if (wait_time >= 0)
    //    {
    //        wait_time -= Time.deltaTime;
    //    }
    //    if (wait_time < 0 && wait_time > -500f) 
    //    {
    //        return;
    //    }

    //    GUI.Label(new Rect(10, update_offset, 960, 50), message);

    //    GUI.Label(new Rect(10, 0, 500, 50), "(1) 单击 \"Lua/Clear Wrap Files, 等待小菊花转完自动重新导出wrap文件\"。");
    //    GUI.Label(new Rect(10, 20, 500, 50), "(2) 单击 \"LuaFramework/Build protobuf-lua-gen, 生成新的协议\"。");
    //    GUI.Label(new Rect(10, 40, 500, 50), "(3) 单击 \"LuaFramework/Build XXX source, 生成新的asset bundle资源\"。");
    //    GUI.Label(new Rect(10, 60, 900, 50), "(4) 将新生成的StreamingAsset下的asset bundle资源拷贝到http服务器\"。");
    //}
}
