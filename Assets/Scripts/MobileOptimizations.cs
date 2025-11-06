using UnityEngine;
using UnityEngine.Rendering;

public class MobileOptimizations : MonoBehaviour
{
    [Header("Memory Management")]
    [SerializeField] private bool enableMemoryOptimizations = true;
    [SerializeField] private float garbageCollectionInterval = 30f;
    
    [Header("Rendering Optimizations")]
    [SerializeField] private bool enableRenderingOptimizations = true;
    
    [Header("Physics Optimizations")]
    [SerializeField] private bool optimizePhysics = true;
    [SerializeField] private float fixedTimestep = 0.02f;
    
    private float gcTimer;
    
    private void Start()
    {
        ApplyMobileOptimizations();
    }
    
    private void ApplyMobileOptimizations()
    {
        // Memory optimizations
        if (enableMemoryOptimizations)
        {
            // Reduce texture memory usage
            QualitySettings.streamingMipmapsActive = true;
            
            // Set appropriate memory budgets based on device RAM
            int memoryBudget = SystemInfo.systemMemorySize > 4000 ? 512 : 256;
            QualitySettings.streamingMipmapsMemoryBudget = memoryBudget;
            
            // Optimize async upload settings
            QualitySettings.asyncUploadTimeSlice = SystemInfo.systemMemorySize > 3000 ? 2 : 1;
            QualitySettings.asyncUploadBufferSize = SystemInfo.systemMemorySize > 3000 ? 16 : 8;
        }
        
        // Rendering optimizations
        if (enableRenderingOptimizations)
        {
            // Configure batching settings
            // Note: PlayerSettings can only be modified in Editor scripts
            
            // Optimize shadow settings
            QualitySettings.shadowDistance = SystemInfo.systemMemorySize > 4000 ? 50f : 25f;
            QualitySettings.shadowCascades = 1; // Single cascade for mobile
            
            // Disable expensive features for low-end devices
            if (SystemInfo.systemMemorySize < 3000)
            {
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.realtimeReflectionProbes = false;
            }
        }
        
        // Physics optimizations
        if (optimizePhysics)
        {
            Time.fixedDeltaTime = fixedTimestep;
            Physics.defaultSolverIterations = SystemInfo.systemMemorySize > 3000 ? 6 : 4;
            Physics.defaultSolverVelocityIterations = SystemInfo.systemMemorySize > 3000 ? 1 : 1;
            
            // Reduce physics update frequency for low-end devices
            if (SystemInfo.systemMemorySize < 2000)
            {
                Time.fixedDeltaTime = 0.025f; // 40Hz instead of 50Hz
            }
        }
        
        // Audio optimizations
        var audioConfig = AudioSettings.GetConfiguration();
        audioConfig.sampleRate = SystemInfo.systemMemorySize > 3000 ? 44100 : 22050;
        AudioSettings.Reset(audioConfig);
        
        // Input optimizations
        Input.multiTouchEnabled = false; // Disable if not needed
        
        Debug.Log($"Mobile Optimizations Applied - RAM: {SystemInfo.systemMemorySize}MB, Cores: {SystemInfo.processorCount}");
    }
    
    private void Update()
    {
        if (enableMemoryOptimizations)
        {
            ManageMemory();
        }
    }
    
    private void ManageMemory()
    {
        gcTimer += Time.unscaledDeltaTime;
        
        if (gcTimer >= garbageCollectionInterval)
        {
            // Force garbage collection periodically to prevent spikes
            System.GC.Collect();
            gcTimer = 0f;
        }
        
        // Monitor memory usage and warn if getting high
        long memoryUsage = System.GC.GetTotalMemory(false);
        if (memoryUsage > 100 * 1024 * 1024) // 100MB threshold
        {
            Debug.LogWarning($"High memory usage detected: {memoryUsage / (1024 * 1024)}MB");
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Release resources when app is paused
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // Reduce update frequency when app loses focus
            Application.targetFrameRate = 15;
        }
        else
        {
            // Restore normal frame rate when app regains focus
            Application.targetFrameRate = SystemInfo.systemMemorySize > 4000 ? 60 : 30;
        }
    }
}