using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QuizQuestionUIInteraction : MonoBehaviour
{
    [SerializeField] UIDocument uIDocument;
    private VisualElement root;

    // Start is called before the first frame update
    void Start()
    {
        root = uIDocument.rootVisualElement;
        root.style.display = DisplayStyle.None; // Hide UI on startup
    }

    private void OnClickEvent(ClickEvent evt)
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
