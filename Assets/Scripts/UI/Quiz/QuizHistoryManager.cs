using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;

public class QuizHistoryManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private TextAsset questionsCsv; // assign questions.csv in the Inspector
    [SerializeField] private TextAsset questionsTlCsv; // assign questions_tl.csv in the Inspector (Tagalog)
    [SerializeField] private GameObject[] otherUIElements; // Array of other UI GameObjects to hide when showing Quiz History
    [SerializeField] private KeyCode closeKey = KeyCode.Escape; // Key to close the Quiz History UI
    
    private VisualElement root;
    private VisualElement questionsContainer;
    private VisualElement questionTemplate;
    private ScrollView questionsScrollView;
    private Button closeButton;
    
    private List<Question> parsedQuestions = new List<Question>();
    private List<Question> parsedQuestionsTl = new List<Question>();
    private bool isTagalog = false;
    private bool isQuizHistoryVisible = false;
    private List<GameObject> hiddenUIElements = new List<GameObject>();

    [Serializable]
    private class Question
    {
        public string q;
        public string a1;
        public string a2;
        public string a3;
        public string a4;
        public string correct;
    }

    void Start()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument is not assigned on " + gameObject.name);
            return;
        }

        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("Root visual element is null");
            return;
        }

        // Hide the UI by default
        root.style.display = DisplayStyle.None;
        isQuizHistoryVisible = false;

        // Get references to UI elements
        questionsScrollView = root.Q<ScrollView>("QuestionsScrollView");
        questionsContainer = root.Q<VisualElement>("QuestionsContainer");
        questionTemplate = root.Q<VisualElement>("QuestionTemplate");
        closeButton = root.Q<Button>("CloseButton");

        if (questionsContainer == null || questionTemplate == null)
        {
            Debug.LogError("Required UI elements not found. Make sure QuestionsContainer and QuestionTemplate exist in the UXML.");
            return;
        }

        // Attach close button event
        if (closeButton != null)
        {
            closeButton.clicked += HideQuizHistory;
        }

        // Parse CSV data
        ParseCSVData();
        
        // Pre-generate question displays (but keep UI hidden)
        GenerateQuestionDisplays();

        // Check for language preference
        try
        {
            if (UILanguage.isEnglish == false)
            {
                SwitchLanguage(true);
            }
        }
        catch
        {
            // UILanguage might not be available, use default English
            Debug.Log("UILanguage not available, using English by default");
        }

        Debug.Log("Quiz History Manager initialized - UI hidden by default, ready for OnClick events");
    }

    void Update()
    {
        // Allow closing with Escape key when Quiz History is visible
        if (isQuizHistoryVisible && Input.GetKeyDown(closeKey))
        {
            HideQuizHistory();
        }
    }

    public void ShowQuizHistory()
    {
        if (isQuizHistoryVisible) return;

        // Hide other UI elements
        HideOtherUIElements();

        // Show Quiz History UI
        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;
            isQuizHistoryVisible = true;
        }

        // Pause game and show cursor
        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        Debug.Log("Quiz History UI shown");
    }

    public void HideQuizHistory()
    {
        if (!isQuizHistoryVisible) return;

        // Hide Quiz History UI
        if (root != null)
        {
            root.style.display = DisplayStyle.None;
            isQuizHistoryVisible = false;
        }

        // Show previously hidden UI elements
        ShowOtherUIElements();

        // Resume game and hide cursor
        Time.timeScale = 1f;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        Debug.Log("Quiz History UI hidden");
    }

    private void HideOtherUIElements()
    {
        hiddenUIElements.Clear();

        // Hide specified UI elements
        if (otherUIElements != null)
        {
            foreach (var uiElement in otherUIElements)
            {
                if (uiElement != null && uiElement.activeInHierarchy)
                {
                    uiElement.SetActive(false);
                    hiddenUIElements.Add(uiElement);
                }
            }
        }

        // Auto-detect and hide main menu UI elements
        AutoDetectAndHideUIElements();

        // Also hide any Canvas elements that are not the Quiz History
        var allCanvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in allCanvases)
        {
            if (canvas.gameObject != this.gameObject && 
                canvas.gameObject != uiDocument.gameObject && 
                canvas.gameObject.activeInHierarchy &&
                !canvas.gameObject.name.Contains("QuizHistory") &&
                !canvas.gameObject.name.Contains("Quiz_History"))
            {
                canvas.gameObject.SetActive(false);
                hiddenUIElements.Add(canvas.gameObject);
            }
        }
    }

    private void AutoDetectAndHideUIElements()
    {
        // Find and hide common main menu elements
        string[] commonUINames = { 
            "MainMenu", "MainMenuUI", "MainMenuCanvas", "MenuCanvas", 
            "UIMainMenu", "Menu", "MainPanel", "MenuPanel", "HUD", "UI"
        };

        foreach (string uiName in commonUINames)
        {
            var foundObject = GameObject.Find(uiName);
            if (foundObject != null && foundObject.activeInHierarchy && 
                foundObject != this.gameObject && foundObject != uiDocument.gameObject)
            {
                foundObject.SetActive(false);
                hiddenUIElements.Add(foundObject);
                Debug.Log($"Auto-detected and hid UI element: {foundObject.name}");
            }
        }

        // Also look for UIMainMenuButtons component and hide its parent
        var mainMenuButtons = FindObjectOfType<UIMainMenuButtons>();
        if (mainMenuButtons != null && mainMenuButtons.gameObject.activeInHierarchy)
        {
            var parentCanvas = mainMenuButtons.GetComponentInParent<Canvas>();
            if (parentCanvas != null && parentCanvas.gameObject.activeInHierarchy && 
                !hiddenUIElements.Contains(parentCanvas.gameObject))
            {
                parentCanvas.gameObject.SetActive(false);
                hiddenUIElements.Add(parentCanvas.gameObject);
                Debug.Log($"Auto-detected and hid main menu canvas: {parentCanvas.gameObject.name}");
            }
        }
    }

    private void ShowOtherUIElements()
    {
        // Restore previously hidden UI elements
        foreach (var uiElement in hiddenUIElements)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(true);
            }
        }
        hiddenUIElements.Clear();
    }

    private void ParseCSVData()
    {
        // Parse English questions
        if (questionsCsv != null)
        {
            parsedQuestions = ParseQuestionsFromCsv(questionsCsv);
            Debug.Log($"Parsed {parsedQuestions.Count} English questions");
        }

        // Parse Tagalog questions
        if (questionsTlCsv != null)
        {
            parsedQuestionsTl = ParseQuestionsFromCsv(questionsTlCsv);
            Debug.Log($"Parsed {parsedQuestionsTl.Count} Tagalog questions");
        }
    }

    private List<Question> ParseQuestionsFromCsv(TextAsset csv)
    {
        var list = new List<Question>();
        if (csv == null) return list;

        var lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var cols = SplitCsvLine(line);
            if (cols.Length < 6) continue;

            var question = new Question()
            {
                q = cols[0].Trim(),
                a1 = cols[1].Trim(),
                a2 = cols[2].Trim(),
                a3 = cols[3].Trim(),
                a4 = cols[4].Trim(),
                correct = cols[5].Trim()
            };
            list.Add(question);
        }

        return list;
    }

    private string[] SplitCsvLine(string line)
    {
        // Parse CSV line handling quoted fields (fields may contain commas inside quotes)
        var fields = new List<string>();
        bool inQuotes = false;
        var currentField = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                // peek next for escaped quote
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++; // skip escaped quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }
        fields.Add(currentField.ToString());
        return fields.ToArray();
    }

    private void GenerateQuestionDisplays()
    {
        // Determine which question set to use based on language preference
        var questionsToDisplay = isTagalog && parsedQuestionsTl.Count > 0 ? parsedQuestionsTl : parsedQuestions;
        
        if (questionsToDisplay.Count == 0)
        {
            Debug.LogWarning("No questions available to display");
            return;
        }

        // Clear existing questions (except template)
        ClearExistingQuestions();

        // Create a question display for each question in the CSV
        for (int i = 0; i < questionsToDisplay.Count; i++)
        {
            var question = questionsToDisplay[i];
            CreateQuestionDisplay(question, i + 1);
        }

        Debug.Log($"Generated {questionsToDisplay.Count} question displays");
    }

    private void ClearExistingQuestions()
    {
        // Remove all children except the template
        var childrenToRemove = new List<VisualElement>();
        
        foreach (var child in questionsContainer.Children())
        {
            if (child.name != "QuestionTemplate")
            {
                childrenToRemove.Add(child);
            }
        }

        foreach (var child in childrenToRemove)
        {
            questionsContainer.Remove(child);
        }
    }

    private void CreateQuestionDisplay(Question question, int questionNumber)
    {
        // Clone the template
        var questionElement = CloneTemplate();
        if (questionElement == null) return;

        // Make it visible
        questionElement.style.display = DisplayStyle.Flex;
        questionElement.name = $"Question_{questionNumber}";

        // Update question text
        var questionLabel = questionElement.Q<Label>("Question");
        if (questionLabel != null)
        {
            questionLabel.text = $"Q{questionNumber}: {question.q}";
        }

        // Update option labels
        var optionsContainer = questionElement.Q<VisualElement>("OptionsContainer");
        if (optionsContainer != null)
        {
            var option1 = optionsContainer.Q<Label>("Option1");
            var option2 = optionsContainer.Q<Label>("Option2");
            var option3 = optionsContainer.Q<Label>("Option3");
            var option4 = optionsContainer.Q<Label>("Option4");

            if (option1 != null) 
            {
                option1.text = $"A) {question.a1}";
                // Highlight correct answer in green
                if (string.Equals(question.a1.Trim(), question.correct.Trim(), StringComparison.Ordinal))
                {
                    option1.style.color = new Color(0.5f, 1f, 0.5f); // Light green
                }
            }
            
            if (option2 != null) 
            {
                option2.text = $"B) {question.a2}";
                if (string.Equals(question.a2.Trim(), question.correct.Trim(), StringComparison.Ordinal))
                {
                    option2.style.color = new Color(0.5f, 1f, 0.5f); // Light green
                }
            }
            
            if (option3 != null) 
            {
                option3.text = $"C) {question.a3}";
                if (string.Equals(question.a3.Trim(), question.correct.Trim(), StringComparison.Ordinal))
                {
                    option3.style.color = new Color(0.5f, 1f, 0.5f); // Light green
                }
            }
            
            if (option4 != null) 
            {
                option4.text = $"D) {question.a4}";
                if (string.Equals(question.a4.Trim(), question.correct.Trim(), StringComparison.Ordinal))
                {
                    option4.style.color = new Color(0.5f, 1f, 0.5f); // Light green
                }
            }
        }

        // Add to container
        questionsContainer.Add(questionElement);
    }

    private VisualElement CloneTemplate()
    {
        if (questionTemplate == null) return null;

        // Create a new VisualElement with the same structure as the template
        var clone = new VisualElement();
        
        // Copy the style from the template
        clone.style.width = questionTemplate.style.width;
        clone.style.justifyContent = questionTemplate.style.justifyContent;
        clone.style.flexDirection = questionTemplate.style.flexDirection;
        clone.style.marginBottom = questionTemplate.style.marginBottom;

        // Create and add question label with explicit styling
        var questionLabel = new Label();
        questionLabel.name = "Question";
        // Apply explicit styling to match the template (using font definition)
        var fontAsset = Resources.Load<UnityEngine.Font>("FredokaOne-Regular");
        if (fontAsset == null)
        {
            // Try alternative path
            fontAsset = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Font>()
                .FirstOrDefault(f => f.name.Contains("FredokaOne"));
        }
        if (fontAsset != null)
        {
            questionLabel.style.unityFontDefinition = FontDefinition.FromFont(fontAsset);
        }
        questionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        questionLabel.style.fontSize = 80; // 80px font size for questions
        questionLabel.style.color = Color.white;
        questionLabel.style.unityTextAlign = TextAnchor.UpperCenter;
        questionLabel.style.whiteSpace = WhiteSpace.Normal;
        questionLabel.style.paddingTop = 250;
        questionLabel.style.marginBottom = Length.Percent(3);
        questionLabel.style.paddingLeft = Length.Percent(2);
        questionLabel.style.paddingRight = Length.Percent(2);
        clone.Add(questionLabel);

        // Create options container
        var optionsContainer = new VisualElement();
        optionsContainer.name = "OptionsContainer";
        optionsContainer.style.width = Length.Percent(100);
        optionsContainer.style.justifyContent = Justify.FlexStart;
        optionsContainer.style.flexDirection = FlexDirection.Column;
        optionsContainer.style.paddingLeft = Length.Percent(4);
        optionsContainer.style.paddingRight = Length.Percent(4);

        // Create option labels with explicit styling
        for (int i = 1; i <= 4; i++)
        {
            var optionLabel = new Label();
            optionLabel.name = $"Option{i}";
            // Apply explicit styling for options (using font definition)
            if (fontAsset != null)
            {
                optionLabel.style.unityFontDefinition = FontDefinition.FromFont(fontAsset);
            }
            optionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            optionLabel.style.fontSize = 50; // Updated font size for options
            optionLabel.style.color = Color.white;
            optionLabel.style.unityTextAlign = TextAnchor.UpperLeft;
            optionLabel.style.whiteSpace = WhiteSpace.Normal;
            optionLabel.style.marginBottom = Length.Percent(2.5f);
            optionLabel.style.paddingLeft = Length.Percent(1);
            optionLabel.style.paddingRight = Length.Percent(1);
            optionsContainer.Add(optionLabel);
        }

        clone.Add(optionsContainer);
        return clone;
    }



    // Public method to switch languages
    public void SwitchLanguage(bool useTagalog)
    {
        isTagalog = useTagalog;
        GenerateQuestionDisplays();
    }

    // Public method to refresh the display
    public void RefreshDisplay()
    {
        GenerateQuestionDisplays();
    }

    // Public method to toggle Quiz History visibility
    public void ToggleQuizHistory()
    {
        if (isQuizHistoryVisible)
        {
            HideQuizHistory();
        }
        else
        {
            ShowQuizHistory();
        }
    }

    // Public method that can be called directly from Unity OnClick events
    public void OnQuestionHistoryButtonClick()
    {
        ShowQuizHistory();
        Debug.Log("Quiz History opened via OnClick event");
    }

    void OnDestroy()
    {
        // Clean up event handlers
        if (closeButton != null)
        {
            closeButton.clicked -= HideQuizHistory;
        }

        // Restore any hidden UI elements when this component is destroyed
        ShowOtherUIElements();
    }
}
