using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using LuaFramework;

public class TextureOptimize
{
    public static int CompressQuality = 50;

    public static float halveRate = 0.5f;

    [MenuItem("Optimize/Texture/HalveAtlas")]
    public static void HalveAtlas()
    {
        HalveSprite();
    }

    [MenuItem("Optimize/Texture/HalveOne")]
    public static void HalveAtla()
    {
        HalveOne(AssetDatabase.GetAssetPath(Selection.activeObject));
    }

    private static void HalveSprite()
    {
        List<string> files = new List<string>();
        files.AddRange(Directory.GetFiles("assets/" + AppConst.TexturePath, "*.*", SearchOption.AllDirectories));
        files.AddRange(Directory.GetFiles("assets/" + AppConst.OtherResourcePath, "*.*", SearchOption.AllDirectories));
        for (int i = 0; i < files.Count; i++)
        {
            string filePath = files[i];
            filePath = filePath.Replace("\\", "/");

            if (filePath.EndsWith(".meta"))
            {
                continue;
            }

            if (filePath.EndsWith(".png") || filePath.EndsWith(".jpg") || filePath.EndsWith(".tga"))
            {
                //筛选出png和jpg图片
                if (!EditorUtility.DisplayCancelableProgressBar("Check TexFormat", filePath, (float)i / (float)files.Count))
                {
                    HalveOne(filePath);
                }
                else
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Cancel", "Progress bar canceled by the user", "Okey");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("Other format texture: " + filePath);
            }
        }
        
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("Success", "Done!", "Okey");
    }

