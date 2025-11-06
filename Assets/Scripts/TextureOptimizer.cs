using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[System.Serializable]
public class TextureOptimizer : MonoBehaviour
{
    [MenuItem("Tools/Optimize Textures for Mobile")]
    public static void OptimizeAllTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");
        int optimizedCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                bool modified = false;
                
                // Get current Android settings
                TextureImporterPlatformSettings androidSettings = importer.GetPlatformTextureSettings("Android");
                
                // Optimize based on texture type and size
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null)
                {
                    int maxSize = DetermineOptimalTextureSize(texture.width, texture.height);
                    TextureImporterFormat format = DetermineOptimalFormat(importer.textureType, texture);
                    
                    if (androidSettings.maxTextureSize != maxSize || 
                        androidSettings.format != format || 
                        !androidSettings.overridden)
                    {
                        androidSettings.overridden = true;
                        androidSettings.maxTextureSize = maxSize;
                        androidSettings.format = format;
                        androidSettings.compressionQuality = 50; // Balanced quality/size
                        
                        importer.SetPlatformTextureSettings(androidSettings);
                        modified = true;
                    }
                    
                    // Additional optimizations
                    if (importer.mipmapEnabled && texture.width < 256 && texture.height < 256)
                    {
                        importer.mipmapEnabled = false;
                        modified = true;
                    }
                    
                    if (importer.isReadable)
                    {
                        importer.isReadable = false;
                        modified = true;
                    }
                }
                
                if (modified)
                {
                    importer.SaveAndReimport();
                    optimizedCount++;
                }
            }
        }
        
        Debug.Log($"Texture Optimizer: Optimized {optimizedCount} textures for mobile");
        AssetDatabase.Refresh();
    }
    
    private static int DetermineOptimalTextureSize(int width, int height)
    {
        int maxDimension = Mathf.Max(width, height);
        
        // UI textures
        if (maxDimension <= 128) return 128;
        if (maxDimension <= 256) return 256;
        if (maxDimension <= 512) return 512;
        if (maxDimension <= 1024) return 1024;
        
        // Large textures should be reduced for mobile
        return 1024;
    }
    
    private static TextureImporterFormat DetermineOptimalFormat(TextureImporterType type, Texture2D texture)
    {
        bool hasAlpha = texture.format == TextureFormat.RGBA32 || 
                       texture.format == TextureFormat.ARGB32 ||
                       texture.format == TextureFormat.RGBA64;
        
        switch (type)
        {
            case TextureImporterType.Sprite:
            case TextureImporterType.GUI:
                return hasAlpha ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.ETC2_RGB4;
                
            case TextureImporterType.Default:
                return hasAlpha ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.ETC2_RGB4;
                
            case TextureImporterType.NormalMap:
                return TextureImporterFormat.ETC2_RGBA8;
                
            default:
                return hasAlpha ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.ETC2_RGB4;
        }
    }
}
#endif

public class RuntimeTextureOptimizer : MonoBehaviour
{
    [Header("Runtime Texture Settings")]
    [SerializeField] private bool optimizeTexturesAtRuntime = true;
    [SerializeField] private int maxTextureSize = 1024;
    [SerializeField] private bool useStreamingMipmaps = true;
    
    private void Start()
    {
        if (optimizeTexturesAtRuntime)
        {
            OptimizeRuntimeTextures();
        }
    }
    
    private void OptimizeRuntimeTextures()
    {
        // Set global texture streaming settings
        if (useStreamingMipmaps)
        {
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsMemoryBudget = SystemInfo.systemMemorySize > 4000 ? 512 : 256;
        }
        
        // Adjust global texture quality based on device memory
        if (SystemInfo.systemMemorySize < 3000)
        {
            QualitySettings.globalTextureMipmapLimit = 2; // Skip 2 highest mip levels
        }
        else if (SystemInfo.systemMemorySize < 4000)
        {
            QualitySettings.globalTextureMipmapLimit = 1; // Skip 1 highest mip level
        }
        
        Debug.Log($"Runtime Texture Optimizer: Configured for device with {SystemInfo.systemMemorySize}MB RAM");
    }
}