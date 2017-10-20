using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(RectTransform))]
public class AnchorToSelf : DecoratorEditor
{
    public AnchorToSelf() : base("RectTransformEditor") { }
    RectTransform m_rect;

    void OnEnable()
    {
        m_rect = target as RectTransform;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("锚到自己四个角") && m_rect.parent)
        {
            RectTransform parent = m_rect.parent.GetComponent<RectTransform>();
            Vector2 parent_size = parent.rect.size;
            Vector2 parent_pivot = parent.pivot;

            Vector2 self_size = m_rect.rect.size;
            Vector2 self_pos = m_rect.localPosition;
            Vector2 self_pivot = m_rect.pivot;

            // 计算四个角落到左下角的各轴距离
            float left = parent_size.x * parent_pivot.x + self_pos.x - self_pivot.x * self_size.x;
            float right = left + self_size.x;
            float bottom = parent_size.y * parent_pivot.y + self_pos.y - self_pivot.y * self_size.y;
            float top = bottom + self_size.y;

            m_rect.anchorMin = new Vector2(left / parent_size.x, bottom / parent_size.y);
            m_rect.anchorMax = new Vector2(right / parent_size.x, top / parent_size.y);

            m_rect.offsetMin = Vector2.zero;
            m_rect.offsetMax = Vector2.zero;
        }
    }
}