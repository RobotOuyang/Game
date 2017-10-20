using UnityEngine;
using UnityEngine.UI;
using LuaFramework;
using System.Collections;

[System.Serializable]
public struct PageGroupData
{
    public Button btn;
    public GameObject page;
}

public class PageView : MonoBehaviour {

    [SerializeField]
    public PageGroupData[] m_Pages;
    public int m_index = 0;

    void AddDelegate(int index)
    {
        Button btn = m_Pages[index].btn;
        bool is_pressing = false;
        ButtonHandler listener = ButtonHandler.Get(btn.gameObject);
        AudioSource audio = btn.GetComponent<AudioSource>();
        if (audio == null)
        {
            audio = btn.gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false;
        }
        audio.reverbZoneMix = 0;
        audio.spatialBlend = 0.5f;

        listener.onDown += (delegate (Vector2 point_event)
        {
            if (!btn.IsInteractable())
            {
                return;
            }
            is_pressing = true;
        });
        listener.onUp += (delegate (Vector2 point_event)
        {
            if (is_pressing)
            {
                Util.CallMethod("SoundManager", "PlayButtonSound", btn, audio);
                StartCoroutine(SetIndexDelay(index));
            }
            is_pressing = false;
        });
        listener.onExit += (delegate (Vector2 point_event)
        {
            is_pressing = false;
        });
    }

    IEnumerator SetIndexDelay(int index)
    {
        yield return null;
        SetIndex(index);
    }

    // Use this for initialization
    void Start ()
    {
        for (int i = 0; i < m_Pages.Length; i++)
        {
            //m_Pages[i].btn.onClick.AddListener(delegate ()
            //{
            //    SetIndex(index);
            //});
            // 不能像上面那样直接加的原因是如果直接访问upvalue里的i，导致多个delegate指向的其实是一个变量，而封装函数后，就变成访问局部变量了。
            AddDelegate(i);
        }

        SetIndex(m_index);
    }

    public int Index
    {
        get { return m_index; }
    }

    public void SetIndex(int index)
    {
        if (index >= m_Pages.Length)
        {
            index = m_Pages.Length - 1;
        }
        if (index < 0)
        {
            index = 0;
        }
        m_index = index;

        for (int i = 0; i < m_Pages.Length; i++)
        {
            m_Pages[i].btn.interactable = (index != i);
            m_Pages[i].page.SetActive(index == i);
        }
    }
}
