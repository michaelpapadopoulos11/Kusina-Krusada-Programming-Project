using UnityEngine;
using UnityEngine.UI;
using System;


//[ExecuteInEditMode()]
public class UIScore : MonoBehaviour
{
    [SerializeField] private Text txtScore;
    [SerializeField] private float score = 0;
    [SerializeField] private UnityEngine.UI.Image container;
    [SerializeField] private UnityEngine.UI.Image mask;
    [SerializeField] private GameObject soapbarIcon; // Reference to the soapbar icon GameObject
    [SerializeField] private float max = 100f;
    [SerializeField] private float cur = 0f;
    [SerializeField] private float drainRate = 14.5f;

    public static bool gameIsPaused = false;
    
    // Performance optimization: cache last displayed score to avoid unnecessary updates
    private int lastDisplayedScore = -1;

    void Awake()
    {
        // Set soapbar icon to hidden by default as early as possible
        ScoreManager.ResetScore();
        InitializeSoapbarIcon();
    }

    // Start is called before the first frame update
    void Start()
    {
        txtScore.text = "0";
        // Initially hide the soapbar elements since player starts without invincibility
        if (container != null)
            container.gameObject.SetActive(false);
        
        // Ensure soapbar icon is hidden (redundant check for safety)
        InitializeSoapbarIcon();
    }

    private void InitializeSoapbarIcon()
    {
        if (soapbarIcon != null)
        {
            soapbarIcon.SetActive(false);
        }
        else
        {
            // If soapbarIcon reference is null, try to find it by name
            GameObject foundIcon = GameObject.Find("SoapbarIcon");
            if (foundIcon != null)
            {
                soapbarIcon = foundIcon;
                soapbarIcon.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Only update UI when game is not paused for better performance
        if (!gameIsPaused)
        {
            updateInvincibilityBar();
        }
        setScoreText();
    }

    private void updateInvincibilityBar()
    {
        // Find the player's Movement component
        Movement playerMovement = FindObjectOfType<Movement>();
        
        if (playerMovement != null && playerMovement.isInvincible)
        {
            // Show the soapbar elements when invincible
            container.gameObject.SetActive(true);
            if (soapbarIcon != null)
                soapbarIcon.SetActive(true);
            
            // Update the bar based on remaining invincibility time
            cur = playerMovement.invincibilityTimer;
            max = playerMovement.invincibilityDuration;
            
            // Update the visual bar
            updateBar();
        }
        else
        {
            // Hide the soapbar elements when not invincible
            container.gameObject.SetActive(false);
            if (soapbarIcon != null)
                soapbarIcon.SetActive(false);
        }
    }

    private void updateBar()
    {
        float fillAmount = cur / max;
        mask.fillAmount = fillAmount; //changes the fill mask on the inner soap bar
    }

    // Time-based scoring removed. Score is now driven by pickups (ScoreManager).
    private void setScoreText()
    {
        // Use ScoreManager.Score (static). If ScoreManager isn't set up, fallback to the local score field.
        int display = ScoreManager.Score != 0 ? ScoreManager.Score : Mathf.RoundToInt(score);
        
        // Performance optimization: only update text when score actually changes
        if (display != lastDisplayedScore)
        {
            txtScore.text = PerformanceHelper.FormatScore(display);
            lastDisplayedScore = display;
        }
    }
}
