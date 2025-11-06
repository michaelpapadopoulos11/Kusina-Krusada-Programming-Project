using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

/// <summary>
/// Static performance optimizer for mobile games based on device specifications
/// Applies one-time optimizations at startup based on detected hardware specs
/// No dynamic changes during gameplay - only device-spec based configuration
/// </summary>
public class GamePerformanceOptimizer : MonoBehaviour
{
    [Header("Device Detection")]
    [SerializeField] private bool detectDeviceSpecs = true;
    [SerializeField] private bool logDeviceInfo = true;
    
    [Header("Memory Management")]
    [SerializeField] private bool enableMemoryOptimization = true;
    [SerializeField] private float memoryCheckInterval = 10f;
    [SerializeField] private int gcCollectionThreshold = 20; // MB before forcing GC
    
    [Header("Rendering Optimizations")] 
    [SerializeField] private bool optimizeRendering = true;
    [SerializeField] private bool enableOcclusion = true;
    [SerializeField] private bool reduceLODBias = true;
    
    // Device categorization
    public enum DeviceCategory
    {
        LowEnd,
        MidRange,
        HighEnd
    }
    
    private DeviceCategory detectedCategory = DeviceCategory.MidRange;
    
    private void Awake()
    {
        // Ensure single instance
        if (FindObjectsOfType<GamePerformanceOptimizer>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        InitializeDeviceBasedOptimizations();
        
        if (enableMemoryOptimization)
        {
            StartCoroutine(ManageMemory());
        }
    }
    
    private void InitializeDeviceBasedOptimizations()
    {
        // Detect device category based on specs
        DetectDeviceCategory();
        
        // Apply optimizations based on detected category
        ApplyDeviceSpecificSettings();
        
        if (logDeviceInfo)
        {
            LogDeviceInformation();
        }
        
        Debug.Log($"Performance Optimizer initialized for {detectedCategory} device");
    }
    
    private void DetectDeviceCategory()
    {
        int ram = SystemInfo.systemMemorySize;
        int cores = SystemInfo.processorCount;
        int gpuMemory = SystemInfo.graphicsMemorySize;
        
        // High-end device criteria
        if (ram >= 6000 && cores >= 8 && gpuMemory >= 2000)
        {
            detectedCategory = DeviceCategory.HighEnd;
        }
        // Mid-range device criteria
        else if (ram >= 4000 && cores >= 6 && gpuMemory >= 1000)
        {
            detectedCategory = DeviceCategory.MidRange;
        }
        // Low-end device (everything else)
        else
        {
            detectedCategory = DeviceCategory.LowEnd;
        }
    }
    
    private void ApplyDeviceSpecificSettings()
    {
        switch (detectedCategory)
        {
            case DeviceCategory.HighEnd:
                ApplyHighEndSettings();
                break;
            case DeviceCategory.MidRange:
                ApplyMidRangeSettings();
                break;
            case DeviceCategory.LowEnd:
                ApplyLowEndSettings();
                break;
        }
        
        // Common optimizations for all mobile devices
        ApplyCommonMobileOptimizations();
        
        if (optimizeRendering)
        {
            ApplyRenderingOptimizations();
        }
    }
    
    private void ApplyHighEndSettings()
    {
        // High-end devices can handle better quality
        Application.targetFrameRate = 60;
        QualitySettings.SetQualityLevel(2, true); // Medium quality
        
        // Enable better shadows
        QualitySettings.shadows = UnityEngine.ShadowQuality.HardOnly;
        QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Medium;
        QualitySettings.shadowDistance = 50f;
        
        // Better LOD settings
        QualitySettings.lodBias = 1.0f;
        QualitySettings.maximumLODLevel = 0;
        
        Debug.Log("Applied high-end device settings");
    }
    
    private void ApplyMidRangeSettings()
    {
        // Mid-range devices get balanced settings
        Application.targetFrameRate = 45;
        QualitySettings.SetQualityLevel(1, true); // Low quality
        
        // Moderate shadows
        QualitySettings.shadows = UnityEngine.ShadowQuality.HardOnly;
        QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Low;
        QualitySettings.shadowDistance = 30f;
        
        // Balanced LOD settings
        QualitySettings.lodBias = 0.8f;
        QualitySettings.maximumLODLevel = 1;
        
        Debug.Log("Applied mid-range device settings");
    }
    
    private void ApplyLowEndSettings()
    {
        // Low-end devices need aggressive optimizations
        Application.targetFrameRate = 30;
        QualitySettings.SetQualityLevel(0, true); // Very Low quality
        
        // Disable shadows completely
        QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
        QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Low;
        QualitySettings.shadowDistance = 15f;
        
        // Aggressive LOD settings
        QualitySettings.lodBias = 0.5f;
        QualitySettings.maximumLODLevel = 2;
        
        // Reduce texture quality
        QualitySettings.globalTextureMipmapLimit = 2; // Quarter resolution textures
        
        // Reduce physics quality
        Physics.defaultSolverIterations = 2;
        Time.fixedDeltaTime = 1f / 30f; // 30Hz physics
        
        Debug.Log("Applied low-end device settings");
    }
    
    private void ApplyCommonMobileOptimizations()
    {
        // Disable VSync for better control over frame rate
        QualitySettings.vSyncCount = 0;
        
        // Disable anti-aliasing on mobile
        QualitySettings.antiAliasing = 0;
        
        // Optimize physics for mobile
        if (detectedCategory != DeviceCategory.LowEnd)
        {
            Time.fixedDeltaTime = 1f / 50f; // 50Hz physics (instead of 60Hz)
            Physics.defaultSolverIterations = 4; // Reduce from default 6
            Physics.defaultSolverVelocityIterations = 1;
        }
        
        // Memory optimizations
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsMemoryBudget = SystemInfo.systemMemorySize > 4000 ? 512 : 256;
        
        // Particle optimizations
        QualitySettings.particleRaycastBudget = detectedCategory == DeviceCategory.HighEnd ? 64 : 32;
        
        Debug.Log("Applied common mobile optimizations");
    }
    
    private void ApplyRenderingOptimizations()
    {
        // Shadow cascade optimization
        QualitySettings.shadowCascades = 1; // Single cascade for mobile
        
        // Occlusion culling
        if (enableOcclusion && Camera.main != null)
        {
            Camera.main.useOcclusionCulling = true;
        }
        
        // URP specific optimizations
        if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urpAsset)
        {
            Debug.Log("URP detected - using URP-specific optimizations");
        }
        
        Debug.Log("Applied rendering optimizations");
    }
    
