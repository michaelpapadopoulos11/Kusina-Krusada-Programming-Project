using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MobilePerformanceManager : MonoBehaviour
{
    [Header("Performance Settings")]
    [SerializeField] private bool enableDynamicQuality = true;
    [SerializeField] private float targetFrameRate = 30f;
    [SerializeField] private int frameCheckInterval = 60; // Check every 60 frames
    [SerializeField] private float qualityAdjustmentThreshold = 5f; // FPS threshold for adjustments
    
    [Header("Quality Levels")]
    [SerializeField] private int highEndQualityLevel = 2; // Medium
    [SerializeField] private int midRangeQualityLevel = 1; // Low
    [SerializeField] private int lowEndQualityLevel = 0;  // Very Low
    
    private float frameTimer;
    private int frameCount;
    private float currentFPS;
    private int currentQualityLevel;
    
    private void Start()
    {
        InitializeMobileOptimizations();
        DetermineInitialQualityLevel();
    }
    
    private void InitializeMobileOptimizations()
    {
        // Set target frame rate based on device capability
        int targetFPS = DetermineTargetFrameRate();
        Application.targetFrameRate = targetFPS;
        
        // Enable GPU skinning for better performance on modern devices
        QualitySettings.skinWeights = SystemInfo.systemMemorySize > 3000 ? SkinWeights.TwoBones : SkinWeights.OneBone;
        
        // Optimize texture streaming
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsMemoryBudget = SystemInfo.systemMemorySize > 4000 ? 512 : 256;
        
        // Disable VSync for mobile to reduce input lag
        QualitySettings.vSyncCount = 0;
        
        // Optimize shadow settings
        if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urpAsset)
        {
            // Additional URP optimizations can be set here if needed
        }
        
        Debug.Log($"Mobile Performance Manager: Initialized for device with {SystemInfo.systemMemorySize}MB RAM, {SystemInfo.processorCount} cores");
    }
    
    private int DetermineTargetFrameRate()
    {
        // Determine target frame rate based on device specs
        if (SystemInfo.systemMemorySize >= 6000 && SystemInfo.processorCount >= 8)
        {
            return 60; // High-end devices
        }
        else if (SystemInfo.systemMemorySize >= 4000 && SystemInfo.processorCount >= 6)
        {
            return 45; // Mid-range devices
        }
        else
        {
            return 30; // Low-end devices
        }
    }
    
    private void DetermineInitialQualityLevel()
    {
        int qualityLevel;
        
        // Auto-detect quality level based on device specs
        if (IsHighEndDevice())
        {
            qualityLevel = highEndQualityLevel;
        }
        else if (IsMidRangeDevice())
        {
            qualityLevel = midRangeQualityLevel;
        }
        else
        {
            qualityLevel = lowEndQualityLevel;
        }
        
        QualitySettings.SetQualityLevel(qualityLevel, true);
        currentQualityLevel = qualityLevel;
        
        Debug.Log($"Mobile Performance Manager: Set initial quality level to {QualitySettings.names[qualityLevel]}");
    }
    
    private bool IsHighEndDevice()
    {
        return SystemInfo.systemMemorySize >= 6000 && 
               SystemInfo.processorCount >= 8 && 
               SystemInfo.graphicsMemorySize >= 2000;
    }
    
    private bool IsMidRangeDevice()
    {
        return SystemInfo.systemMemorySize >= 4000 && 
               SystemInfo.processorCount >= 6 && 
               SystemInfo.graphicsMemorySize >= 1000;
    }
    
    private void Update()
    {
        if (enableDynamicQuality)
        {
            MonitorPerformance();
        }
    }
    
    private void MonitorPerformance()
    {
        frameTimer += Time.unscaledDeltaTime;
        frameCount++;
        
        if (frameCount >= frameCheckInterval)
        {
            currentFPS = frameCount / frameTimer;
            
            // Adjust quality based on performance
            if (currentFPS < targetFrameRate - qualityAdjustmentThreshold && currentQualityLevel > 0)
            {
                // Performance is poor, reduce quality
                currentQualityLevel--;
                QualitySettings.SetQualityLevel(currentQualityLevel, true);
                Debug.Log($"Performance Manager: Reduced quality to {QualitySettings.names[currentQualityLevel]} (FPS: {currentFPS:F1})");
            }
            else if (currentFPS > targetFrameRate + qualityAdjustmentThreshold && currentQualityLevel < QualitySettings.names.Length - 1)
            {
                // Performance is good, try increasing quality
                currentQualityLevel++;
                QualitySettings.SetQualityLevel(currentQualityLevel, true);
                Debug.Log($"Performance Manager: Increased quality to {QualitySettings.names[currentQualityLevel]} (FPS: {currentFPS:F1})");
            }
            
            // Reset counters
            frameTimer = 0f;
            frameCount = 0;
        }
    }
    
    public void ForceQualityLevel(int level)
    {
        if (level >= 0 && level < QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(level, true);
            currentQualityLevel = level;
            Debug.Log($"Forced quality level to {QualitySettings.names[level]}");
        }
    }
    
    public void OptimizeForBattery()
    {
        // Reduce performance for battery life
        Application.targetFrameRate = 30;
        QualitySettings.SetQualityLevel(lowEndQualityLevel, true);
        currentQualityLevel = lowEndQualityLevel;
        Debug.Log("Optimized for battery life");
    }
    
    public void OptimizeForPerformance()
    {
        // Maximize performance
        Application.targetFrameRate = 60;
        DetermineInitialQualityLevel();
        Debug.Log("Optimized for performance");
    }
    
    private void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            GUI.Label(new Rect(10, 10, 200, 20), $"FPS: {currentFPS:F1}");
            GUI.Label(new Rect(10, 30, 200, 20), $"Quality: {QualitySettings.names[currentQualityLevel]}");
            GUI.Label(new Rect(10, 50, 200, 20), $"Memory: {SystemInfo.systemMemorySize}MB");
        }
    }
}