using UnityEngine;

public class GameProgression : MonoBehaviour
{
    [Header("Speed Progression Settings")]
    [Tooltip("Time interval in seconds between speed increases")]
    public float speedIncreaseInterval = 30f;
    
    [Tooltip("Amount to increase speed by each interval")]
    public float speedIncreaseAmount = 1f;
    
    [Tooltip("Maximum speed the game can reach")]
    public float maxSpeed = 30f;
    
    [Tooltip("Reference to the player's Movement component")]
    public Movement playerMovement;

    private float timer = 0f;
    private float originalSpeed;

    // Static instance for easy access from other scripts
    public static GameProgression Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Find player movement if not assigned
        if (playerMovement == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerMovement = playerObj.GetComponent<Movement>();
        }

        if (playerMovement != null)
        {
            originalSpeed = playerMovement.forwardSpeed;
        }
        else
        {
            Debug.LogError("GameProgression: Could not find player Movement component!");
        }
    }

    void Update()
    {
        if (playerMovement == null || UIScore.gameIsPaused) return;

        timer += Time.deltaTime;

        // Check if it's time to increase speed
        if (timer >= speedIncreaseInterval)
        {
            IncreaseSpeed();
            timer = 0f;
        }
    }

    private void IncreaseSpeed()
    {
        if (playerMovement == null) return;

        // Only increase base speed if we haven't reached the maximum
        float newBaseSpeed = playerMovement.baseForwardSpeed + speedIncreaseAmount;
        if (newBaseSpeed <= maxSpeed)
        {
            playerMovement.baseForwardSpeed = newBaseSpeed;
            
            // If the player is not currently slowed, update the current speed too
            if (!playerMovement.isSlowed)
            {
                playerMovement.forwardSpeed = newBaseSpeed;
            }

            Debug.Log($"Speed increased to {newBaseSpeed}. Next increase in {speedIncreaseInterval} seconds.");
        }
        else
        {
            Debug.Log("Maximum speed reached!");
        }
    }


    /// Get the current base speed (what the speed would be without any effects)

    public float GetCurrentBaseSpeed()
    {
        return playerMovement != null ? playerMovement.baseForwardSpeed : originalSpeed;
    }


    /// Get the original speed (starting speed before any increases)

    public float GetOriginalSpeed()
    {
        return originalSpeed;
    }


    /// Reset the speed progression (useful for restarting the game)

    public void ResetProgression()
    {
        if (playerMovement != null)
        {
            playerMovement.baseForwardSpeed = originalSpeed;
            playerMovement.forwardSpeed = originalSpeed;
        }
        timer = 0f;
        Debug.Log("Speed progression reset to original values.");
    }


    /// Get the time until the next speed increase
 
    public float GetTimeUntilNextIncrease()
    {
        return speedIncreaseInterval - timer;
    }
}