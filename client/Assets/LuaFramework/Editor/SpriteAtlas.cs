using UnityEngine;
using UnityEditor;
using System.IO;
using LuaFramework;

public class Post : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        string streamDir = "assets/" + AppConst.TexturePath;
        TextureImporter textureImporter = assetImporter as TextureImporter;
        textureImporter.mipmapEnabled = false;
        // 如果是图集模式，需要为每个图片生成一个预制体，便于动态创建， 否则直接拷贝
        if (AppConst.AtlasMode)
        {
            string AtlasName = assetPath.ToLower().Replace(streamDir, string.Empty);

            if (AtlasName.LastIndexOf("/") < 0)
            {
                // 直接在根目录下
                AtlasName = "_textures";
            }
            else
            {
                AtlasName = AtlasName.Substring(0, AtlasName.LastIndexOf("/")).Replace("/", "_");
            }
            textureImporter.spritePackingTag = AtlasName;
        }
        else
        {
            textureImporter.spritePackingTag = null;
        }
        // 压缩格式
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.ClearPlatformTextureSettings("iPhone");
        if (true)
        {
            textureImporter.SetAllowsAlphaSplitting(false);
            textureImporter.ClearPlatformTextureSettings("Android");
        }
        else
        {
            if (textureImporter.textureType == TextureImporterType.Sprite)
            {
                //注意，使用etc1格式打包，需要在edit/project settings/Graphics 里包含etc专用的shader
                textureImporter.SetPlatformTextureSettings("Android", textureImporter.maxTextureSize, TextureImporterFormat.ETC_RGB4, 50, true);
                textureImporter.SetAllowsAlphaSplitting(true);
            }
            else
            {
                textureImporter.ClearPlatformTextureSettings("Android");
            }
        }
    }
}