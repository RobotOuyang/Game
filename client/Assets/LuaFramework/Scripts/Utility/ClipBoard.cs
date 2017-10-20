using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using LuaInterface;
using System.IO;
using LuaFramework;
using LitJson;
using System.Collections;
using System.Collections.Generic;

public class ClipBoard : MonoBehaviour
{
#if UNITY_IPHONE
	[DllImport ("__Internal")]
    private static extern void _copyTextToClipboard(string text);
#endif

    public static void CopyToClipboard( string input)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    jo.Call("TextToClipboard", input);
                }
            }
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IPHONE
            _copyTextToClipboard(input);
#endif
        }
    }
}
