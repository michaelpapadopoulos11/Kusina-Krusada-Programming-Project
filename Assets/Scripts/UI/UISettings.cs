using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class UISettings : MonoBehaviour
{
    [SerializeField] private GameObject SettingsCanvas;

    void Update()
    {
        // Monitor settings canvas state and sync with the flag
        if (SettingsCanvas != null)
        {
            bool canvasActive = SettingsCanvas.activeInHierarchy;
            if (UIMainMenuButtons.IsSettingsOpen != canvasActive)
            {
                UIMainMenuButtons.IsSettingsOpen = canvasActive;
                Debug.Log($"Settings state synchronized: {(canvasActive ? "OPEN" : "CLOSED")}");
            }
        }

        // Handle Android back button or ESC key to close settings
        if (SettingsCanvas != null && SettingsCanvas.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                onCloseClick();
            }
        }
    }

    public void onCloseClick()
    {
        Debug.Log("Closed");
        SettingsCanvas.SetActive(false);
        
        // Reset the settings open flag to re-enable Questions History
        UIMainMenuButtons.IsSettingsOpen = false;
        Debug.Log("Settings closed - Questions History access restored");
    }
}
