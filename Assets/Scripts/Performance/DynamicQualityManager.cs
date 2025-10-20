using UnityEngine;

/// <summary>
/// Manages quality settings dynamically based on performance
/// </summary>
public class DynamicQualityManager : MonoBehaviour
{
    [Header("Performance Monitoring")]
    public float targetFrameRate = 60f;
    public float checkInterval = 2f;
    
    [Header("Quality Adjustment")]
    public bool autoAdjustQuality = true;
    public int minQualityLevel = 0;
    public int maxQualityLevel = 5;
    
    private float frameRateSum = 0f;
    private int frameCount = 0;
    private float lastCheckTime = 0f;
    private int currentQualityLevel;
    
    void Start()
    {
        // Set initial quality level
        currentQualityLevel = QualitySettings.GetQualityLevel();
        
        // Optimize rendering settings for mobile performance
        OptimizeRenderingSettings();
        
        // Set target frame rate
        Application.targetFrameRate = Mathf.RoundToInt(targetFrameRate);
    }
    
    void Update()
    {
        if (!autoAdjustQuality) return;
        
        // Accumulate frame rate data
        frameRateSum += 1f / Time.unscaledDeltaTime;
        frameCount++;
        
        // Check performance periodically
        if (Time.time - lastCheckTime >= checkInterval)
        {
            AdjustQualityBasedOnPerformance();
            lastCheckTime = Time.time;
            frameRateSum = 0f;
            frameCount = 0;
        }
    }
    
    private void AdjustQualityBasedOnPerformance()
    {
        if (frameCount == 0) return;
        
        float averageFrameRate = frameRateSum / frameCount;
        
        // If performance is poor, decrease quality
        if (averageFrameRate < targetFrameRate * 0.8f && currentQualityLevel > minQualityLevel)
        {
            currentQualityLevel--;
            QualitySettings.SetQualityLevel(currentQualityLevel, true);
            Debug.Log($"Performance optimization: Quality decreased to level {currentQualityLevel}");
        }
        // If performance is good, try increasing quality
        else if (averageFrameRate > targetFrameRate * 1.1f && currentQualityLevel < maxQualityLevel)
        {
            currentQualityLevel++;
            QualitySettings.SetQualityLevel(currentQualityLevel, true);
            Debug.Log($"Performance optimization: Quality increased to level {currentQualityLevel}");
        }
    }
    
    private void OptimizeRenderingSettings()
    {
        // Optimize rendering for better performance
        QualitySettings.vSyncCount = 0; // Disable VSync for consistent frame timing
        QualitySettings.antiAliasing = 0; // Disable anti-aliasing on mobile
        
        // Optimize shadow settings
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.shadowDistance = 50f; // Reduce shadow distance
        
        // Optimize texture settings
        QualitySettings.globalTextureMipmapLimit = 1; // Use half-resolution textures
        
        // Optimize particle settings
        QualitySettings.particleRaycastBudget = 64; // Reduce particle collision budget
        
        // Optimize LOD settings
        QualitySettings.lodBias = 0.7f; // Use lower LOD levels sooner
        QualitySettings.maximumLODLevel = 1; // Skip highest LOD level
        
        Debug.Log("Rendering settings optimized for performance");
    }
    
    /// <summary>
    /// Force a specific quality level
    /// </summary>
    public void SetQualityLevel(int level)
    {
        currentQualityLevel = Mathf.Clamp(level, minQualityLevel, maxQualityLevel);
        QualitySettings.SetQualityLevel(currentQualityLevel, true);
    }
    
    /// <summary>
    /// Get current performance metrics
    /// </summary>
    public float GetCurrentFrameRate()
    {
        return frameCount > 0 ? frameRateSum / frameCount : 0f;
    }
}