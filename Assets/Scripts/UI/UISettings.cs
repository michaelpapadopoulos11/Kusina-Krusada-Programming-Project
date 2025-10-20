using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class UISettings : MonoBehaviour
{
    [SerializeField] private GameObject SettingsCanvas;
    [SerializeField] private TMP_Dropdown LanguageDropdown;
    public static bool isEnglish = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onCloseClick()
    {
        Debug.Log("Closed");
        SettingsCanvas.SetActive(false);
    }
    
    public void changeLanguage()
    {
        switch (LanguageDropdown.value)
        {
            case 0:
                Debug.Log("English");
                isEnglish = true;
                break;

            case 1:
                Debug.Log("Tagalog");
                isEnglish = false;
                break;

            default:
                Debug.Log("Error"); 
                break;
        }
        //Debug.Log("Selected: " + LanguageDropdown.value);
    }
}
