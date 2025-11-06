using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UILanguage : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown LanguageDropdown;

    public static bool isEnglish = true;
    
    // Event that fires when language changes
    public static event Action<bool> OnLanguageChanged;

    // Start is called before the first frame update
    void Start()
    {
        if (isEnglish)
        {
            LanguageDropdown.value = 0;
        }
        else
        {
            LanguageDropdown.value = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void changeLanguage()
    {
        switch (LanguageDropdown.value)
        {
            case 0:
                Debug.Log("English");
                isEnglish = true;
                NotifyLanguageChanged();
                break;

            case 1:
                Debug.Log("Tagalog");
                isEnglish = false;
                NotifyLanguageChanged();
                break;

            default:
                Debug.Log("Error"); 
                break;
        }
    }

    /// <summary>
    /// Notify all subscribers that the language has changed
    /// </summary>
    private void NotifyLanguageChanged()
    {
        // Fire the event for any subscribers
        OnLanguageChanged?.Invoke(isEnglish);
        
        // Direct update for Quiz History (backup method)
        UpdateQuizHistoryLanguage();
    }

    /// <summary>
    /// Update the Questions History display language when language settings change
    /// </summary>
    private void UpdateQuizHistoryLanguage()
    {
        // Find QuizHistoryManager and update its language
        QuizHistoryManager historyManager = FindObjectOfType<QuizHistoryManager>();
        if (historyManager != null)
        {
            historyManager.SwitchLanguage(!isEnglish);
            Debug.Log($"Updated Questions History language to: {(isEnglish ? "English" : "Tagalog")}");
        }
        else
        {
            Debug.LogWarning("QuizHistoryManager not found - language change not applied to Questions History");
        }
    }
}
