using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using ShadowResolution = UnityEngine.ShadowResolution;
using ShadowQuality = UnityEngine.ShadowQuality;

/// <summary>
/// Specialized manager for maintaining consistent 60 FPS performance
/// Uses adaptive quality scaling and frame pacing to ensure smooth gameplay
/// </summary>
public class ConsistentFPSManager : MonoBehaviour
{
    [Header("Target Performance")]
    [SerializeField] private int targetFPS = 60;
    [SerializeField] private float fpsTolerance = 5f; // Acceptable FPS variance
    [SerializeField] private bool maintainQualityWhenPossible = true;
    
    [Header("Frame Pacing")]
    [SerializeField] private bool useFixedTimeStep = true;
    [SerializeField] private bool optimizePhysicsStep = true;
    
    [Header("Quality Adaptation")]
    [SerializeField] private bool enableAdaptiveQuality = true;
    [SerializeField] private float qualityAdjustInterval = 2f;
    [SerializeField] private int minQualityLevel = 0;
    [SerializeField] private int maxQualityLevel = 3; // Cap at Medium to maintain performance
    
    [Header("Memory Management")]
    [SerializeField] private bool aggressiveMemoryManagement = true;
    [SerializeField] private float memoryCleanupInterval = 15f;
    [SerializeField] private int memoryThresholdMB = 400;
    
    [Header("Debug")]
    [SerializeField] private bool showFPSDisplay = true;
    [SerializeField] private bool logPerformanceChanges = true;
    
    // Performance tracking
    private float[] frameTimeHistory;
    private int frameTimeIndex = 0;
    private const int FRAME_HISTORY_SIZE = 60;
    private float averageFPS = 60f;
    private float lastQualityAdjustTime = 0f;
    
    // Current settings
    private int currentQualityLevel;
    
    // UI Components
    private GUIStyle fpsStyle;
    
    private void Awake()
    {
        // Ensure single instance
        if (FindObjectsOfType<ConsistentFPSManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        InitializeFrameTimeHistory();
    }
    
    private void Start()
    {
        InitializePerformanceSettings();
        StartCoroutine(PerformanceMonitoringLoop());
        
        if (aggressiveMemoryManagement)
        {
            StartCoroutine(MemoryManagementLoop());
        }
    }
    
    private void InitializeFrameTimeHistory()
    {
        frameTimeHistory = new float[FRAME_HISTORY_SIZE];
        for (int i = 0; i < FRAME_HISTORY_SIZE; i++)
        {
            frameTimeHistory[i] = 1f / targetFPS; // Initialize with target frame time
        }
    }
    
    private void InitializePerformanceSettings()
    {
        // Set consistent frame rate target
        Application.targetFrameRate = targetFPS;
        
        // Disable VSync for consistent frame pacing
        QualitySettings.vSyncCount = 0;
        
        // Set fixed time step for consistent physics
        if (useFixedTimeStep)
        {
            Time.fixedDeltaTime = 1f / Mathf.Min(targetFPS, 50f); // Cap physics at 50Hz for performance
        }
        
        // Optimize physics for consistent performance
        if (optimizePhysicsStep)
        {
            Physics.defaultSolverIterations = 4;
            Physics.defaultSolverVelocityIterations = 1;
        }
        
        // Set initial quality level based on device capability
        currentQualityLevel = DetermineOptimalQualityLevel();
        QualitySettings.SetQualityLevel(currentQualityLevel, true);
        
        // Apply immediate optimizations for 60 FPS
        ApplyConsistentFPSOptimizations();
        
        if (logPerformanceChanges)
        {
            Debug.Log($"ConsistentFPSManager: Initialized with quality level {currentQualityLevel}, target {targetFPS} FPS");
        }
    }
    
    private int DetermineOptimalQualityLevel()
    {
        int ram = SystemInfo.systemMemorySize;
        int cores = SystemInfo.processorCount;
        int gpuMemory = SystemInfo.graphicsMemorySize;
        
        // Conservative quality selection for consistent 60 FPS
        if (ram >= 8000 && cores >= 8 && gpuMemory >= 3000)
        {
            return Mathf.Min(3, maxQualityLevel); // Medium quality max
        }
        else if (ram >= 6000 && cores >= 6 && gpuMemory >= 2000)
        {
            return Mathf.Min(2, maxQualityLevel); // Low-Medium quality
        }
        else if (ram >= 4000 && cores >= 4 && gpuMemory >= 1000)
        {
            return Mathf.Min(1, maxQualityLevel); // Low quality
        }
        else
        {
            return 0; // Very Low quality
        }
    }
    
    private void ApplyConsistentFPSOptimizations()
    {
        // Rendering optimizations for consistent 60 FPS
        QualitySettings.antiAliasing = 0; // Disable AA for performance
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.shadowDistance = Mathf.Min(30f, QualitySettings.shadowDistance);
        QualitySettings.shadowCascades = 1; // Single cascade for mobile
        
        // LOD optimizations
        QualitySettings.lodBias = 0.8f;
        QualitySettings.maximumLODLevel = 1;
        
        // Texture optimizations
        QualitySettings.globalTextureMipmapLimit = SystemInfo.systemMemorySize < 6000 ? 1 : 0;
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsMemoryBudget = SystemInfo.systemMemorySize > 6000 ? 512 : 256;
        
        // Particle optimizations
        QualitySettings.particleRaycastBudget = 32;
        
        // URP specific optimizations
        OptimizeURPSettings();
    }
    
    private void OptimizeURPSettings()
    {
        if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urpAsset)
        {
            // These optimizations would typically be set in the URP asset inspector
            // For runtime, we can suggest optimal settings
            if (logPerformanceChanges)
            {
                Debug.Log("URP detected. Recommend these settings in URP Asset:");
                Debug.Log("- Render Scale: 0.85-0.9 for better performance");
                Debug.Log("- MSAA: Disabled");
                Debug.Log("- HDR: Disabled on mobile");
                Debug.Log("- Shadow Resolution: 256 for main light");
                Debug.Log("- Additional Lights: Disabled or Per-Vertex");
            }
        }
    }
    
