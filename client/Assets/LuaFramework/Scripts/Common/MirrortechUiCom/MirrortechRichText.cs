using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LuaInterface;

enum RichType
{
    Text,
    Rect,
}

class RichElementData
{
    public RichType type;
    public string content;
    public bool with_line = false;
    public Color color = Color.white;
    public int font_size = 14;
    public RectTransform rect;
    public Vector2 original_size;   // 原始大小，防止出现累积误差

    public LuaFunction call_back = null;

    // 如果是rect需要callback，可以创建好了按钮再传进来
    public RichElementData(RectTransform _rect)
    {
        type = RichType.Rect;
        rect = _rect;
        original_size = new Vector2(rect.rect.width, rect.rect.height);
    }

    public RichElementData(string val)
    {
        type = RichType.Text;
        content = val;
    }
}

//从左到右添加，默认换行, 原点默认为左上角
public class MirrortechRichText : MonoBehaviour
{
    public RectTransform Rect
    {
        get; set;
    }
    public float Width { get { return m_width; } }

    public bool mVertical = true;

    public int MaxElement { get; set; }

    float m_width;
    float m_height = 0;
    List<RichElementData> m_elementList;
    static GameObject text_prefab;
    static Font m_font;
    public float max_width = 0;

    public static void Init()
    {
        LuaFramework.LuaHelper.GetResManager().LoadPrefab("others_common/text_for_rich", delegate (Object[] objs)
        {
            text_prefab = objs[0] as GameObject;
            m_font = text_prefab.GetComponent<Text>().font;
        });
    }

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
        if (mVertical)
        {
            m_width = Rect.rect.width;
        }
        else
        {
            m_height = Rect.rect.height;
        }

