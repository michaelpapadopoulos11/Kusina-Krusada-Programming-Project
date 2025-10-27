using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UILanguage : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown LanguageDropdown;

    public static bool isEnglish = true;

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
                break;

            case 1:
                Debug.Log("Tagalog");
                isEnglish = false;
                break;

            default:
                Debug.Log("Error"); 
                break;
        }
    }
}
