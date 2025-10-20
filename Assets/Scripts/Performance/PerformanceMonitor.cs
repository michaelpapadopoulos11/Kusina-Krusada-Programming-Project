using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Performance monitoring and display for debugging
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    [Header("Display Settings")]
    public bool showFPS = true;
    public bool showMemory = true;
    public KeyCode toggleKey = KeyCode.F12;
    
    [Header("UI References")]
    public Text fpsText;
    public Text memoryText;
    
    // Performance tracking
    private float frameTime = 0f;
    private int frameCount = 0;
    private float lastInterval = 0f;
    private float fps = 0f;
    
    // Memory tracking
    private long lastMemory = 0;
    
    // UI state
    private bool isVisible = false;
    
    void Start()
    {
        lastInterval = Time.realtimeSinceStartup;
        
        // Create UI elements if not assigned
        if (showFPS && fpsText == null)
        {
            CreateFPSDisplay();
        }
        
        if (showMemory && memoryText == null)
        {
            CreateMemoryDisplay();
        }
        
        SetUIVisibility(false);
    }
    
    void Update()
    {
        // Toggle visibility
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            SetUIVisibility(isVisible);
        }
        
        if (!isVisible) return;
        
        // Update performance metrics
        UpdateFPS();
        UpdateMemory();
    }
    
    private void UpdateFPS()
    {
        frameCount++;
        frameTime += Time.unscaledDeltaTime;
        
        if (Time.realtimeSinceStartup > lastInterval + 1f)
        {
            fps = frameCount / frameTime;
            frameCount = 0;
            frameTime = 0f;
            lastInterval = Time.realtimeSinceStartup;
            
            if (fpsText != null)
            {
                Color color = Color.green;
                if (fps < 30f) color = Color.red;
                else if (fps < 45f) color = Color.yellow;
                
                fpsText.text = $"FPS: {fps:F1}";
                fpsText.color = color;
            }
        }
    }
    
    private void UpdateMemory()
    {
        if (memoryText != null)
        {
            long memory = System.GC.GetTotalMemory(false);
            float memoryMB = memory / (1024f * 1024f);
            
            Color color = Color.green;
            if (memoryMB > 512f) color = Color.red;
            else if (memoryMB > 256f) color = Color.yellow;
            
            memoryText.text = $"Memory: {memoryMB:F1} MB";
            memoryText.color = color;
            
            lastMemory = memory;
        }
    }
    
    private void CreateFPSDisplay()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            // Create canvas if none exists
            GameObject canvasGO = new GameObject("PerformanceCanvas");
            Canvas canvasComponent = canvasGO.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = 9999;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            canvas = canvasGO;
        }
        
        GameObject fpsGO = new GameObject("FPSText");
        fpsGO.transform.SetParent(canvas.transform, false);
        
        fpsText = fpsGO.AddComponent<Text>();
        fpsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        fpsText.fontSize = 24;
        fpsText.color = Color.white;
        fpsText.text = "FPS: --";
        
        RectTransform rect = fpsText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(10, -10);
        rect.sizeDelta = new Vector2(200, 30);
    }
    
    private void CreateMemoryDisplay()
    {
        GameObject canvas = GameObject.Find("Canvas") ?? GameObject.Find("PerformanceCanvas");
        if (canvas == null) return;
        
        GameObject memoryGO = new GameObject("MemoryText");
        memoryGO.transform.SetParent(canvas.transform, false);
        
        memoryText = memoryGO.AddComponent<Text>();
        memoryText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        memoryText.fontSize = 24;
        memoryText.color = Color.white;
        memoryText.text = "Memory: -- MB";
        
        RectTransform rect = memoryText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(10, -50);
        rect.sizeDelta = new Vector2(200, 30);
    }
    
    private void SetUIVisibility(bool visible)
    {
        if (fpsText != null) fpsText.gameObject.SetActive(visible);
        if (memoryText != null) memoryText.gameObject.SetActive(visible);
    }
    
    /// <summary>
    /// Get current FPS for external monitoring
    /// </summary>
    public float GetCurrentFPS()
    {
        return fps;
    }
    
    /// <summary>
    /// Get current memory usage in MB
    /// </summary>
    public float GetCurrentMemoryMB()
    {
        return System.GC.GetTotalMemory(false) / (1024f * 1024f);
    }
}