    static void HalveOne(string filePath)
    {
        TextureImporter textureImporter = TextureImporter.GetAtPath(filePath) as TextureImporter;
        if (textureImporter == null)
            return;
        AssetDatabase.ImportAsset(filePath);

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
        int textureSize = Math.Max(texture.height, texture.width);

        if (TextureFormat.RGB24 == texture.format || TextureFormat.RGBA32 == texture.format)
        {
            TextureImporterSettings settings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(settings);

            if (TextureImporterNPOTScale.None == settings.npotScale)
            {
                textureImporter.textureType = TextureImporterType.Default;
                int defaultMaxTextureSize = textureImporter.maxTextureSize;
                defaultMaxTextureSize = Math.Min(textureSize, defaultMaxTextureSize);
                defaultMaxTextureSize = (int)(defaultMaxTextureSize * halveRate);
                //textureImporter.maxTextureSize = GetValidSize(defaultMaxTextureSize);

                int androidMaxTextureSize = 0;
                TextureImporterFormat androidTextureFormat = TextureImporterFormat.ETC_RGB4;
                bool isAndroidOverWrite = textureImporter.GetPlatformTextureSettings("Android", out androidMaxTextureSize, out androidTextureFormat);
                androidMaxTextureSize = Math.Min(textureSize, androidMaxTextureSize);
                androidMaxTextureSize = (int)(androidMaxTextureSize * halveRate);
                TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                platformSettings.overridden = true;
                platformSettings.name = "Android";
                //platformSettings.maxTextureSize = GetValidSize(androidMaxTextureSize);
                platformSettings.format = TextureImporterFormat.ETC_RGB4;
                platformSettings.compressionQuality = CompressQuality;
                platformSettings.allowsAlphaSplitting = TextureFormat.RGBA32 == texture.format;
                textureImporter.SetPlatformTextureSettings(platformSettings);

                int iphoneMaxTextureSize = 0;
                TextureImporterFormat iphoneTextureFormat = TextureImporterFormat.PVRTC_RGBA4;
                bool isIphoneOverWrite = textureImporter.GetPlatformTextureSettings("iPhone", out iphoneMaxTextureSize, out iphoneTextureFormat);
                iphoneMaxTextureSize = Math.Min(textureSize, iphoneMaxTextureSize);
                iphoneMaxTextureSize = (int)(iphoneMaxTextureSize * halveRate);
                TextureImporterPlatformSettings iplatformSettings = new TextureImporterPlatformSettings();
                iplatformSettings.overridden = true;
                iplatformSettings.name = "iPhone";
                //iplatformSettings.maxTextureSize = GetValidSize(iphoneMaxTextureSize);
                iplatformSettings.format = TextureImporterFormat.PVRTC_RGBA4;
                iplatformSettings.compressionQuality = CompressQuality;
                iplatformSettings.allowsAlphaSplitting = false;
                textureImporter.SetPlatformTextureSettings(iplatformSettings);
            }
            else
            {
                textureImporter.textureType = TextureImporterType.Default;
                int defaultMaxTextureSize = textureImporter.maxTextureSize;
                defaultMaxTextureSize = Math.Min(textureSize, defaultMaxTextureSize);
                defaultMaxTextureSize = (int)(defaultMaxTextureSize * halveRate);
                //textureImporter.maxTextureSize = GetValidSize(defaultMaxTextureSize);

                int androidMaxTextureSize = 0;
                TextureImporterFormat androidTextureFormat = TextureImporterFormat.ETC_RGB4;
                bool isAndroidOverWrite = textureImporter.GetPlatformTextureSettings("Android", out androidMaxTextureSize, out androidTextureFormat);
                androidMaxTextureSize = Math.Min(textureSize, androidMaxTextureSize);
                androidMaxTextureSize = (int)(androidMaxTextureSize * halveRate);
                TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                platformSettings.overridden = true;
                platformSettings.name = "Android";
                //platformSettings.maxTextureSize = GetValidSize(androidMaxTextureSize);
                platformSettings.format = TextureFormat.RGBA32 == texture.format ? TextureImporterFormat.RGBA16 : TextureImporterFormat.RGB16;
                platformSettings.compressionQuality = CompressQuality;
                platformSettings.allowsAlphaSplitting = TextureFormat.RGBA32 == texture.format;
                textureImporter.SetPlatformTextureSettings(platformSettings);

                int iphoneMaxTextureSize = 0;
                TextureImporterFormat iphoneTextureFormat = TextureImporterFormat.PVRTC_RGBA4;
                bool isIphoneOverWrite = textureImporter.GetPlatformTextureSettings("iPhone", out iphoneMaxTextureSize, out iphoneTextureFormat);
                iphoneMaxTextureSize = Math.Min(textureSize, iphoneMaxTextureSize);
                iphoneMaxTextureSize = (int)(iphoneMaxTextureSize * halveRate);
                TextureImporterPlatformSettings iplatformSettings = new TextureImporterPlatformSettings();
                iplatformSettings.overridden = true;
                iplatformSettings.name = "iPhone";
                //iplatformSettings.maxTextureSize = GetValidSize(iphoneMaxTextureSize);
                iplatformSettings.format = TextureFormat.RGBA32 == texture.format ? TextureImporterFormat.RGBA16 : TextureImporterFormat.RGB16;
                iplatformSettings.compressionQuality = CompressQuality;
                iplatformSettings.allowsAlphaSplitting = false;
                textureImporter.SetPlatformTextureSettings(iplatformSettings);
            }

            textureImporter.SetTextureSettings(settings);
            AssetDatabase.SaveAssets();
            DoAssetReimport(filePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }
        else
        {
            Debug.LogWarning(string.Format("{0} is {1}", filePath, texture.format));
        }
    }

    private static int GetValidSize(int size)
    {
        int result = 0;
        if (size <= 48)
        {
            result = 32;
        }
        else if (size <= 96)
        {
            result = 64;
        }
        else if (size <= 192)
        {
            result = 128;
        }
        else if (size <= 384)
        {
            result = 256;
        }
        else if (size <= 768)
        {
            result = 512;
        }
        else if (size <= 1536)
        {
            result = 1024;
        }
        else if (size <= 3072)
        {
            result = 2048;
        }

        return result;
    }

    public static void DoAssetReimport(string path, ImportAssetOptions options)
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            AssetDatabase.ImportAsset(path, options);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }
}