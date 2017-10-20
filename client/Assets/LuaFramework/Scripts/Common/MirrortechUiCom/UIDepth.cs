using UnityEngine;
using UnityEngine.UI;

public class UIDepth : MonoBehaviour
{
    public int order = 0;
    public static string layername = "Default";
    int old_order = 0;

    bool is_root = false;
    Canvas canvas;
    GraphicRaycaster raycaster;

    void SetOrder()
    {
        canvas.sortingLayerName = layername;
        canvas.overrideSorting = true;
        canvas.sortingOrder = order;

        // 这里因为unity update遍历顺序是先父亲后儿子，所以儿子总能覆盖
        Renderer[] renders = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer render in renders)
        {
            render.sortingLayerName = layername;
            // 更新的时候的话，render的layer要做相对变化
            render.sortingOrder = order + render.sortingOrder - old_order;
        }
        // 非根节点不驱动儿子节点里的UIdepth， 也不驱动粒子
        if (is_root)
        {
            UIDepth[] uidepths = GetComponentsInChildren<UIDepth>(true);
            foreach (UIDepth depth in uidepths)
            {
                if (depth == this) continue;
                depth.order = order + depth.order - old_order;
            }
        }
        old_order = order;
    }

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    void Start()
    {
        SetOrder();
    }

    void Update()
    {
        if (old_order != order)
        {
            SetOrder();
        }
    }

    // 用这个添加的一定是根Uidepth节点
    public static void AddDepth(GameObject go, int order)
    {
        UIDepth depth = go.GetComponent<UIDepth>();
        if (depth == null)
        {
            depth = go.AddComponent<UIDepth>();
        }
        depth.order = order;
        depth.is_root = true;
    }

    public static void SetOrderToParent(Transform trans)
    {
        Transform parent = trans.parent;
        while (parent != null)
        {
            UIDepth depth = parent.GetComponent<UIDepth>();
            if (depth != null)
            {
                UIDepth my_depth = trans.GetComponent<UIDepth>();
                if (my_depth != null)
                {
                    my_depth.order = my_depth.order + depth.order;
                }
                else
                {
                    Renderer[] renders = trans.GetComponentsInChildren<Renderer>();
                    foreach (Renderer render in renders)
                    {
                        render.sortingLayerName = UIDepth.layername;
                        // 更新的时候的话，render的layer要做相对变化
                        render.sortingOrder = depth.order + render.sortingOrder;
                    }
                }

                break;
            }
            parent = parent.parent;
        }
    }
}