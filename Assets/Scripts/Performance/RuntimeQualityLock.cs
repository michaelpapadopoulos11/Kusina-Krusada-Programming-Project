using UnityEngine;
using System.Collections;

/// <summary>
/// Runtime quality adjustment system that locks settings once the game starts
/// Ensures consistent quality and performance throughout gameplay
/// </summary>
public class RuntimeQualityLock : MonoBehaviour
{
    [Header("Quality Lock Settings")]
    [SerializeField] private bool lockQualityOnStart = true;
    [SerializeField] private bool preventRuntimeChanges = true;
    [SerializeField] private bool lockFrameRate = true;
    
    [Header("Locked Settings")]
    [SerializeField] private int lockedQualityLevel = -1; // -1 = use current
    [SerializeField] private int lockedFrameRate = 60;
    [SerializeField] private bool lockedVSync = false;
    
    [Header("Emergency Performance Mode")]
    [SerializeField] private bool enableEmergencyMode = true;
    [SerializeField] private float emergencyFPSThreshold = 45f;
    [SerializeField] private float emergencyCheckDuration = 5f;
    
    private bool isLocked = false;
    private int originalQualityLevel;
    private int originalFrameRate;
    private int originalVSync;
    
    // Emergency mode tracking
    private float lowFPSTimer = 0f;
    private bool emergencyModeActive = false;
    
    private void Start()
    {
        // Store original settings
        originalQualityLevel = QualitySettings.GetQualityLevel();
        originalFrameRate = Application.targetFrameRate;
        originalVSync = QualitySettings.vSyncCount;
        
        if (lockQualityOnStart)
        {
            LockQualitySettings();
        }
        
        if (enableEmergencyMode)
        {
            StartCoroutine(MonitorPerformanceForEmergency());
        }
    }
    
    private void LockQualitySettings()
    {
        if (isLocked) return;
        
        // Set locked quality level
        if (lockedQualityLevel >= 0)
        {
            QualitySettings.SetQualityLevel(lockedQualityLevel, true);
        }
        
        // Lock frame rate
        if (lockFrameRate)
        {
            Application.targetFrameRate = lockedFrameRate;
        }
        
        // Lock VSync
        QualitySettings.vSyncCount = lockedVSync ? 1 : 0;
        
        // Apply consistent performance settings
        ApplyConsistentSettings();
        
        isLocked = true;
        
        Debug.Log($"Quality settings locked - Level: {QualitySettings.GetQualityLevel()}, " +
                 $"FPS: {Application.targetFrameRate}, VSync: {QualitySettings.vSyncCount > 0}");
    }
    
    private void ApplyConsistentSettings()
    {
        // These settings ensure consistency throughout gameplay
        
        // Disable expensive features for consistent performance
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.billboardsFaceCameraPosition = false;
        
        // Consistent texture streaming
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsAddAllCameras = true;
        
        // Optimize async operations
        QualitySettings.asyncUploadTimeSlice = 2;
        QualitySettings.asyncUploadBufferSize = 8;
        QualitySettings.asyncUploadPersistentBuffer = true;
        
        // Consistent physics settings
        Time.maximumDeltaTime = 1f / 30f; // Prevent physics spiral of death
        Time.maximumParticleDeltaTime = 1f / 30f;
        
        // Memory management
        Application.lowMemory += OnLowMemory;
    }
    