        m_elementList = new List<RichElementData>();
    }

    // 暂时只实现垂直的
    public static MirrortechRichText Create(float width)
    {
        
        GameObject obj = new GameObject();
        obj.AddComponent<RectTransform>().sizeDelta = new Vector2(width, 0);

        MirrortechRichText text = obj.AddComponent<MirrortechRichText>();

        text.mVertical = true;
        return text;
    }

    public void AddRect(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            m_elementList.Add(new RichElementData(rect));
        }
        else
        {
            Debug.Log("rect上的RectTransform 为空");
        }
    }

    public void AddRect(RectTransform rect)
    {
        m_elementList.Add(new RichElementData(rect));
    }

    public void AddText(string text, Color color, int size, bool with_line, LuaFunction func)
    {
        RichElementData data = new RichElementData(text);
        data.color = color;
        data.font_size = size;
        data.with_line = with_line;
        data.call_back = func;
        m_elementList.Add(data);
    }

    public void AddText(string text, Color color, int size)
    {
        AddText(text, color, size, false, null);
    }

    public void AddText(string text, Color color, int size, bool with_line)
    {
        AddText(text, color, size, with_line, null);
    }

    public void AddText(string text, int size)
    {
        AddText(text, Color.white, size, false, null);
    }

    public float GetMaxWidth()
    {
        return max_width;
    }

    float cur_line_height = 0;
    List<float> line_heights = new List<float>();
    List<List<RectTransform>> childs = new List<List<RectTransform>>();
    List<RectTransform> cur_child = new List<RectTransform>();

    Vector3 next_put_pos = Vector3.zero;

    void ChangeLine()
    {
        next_put_pos.x = 0;
        next_put_pos.y -= cur_line_height;
        m_height += cur_line_height;
        line_heights.Add(cur_line_height);
        cur_line_height = 0;
        childs.Add(new List<RectTransform>(cur_child));
        cur_child.Clear();
    }

    void DrawRect(RectTransform rect)
    {
        if ( rect == null )
        {
            return;
        }
        Vector2 content_size = new Vector2(rect.rect.width, rect.rect.height);

        //看是不是要换行
        if (mVertical && next_put_pos.x + content_size.x > m_width)
        {
            ChangeLine();
        }

        rect.SetParent(Rect);
        rect.localScale = Vector3.one;
        rect.pivot = Vector2.up;
        rect.anchorMin = Vector2.up;
        rect.anchorMax = Vector2.up;
        rect.localPosition = next_put_pos;
        rect.sizeDelta = content_size;  // 在第二次调整的时候不设这个会导致大小变0

        next_put_pos.x += content_size.x;
        if (next_put_pos.x > max_width)
        {
            max_width = next_put_pos.x;
        }
            
        if (content_size.y > cur_line_height)
        {
            cur_line_height = content_size.y;
        }
        cur_child.Add(rect);
    }

    RectTransform GenTextRect(RichElementData data, Vector2 size, int begin_index, int length)
    {
        if (length <=0 )
        {
            return null;
        }

        GameObject obj = GameObject.Instantiate(text_prefab) as GameObject;
        Text text = obj.GetComponent<Text>();
        text.fontSize = data.font_size;
        text.color = data.color;
        text.text = data.content.Substring(begin_index, length);
        float text_width = text.preferredWidth;

        RectTransform res = obj.GetComponent<RectTransform>();
        res.sizeDelta = size;

        if (data.with_line)
        {
            GameObject line = GameObject.Instantiate(text_prefab) as GameObject;
            Text under_line = line.GetComponent<Text>();
            under_line.color = data.color;
            under_line.fontSize = data.font_size;
            under_line.text = "_";

            float width = under_line.preferredWidth;
            int under_count = (int)(text_width / width);

            // 用下划线填充
            string total_under = new string('_', under_count);
            under_line.text = total_under;

            RectTransform trans = under_line.GetComponent<RectTransform>();
            trans.SetParent(res);
            trans.localScale = Vector3.one;
            trans.anchorMin = Vector2.zero;
            trans.anchorMax = Vector2.one;
            trans.sizeDelta = Vector2.zero;
        }

        if (data.call_back != null)
        {
            obj.AddComponent<Button>().onClick.AddListener(
                delegate ()
                {
                    data.call_back.Call();
                }
            );
        }

        return res;
    }

    const float _edge = 8f;
    public void DrawOrRefresh()
    {
        cur_line_height = 0;
        line_heights.Clear();
        next_put_pos = Vector3.zero;
        m_height = 0;
        childs.Clear();
        cur_child.Clear();
        List<RichElementData> new_m_elementList = new List<RichElementData>();  // 理论上来说，字符一旦初始化好了，后面也不会变化，字符会以rect形式保存到这个表里,防止不停的new

        if (MaxElement > 0 && m_elementList.Count > MaxElement)
        {
            // 删除那些超出范围的儿子
            for (int i = 0; i < m_elementList.Count - MaxElement; i++)
            {
                m_elementList[i].rect.SetParent(null);
            }
            m_elementList.RemoveRange(0, m_elementList.Count - MaxElement);
        }

        // 先创建左上对齐的。
        foreach (RichElementData data in m_elementList)
        {
            if (data.type == RichType.Rect)
            {
                data.rect.anchorMin = Vector2.up;
                data.rect.anchorMax = Vector2.up;
                data.rect.sizeDelta = data.original_size;   // 恢复成原来大小，防止每次刷新引入累积误差
                DrawRect(data.rect);
                new_m_elementList.Add(data);
            }
            else if (data.type == RichType.Text)
            {
                CharacterInfo characterInfo;
                m_font.RequestCharactersInTexture(data.content, data.font_size, FontStyle.Normal);
                float width = 0;
                int last_begin = 0;
                for (int i = 0; i < data.content.Length; i++)
                {
                    m_font.GetCharacterInfo(data.content[i], out characterInfo, data.font_size);
                    width += characterInfo.advance;
                    if (mVertical && width + next_put_pos.x > m_width)
                    {
                        // 如果一个都没渲染就超了，那就要换行
                        if (i > last_begin)
                        {
                            RectTransform left_rect = GenTextRect(data, new Vector2(width - characterInfo.advance, characterInfo.size + _edge) , last_begin, i - last_begin);
                            new_m_elementList.Add(new RichElementData(left_rect));
                            DrawRect(left_rect);
                            // 手动换行，因为后面的判断需要next_put_pos.x。
                            ChangeLine();
                        }
                        //考虑下一行
                        width = characterInfo.advance;
                        last_begin = i;
                    }
                    if (i == data.content.Length - 1 && i >= last_begin)
                    {
                        // 最后一行
                        RectTransform left_rect = GenTextRect(data, new Vector2(width, characterInfo.size + _edge), last_begin, i - last_begin + 1);
                        new_m_elementList.Add(new RichElementData(left_rect));
                        DrawRect(left_rect);
                    }
                }
            }
        }
        // 最后一行, 就算是只有一行也要做这个，才能保存
        if (cur_line_height > 0)
        {
            ChangeLine();
        }
        m_elementList = new_m_elementList;

        AdjustPos();
    }

    void AdjustPos()
    {
        if (mVertical)
        {
            Rect.sizeDelta = new Vector2(Rect.sizeDelta.x,  m_height);
        }
        else
        {
            Rect.sizeDelta = new Vector2(m_width, Rect.sizeDelta.y);
        }
        float total_height = 0;
        float cur_height;
        //调整为居中
        for (int i = 0; i < line_heights.Count; i++)
        {
            List<RectTransform> rect_list = childs[i];
            float max_height = line_heights[i];
            cur_height = total_height + max_height / 2;
            total_height += max_height;

            float cur_width = 0;
            foreach(RectTransform rect in rect_list)
            {
                // width 决定锚点，锚点决定 width，这样就会产生累积误差
                Vector2 content_size = new Vector2(rect.rect.width, rect.rect.height);

                rect.pivot = new Vector2(0, 0.5f);
                // 描点定在四个角落，以跟着父亲等比缩放
                rect.anchorMin = new Vector2(cur_width / m_width, (m_height - cur_height - content_size.y / 2) / m_height);  // 左下
                cur_width += content_size.x;
                rect.anchorMax = new Vector2(cur_width / m_width, (m_height - cur_height + content_size.y / 2) / m_height); // 右上
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }
        }
    }
}
