using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using ShadowQuality = UnityEngine.ShadowQuality;
using ShadowResolution = UnityEngine.ShadowResolution;

/// <summary>
/// Editor tools for optimizing the entire project for consistent 60 FPS performance
/// Use these tools in the Unity Editor to configure optimal settings
/// </summary>
public class ProjectFPSOptimizer : EditorWindow
{
    private bool optimizePlayerSettings = true;
    private bool optimizeQualitySettings = true;
    private bool optimizeURPSettings = true;
    private bool optimizePhysicsSettings = true;
    private bool setupPerformanceComponents = true;
    
    private int targetFPS = 60;
    private bool isMobileBuild = false;
    
    [MenuItem("Tools/Performance/60 FPS Project Optimizer")]
    public static void ShowWindow()
    {
        GetWindow<ProjectFPSOptimizer>("60 FPS Optimizer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Unity Project 60 FPS Optimizer", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("This tool will optimize your entire Unity project for consistent 60 FPS performance.", MessageType.Info);
        GUILayout.Space(10);
        
        // Target settings
        GUILayout.Label("Target Settings", EditorStyles.boldLabel);
        targetFPS = EditorGUILayout.IntSlider("Target FPS", targetFPS, 30, 120);
        isMobileBuild = EditorGUILayout.Toggle("Mobile Build Optimizations", isMobileBuild);
        GUILayout.Space(10);
        
        // Optimization options
        GUILayout.Label("Optimization Options", EditorStyles.boldLabel);
        optimizePlayerSettings = EditorGUILayout.Toggle("Optimize Player Settings", optimizePlayerSettings);
        optimizeQualitySettings = EditorGUILayout.Toggle("Optimize Quality Settings", optimizeQualitySettings);
        optimizeURPSettings = EditorGUILayout.Toggle("Optimize URP Settings", optimizeURPSettings);
        optimizePhysicsSettings = EditorGUILayout.Toggle("Optimize Physics Settings", optimizePhysicsSettings);
        setupPerformanceComponents = EditorGUILayout.Toggle("Setup Performance Components", setupPerformanceComponents);
        GUILayout.Space(20);
        
        // Action buttons
        if (GUILayout.Button("Optimize Project for 60 FPS", GUILayout.Height(40)))
        {
            OptimizeProject();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Reset to Unity Defaults"))
        {
            ResetToDefaults();
        }
        
        GUILayout.Space(20);
        
        // Current status
        GUILayout.Label("Current Project Status", EditorStyles.boldLabel);
        DisplayCurrentSettings();
    }
    
    private void OptimizeProject()
    {
        EditorUtility.DisplayProgressBar("Optimizing Project", "Starting optimization...", 0f);
        
        try
        {
            if (optimizePlayerSettings)
            {
                EditorUtility.DisplayProgressBar("Optimizing Project", "Player Settings...", 0.2f);
                OptimizePlayerSettings();
            }
            
            if (optimizeQualitySettings)
            {
                EditorUtility.DisplayProgressBar("Optimizing Project", "Quality Settings...", 0.4f);
                OptimizeQualitySettings();
            }
            
            if (optimizeURPSettings)
            {
                EditorUtility.DisplayProgressBar("Optimizing Project", "URP Settings...", 0.6f);
                OptimizeURPSettings();
            }
            
            if (optimizePhysicsSettings)
            {
                EditorUtility.DisplayProgressBar("Optimizing Project", "Physics Settings...", 0.8f);
                OptimizePhysicsSettings();
            }
            
            if (setupPerformanceComponents)
            {
                EditorUtility.DisplayProgressBar("Optimizing Project", "Performance Components...", 0.9f);
                SetupPerformanceComponents();
            }
            
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayProgressBar("Optimizing Project", "Saving...", 1f);
            
            Debug.Log("Project optimization complete! Your project is now optimized for " + targetFPS + " FPS.");
            EditorUtility.DisplayDialog("Optimization Complete", 
                "Project has been optimized for " + targetFPS + " FPS performance!", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    
    private void OptimizePlayerSettings()
    {
        // Graphics settings that are safe to set
        try
        {
            PlayerSettings.colorSpace = ColorSpace.Linear; // Better for rendering pipeline
            
            // Mobile-specific optimizations
            if (isMobileBuild)
            {
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
                PlayerSettings.allowedAutorotateToLandscapeLeft = true;
                PlayerSettings.allowedAutorotateToLandscapeRight = true;
                PlayerSettings.allowedAutorotateToPortrait = false;
                PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
                
                // Android optimizations
                PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
                
                // iOS optimizations  
                PlayerSettings.iOS.targetOSVersionString = "12.0";
            }
            
            Debug.Log("Player Settings optimized for " + targetFPS + " FPS");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Some player settings could not be applied: " + e.Message);
        }
    }
    
    private void OptimizeQualitySettings()
    {
        // Create optimized quality levels
        string[] qualityNames = QualitySettings.names;
        
        // Optimize Very Low (Level 0) - Ultra Performance
        QualitySettings.SetQualityLevel(0, true);
        QualitySettings.pixelLightCount = 0;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.shadowDistance = 10f;
        QualitySettings.antiAliasing = 0;
        QualitySettings.globalTextureMipmapLimit = 2; // Quarter resolution
        QualitySettings.lodBias = 0.3f;
        QualitySettings.maximumLODLevel = 2;
        QualitySettings.particleRaycastBudget = 16;
        QualitySettings.vSyncCount = 0;
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsMemoryBudget = 128;
        
        // Optimize Low (Level 1) - High Performance
        QualitySettings.SetQualityLevel(1, true);
        QualitySettings.pixelLightCount = 0;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.shadowDistance = 15f;
        QualitySettings.antiAliasing = 0;
        QualitySettings.globalTextureMipmapLimit = 1; // Half resolution
        QualitySettings.lodBias = 0.5f;
        QualitySettings.maximumLODLevel = 1;
        QualitySettings.particleRaycastBudget = 32;
        QualitySettings.vSyncCount = 0;
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsMemoryBudget = 256;
        
        // Optimize Medium (Level 2) - Balanced
        QualitySettings.SetQualityLevel(2, true);
        QualitySettings.pixelLightCount = 1;
        QualitySettings.shadows = ShadowQuality.HardOnly;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.shadowDistance = 25f;
        QualitySettings.antiAliasing = 0;
        QualitySettings.globalTextureMipmapLimit = 0; // Full resolution
        QualitySettings.lodBias = 0.7f;
        QualitySettings.maximumLODLevel = 0;
        QualitySettings.particleRaycastBudget = 64;
        QualitySettings.vSyncCount = 0;
        QualitySettings.streamingMipmapsActive = true;
        QualitySettings.streamingMipmapsMemoryBudget = 512;
        
        // Set appropriate default quality for different platforms
        if (isMobileBuild)
        {
            QualitySettings.SetQualityLevel(1, true); // Default to Low for mobile
        }
        else
        {
            QualitySettings.SetQualityLevel(2, true); // Default to Medium for desktop
        }
        
        Debug.Log("Quality Settings optimized with 3 performance-focused levels");
    }
    
    private void OptimizeURPSettings()
    {
        UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogWarning("URP Asset not found. Please assign a URP Asset in Graphics Settings.");
            return;
        }
        
        // Use reflection to set private fields (be careful with this approach)
        var serializedObject = new SerializedObject(urpAsset);
        
        // Rendering settings
        serializedObject.FindProperty("m_RenderScale").floatValue = isMobileBuild ? 0.85f : 0.9f;
        serializedObject.FindProperty("m_MSAA").intValue = 1; // Disable MSAA
        serializedObject.FindProperty("m_SupportsHDR").boolValue = false; // Disable HDR for mobile
        
        // Shadow settings
        serializedObject.FindProperty("m_MainLightShadowmapResolution").intValue = 256; // Low shadow resolution
        serializedObject.FindProperty("m_AdditionalLightsRenderingMode").intValue = 0; // Disabled
        serializedObject.FindProperty("m_ShadowDistance").floatValue = isMobileBuild ? 25f : 40f;
        serializedObject.FindProperty("m_ShadowCascadeCount").intValue = 1; // Single cascade
        
        // Advanced settings
        serializedObject.FindProperty("m_UseSRPBatcher").boolValue = true; // Enable SRP Batcher
        serializedObject.FindProperty("m_SupportsDynamicBatching").boolValue = false; // Disable dynamic batching
        serializedObject.FindProperty("m_UseAdaptivePerformance").boolValue = true; // Enable adaptive performance
        
        serializedObject.ApplyModifiedProperties();
        
        Debug.Log("URP Settings optimized for " + targetFPS + " FPS performance");
    }
    
    private void OptimizePhysicsSettings()
    {
        // Fixed timestep optimizations
        Time.fixedDeltaTime = 1f / Mathf.Min(targetFPS, 50f); // Cap physics at 50Hz
        
        // Physics solver optimizations
        Physics.defaultSolverIterations = 4; // Reduce from default 6
        Physics.defaultSolverVelocityIterations = 1; // Reduce from default 1
        Physics.bounceThreshold = 2f; // Default is 2
        Physics.sleepThreshold = 0.005f; // Default is 0.005f
        
        // 2D Physics optimizations (if used)
        Physics2D.velocityIterations = 4; // Reduce from default 8
        Physics2D.positionIterations = 2; // Reduce from default 3
        
        Debug.Log("Physics Settings optimized for " + targetFPS + " FPS");
    }
    
    private void SetupPerformanceComponents()
    {
        // Check if scene has performance components
        var existingOptimizer = FindObjectOfType<ConsistentFPSManager>();
        if (existingOptimizer == null)
        {
            // Create performance manager
            GameObject perfManager = new GameObject("ConsistentFPSManager");
            var component = perfManager.AddComponent<ConsistentFPSManager>();
            // Configure for target FPS
            var serializedObject = new SerializedObject(component);
            serializedObject.FindProperty("targetFPS").intValue = targetFPS;
            serializedObject.FindProperty("enableAdaptiveQuality").boolValue = true;
            serializedObject.FindProperty("maxQualityLevel").intValue = isMobileBuild ? 2 : 3;
            serializedObject.ApplyModifiedProperties();
            
            Debug.Log("Added ConsistentFPSManager to scene");
        }
        
        // Create performance manager prefab
        string prefabPath = "Assets/Prefabs/Performance/";
        if (!AssetDatabase.IsValidFolder(prefabPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Performance");
        }
        
        // Save as prefab for easy reuse
        GameObject prefabSource = new GameObject("ConsistentFPSManager");
        var fpsManager = prefabSource.AddComponent<ConsistentFPSManager>();
        
        // Configure the prefab
        var serializedPrefab = new SerializedObject(fpsManager);
        serializedPrefab.FindProperty("targetFPS").intValue = targetFPS;
        serializedPrefab.FindProperty("enableAdaptiveQuality").boolValue = true;
        serializedPrefab.FindProperty("maxQualityLevel").intValue = isMobileBuild ? 2 : 3;
        serializedPrefab.ApplyModifiedProperties();
        
        PrefabUtility.SaveAsPrefabAsset(prefabSource, prefabPath + "ConsistentFPSManager.prefab");
        DestroyImmediate(prefabSource);
        
        Debug.Log("Created ConsistentFPSManager prefab at " + prefabPath);
    }
    
    private void ResetToDefaults()
    {
        if (EditorUtility.DisplayDialog("Reset to Defaults", 
            "This will reset all settings to Unity defaults. Continue?", "Yes", "No"))
        {
            // Reset quality settings
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, true);
                // Reset to more standard values
                QualitySettings.vSyncCount = 1; // Re-enable VSync
                QualitySettings.globalTextureMipmapLimit = 0;
                QualitySettings.streamingMipmapsActive = false;
            }
            
            // Reset physics
            Time.fixedDeltaTime = 0.02f; // 50Hz default
            Physics.defaultSolverIterations = 6;
            Physics.defaultSolverVelocityIterations = 1;
            
            Debug.Log("Settings reset to Unity defaults");
            EditorUtility.DisplayDialog("Reset Complete", "All settings have been reset to Unity defaults.", "OK");
        }
    }
    
    private void DisplayCurrentSettings()
    {
        EditorGUILayout.BeginVertical("box");
        
        GUILayout.Label($"Current Quality Level: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        GUILayout.Label($"VSync Count: {QualitySettings.vSyncCount}");
        GUILayout.Label($"Shadow Quality: {QualitySettings.shadows}");
        GUILayout.Label($"Anti Aliasing: {QualitySettings.antiAliasing}");
        GUILayout.Label($"Texture Mipmap Limit: {QualitySettings.globalTextureMipmapLimit}");
        GUILayout.Label($"Fixed Delta Time: {Time.fixedDeltaTime:F4}s ({(1f/Time.fixedDeltaTime):F0} Hz)");
        GUILayout.Label($"Physics Solver Iterations: {Physics.defaultSolverIterations}");
        
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            GUILayout.Label("URP Asset: Detected ✓");
        }
        else
        {
            GUILayout.Label("URP Asset: Not Found ⚠️");
        }
        
        EditorGUILayout.EndVertical();
    }
}