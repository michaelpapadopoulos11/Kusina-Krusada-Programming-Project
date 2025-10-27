using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class UISettings : MonoBehaviour
{
    [SerializeField] private GameObject SettingsCanvas;

    public void onCloseClick()
    {
        Debug.Log("Closed");
        SettingsCanvas.SetActive(false);
    }
}
