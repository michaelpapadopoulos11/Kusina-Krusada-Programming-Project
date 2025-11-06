using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

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

    [Header("Performance Settings")]
    [Tooltip("How often to update UI text (in seconds)")]
    public float updateInterval = 0.1f;
    
    // Performance optimization: reduce string allocations
    private StringBuilder stringBuilder = new StringBuilder(32);
    private float lastUpdateTime = 0f;
    private float cachedSpeed = 0f;
    private float cachedTimeUntilNext = 0f;
    private float cachedSpawnInterval = 0f;

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
        
        // Only update UI at specified intervals to reduce performance overhead
        if (Time.time - lastUpdateTime < updateInterval) return;
        
        UpdateUIElements();
        lastUpdateTime = Time.time;
    }
    
    private void UpdateUIElements()
    {
        // Update speed display
        if (speedText != null)
        {
            float currentSpeed = gameProgression.GetCurrentBaseSpeed();
            if (Mathf.Abs(currentSpeed - cachedSpeed) > 0.01f) // Only update if changed significantly
            {
                cachedSpeed = currentSpeed;
                stringBuilder.Clear();
                stringBuilder.Append("Speed: ");
                stringBuilder.Append(currentSpeed.ToString("F1"));
                speedText.text = stringBuilder.ToString();
            }
        }

        // Update timer display
        if (timerText != null)
        {
            float timeUntilNext = gameProgression.GetTimeUntilNextIncrease();
            if (Mathf.Abs(timeUntilNext - cachedTimeUntilNext) > 0.5f) // Only update if changed by more than 0.5 seconds
            {
                cachedTimeUntilNext = timeUntilNext;
                
                if (gameProgression.GetCurrentBaseSpeed() >= gameProgression.maxSpeed)
                {
                    timerText.text = "Max Speed Reached!";
                }
                else
                {
                    int minutes = Mathf.FloorToInt(timeUntilNext / 60f);
                    int seconds = Mathf.FloorToInt(timeUntilNext % 60f);
                    
                    stringBuilder.Clear();
                    stringBuilder.Append("Next increase: ");
                    stringBuilder.Append(minutes.ToString("00"));
                    stringBuilder.Append(":");
                    stringBuilder.Append(seconds.ToString("00"));
                    timerText.text = stringBuilder.ToString();
                }
            }
        }

        // Update spawn interval display
        if (spawnIntervalText != null && coinGenerator != null)
        {
            float currentSpawnInterval = coinGenerator.GetCurrentSpawnInterval();
            if (Mathf.Abs(currentSpawnInterval - cachedSpawnInterval) > 0.01f) // Only update if changed significantly
            {
                cachedSpawnInterval = currentSpawnInterval;
                stringBuilder.Clear();
                stringBuilder.Append("Spawn Interval: ");
                stringBuilder.Append(currentSpawnInterval.ToString("F2"));
                stringBuilder.Append("s");
                spawnIntervalText.text = stringBuilder.ToString();
            }
        }
    }
}