using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToggleSwitch : MonoBehaviour {

    Toggle m_tog;
    Graphic m_img;
    // Use this for initialization
    void hide_bg(bool val)
    {
        Color color = m_img.color;
        color.a = val ? 0 : 255;
        m_img.color = color;
    }

    void Start () {
        m_tog = GetComponent<Toggle>();
        m_img = m_tog.targetGraphic;
        if (m_tog != null)
        {
            m_tog.onValueChanged.AddListener(hide_bg);
            hide_bg(m_tog.isOn);
        }
	}
}
