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
    }

    private void switchScene() 
    {
        //UI -> Gameplay 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); //Check build settings (File -> Build Settings) for indexes
    }


    private void Update()
    {
        //detectSwipe();
    }
}
