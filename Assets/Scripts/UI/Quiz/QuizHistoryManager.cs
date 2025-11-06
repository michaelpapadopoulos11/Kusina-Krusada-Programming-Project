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
    
    // Cache font asset to avoid repeated loading
    private UnityEngine.Font cachedFontAsset;
    
    private void LoadAndCacheFontAsset()
    {
        if (cachedFontAsset == null)
        {
            cachedFontAsset = Resources.Load<UnityEngine.Font>("FredokaOne-Regular");
            if (cachedFontAsset == null)
            {
                // Try alternative path
                cachedFontAsset = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Font>()
                    .FirstOrDefault(f => f.name.Contains("FredokaOne"));
            }
        }
    }

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

        // Subscribe to language change events
        UILanguage.OnLanguageChanged += OnLanguageChangedHandler;

        // Get references to UI elements
        questionsScrollView = root.Q<ScrollView>("QuestionsScrollView");
        questionsContainer = root.Q<VisualElement>("QuestionsContainer");
        questionTemplate = root.Q<VisualElement>("QuestionTemplate");
        closeButton = root.Q<Button>("CloseButton");

        // Debug logging for mobile builds
        Debug.Log($"Quiz History UI Elements Found - ScrollView: {questionsScrollView != null}, Container: {questionsContainer != null}, Template: {questionTemplate != null}, CloseButton: {closeButton != null}");

        // Hide the scrollbar
        if (questionsScrollView != null)
        {
            questionsScrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            questionsScrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        }

        if (questionsContainer == null || questionTemplate == null)
        {
            Debug.LogError("Required UI elements not found. Make sure QuestionsContainer and QuestionTemplate exist in the UXML.");
            return;
        }

        // If close button is not found, create a fallback one
        if (closeButton == null)
        {
            CreateFallbackCloseButton();
        }

        // Attach close button event and ensure it's visible on mobile
        if (closeButton != null)
        {
            closeButton.clicked += HideQuizHistory;
            
            // Ensure button is visible and has proper mobile styling
            EnsureCloseButtonVisibility();
        }

        // Cache font asset to improve performance
        LoadAndCacheFontAsset();
        
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
        // Allow closing with multiple input methods when Quiz History is visible
        // Only check for input when the UI is visible to reduce Update overhead
        if (isQuizHistoryVisible)
        {
            // PC: Escape key
            if (Input.GetKeyDown(closeKey))
            {
                HideQuizHistory();
            }
            
            // Mobile: Android back button
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideQuizHistory();
            }
            
            // Additional check: if close button becomes null, try to re-find it
            if (closeButton == null && root != null)
            {
                closeButton = root.Q<Button>("CloseButton");
                if (closeButton != null)
                {
                    closeButton.clicked += HideQuizHistory;
                    EnsureCloseButtonVisibility();
                    Debug.Log("Close button re-found and re-attached");
                }
            }
        }
    }

    public void ShowQuizHistory()
    {
        if (isQuizHistoryVisible) return;

        // Prevent opening if settings menu is open
        if (UIMainMenuButtons.IsSettingsOpen)
        {
            Debug.Log("Cannot open Questions History while Settings menu is open");
            return;
        }

        // Check current language setting and update if needed
        try
        {
            bool shouldUseTagalog = !UILanguage.isEnglish;
            if (isTagalog != shouldUseTagalog)
            {
                SwitchLanguage(shouldUseTagalog);
            }
        }
        catch
        {
            // UILanguage might not be available, keep current setting
            Debug.Log("UILanguage not available when showing Quiz History");
        }

        // Hide other UI elements
        HideOtherUIElements();

        // Show Quiz History UI
        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;
            isQuizHistoryVisible = true;
            
            // Re-ensure close button visibility when showing UI (mobile fix)
            EnsureCloseButtonVisibility();
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

    // Cache canvases to avoid expensive FindObjectsOfType calls
    private Canvas[] cachedCanvases;
    private bool canvasesCached = false;

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

        // Cache canvases on first use to avoid repeated FindObjectsOfType calls
        if (!canvasesCached)
        {
            cachedCanvases = FindObjectsOfType<Canvas>();
            canvasesCached = true;
        }

        // Also hide any Canvas elements that are not the Quiz History
        foreach (var canvas in cachedCanvases)
        {
            if (canvas != null && canvas.gameObject != this.gameObject && 
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

    // Cache main menu UI elements to avoid repeated searching
    private UIMainMenuButtons cachedMainMenuButtons;
    private GameObject[] cachedCommonUIElements;
    private bool commonUIElementsCached = false;

    private void AutoDetectAndHideUIElements()
    {
        // Cache common UI elements on first use to improve performance
        if (!commonUIElementsCached)
        {
            CacheCommonUIElements();
            commonUIElementsCached = true;
        }

        // Hide cached common UI elements
        foreach (var uiElement in cachedCommonUIElements)
        {
            if (uiElement != null && uiElement.activeInHierarchy && 
                uiElement != this.gameObject && uiElement != uiDocument.gameObject)
            {
                uiElement.SetActive(false);
                hiddenUIElements.Add(uiElement);
            }
        }

        // Use cached main menu buttons reference
        if (cachedMainMenuButtons == null)
        {
            cachedMainMenuButtons = FindObjectOfType<UIMainMenuButtons>();
        }

        if (cachedMainMenuButtons != null && cachedMainMenuButtons.gameObject.activeInHierarchy)
        {
            var parentCanvas = cachedMainMenuButtons.GetComponentInParent<Canvas>();
            if (parentCanvas != null && parentCanvas.gameObject.activeInHierarchy && 
                !hiddenUIElements.Contains(parentCanvas.gameObject))
            {
                parentCanvas.gameObject.SetActive(false);
                hiddenUIElements.Add(parentCanvas.gameObject);
            }
        }
    }

    private void CacheCommonUIElements()
    {
        string[] commonUINames = { 
            "MainMenu", "MainMenuUI", "MainMenuCanvas", "MenuCanvas", 
            "UIMainMenu", "Menu", "MainPanel", "MenuPanel", "HUD", "UI"
        };

        var foundElements = new System.Collections.Generic.List<GameObject>();
        foreach (string uiName in commonUINames)
        {
            var foundObject = GameObject.Find(uiName);
            if (foundObject != null)
            {
                foundElements.Add(foundObject);
            }
        }

        cachedCommonUIElements = foundElements.ToArray();
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
        // Apply explicit styling using cached font asset for better performance
        if (cachedFontAsset != null)
        {
            questionLabel.style.unityFontDefinition = FontDefinition.FromFont(cachedFontAsset);
        }
        questionLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
        questionLabel.style.fontSize = 110; // 80px font size for questions
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
            // Apply explicit styling for options using cached font asset
            if (cachedFontAsset != null)
            {
                optionLabel.style.unityFontDefinition = FontDefinition.FromFont(cachedFontAsset);
            }
            optionLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
            optionLabel.style.fontSize = 70; // Updated font size for options
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
        // Double-check settings state before opening
        if (UIMainMenuButtons.IsSettingsOpen)
        {
            Debug.Log("Questions History button clicked but Settings menu is open - access blocked");
            return;
        }
        
        ShowQuizHistory();
        Debug.Log("Quiz History opened via OnClick event");
    }

    /// <summary>
    /// Debug method to test close button visibility - can be called from Unity Inspector
    /// </summary>
    [ContextMenu("Test Close Button Visibility")]
    public void TestCloseButtonVisibility()
    {
        if (closeButton != null)
        {
            Debug.Log($"Close Button Found: {closeButton.name}");
            Debug.Log($"Button Text: '{closeButton.text}'");
            Debug.Log($"Button Display: {closeButton.style.display.value}");
            Debug.Log($"Button Visibility: {closeButton.style.visibility.value}");
            Debug.Log($"Button Size: {closeButton.style.width.value} x {closeButton.style.height.value}");
            Debug.Log($"Button Position: Top={closeButton.style.top.value}, Right={closeButton.style.right.value}");
        }
        else
        {
            Debug.LogError("Close button is NULL!");
        }
    }

    /// <summary>
    /// Ensure the close button is visible and properly styled for mobile devices
    /// </summary>
    private void EnsureCloseButtonVisibility()
    {
        if (closeButton == null) return;

        // Force button visibility
        closeButton.style.display = DisplayStyle.Flex;
        closeButton.style.visibility = Visibility.Visible;
        
        // Set fallback text in case the ✕ character doesn't render on mobile
        if (string.IsNullOrEmpty(closeButton.text) || closeButton.text == "✕")
        {
            closeButton.text = "X"; // Fallback to simple X character
        }
        
        // Ensure minimum touch target size for mobile (44px minimum recommended)
        if (closeButton.style.width.value.value < 44f)
        {
            closeButton.style.width = 104;
            closeButton.style.height = 100;
        }
        
        // Ensure button has proper z-index and positioning
        closeButton.style.position = Position.Relative;
        closeButton.BringToFront();
        
        // Make sure background color is visible
        closeButton.style.backgroundColor = new Color(1f, 0f, 0f, 0.7f); // Red with transparency
        closeButton.style.color = Color.white;
        
        Debug.Log("Close button visibility ensured for mobile build");
    }

    /// <summary>
    /// Create a fallback close button if the original one is not found
    /// </summary>
    private void CreateFallbackCloseButton()
    {
        if (root == null) return;

        Debug.Log("Creating fallback close button for mobile compatibility");

        // Create a new close button
        closeButton = new Button();
        closeButton.name = "FallbackCloseButton";
        closeButton.text = "X";
        
        // Style the fallback button
        closeButton.style.position = Position.Absolute;
        closeButton.style.top = 20;
        closeButton.style.right = 20;
        closeButton.style.width = 80;
        closeButton.style.height = 80;
        closeButton.style.fontSize = 40;
        closeButton.style.color = Color.white;
        closeButton.style.backgroundColor = new Color(1f, 0f, 0f, 0.8f);
        closeButton.style.borderTopLeftRadius = 25;
        closeButton.style.borderTopRightRadius = 25;
        closeButton.style.borderBottomLeftRadius = 25;
        closeButton.style.borderBottomRightRadius = 25;
        closeButton.style.unityTextAlign = TextAnchor.MiddleCenter;
        closeButton.style.unityFontStyleAndWeight = FontStyle.Bold;
        
        // Add to root element
        root.Add(closeButton);
        
        Debug.Log("Fallback close button created and added to UI");
    }

    /// <summary>
    /// Handle language change events from UILanguage
    /// </summary>
    /// <param name="isEnglish">True if English is selected, false if Tagalog</param>
    private void OnLanguageChangedHandler(bool isEnglish)
    {
        // Safety check - only update if we're fully initialized
        if (parsedQuestions == null && parsedQuestionsTl == null)
        {
            Debug.LogWarning("Quiz History Manager not fully initialized, language change will be applied on next display");
            return;
        }

        bool shouldUseTagalog = !isEnglish;
        if (isTagalog != shouldUseTagalog)
        {
            SwitchLanguage(shouldUseTagalog);
            Debug.Log($"Questions History language updated to: {(isEnglish ? "English" : "Tagalog")}");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from language change events
        UILanguage.OnLanguageChanged -= OnLanguageChangedHandler;
        
        // Clean up event handlers
        if (closeButton != null)
        {
            closeButton.clicked -= HideQuizHistory;
        }

        // Restore any hidden UI elements when this component is destroyed
        ShowOtherUIElements();
    }
}
