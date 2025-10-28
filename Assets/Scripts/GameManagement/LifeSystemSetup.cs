using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple script to create heart GameObjects for the life system
/// Attach this to any GameObject and use the context menu to create hearts
/// </summary>
public class LifeSystemSetup : MonoBehaviour
{
    [Header("Heart Setup")]
    [Tooltip("Canvas to parent the hearts to (will find one if not assigned)")]
    public Canvas targetCanvas;
    
    [Tooltip("Heart sprite to use (optional - will use red squares if not assigned)")]
    public Sprite heartSprite;
    
    [Tooltip("Size of each heart")]
    public Vector2 heartSize = new Vector2(50f, 50f);
    
    [Tooltip("Position for the first heart")]
    public Vector2 startPosition = new Vector2(-100f, 100f);
    
    [Tooltip("Spacing between hearts")]
    public float heartSpacing = 60f;
    

    
    /// <summary>
    /// Creates Life1, Life2, Life3 GameObjects with Image components
    /// </summary>
    [ContextMenu("Create Heart GameObjects")]
    public void CreateHeartGameObjects()
    {
        // Find canvas if not assigned
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas == null)
            {
                Debug.LogError("LifeSystemSetup: No Canvas found in scene. Please create a Canvas first.");
                return;
            }
        }
        
        // Remove existing hearts first
        RemoveExistingHearts();
        
        // Create the three hearts
        CreateHeart("Life1", 0);
        CreateHeart("Life2", 1);
        CreateHeart("Life3", 2);
        
        Debug.Log("LifeSystemSetup: Created Life1, Life2, Life3 GameObjects with Image components");
        Debug.Log("LifeSystemSetup: You can now test the life system by selecting wrong answers in quizzes");
    }
    
    /// <summary>
    /// Creates a single heart GameObject
    /// </summary>
    private void CreateHeart(string heartName, int position)
    {
        // Create GameObject
        GameObject heart = new GameObject(heartName);
        heart.transform.SetParent(targetCanvas.transform, false);
        
        // Add RectTransform and position it
        RectTransform rectTransform = heart.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1); // Top-left anchor
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(startPosition.x + (position * heartSpacing), startPosition.y);
        rectTransform.sizeDelta = heartSize;
        
        // Add Image component
        Image imageComponent = heart.AddComponent<Image>();
        
        if (heartSprite != null)
        {
            imageComponent.sprite = heartSprite;
        }
        else
        {
            // Use a red color as fallback
            imageComponent.color = Color.red;
        }
        
        Debug.Log($"LifeSystemSetup: Created {heartName} at position {rectTransform.anchoredPosition}");
    }
    
    /// <summary>
    /// Removes existing Life1, Life2, Life3 GameObjects
    /// </summary>
    [ContextMenu("Remove Heart GameObjects")]
    public void RemoveExistingHearts()
    {
        GameObject life1 = GameObject.Find("Life1");
        GameObject life2 = GameObject.Find("Life2");
        GameObject life3 = GameObject.Find("Life3");
        
        if (life1 != null) DestroyImmediate(life1);
        if (life2 != null) DestroyImmediate(life2);
        if (life3 != null) DestroyImmediate(life3);
        
        Debug.Log("LifeSystemSetup: Removed existing heart GameObjects");
    }
    
    /// <summary>
    /// Test the life system by manually triggering wrong answers
    /// </summary>
    [ContextMenu("Test Life System")]
    public void TestLifeSystem()
    {
        Debug.Log("LifeSystemSetup: Testing life system...");
        
        // Test first wrong answer
        LifeManager.OnWrongAnswer();
        
        // Wait a moment then test second wrong answer
        StartCoroutine(TestDelayedWrongAnswers());
    }
    
    /// <summary>
    /// Test subsequent wrong answers with delays
    /// </summary>
    private System.Collections.IEnumerator TestDelayedWrongAnswers()
    {
        yield return new WaitForSeconds(1f);
        LifeManager.OnWrongAnswer(); // Second wrong answer
        
        yield return new WaitForSeconds(1f);
        LifeManager.OnWrongAnswer(); // Third wrong answer (should trigger game over)
    }
    
    /// <summary>
    /// Reset the life system for testing
    /// </summary>
    [ContextMenu("Reset Life System")]
    public void ResetLifeSystem()
    {
        LifeManager.ResetLives();
        Debug.Log("LifeSystemSetup: Reset life system");
    }
    
    /// <summary>
    /// Check current life status
    /// </summary>
    [ContextMenu("Check Life Status")]
    public void CheckLifeStatus()
    {
        Debug.Log($"LifeSystemSetup: Current lives: {LifeManager.GetCurrentLives()}/{LifeManager.GetMaxLives()}");
        Debug.Log($"LifeSystemSetup: Game over: {LifeManager.IsGameOver()}");
        
        // Also check Movement component
        Movement playerMovement = FindObjectOfType<Movement>();
        if (playerMovement != null)
        {
            Debug.Log($"LifeSystemSetup: Movement.isGameOver: {playerMovement.isGameOver}");
        }
    }
    
    /// <summary>
    /// Test the full game over and reset cycle
    /// </summary>
    [ContextMenu("Test Full Reset Cycle")]
    public void TestFullResetCycle()
    {
        StartCoroutine(TestResetCycleCoroutine());
    }
    
    /// <summary>
    /// Coroutine to test the complete reset functionality
    /// </summary>
    private System.Collections.IEnumerator TestResetCycleCoroutine()
    {
        Debug.Log("=== Testing Full Reset Cycle ===");
        
        // 1. Reset to start fresh
        LifeManager.ResetLives();
        yield return new WaitForSeconds(0.5f);
        
        // 2. Trigger 3 wrong answers to cause game over
        Debug.Log("Step 1: Triggering 3 wrong answers...");
        LifeManager.OnWrongAnswer(); // Life1 should hide
        yield return new WaitForSeconds(0.5f);
        
        LifeManager.OnWrongAnswer(); // Life3 should hide  
        yield return new WaitForSeconds(0.5f);
        
        LifeManager.OnWrongAnswer(); // Life2 should hide + game over
        yield return new WaitForSeconds(1f);
        
        // 3. Check game over state
        Debug.Log("Step 2: Checking game over state...");
        CheckLifeStatus();
        yield return new WaitForSeconds(1f);
        
        // 4. Reset and verify restoration
        Debug.Log("Step 3: Resetting life system...");
        LifeManager.ResetLives();
        yield return new WaitForSeconds(0.5f);
        
        // 5. Check restored state
        Debug.Log("Step 4: Checking restored state...");
        CheckLifeStatus();
        
        Debug.Log("=== Reset Cycle Test Complete ===");
    }
}