using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using LuaFramework;

public class AnimationOptimize
{
    [MenuItem("Optimize/Animation/CompressCurve")]
    public static void CompressCurve()
    {
        Object obj = Selection.activeObject;
        HandleScaleCurve(obj);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Optimize/Animation/CompressPrecision")]
    public static void CompressPrecision()
    {
        Object obj = Selection.activeObject;
        HandlePrecision(obj);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Optimize/Animation/CompressPrecisionALL")]
    public static void CompressPrecisionAll()
    {
        string texDir = AppDataPath + "/" + AppConst.TexturePath;
        string aniDir = AppDataPath + "/" + AppConst.OtherResourcePath + "animation";
        List<string> files = new List<string>();
        files.AddRange(Directory.GetFiles(texDir, "*.anim", SearchOption.AllDirectories));
        files.AddRange(Directory.GetFiles(aniDir, "*.anim", SearchOption.AllDirectories));
        for (int i = 0; i < files.Count; i++)
        {
            string path = files[i].Replace(AppDataPath, "Assets");
            HandlePrecisionOne(AssetDatabase.LoadAssetAtPath<Object>(path));
        }
        AssetDatabase.SaveAssets();
    }

    static void HandleScaleCurve(Object obj)
    {
        // for skeleton animations.
        List<AnimationClip> animationClipList = new List<AnimationClip>(AnimationUtility.GetAnimationClips(obj as GameObject));
        if (animationClipList.Count == 0)
        {
            AnimationClip[] objectList = UnityEngine.Object.FindObjectsOfType(typeof(AnimationClip)) as AnimationClip[];
            animationClipList.AddRange(objectList);
        }

        foreach (AnimationClip theAnimation in animationClipList)
        {
            try
            {
                //去除scale曲线
                foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(theAnimation))
                {
                    string name = theCurveBinding.propertyName.ToLower();
                    if (name.Contains("scale"))
                    {
                        AnimationUtility.SetEditorCurve(theAnimation, theCurveBinding, null);
                    }
                }
                Debug.Log(string.Format("CompressAnimationClip {0}", theAnimation.name));
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("CompressAnimationClip Failed !!! error: {0}", e));
            }
        }
    }

    static void HandlePrecision(Object obj)
    {
        // for skeleton animations.
        List<AnimationClip> animationClipList = new List<AnimationClip>(AnimationUtility.GetAnimationClips(obj as GameObject));
        if (animationClipList.Count == 0)
        {
            AnimationClip[] objectList = UnityEngine.Object.FindObjectsOfType(typeof(AnimationClip)) as AnimationClip[];
            animationClipList.AddRange(objectList);
        }

        foreach (AnimationClip theAnimation in animationClipList)
        {
            HandlePrecisionOne(theAnimation);
        }
    }

    static void HandlePrecisionOne(Object obj)
    {
        try
        {
            AnimationClip theAnimation = obj as AnimationClip;
            //浮点数精度压缩到f3
            AnimationClipCurveData[] curves = null;
            curves = AnimationUtility.GetAllCurves(theAnimation);
            Keyframe key;
            Keyframe[] keyFrames;
            for (int ii = 0; ii < curves.Length; ++ii)
            {
                AnimationClipCurveData curveDate = curves[ii];
                if (curveDate.curve == null || curveDate.curve.keys == null)
                {
                    //Debug.LogWarning(string.Format("AnimationClipCurveData {0} don't have curve; Animation name {1} ", curveDate, animationPath));
                    continue;
                }
                keyFrames = curveDate.curve.keys;
                for (int i = 0; i < keyFrames.Length; i++)
                {
                    key = keyFrames[i];
                    key.value = float.Parse(key.value.ToString("f3"));
                    key.inTangent = float.Parse(key.inTangent.ToString("f3"));
                    key.outTangent = float.Parse(key.outTangent.ToString("f3"));
                    keyFrames[i] = key;
                }
                curveDate.curve.keys = keyFrames;
                theAnimation.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curveDate.curve);
            }
            Debug.Log(string.Format("CompressAnimationClip {0}", theAnimation.name));
        }
        catch (System.Exception e)
        {
            Debug.LogError(string.Format("CompressAnimationClip Failed !!! error: {0}", e));
        }
    }

    static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }
}