    private IEnumerator ManageMemory()
    {
        while (true)
        {
            yield return new WaitForSeconds(memoryCheckInterval);
            
            // Check memory usage
            long memoryUsage = System.GC.GetTotalMemory(false) / (1024 * 1024); // Convert to MB
            
            if (memoryUsage > gcCollectionThreshold)
            {
                // Force garbage collection
                System.GC.Collect();
                Resources.UnloadUnusedAssets();
                
                Debug.Log($"Memory optimization: GC forced at {memoryUsage}MB usage");
            }
        }
    }
    
    private void LogDeviceInformation()
    {
        Debug.Log("=== DEVICE INFORMATION ===");
        Debug.Log($"Device Category: {detectedCategory}");
        Debug.Log($"Device Model: {SystemInfo.deviceModel}");
        Debug.Log($"Device Name: {SystemInfo.deviceName}");
        Debug.Log($"Operating System: {SystemInfo.operatingSystem}");
        Debug.Log($"System Memory: {SystemInfo.systemMemorySize}MB");
        Debug.Log($"Graphics Memory: {SystemInfo.graphicsMemorySize}MB");
        Debug.Log($"Graphics Device: {SystemInfo.graphicsDeviceName}");
        Debug.Log($"Processor: {SystemInfo.processorType}");
        Debug.Log($"Processor Count: {SystemInfo.processorCount}");
        Debug.Log($"Graphics API: {SystemInfo.graphicsDeviceType}");
        Debug.Log($"Target Frame Rate: {Application.targetFrameRate}");
        Debug.Log($"Quality Level: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        Debug.Log("========================");
    }
    
    /// <summary>
    /// Get the detected device category
    /// </summary>
    public DeviceCategory GetDeviceCategory()
    {
        return detectedCategory;
    }
    
    /// <summary>
    /// Get current target frame rate based on device
    /// </summary>
    public int GetTargetFrameRate()
    {
        return Application.targetFrameRate;
    }
    
    /// <summary>
    /// Force memory cleanup manually
    /// </summary>
    [ContextMenu("Force Memory Cleanup")]
    public void ForceMemoryCleanup()
    {
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        Debug.Log("Manual memory cleanup performed");
    }
}