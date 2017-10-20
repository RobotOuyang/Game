using UnityEngine;
using UnityEngine.UI;
using LuaFramework;

public class ClosePopupOnBg : MonoBehaviour {

    Button button;
    bool is_enable = true;

	// Use this for initialization
	void Start () {
        if (!name.EndsWith("Panel"))
        {
            Debug.LogWarning("ClosePopupOnBg 只能添加在panel上");
            return;
        }

        string ctrl_name = name.Replace("Panel", "Ctrl");

        button = gameObject.GetComponent<Button>();

        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        button.onClick.AddListener(delegate ()
        {
            if (is_enable)
            {
                Util.CallMethod(ctrl_name, "Close");
            }
        });
	}

    public void SetEnable(bool val)
    {
        is_enable = val;
    }
}