    private IEnumerator MonitorPerformanceForEmergency()
    {
        yield return new WaitForSeconds(2f); // Wait for initialization
        
        while (true)
        {
            float currentFPS = 1f / Time.unscaledDeltaTime;
            
            if (currentFPS < emergencyFPSThreshold && !emergencyModeActive)
            {
                lowFPSTimer += Time.unscaledDeltaTime;
                
                if (lowFPSTimer >= emergencyCheckDuration)
                {
                    ActivateEmergencyPerformanceMode();
                }
            }
            else if (currentFPS >= emergencyFPSThreshold)
            {
                lowFPSTimer = 0f;
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private void ActivateEmergencyPerformanceMode()
    {
        if (emergencyModeActive) return;
        
        emergencyModeActive = true;
        
        Debug.LogWarning("Emergency Performance Mode Activated - Applying aggressive optimizations");
        
        // Emergency quality reduction
        QualitySettings.SetQualityLevel(0, true); // Force to lowest quality
        
        // Emergency rendering optimizations
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.globalTextureMipmapLimit = 2; // Quarter resolution
        QualitySettings.lodBias = 0.3f;
        QualitySettings.maximumLODLevel = 2;
        QualitySettings.particleRaycastBudget = 16;
        
        // Emergency physics optimizations
        Physics.defaultSolverIterations = 2;
        Physics.defaultSolverVelocityIterations = 1;
        Time.fixedDeltaTime = 1f / 30f; // 30Hz physics
        
        // Force garbage collection
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        
        Debug.LogWarning("Emergency Performance Mode: All quality settings reduced to minimum");
    }
    
    private void OnLowMemory()
    {
        Debug.LogWarning("Low memory warning received - performing cleanup");
        
        // Aggressive memory cleanup
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        
        // Reduce texture memory usage
        if (QualitySettings.globalTextureMipmapLimit < 2)
        {
            QualitySettings.globalTextureMipmapLimit++;
            Debug.LogWarning($"Reduced texture quality due to low memory (limit: {QualitySettings.globalTextureMipmapLimit})");
        }
    }
    
    // Prevent external quality changes during gameplay
    private void Update()
    {
        if (!preventRuntimeChanges || !isLocked) return;
        
        // Enforce locked settings
        if (lockFrameRate && Application.targetFrameRate != lockedFrameRate)
        {
            Application.targetFrameRate = lockedFrameRate;
            Debug.LogWarning("Frame rate was changed externally - restored to locked value");
        }
        
        if (QualitySettings.vSyncCount != (lockedVSync ? 1 : 0))
        {
            QualitySettings.vSyncCount = lockedVSync ? 1 : 0;
            Debug.LogWarning("VSync was changed externally - restored to locked value");
        }
        
        // Check if quality level was changed
        int expectedQuality = lockedQualityLevel >= 0 ? lockedQualityLevel : originalQualityLevel;
        if (!emergencyModeActive && QualitySettings.GetQualityLevel() != expectedQuality)
        {
            QualitySettings.SetQualityLevel(expectedQuality, true);
            Debug.LogWarning("Quality level was changed externally - restored to locked value");
        }
    }
    
    // Public methods for controlled quality changes
    public void SetQualityLevel(int level, bool overrideLock = false)
    {
        if (isLocked && !overrideLock)
        {
            Debug.LogWarning("Cannot change quality level - settings are locked. Use overrideLock parameter if needed.");
            return;
        }
        
        QualitySettings.SetQualityLevel(level, true);
        if (overrideLock)
        {
            lockedQualityLevel = level;
            Debug.Log($"Quality level changed to {level} and lock updated");
        }
    }
    
    public void SetFrameRate(int frameRate, bool overrideLock = false)
    {
        if (lockFrameRate && isLocked && !overrideLock)
        {
            Debug.LogWarning("Cannot change frame rate - settings are locked. Use overrideLock parameter if needed.");
            return;
        }
        
        Application.targetFrameRate = frameRate;
        if (overrideLock)
        {
            lockedFrameRate = frameRate;
            Debug.Log($"Frame rate changed to {frameRate} and lock updated");
        }
    }
    
    public void UnlockSettings()
    {
        isLocked = false;
        Debug.Log("Quality settings unlocked");
    }
    
    public void RestoreOriginalSettings()
    {
        QualitySettings.SetQualityLevel(originalQualityLevel, true);
        Application.targetFrameRate = originalFrameRate;
        QualitySettings.vSyncCount = originalVSync;
        
        isLocked = false;
        emergencyModeActive = false;
        
        Debug.Log("Original quality settings restored");
    }
    
    public bool IsEmergencyModeActive()
    {
        return emergencyModeActive;
    }
    
    public bool IsLocked()
    {
        return isLocked;
    }
    
    // Editor helper methods
    [ContextMenu("Lock Current Settings")]
    private void LockCurrentSettings()
    {
        lockedQualityLevel = QualitySettings.GetQualityLevel();
        lockedFrameRate = Application.targetFrameRate;
        lockedVSync = QualitySettings.vSyncCount > 0;
        LockQualitySettings();
    }
    
    [ContextMenu("Test Emergency Mode")]
    private void TestEmergencyMode()
    {
        ActivateEmergencyPerformanceMode();
    }
    
    private void OnDestroy()
    {
        Application.lowMemory -= OnLowMemory;
    }
}