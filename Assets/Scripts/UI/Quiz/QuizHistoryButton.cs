using UnityEngine;
using UnityEngine.UIElements;

public class QuizHistoryButton : MonoBehaviour
{
    [SerializeField] private UIDocument buttonUIDocument; // UI Document containing the button
    [SerializeField] private string buttonName = "QuestionHistoryButton"; // Name of the button in UXML
    [SerializeField] private QuizHistoryManager quizHistoryManager; // Reference to the Quiz History Manager
    
    private Button historyButton;
    private VisualElement root;

    void Start()
    {
        if (buttonUIDocument == null)
        {
            Debug.LogError("Button UI Document is not assigned on " + gameObject.name);
            return;
        }

        if (quizHistoryManager == null)
        {
            Debug.LogError("Quiz History Manager is not assigned on " + gameObject.name);
            return;
        }

        root = buttonUIDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("Root visual element is null for button UI Document");
            return;
        }

        // Find the button
        historyButton = root.Q<Button>(buttonName);
        if (historyButton == null)
        {
            Debug.LogError($"Button with name '{buttonName}' not found in UI Document");
            return;
        }

        // Attach click event
        historyButton.clicked += OnQuestionHistoryButtonClicked;
        
        Debug.Log("Quiz History Button initialized successfully");
    }

    private void OnQuestionHistoryButtonClicked()
    {
        if (quizHistoryManager != null)
        {
            quizHistoryManager.ShowQuizHistory();
            Debug.Log("Question History button clicked - showing Quiz History");
        }
    }

    void OnDestroy()
    {
        // Clean up event handlers
        if (historyButton != null)
        {
            historyButton.clicked -= OnQuestionHistoryButtonClicked;
        }
    }
}