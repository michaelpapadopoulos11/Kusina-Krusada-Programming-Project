using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UIEvents : MonoBehaviour
{

    public void OnPlayClick ()
    {
        Debug.Log("Play Clicked");
        switchScene();
    }

    public void OnQuestionHistoryClick()
    {
        //Function of Question History
        Debug.Log("Question History Clicked");
    }

    public void OnSettingsClick()
    {
        //Function of Settings
        Debug.Log("Settings Clicked");
    }

    private void switchScene() 
    {
        //UI -> Gameplay 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); //Check build settings (File -> Build Settings) for indexes
    }

    // Update method removed for performance - no per-frame updates needed
}
