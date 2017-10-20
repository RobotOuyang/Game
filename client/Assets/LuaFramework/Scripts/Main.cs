using UnityEngine;
using System.Text;

namespace LuaFramework {

    public enum BundleMode
    {
        DEFAULT,
        EDITOR_USE_BUNDLE,
    }

    public enum UpdateMode
    {
        MOBILE_UPDATE,
        NOT_UPDATE,
        MOBILE_AND_EDITOR_UPDATE,
    }
    /// <summary>
    /// </summary>
    public class Main : MonoBehaviour {
        public string m_Chanel;

        public BundleMode m_BundleMode = BundleMode.DEFAULT;
        public UpdateMode m_UpdateMode = UpdateMode.MOBILE_UPDATE;
        // 进入游戏后首先显示的玩法
        // public int m_FirstGameIndex = 1;
        // public string m_AppstoreId;
        // public string m_AppstorePayId = "";
        // public string m_LoadOrder = "2|1|3|4";
        // public int m_RemovePortal = 2;
        // public static string m_ConfForLua = "";  // 给lua的用于运营的针对不同包的配置。json做字典
        // 在启动前如果也需要一些常量的修改就放在这里
        void Awake()
        {
            if (!string.IsNullOrEmpty(m_Chanel))
            {
                AppConst.Channel = m_Chanel;
            }

            // StringBuilder sb = new StringBuilder();
            // sb.Append("{");
            // // 在这下面加新的
            // sb.AppendLine("\"visit_order\" : \"" + m_LoadOrder + "\","); //大厅Ui的摆放顺序
            // sb.AppendLine("\"remove_portal\" : \"" + m_RemovePortal + "\","); //是否存在portal,1为不存在
            // sb.AppendLine("\"appstorepayid\" : \"" + m_AppstorePayId + "\",");
            // sb.AppendLine("\"appstoreid\" : \"" + m_AppstoreId + "\",");
            // sb.AppendLine("\"firstgame\" : " + m_FirstGameIndex);
            // sb.Append("}");
            // m_ConfForLua = sb.ToString();

            AppConst.UpdateMode = (m_UpdateMode != UpdateMode.NOT_UPDATE);
            AppConst.EditorUpdate = (m_UpdateMode == UpdateMode.MOBILE_AND_EDITOR_UPDATE);

            AppConst.EidtorNotUseBundle = (Application.isEditor && m_BundleMode == BundleMode.DEFAULT);
        }

        void Start()
        {

            AppFacade.Instance.StartUp();   //启动游戏
        }
    }
}