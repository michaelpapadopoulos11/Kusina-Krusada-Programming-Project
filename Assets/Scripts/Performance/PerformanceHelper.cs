using UnityEngine;

/// <summary>
/// Static helper class for performance optimizations across the game
/// </summary>
public static class PerformanceHelper
{
    // Cache for time-related calculations to avoid repeated Time.deltaTime access
    private static float cachedDeltaTime;
    private static int lastFrameCount = -1;
    
    /// <summary>
    /// Get cached deltaTime - only calculates once per frame
    /// </summary>
    public static float CachedDeltaTime
    {
        get
        {
            if (Time.frameCount != lastFrameCount)
            {
                cachedDeltaTime = Time.deltaTime;
                lastFrameCount = Time.frameCount;
            }
            return cachedDeltaTime;
        }
    }
    
    /// <summary>
    /// Optimized string concatenation for score display
    /// </summary>
    private static readonly System.Text.StringBuilder scoreStringBuilder = new System.Text.StringBuilder(16);
    
    public static string FormatScore(int score)
    {
        scoreStringBuilder.Clear();
        scoreStringBuilder.Append(score);
        return scoreStringBuilder.ToString();
    }
    
    /// <summary>
    /// Check if game is running at acceptable framerate
    /// </summary>
    public static bool IsPerformanceGood()
    {
        return Application.targetFrameRate <= 0 || Time.smoothDeltaTime < (1.0f / 30.0f);
    }
    
    /// <summary>
    /// Set quality settings based on performance
    /// </summary>
    public static void OptimizeQualitySettings()
    {
        if (!IsPerformanceGood())
        {
            // Reduce quality if performance is poor
            QualitySettings.DecreaseLevel();
            Debug.Log("Performance optimization: Quality level decreased");
        }
    }
}