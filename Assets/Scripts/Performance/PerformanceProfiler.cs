using UnityEngine;
using UnityEngine.Profiling;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Real-time performance monitor and bottleneck detector
/// Shows FPS, memory usage, and performance warnings
/// </summary>
public class PerformanceProfiler : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private bool showOnScreen = true;
    [SerializeField] private bool logToConsole = false;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    
    [Header("Monitoring Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private int maxFrameTimeEntries = 60;
    [SerializeField] private float frameTimeWarningThreshold = 0.033f; // 30 FPS
    
    // Performance data
    private Queue<float> frameTimeHistory = new Queue<float>();
    private float averageFrameTime = 0f;
    private float averageFPS = 0f;
    private long totalMemory = 0;
    private long reservedMemory = 0;
    
    // Update timer
    private float lastUpdateTime = 0f;
    
    // GUI Style
    private GUIStyle boxStyle;
    private GUIStyle labelStyle;
    private bool stylesInitialized = false;
    
    private void Start()
    {
        // Initial update
        UpdatePerformanceMetrics();
        lastUpdateTime = Time.time;
    }
    
    private void Update()
    {
        // Track frame time
        float currentFrameTime = Time.unscaledDeltaTime;
        frameTimeHistory.Enqueue(currentFrameTime);
        
        // Keep history size manageable
        if (frameTimeHistory.Count > maxFrameTimeEntries)
        {
            frameTimeHistory.Dequeue();
        }
        
        // Update metrics periodically
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdatePerformanceMetrics();
            CheckForPerformanceIssues();
            lastUpdateTime = Time.time;
        }
        
        // Toggle display
        if (Input.GetKeyDown(toggleKey))
        {
            showOnScreen = !showOnScreen;
        }
    }
    
    private void UpdatePerformanceMetrics()
    {
        // Calculate average frame time and FPS
        if (frameTimeHistory.Count > 0)
        {
            averageFrameTime = frameTimeHistory.Average();
            averageFPS = 1f / averageFrameTime;
        }
        
        // Memory usage
        totalMemory = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024); // MB
        reservedMemory = Profiler.GetTotalReservedMemoryLong() / (1024 * 1024); // MB
        
        // Rendering stats would require frame debugger API which is editor-only
        // For runtime, we'll skip draw call counting to avoid compilation issues
    }
    
    private void CheckForPerformanceIssues()
    {
        List<string> warnings = new List<string>();
        
        // FPS warnings
        if (averageFrameTime > frameTimeWarningThreshold)
        {
            warnings.Add($"Low FPS detected: {averageFPS:F1} FPS");
        }
        
        // Memory warnings
        if (totalMemory > 500) // 500MB
        {
            warnings.Add($"High memory usage: {totalMemory}MB");
        }
        
        // GC warnings
        if (System.GC.CollectionCount(0) > 10)
        {
            warnings.Add("Frequent garbage collection detected");
        }
        
        // Log warnings if enabled
        if (logToConsole && warnings.Count > 0)
        {
            foreach (string warning in warnings)
            {
                Debug.LogWarning($"Performance Warning: {warning}");
            }
        }
    }
    
    private void OnGUI()
    {
        if (!showOnScreen) return;
        
        InitializeGUIStyles();
        
        // Main performance box
        GUI.Box(new Rect(10, 10, 300, 200), "", boxStyle);
        
        float yPos = 25;
        float lineHeight = 18f;
        
        GUI.Label(new Rect(20, yPos, 280, lineHeight), "=== PERFORMANCE MONITOR ===", labelStyle);
        yPos += lineHeight + 5;
        
        // FPS and Frame Time
        Color fpsColor = averageFPS < 30 ? Color.red : (averageFPS < 45 ? Color.yellow : Color.green);
        GUI.color = fpsColor;
        GUI.Label(new Rect(20, yPos, 280, lineHeight), $"FPS: {averageFPS:F1} ({averageFrameTime * 1000:F1}ms)", labelStyle);
        GUI.color = Color.white;
        yPos += lineHeight;
        
        // Memory Usage
        Color memColor = totalMemory > 500 ? Color.red : (totalMemory > 300 ? Color.yellow : Color.green);
        GUI.color = memColor;
        GUI.Label(new Rect(20, yPos, 280, lineHeight), $"Memory: {totalMemory}MB / {reservedMemory}MB", labelStyle);
        GUI.color = Color.white;
        yPos += lineHeight;
        
        // System Info
        GUI.Label(new Rect(20, yPos, 280, lineHeight), $"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}", labelStyle);
        yPos += lineHeight;
        
        GUI.Label(new Rect(20, yPos, 280, lineHeight), $"Target FPS: {Application.targetFrameRate}", labelStyle);
        yPos += lineHeight;
        
        GUI.Label(new Rect(20, yPos, 280, lineHeight), $"VSync: {(QualitySettings.vSyncCount > 0 ? "ON" : "OFF")}", labelStyle);
        yPos += lineHeight;
        
        // Device Info
        GUI.Label(new Rect(20, yPos, 280, lineHeight), $"Device RAM: {SystemInfo.systemMemorySize}MB", labelStyle);
        yPos += lineHeight;
        
        GUI.Label(new Rect(20, yPos, 280, lineHeight), $"GPU Memory: {SystemInfo.graphicsMemorySize}MB", labelStyle);
        yPos += lineHeight;
        
        GUI.Label(new Rect(20, yPos, 280, lineHeight), $"CPU Cores: {SystemInfo.processorCount}", labelStyle);
        yPos += lineHeight;
        
        // Controls
        GUI.Label(new Rect(20, yPos + 5, 280, lineHeight), $"Press {toggleKey} to toggle", labelStyle);
        
        // Performance recommendations box
        if (averageFPS < 45 || totalMemory > 300)
        {
            GUI.Box(new Rect(320, 10, 280, 120), "", boxStyle);
            
            yPos = 25;
            GUI.color = Color.yellow;
            GUI.Label(new Rect(330, yPos, 260, lineHeight), "PERFORMANCE RECOMMENDATIONS:", labelStyle);
            GUI.color = Color.white;
            yPos += lineHeight + 5;
            
            if (averageFPS < 30)
            {
                GUI.Label(new Rect(330, yPos, 260, lineHeight), "• Reduce quality settings", labelStyle);
                yPos += lineHeight;
                GUI.Label(new Rect(330, yPos, 260, lineHeight), "• Disable shadows", labelStyle);
                yPos += lineHeight;
                GUI.Label(new Rect(330, yPos, 260, lineHeight), "• Lower texture resolution", labelStyle);
                yPos += lineHeight;
            }
            
            if (totalMemory > 300)
            {
                GUI.Label(new Rect(330, yPos, 260, lineHeight), "• Force garbage collection", labelStyle);
                yPos += lineHeight;
                GUI.Label(new Rect(330, yPos, 260, lineHeight), "• Unload unused assets", labelStyle);
                yPos += lineHeight;
            }
        }
    }
    
    private void InitializeGUIStyles()
    {
        if (stylesInitialized) return;
        
        boxStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTexture(2, 2, new Color(0, 0, 0, 0.8f)) }
        };
        
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = Color.white },
            fontSize = 12,
            fontStyle = FontStyle.Normal
        };
        
        stylesInitialized = true;
    }
    
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    /// <summary>
    /// Force garbage collection and resource cleanup
    /// </summary>
    [ContextMenu("Force Memory Cleanup")]
    public void ForceMemoryCleanup()
    {
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        Debug.Log("Manual memory cleanup performed");
    }
    
    /// <summary>
    /// Get current performance summary
    /// </summary>
    public string GetPerformanceSummary()
    {
        return $"FPS: {averageFPS:F1}, Frame Time: {averageFrameTime * 1000:F1}ms, Memory: {totalMemory}MB";
    }
    
    /// <summary>
    /// Check if the game is running smoothly
    /// </summary>
    public bool IsPerformanceGood()
    {
        return averageFPS >= 45 && totalMemory < 400;
    }
}