    private void Update()
    {
        UpdateFrameTimeHistory();
        CalculateAverageFPS();
        
        if (enableAdaptiveQuality && Time.time - lastQualityAdjustTime > qualityAdjustInterval)
        {
            AdjustQualityForTargetFPS();
            lastQualityAdjustTime = Time.time;
        }
    }
    
    private void UpdateFrameTimeHistory()
    {
        frameTimeHistory[frameTimeIndex] = Time.unscaledDeltaTime;
        frameTimeIndex = (frameTimeIndex + 1) % FRAME_HISTORY_SIZE;
    }
    
    private void CalculateAverageFPS()
    {
        float totalFrameTime = 0f;
        for (int i = 0; i < FRAME_HISTORY_SIZE; i++)
        {
            totalFrameTime += frameTimeHistory[i];
        }
        
        float averageFrameTime = totalFrameTime / FRAME_HISTORY_SIZE;
        averageFPS = 1f / averageFrameTime;
    }
    
    private void AdjustQualityForTargetFPS()
    {
        float fpsLowerBound = targetFPS - fpsTolerance;
        float fpsUpperBound = targetFPS + fpsTolerance;
        
        // If FPS is below target, reduce quality
        if (averageFPS < fpsLowerBound && currentQualityLevel > minQualityLevel)
        {
            currentQualityLevel = Mathf.Max(currentQualityLevel - 1, minQualityLevel);
            QualitySettings.SetQualityLevel(currentQualityLevel, true);
            
            if (logPerformanceChanges)
            {
                Debug.Log($"ConsistentFPSManager: Reduced quality to level {currentQualityLevel} (FPS: {averageFPS:F1})");
            }
        }
        // If FPS is consistently above target and we want to maintain quality, try increasing
        else if (maintainQualityWhenPossible && averageFPS > fpsUpperBound + 10 && 
                 currentQualityLevel < maxQualityLevel)
        {
            currentQualityLevel = Mathf.Min(currentQualityLevel + 1, maxQualityLevel);
            QualitySettings.SetQualityLevel(currentQualityLevel, true);
            
            if (logPerformanceChanges)
            {
                Debug.Log($"ConsistentFPSManager: Increased quality to level {currentQualityLevel} (FPS: {averageFPS:F1})");
            }
        }
    }
    
