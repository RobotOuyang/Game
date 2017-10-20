using UnityEngine;
using System.Collections;

public class ShowRectVal : MonoBehaviour {

    RectTransform rect;

    public Vector2 offsetMin;
    public Vector2 offsetMax;
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    public Vector3 localPosition;
    public Vector3 position;
    public Vector2 pivot;
    public Vector2 size;
    public Rect rect_size;

    public Vector2 screen_size;

    // Use this for initialization
    void Start () {
        rect = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
        offsetMin = rect.offsetMin;
        offsetMax = rect.offsetMax;
        localPosition = rect.localPosition;
        position = rect.position;
        pivot = rect.pivot;
        size = rect.sizeDelta;
        anchorMin = rect.anchorMin;
        anchorMax = rect.anchorMax;
        rect_size = rect.rect;
        screen_size = new Vector2(Screen.width, Screen.height);
    }
}
