using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QuizQuestionUIInteraction : MonoBehaviour
{
    [SerializeField] UIDocument uIDocument;
    private VisualElement root;
    public GameObject Quiz;

    // Start is called before the first frame update
    void Start()
    {
        root = uIDocument.rootVisualElement;
        root.style.display = DisplayStyle.None; // Hide UI on startup

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Time.timeScale = 0f; // Pause the game
            UnityEngine.Cursor.lockState = CursorLockMode.None; // Unlock the cursor
            UnityEngine.Cursor.visible = true; // Make the cursor visible
            root.style.display = DisplayStyle.Flex; // Show the UI

            // now change button text
            var button1 = root.Q<Button>("Button1");
            if (button1 != null) button1.text = "New dynamic text";
            else Debug.LogWarning("Button1 not found. Make sure the UXML name is Button1 and the UIDocument is active.");
            // optionally add a click handler:
            button1.clicked += () =>        
            {
                Debug.Log("Button1 clicked");
                // resume game and hide UI when the button is clicked
                Time.timeScale = 1f; // Resume the game
                UnityEngine.Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
                UnityEngine.Cursor.visible = false; // Hide the cursor
                if (root != null)
                    root.style.display = DisplayStyle.None; // Hide the UI
            };
        }
    }

    private void ShowQuizUI()
    {
        var button1 = root.Q<Button>("Button1");
        if (button1 != null)
        {
            // button1.text = "My new answer text";
            // // optionally add a click handler:
            // button1.clicked += () =>
            // {
            //     Debug.Log("Button1 clicked");
            //     // resume game and hide UI when the button is clicked
            //     Time.timeScale = 1f; // Resume the game
            //     UnityEngine.Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
            //     UnityEngine.Cursor.visible = false; // Hide the cursor
            //     if (root != null)
            //         root.style.display = DisplayStyle.None; // Hide the UI
            // };
        }
        else
        {
            Debug.LogWarning("Button1 not found. Make sure the UXML name is Button1 and the UIDocument is active.");
        }

        // then show the UI (if not already)
        root.style.display = DisplayStyle.Flex;
    }
    
    // private void OnClickEvent(ClickEvent evt)
    // {

    // }

    // Update is called once per frame
    void Update()
    {
        
    }
}
