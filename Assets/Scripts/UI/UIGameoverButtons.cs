using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIGameoverButtons : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void switchToMenu()
    {
        SceneManager.LoadScene("UI"); //Check build settings (File -> Build Settings) for indexes
    }

    private void reloadScene()
    {
        Scene curScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(curScene.name);
    }

    public void OnRetryClick()
    {
        Debug.Log("r");
        reloadScene();
    }
    
    public void OnMenuClick()
    {
        Debug.Log("menu");
        switchToMenu();
    }
}
