using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Auto-setup script that applies all performance optimizations when a scene loads
/// Add this to your first scene or use DontDestroyOnLoad to persist across scenes
/// </summary>
public class AutoPerformanceSetup : MonoBehaviour
{
    [Header("Auto-Setup Options")]
    [SerializeField] private bool setupOnAwake = true;
    [SerializeField] private bool addPerformanceOptimizer = true;
    [SerializeField] private bool addPerformanceProfiler = true;
    [SerializeField] private bool applyMobileOptimizations = true;
    
    [Header("Performance Targets")]
    [SerializeField] private int targetFrameRate = 60;
    
    private void Awake()
    {
        if (setupOnAwake)
        {
            SetupPerformanceOptimizations();
        }
    }
    
    [ContextMenu("Setup Performance Optimizations")]
    public void SetupPerformanceOptimizations()
    {
        Debug.Log("Setting up performance optimizations...");
        
        // Add GamePerformanceOptimizer if requested and not already present
        if (addPerformanceOptimizer && FindObjectOfType<GamePerformanceOptimizer>() == null)
        {
            GameObject optimizerGO = new GameObject("GamePerformanceOptimizer");
            var optimizer = optimizerGO.AddComponent<GamePerformanceOptimizer>();
            DontDestroyOnLoad(optimizerGO);
            Debug.Log("Added GamePerformanceOptimizer");
        }
        
        // Add PerformanceProfiler if requested and not already present
        if (addPerformanceProfiler && FindObjectOfType<PerformanceProfiler>() == null)
        {
            GameObject profilerGO = new GameObject("PerformanceProfiler");
            var profiler = profilerGO.AddComponent<PerformanceProfiler>();
            DontDestroyOnLoad(profilerGO);
            Debug.Log("Added PerformanceProfiler - Press F1 to toggle display");
        }
        
        // Apply basic mobile optimizations
        if (applyMobileOptimizations)
        {
            ApplyBasicMobileOptimizations();
        }
        
        Debug.Log("Performance optimization setup complete!");
    }
    
    private void ApplyBasicMobileOptimizations()
    {
        // Set target frame rate
        Application.targetFrameRate = targetFrameRate;
        
        // Disable VSync for better frame rate control
        QualitySettings.vSyncCount = 0;
        
        // Basic quality optimizations
        QualitySettings.antiAliasing = 0; // Disable AA on mobile
        QualitySettings.shadows = SystemInfo.systemMemorySize > 3000 ? UnityEngine.ShadowQuality.HardOnly : UnityEngine.ShadowQuality.Disable;
        QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Low;
        QualitySettings.shadowDistance = SystemInfo.systemMemorySize > 4000 ? 30f : 15f;
        
        // LOD optimizations
        QualitySettings.lodBias = 0.7f;
        QualitySettings.maximumLODLevel = 1;
        
        // Texture optimizations for low-end devices
        if (SystemInfo.systemMemorySize < 4000)
        {
            QualitySettings.globalTextureMipmapLimit = 1; // Half-resolution textures
        }
        
        // Physics optimizations
        Time.fixedDeltaTime = 1f / 50f; // 50Hz physics instead of 60Hz
        Physics.defaultSolverIterations = 4; // Reduce from default 6
        Physics.defaultSolverVelocityIterations = 1;
        
        // Particle optimizations
        QualitySettings.particleRaycastBudget = 32;
        
        // Memory optimizations
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsMemoryBudget = SystemInfo.systemMemorySize > 4000 ? 512 : 256;
        
        Debug.Log("Applied basic mobile optimizations");
    }
    
    // Optional: Manual setup only - no automatic hierarchy addition
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    // private static void AutoSetupOnSceneLoad()
    // {
    //     // Only auto-setup if there's no existing AutoPerformanceSetup in the scene
    //     if (FindObjectOfType<AutoPerformanceSetup>() == null)
    //     {
    //         GameObject autoSetupGO = new GameObject("AutoPerformanceSetup");
    //         var autoSetup = autoSetupGO.AddComponent<AutoPerformanceSetup>();
    //         // This will trigger the Awake() method automatically
    //     }
    // }
}