using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ButtonHandler : EventTrigger
{
    public delegate void VoidDelegate(Vector2 point);

    public VoidDelegate onDown;
    public VoidDelegate onMove;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;

    static public ButtonHandler Get(GameObject go)
    {
        ButtonHandler listener = go.GetComponent<ButtonHandler>();
        if (listener == null) listener = go.AddComponent<ButtonHandler>();
        return listener;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (onMove != null) onMove.Invoke(eventData.position);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown.Invoke(eventData.position);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter.Invoke(eventData.position);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit.Invoke(eventData.position);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp.Invoke(eventData.position);
    }

    //把事件透下去
    public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function)
        where T : IEventSystemHandler
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        GameObject current = data.pointerCurrentRaycast.gameObject;
        for (int i = 0; i < results.Count; i++)
        {
            if (current != results[i].gameObject)
            {
                ExecuteEvents.Execute(results[i].gameObject, data, function);
                break;
            }
        }
    }
}