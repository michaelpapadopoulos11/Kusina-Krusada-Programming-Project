using UnityEngine;

/// <summary>
/// Simple static life manager that works with any UI system
/// Handles wrong answer tracking and game over logic.
/// </summary>
public static class LifeManager
{
    // Life system state
    private static int maxLives = 3;
    private static int currentLives = 3;
    private static bool gameOverTriggered = false;
    
    // Events for other systems to listen to
    public static System.Action<int> OnLifeLost; // Fired when a life is lost, passes remaining lives
    public static System.Action OnAllLivesLost; // Fired when all lives are lost
    
    // Heart GameObject references (will be found automatically)
    private static GameObject life1Heart;
    private static GameObject life2Heart;
    private static GameObject life3Heart;
    
    // Initialize flag
    private static bool isInitialized = false;
    
    /// <summary>
    /// Initialize the life system - call this at game start
    /// </summary>
    public static void Initialize()
    {
        if (isInitialized) return;
        
        currentLives = maxLives;
        gameOverTriggered = false;
        isInitialized = true;
        
        // Try to find heart GameObjects
        FindHeartGameObjects();
        
        // Show all hearts at start
        ShowAllHearts();
        
        Debug.Log($"LifeManager: Initialized with {currentLives} lives");
    }
    
    /// <summary>
    /// Try to find Life1, Life2, Life3 GameObjects in the scene
    /// </summary>
    private static void FindHeartGameObjects()
    {
        if (life1Heart == null)
        {
            life1Heart = GameObject.Find("Life1");
            if (life1Heart != null)
                Debug.Log("LifeManager: Found Life1 GameObject");
            else
                Debug.LogWarning("LifeManager: Life1 GameObject not found in scene. Create a GameObject named 'Life1' for the first heart.");
        }
        
        if (life2Heart == null)
        {
            life2Heart = GameObject.Find("Life2");
            if (life2Heart != null)
                Debug.Log("LifeManager: Found Life2 GameObject");
            else
                Debug.LogWarning("LifeManager: Life2 GameObject not found in scene. Create a GameObject named 'Life2' for the second heart.");
        }
        
        if (life3Heart == null)
        {
            life3Heart = GameObject.Find("Life3");
            if (life3Heart != null)
                Debug.Log("LifeManager: Found Life3 GameObject");
            else
                Debug.LogWarning("LifeManager: Life3 GameObject not found in scene. Create a GameObject named 'Life3' for the third heart.");
        }
    }
    

    
    /// <summary>
    /// Show all heart GameObjects
    /// </summary>
    private static void ShowAllHearts()
    {
        if (life1Heart != null) life1Heart.SetActive(true);
        if (life2Heart != null) life2Heart.SetActive(true);
        if (life3Heart != null) life3Heart.SetActive(true);
    }
    
    /// <summary>
    /// Called when a wrong answer is selected in the quiz
    /// Deducts a life and hides the appropriate heart
    /// </summary>
    public static void OnWrongAnswer()
    {
        // Initialize if not already done
        if (!isInitialized)
        {
            Initialize();
        }
        
        if (gameOverTriggered || currentLives <= 0)
        {
            Debug.Log("LifeManager: Game already over or no lives remaining");
            return;
        }
        
        currentLives--;
        Debug.Log($"LifeManager: Wrong answer! Lives remaining: {currentLives}");
        
        // Hide hearts in the specific order: Life1, Life3, Life2
        HideHeartByWrongAnswerCount();
        
        // Fire event
        OnLifeLost?.Invoke(currentLives);
        
        // Check if game over
        if (currentLives <= 0)
        {
            TriggerGameOver();
        }
    }
    
    /// <summary>
    /// Hides hearts in the specified order based on wrong answer count
    /// 1st wrong answer: hide Life1
    /// 2nd wrong answer: hide Life3  
    /// 3rd wrong answer: hide Life2 and trigger game over
    /// </summary>
    private static void HideHeartByWrongAnswerCount()
    {
        int wrongAnswerCount = maxLives - currentLives;
        
        switch (wrongAnswerCount)
        {
            case 1: // First wrong answer - hide Life1
                if (life1Heart != null)
                {
                    life1Heart.SetActive(false);
                    Debug.Log("LifeManager: Hidden Life1 heart");
                }
                else
                {
                    Debug.LogWarning("LifeManager: Life1 heart not found - cannot hide");
                }
                break;
                
            case 2: // Second wrong answer - hide Life3
                if (life3Heart != null)
                {
                    life3Heart.SetActive(false);
                    Debug.Log("LifeManager: Hidden Life3 heart");
                }
                else
                {
                    Debug.LogWarning("LifeManager: Life3 heart not found - cannot hide");
                }
                break;
                
            case 3: // Third wrong answer - hide Life2 and trigger game over
                if (life2Heart != null)
                {
                    life2Heart.SetActive(false);
                    Debug.Log("LifeManager: Hidden Life2 heart");
                }
                else
                {
                    Debug.LogWarning("LifeManager: Life2 heart not found - cannot hide");
                }
                break;
                
            default:
                Debug.LogWarning($"LifeManager: Unexpected wrong answer count: {wrongAnswerCount}");
                break;
        }
    }
    
    /// <summary>
    /// Triggers game over through the movement system
    /// </summary>
    private static void TriggerGameOver()
    {
        if (gameOverTriggered)
        {
            Debug.Log("LifeManager: Game over already triggered");
            return;
        }
        
        gameOverTriggered = true;
        Debug.Log("LifeManager: All lives lost! Triggering game over...");
        
        // Fire event
        OnAllLivesLost?.Invoke();
        
        // Find and trigger game over through Movement component
        Movement playerMovement = Object.FindObjectOfType<Movement>();
        if (playerMovement != null)
        {
            playerMovement.TriggerGameOver();
        }
        else
        {
            Debug.LogError("LifeManager: Cannot trigger game over - no Movement component found in scene");
        }
    }
    
    /// <summary>
    /// Reset the life system (useful for restarting the game)
    /// </summary>
    public static void ResetLives()
    {
        Debug.Log("LifeManager: Resetting life system");
        
        // Reset life system state
        currentLives = maxLives;
        gameOverTriggered = false;
        isInitialized = false;
        
        // Clear heart references so they get found again
        life1Heart = null;
        life2Heart = null;
        life3Heart = null;
        
        // Initialize fresh
        Initialize();
        
        // Also reset the Movement component's game over state if found
        Movement playerMovement = Object.FindObjectOfType<Movement>();
        if (playerMovement != null)
        {
            playerMovement.isGameOver = false;
            Debug.Log("LifeManager: Reset Movement.isGameOver to false");
        }
    }
    
    /// <summary>
    /// Get the current number of lives remaining
    /// </summary>
    public static int GetCurrentLives()
    {
        if (!isInitialized) Initialize();
        return currentLives;
    }
    
    /// <summary>
    /// Get the maximum number of lives
    /// </summary>
    public static int GetMaxLives()
    {
        return maxLives;
    }
    
    /// <summary>
    /// Check if the game is over due to no lives remaining
    /// </summary>
    public static bool IsGameOver()
    {
        return gameOverTriggered;
    }
    
    /// <summary>
    /// Manually set heart GameObject references (for setup)
    /// </summary>
    public static void SetHeartReferences(GameObject heart1, GameObject heart2, GameObject heart3)
    {
        life1Heart = heart1;
        life2Heart = heart2;
        life3Heart = heart3;
        Debug.Log("LifeManager: Heart references manually set");
    }
}