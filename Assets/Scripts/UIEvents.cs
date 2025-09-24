using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;

public class UIEvents : MonoBehaviour
{
    private UIDocument _uiDoc;

    private Button _btnSelectCharacter;
    private Button _btnQuestionHistory;
    private Button _btnSettings;
    private Button _btnPlay;



    //private List<Button> _btn = new List<Button>();

    private void Awake()
    {
        _uiDoc = GetComponent<UIDocument>();

        /*
        _btn = _uiDoc.rootVisualElement.Query<Button>().ToList();

        for (int i = 0; i < _btn.Count; i++)
        {
            _btn[i].RegisterCallback<ClickEvent>(OnAllButtonClick);
        }
        */

        //_btnSelectCharacter = _uiDoc.rootVisualElement.Q("btnSelectCharacter") as Button;
        _btnPlay = _uiDoc.rootVisualElement.Q("btnPlay") as Button;
        _btnQuestionHistory = _uiDoc.rootVisualElement.Q("btnQuestionHistory") as Button;
        _btnSettings = _uiDoc.rootVisualElement.Q("btnSettings") as Button;

        //Register CB
        //_btnSelectCharacter.RegisterCallback<ClickEvent>(OnSelectCharacterClick);
        _btnPlay.RegisterCallback<ClickEvent>(OnPlayClick);
        _btnQuestionHistory.RegisterCallback<ClickEvent>(OnQuestionHistoryClick);
        _btnSettings.RegisterCallback<ClickEvent>(OnSettingsClick);
    }

    /*
    private void OnSelectCharacterClick(ClickEvent e)
    {
        //Function of Select Character
        Debug.Log("Select Character Clicked");
    }
    */

    private void OnPlayClick (ClickEvent e)
    {
        Debug.Log("Play Clicked");
    }

    private void OnQuestionHistoryClick(ClickEvent e)
    {
        //Function of Question History
        Debug.Log("Question History Clicked");
    }

    private void OnSettingsClick(ClickEvent e)
    {
        //Function of Settings
        Debug.Log("Settings Clicked");
    }

    private void OnDisable()
    {
        //_btnSelectCharacter.UnregisterCallback<ClickEvent>(OnSelectCharacterClick);
        _btnPlay.UnregisterCallback<ClickEvent>(OnPlayClick);
        _btnQuestionHistory.UnregisterCallback<ClickEvent>(OnQuestionHistoryClick);
        _btnSettings.UnregisterCallback<ClickEvent>(OnSettingsClick);
    }

    private void detectSwipe()
    {
        //Detect swipe up at bottom of screen to start game
    }

    private void Update()
    {
        //detectSwipe();
    }
}
