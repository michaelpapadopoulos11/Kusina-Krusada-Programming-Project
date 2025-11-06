using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIMainMenuButtons : MonoBehaviour
{
    [SerializeField] private GameObject SettingsCanvas;
    [SerializeField] private QuizHistoryManager quizHistoryManager; // Reference to Quiz History Manager
    
    // Static property to track if settings are open (accessible by other scripts)
    public static bool IsSettingsOpen { get; set; } = false;

    void Start()
    {
        if (SettingsCanvas is not null) { SettingsCanvas.SetActive(false); }
    }
    public void OnPlayClick()
    {
        Debug.Log("Play Clicked");
        // Reset the life system when starting a new game
        LifeManager.ResetLives();
        switchScene();
    }

    public void OnQuestionHistoryClick()
    {
        //Function of Question History
        Debug.Log("Question History Clicked");
        
        // Prevent access if settings are open
        if (IsSettingsOpen)
        {
            Debug.Log("Cannot access Questions History while Settings menu is open");
            ShowSettingsBlockedFeedback();
            return;
        }
        
        if (quizHistoryManager != null)
        {
            quizHistoryManager.ShowQuizHistory();
        }
        else
        {
            Debug.LogWarning("QuizHistoryManager reference is not assigned in UIMainMenuButtons!");
        }
    }

    public void OnSettingsClick()
    {
        //Function of Settings
        Debug.Log("Settings Clicked");
        showSettingsPanel();
    }

    private void showSettingsPanel()
    {
        SettingsCanvas.SetActive(true);
        IsSettingsOpen = true;
        Debug.Log("Settings opened - Questions History access blocked");
    }

    /// <summary>
    /// Public method to close settings (can be called from other scripts)
    /// </summary>
    public void CloseSettings()
    {
        if (SettingsCanvas != null && SettingsCanvas.activeInHierarchy)
        {
            SettingsCanvas.SetActive(false);
            IsSettingsOpen = false;
            Debug.Log("Settings closed externally - Questions History access restored");
        }
    }

    /// <summary>
    /// Check if Questions History can be accessed (public utility method)
    /// </summary>
    public static bool CanAccessQuestionHistory()
    {
        return !IsSettingsOpen;
    }

    /// <summary>
    /// Show feedback when Questions History is blocked by open settings
    /// </summary>
    private void ShowSettingsBlockedFeedback()
    {
        // This could be enhanced with UI feedback like a brief message or animation
        // For now, we'll use console feedback and could add visual feedback later
        Debug.Log("Please close the Settings menu first to access Questions History");
        
        // Optional: Add a brief visual indication (could be a popup, flash, etc.)
        // You could implement a UI message here if desired
    }

    /// <summary>
    /// Debug method to test the settings blocking functionality
    /// </summary>
    [ContextMenu("Test Settings Blocking")]
    public void TestSettingsBlocking()
    {
        Debug.Log($"Settings Open: {IsSettingsOpen}");
        Debug.Log($"Can Access Question History: {CanAccessQuestionHistory()}");
        
        if (IsSettingsOpen)
        {
            Debug.Log("Settings are currently open - Questions History should be blocked");
        }
        else
        {
            Debug.Log("Settings are closed - Questions History should be accessible");
        }
    }

    private void switchScene() 
    {
        //UI -> Gameplay 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); //Check build settings (File -> Build Settings) for indexes
    }

    private void Update()
    {
        // Monitor for ESC key to close settings
        if (IsSettingsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSettings();
        }
        //detectSwipe();
    }
}
