using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class RectWebView : MonoBehaviour {

    public string Url;
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
    UniWebView webView;
#endif
    RectTransform rect;

    public float canvas_width = 720;
    public float canvas_height = 1280;

    // Use this for initialization
    void Start () {
        rect = GetComponent<RectTransform>();
        rect.pivot = Vector2.one / 2;
    }

    public void LoadUrl(string url)
    {
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
        webView = GetComponent<UniWebView>();
        if (webView == null)
        {
            webView = gameObject.AddComponent<UniWebView>();
        }

        float x_scale = Screen.width / canvas_width;
        float y_scale = Screen.height / canvas_height;
#if UNITY_IOS && !UNITY_EDITOR
        float scale = UniWebViewPlugin.GetUiBoundWidth() / Screen.width;
        x_scale *= scale;
        y_scale *= scale;
#endif
        webView.insets = new UniWebViewEdgeInsets(
            (int)(y_scale * ((canvas_height - rect.rect.height) / 2 - rect.localPosition.y)),
            (int)(x_scale * ((canvas_width - rect.rect.width) / 2 + rect.localPosition.x)),
            (int)(y_scale * ((canvas_height - rect.rect.height) / 2 + rect.localPosition.y)),
            (int)(x_scale * ((canvas_width - rect.rect.width) / 2 - rect.localPosition.x)));

        webView.backButtonEnable = false;
        webView.autoShowWhenLoadComplete = true;
        webView.Load(url);
#endif
    }
}
