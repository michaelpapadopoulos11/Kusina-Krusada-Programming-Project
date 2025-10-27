using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedProgressionUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component to display current speed")]
    public TextMeshProUGUI speedText;
    
    [Tooltip("Text component to display time until next speed increase")]
    public TextMeshProUGUI timerText;
    
    [Tooltip("Text component to display current spawn interval")]
    public TextMeshProUGUI spawnIntervalText;
    
    [Tooltip("Reference to GameProgression (will find automatically if not set)")]
    public GameProgression gameProgression;
    
    [Tooltip("Reference to Generate component (will find automatically if not set)")]
    public Generate coinGenerator;

    void Start()
    {
        // Find GameProgression if not assigned
        if (gameProgression == null)
        {
            gameProgression = GameProgression.Instance;
        }
        
        // Find Generate component if not assigned
        if (coinGenerator == null)
        {
            coinGenerator = FindObjectOfType<Generate>();
        }
    }

    void Update()
    {
        if (gameProgression == null) return;

        // Update speed display
        if (speedText != null)
        {
            float currentSpeed = gameProgression.GetCurrentBaseSpeed();
            speedText.text = $"Speed: {currentSpeed:F1}";
        }

        // Update timer display
        if (timerText != null)
        {
            float timeUntilNext = gameProgression.GetTimeUntilNextIncrease();
            int minutes = Mathf.FloorToInt(timeUntilNext / 60f);
            int seconds = Mathf.FloorToInt(timeUntilNext % 60f);
            
            if (gameProgression.GetCurrentBaseSpeed() >= gameProgression.maxSpeed)
            {
                timerText.text = "Max Speed Reached!";
            }
            else
            {
                timerText.text = $"Next increase: {minutes:00}:{seconds:00}";
            }
        }

        // Update spawn interval display
        if (spawnIntervalText != null && coinGenerator != null)
        {
            // Access the private field using reflection or add a public property
            // For now, we'll add a public getter to Generate.cs
            spawnIntervalText.text = $"Spawn Interval: {coinGenerator.GetCurrentSpawnInterval():F2}s";
        }
    }
}