    private IEnumerator PerformanceMonitoringLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            
            // Log performance metrics periodically
            if (logPerformanceChanges && averageFPS < targetFPS - fpsTolerance)
            {
                Debug.LogWarning($"ConsistentFPSManager: Below target FPS - Current: {averageFPS:F1}, Target: {targetFPS}");
                
                // Suggest additional optimizations if still struggling
                if (currentQualityLevel <= minQualityLevel)
                {
                    SuggestAdditionalOptimizations();
                }
            }
        }
    }
    
    private IEnumerator MemoryManagementLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(memoryCleanupInterval);
            
            long currentMemoryMB = System.GC.GetTotalMemory(false) / (1024 * 1024);
            
            if (currentMemoryMB > memoryThresholdMB)
            {
                // Aggressive memory cleanup
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();
                Resources.UnloadUnusedAssets();
                
                if (logPerformanceChanges)
                {
                    long newMemoryMB = System.GC.GetTotalMemory(false) / (1024 * 1024);
                    Debug.Log($"ConsistentFPSManager: Memory cleanup - Before: {currentMemoryMB}MB, After: {newMemoryMB}MB");
                }
            }
        }
    }
    
    private void SuggestAdditionalOptimizations()
    {
        Debug.Log("ConsistentFPSManager: Additional optimization suggestions:");
        Debug.Log("- Reduce particle effects in scenes");
        Debug.Log("- Optimize mesh complexity and polygon count");
        Debug.Log("- Use object pooling for frequently instantiated objects");
        Debug.Log("- Implement level-of-detail (LOD) models");
        Debug.Log("- Consider reducing texture resolutions");
        Debug.Log("- Disable expensive post-processing effects");
    }
    
    private void OnGUI()
    {
        if (!showFPSDisplay) return;
        
        if (fpsStyle == null)
        {
            fpsStyle = new GUIStyle(GUI.skin.label);
            fpsStyle.fontSize = 24;
            fpsStyle.normal.textColor = averageFPS >= targetFPS - fpsTolerance ? Color.green : Color.red;
        }
        
        fpsStyle.normal.textColor = averageFPS >= targetFPS - fpsTolerance ? Color.green : Color.red;
        
        string fpsText = $"FPS: {averageFPS:F1} / {targetFPS}";
        string qualityText = $"Quality: {QualitySettings.names[currentQualityLevel]}";
        string memoryText = $"Memory: {System.GC.GetTotalMemory(false) / (1024 * 1024)}MB";
        
        GUI.Label(new Rect(10, 10, 300, 30), fpsText, fpsStyle);
        GUI.Label(new Rect(10, 40, 300, 25), qualityText);
        GUI.Label(new Rect(10, 65, 300, 25), memoryText);
        
        // Performance status indicator
        string status = averageFPS >= targetFPS - fpsTolerance ? "STABLE" : "OPTIMIZING";
        GUI.color = averageFPS >= targetFPS - fpsTolerance ? Color.green : Color.yellow;
        GUI.Label(new Rect(10, 90, 300, 25), $"Status: {status}");
        GUI.color = Color.white;
    }
    
    // Public methods for external control
    public void SetTargetFPS(int newTargetFPS)
    {
        targetFPS = newTargetFPS;
        Application.targetFrameRate = targetFPS;
        
        if (useFixedTimeStep)
        {
            Time.fixedDeltaTime = 1f / Mathf.Min(targetFPS, 50f);
        }
        
        if (logPerformanceChanges)
        {
            Debug.Log($"ConsistentFPSManager: Target FPS changed to {targetFPS}");
        }
    }
    
    public void ForceQualityLevel(int qualityLevel)
    {
        currentQualityLevel = Mathf.Clamp(qualityLevel, minQualityLevel, maxQualityLevel);
        QualitySettings.SetQualityLevel(currentQualityLevel, true);
        
        if (logPerformanceChanges)
        {
            Debug.Log($"ConsistentFPSManager: Quality forced to level {currentQualityLevel}");
        }
    }
    
    public float GetCurrentFPS()
    {
        return averageFPS;
    }
    
    public bool IsPerformanceStable()
    {
        return averageFPS >= targetFPS - fpsTolerance;
    }
    
    public void EnablePerformanceMode()
    {
        // Ultra-aggressive settings for maximum performance
        QualitySettings.SetQualityLevel(0, true); // Very Low
        currentQualityLevel = 0;
        
        QualitySettings.globalTextureMipmapLimit = 2; // Quarter resolution
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.lodBias = 0.5f;
        QualitySettings.maximumLODLevel = 2;
        
        // Reduce physics quality
        Time.fixedDeltaTime = 1f / 30f; // 30Hz physics
        Physics.defaultSolverIterations = 2;
        
        if (logPerformanceChanges)
        {
            Debug.Log("ConsistentFPSManager: Performance mode enabled - maximum optimization applied");
        }
    }
}