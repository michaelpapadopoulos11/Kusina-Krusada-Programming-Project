using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class QuizQuestionUIInteraction : MonoBehaviour
{
    [SerializeField] UIDocument uIDocument;
    private VisualElement root;
    public GameObject Quiz;
    [SerializeField] TextAsset questionsCsv; // assign questions.csv in the Inspector
    [SerializeField] TextAsset questionsTlCsv; // assign questions_tl.csv in the Inspector (Tagalog)
    private TextAsset activeQuestionsCsv; // currently selected CSV (English by default)

    // cached UI elements
    private Label questionLabel;
    private Button button1;
    private Button button2;
    private Button button3;
    private Button button4;
    private Button tagalogButton;
    private Button englishButton;

    private string currentCorrectAnswer;
    private bool handlersAttached = false;
    private bool answerAccepted = false; // when true, further answer clicks are ignored until next question
    private Dictionary<Button, bool> buttonIsCorrect = new Dictionary<Button, bool>();
    private List<Question> parsedQuestions = new List<Question>();
    private List<Question> parsedQuestionsTl = new List<Question>();
    private int lastQuestionIndex = -1;
    private bool activeIsTagalog = false;
    private int currentQuestionPoints = 100; // Points available for current question

    [SerializeField] private GameObject UIFade;

    // stored delegates so we can unsubscribe cleanly
    private System.Action onBtn1;
    private System.Action onBtn2;
    private System.Action onBtn3;
    private System.Action onBtn4;
    private System.Action onTagalog;
    private System.Action onEnglish;

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

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(UISettings.isEnglish); //This is a check for the global variable
        UIFade.SetActive(false);

        if (uIDocument == null)
        {
            Debug.LogError("UIDocument is not assigned on " + gameObject.name);
        }
        else
        {
            root = uIDocument.rootVisualElement;
            if (root != null)
                root.style.display = DisplayStyle.None; // Hide UI on startup
            else
                Debug.LogWarning("UIDocument.rootVisualElement is null at Start(); the visual tree may not be built yet.");
        }

        // select default CSV (English) and parse both English and Tagalog (if provided)
        activeQuestionsCsv = questionsCsv;
        parsedQuestions = ParseQuestionsFromCsv(questionsCsv);
        if (questionsTlCsv != null)
            parsedQuestionsTl = ParseQuestionsFromCsv(questionsTlCsv);

    }

    IEnumerator NewRoutine()
    {
        yield return new WaitForSeconds(2.5f);
        if (UIFade != null) UIFade.SetActive(true);

        Time.timeScale = 0f; // Pause the game
            UnityEngine.Cursor.lockState = CursorLockMode.None; // Unlock the cursor
            UnityEngine.Cursor.visible = true; // Make the cursor visible

            if (!handlersAttached)
                CacheUIElements();

            // Ensure language buttons show correctly (English by default)
            UpdateLanguageButtonsVisibility();

            // Load a random question from parsed CSV and populate UI
            LoadRandomQuestionFromCsv();

        // Show UI
        if (root != null) root.style.display = DisplayStyle.Flex;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if player is invincible - if so, destroy this quiz trigger and don't show quiz
            Movement player = other.GetComponent<Movement>();
            if (player != null && player.isInvincible)
            {
                Debug.Log("Player is invincible - destroying quiz trigger without showing quiz");
                Destroy(gameObject);
                return;
            }
            
            // Player is vulnerable, show the quiz as normal
            UIFade.SetActive(true);
            StartCoroutine(NewRoutine());

            UIScore.gameIsPaused = true;
        }
    }

    private void CacheUIElements()
    {
        // Cache UI elements only once for better performance
        if (questionLabel == null) questionLabel = root.Q<Label>("question_text");
        if (button1 == null) button1 = root.Q<Button>("Button1");
        if (button2 == null) button2 = root.Q<Button>("Button2");
        if (button3 == null) button3 = root.Q<Button>("Button3");
        if (button4 == null) button4 = root.Q<Button>("Button4");
        if (tagalogButton == null) tagalogButton = root.Q<Button>("Tagalog");
        if (englishButton == null) englishButton = root.Q<Button>("English");
        
        if (UISettings.isEnglish is false)
        {
            SwitchToTagalog();
        }

        // Attach handlers only once - use proper event unsubscription pattern
        AttachEventHandlers();

        // set initial visibility: English button hidden, Tagalog visible
        UpdateLanguageButtonsVisibility();

        handlersAttached = true;
    }
    
    private void AttachEventHandlers()
    {
        // Store delegates for proper cleanup
        onBtn1 = () => { if (!answerAccepted) StartCoroutine(OnAnswerSelected(button1)); };
        onBtn2 = () => { if (!answerAccepted) StartCoroutine(OnAnswerSelected(button2)); };
        onBtn3 = () => { if (!answerAccepted) StartCoroutine(OnAnswerSelected(button3)); };
        onBtn4 = () => { if (!answerAccepted) StartCoroutine(OnAnswerSelected(button4)); };
        onTagalog = SwitchToTagalog;
        onEnglish = SwitchToEnglish;

        if (button1 != null) button1.clicked += onBtn1;
        if (button2 != null) button2.clicked += onBtn2;
        if (button3 != null) button3.clicked += onBtn3;
        if (button4 != null) button4.clicked += onBtn4;
        if (tagalogButton != null) tagalogButton.clicked += onTagalog;
        if (englishButton != null) englishButton.clicked += onEnglish;
    }

    private void OnDisable()
    {
        // Unsubscribe UI handlers to avoid callbacks into destroyed objects
        if (handlersAttached)
        {
            if (button1 != null && onBtn1 != null) button1.clicked -= onBtn1;
            if (button2 != null && onBtn2 != null) button2.clicked -= onBtn2;
            if (button3 != null && onBtn3 != null) button3.clicked -= onBtn3;
            if (button4 != null && onBtn4 != null) button4.clicked -= onBtn4;
            if (tagalogButton != null && onTagalog != null) tagalogButton.clicked -= onTagalog;
            if (englishButton != null && onEnglish != null) englishButton.clicked -= onEnglish;
            handlersAttached = false;
        }

        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        OnDisable();
    }

    private void LoadFirstQuestionFromCsv()
    {
        // Deprecated: previously loaded only the first CSV row. Kept for compatibility but not used.
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
            var q = new Question()
            {
                q = cols[0].Trim(),
                a1 = cols[1].Trim(),
                a2 = cols[2].Trim(),
                a3 = cols[3].Trim(),
                a4 = cols[4].Trim(),
                correct = cols[5].Trim()
            };
            list.Add(q);
        }
        if (list.Count == 0)
            Debug.LogError("No valid question rows found in provided CSV");
        return list;
    }

    private void SwitchToTagalog()
    {
        // switch active CSV to Tagalog and reparse
        if (questionsTlCsv == null)
        {
            Debug.LogError("Tagalog CSV not assigned in Inspector.");
            return;
        }

        activeIsTagalog = true;
        // Preserve current question index if possible so translations line up
        if (root != null && root.style.display == DisplayStyle.Flex)
        {
            if (lastQuestionIndex >= 0 && parsedQuestionsTl != null && lastQuestionIndex < parsedQuestionsTl.Count)
                LoadQuestionByIndex(lastQuestionIndex, true);
            else
                LoadRandomQuestionFromCsv();
        }
        UpdateLanguageButtonsVisibility();
    }

    private void SwitchToEnglish()
    {
        activeIsTagalog = false;
        if (root != null && root.style.display == DisplayStyle.Flex)
        {
            if (lastQuestionIndex >= 0 && parsedQuestions != null && lastQuestionIndex < parsedQuestions.Count)
                LoadQuestionByIndex(lastQuestionIndex, false);
            else
                LoadRandomQuestionFromCsv();
        }
        UpdateLanguageButtonsVisibility();
    }

    private void UpdateLanguageButtonsVisibility()
    {
        // When Tagalog is active, show English button and hide Tagalog button, and vice-versa.
        if (tagalogButton != null)
            tagalogButton.style.display = activeIsTagalog ? DisplayStyle.None : DisplayStyle.Flex;
        if (englishButton != null)
            englishButton.style.display = activeIsTagalog ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void LoadQuestionByIndex(int index, bool isTagalog)
    {
        var list = isTagalog ? parsedQuestionsTl : parsedQuestions;
        if (list == null || list.Count == 0)
        {
            Debug.LogError("No parsed questions available to load for requested language.");
            return;
        }

        if (index < 0 || index >= list.Count)
        {
            // fallback to random
            LoadRandomQuestionFromCsv();
            return;
        }

        // prepare and clear any leftover state from previous questions
        ResetButtons();

        lastQuestionIndex = index;
        var item = list[index];
        currentCorrectAnswer = item.correct;

    // reset answer acceptance for the new question and ensure buttons enabled
        answerAccepted = false;
        currentQuestionPoints = 100; // Reset points for new question
        if (button1 != null) button1.SetEnabled(true);
        if (button2 != null) button2.SetEnabled(true);
        if (button3 != null) button3.SetEnabled(true);
        if (button4 != null) button4.SetEnabled(true);

        if (questionLabel != null) questionLabel.text = item.q;
        
        // Use optimized button property setting
        SetButtonProperties(button1, item.a1, string.Equals(item.a1.Trim(), item.correct.Trim(), StringComparison.Ordinal));
        SetButtonProperties(button2, item.a2, string.Equals(item.a2.Trim(), item.correct.Trim(), StringComparison.Ordinal));
        SetButtonProperties(button3, item.a3, string.Equals(item.a3.Trim(), item.correct.Trim(), StringComparison.Ordinal));
        SetButtonProperties(button4, item.a4, string.Equals(item.a4.Trim(), item.correct.Trim(), StringComparison.Ordinal));
    }

    // Optimize button updates by batching operations
    private void SetButtonProperties(Button button, string text, bool isCorrect, bool enabled = true)
    {
        if (button == null) return;
        
        button.text = text;
        button.SetEnabled(enabled);
        buttonIsCorrect[button] = isCorrect;
    }

    private void LoadRandomQuestionFromCsv()
    {
        var list = activeIsTagalog ? parsedQuestionsTl : parsedQuestions;
        if (list == null || list.Count == 0)
        {
            Debug.LogError("No parsed questions available to load for current language.");
            return;
        }

        int index = UnityEngine.Random.Range(0, list.Count);
        // avoid immediate repeat when possible
        if (list.Count > 1 && index == lastQuestionIndex)
        {
            int attempts = 0;
            while (index == lastQuestionIndex && attempts < 5)
            {
                index = UnityEngine.Random.Range(0, list.Count);
                attempts++;
            }
        }

    // prepare and clear any leftover state from previous questions
    ResetButtons();

    lastQuestionIndex = index;
    var item = list[index];
    currentCorrectAnswer = item.correct;

    // reset answer acceptance for the new question and ensure buttons enabled
    answerAccepted = false;
    currentQuestionPoints = 100; // Reset points for new question
    if (button1 != null) button1.SetEnabled(true);
    if (button2 != null) button2.SetEnabled(true);
    if (button3 != null) button3.SetEnabled(true);
    if (button4 != null) button4.SetEnabled(true);

    if (questionLabel != null) questionLabel.text = item.q;
    
    // Use optimized button property setting
    SetButtonProperties(button1, item.a1, string.Equals(item.a1.Trim(), item.correct.Trim(), StringComparison.Ordinal));
    SetButtonProperties(button2, item.a2, string.Equals(item.a2.Trim(), item.correct.Trim(), StringComparison.Ordinal));
    SetButtonProperties(button3, item.a3, string.Equals(item.a3.Trim(), item.correct.Trim(), StringComparison.Ordinal));
    SetButtonProperties(button4, item.a4, string.Equals(item.a4.Trim(), item.correct.Trim(), StringComparison.Ordinal));
    }

    // Cached StringBuilder for CSV parsing to reduce allocations
    private readonly System.Text.StringBuilder csvStringBuilder = new System.Text.StringBuilder();
    
    // Cached Length values to avoid repeated allocations
    private static readonly Length FontSize75Percent = new Length(75, LengthUnit.Percent);
    private static readonly Length FontSize200Percent = new Length(200, LengthUnit.Percent);

    private string[] SplitCsvLine(string line)
    {
        // Parse CSV line handling quoted fields (fields may contain commas inside quotes)
        var fields = new List<string>();
        bool inQuotes = false;
        csvStringBuilder.Clear(); // Reuse StringBuilder to reduce allocations
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                // peek next for escaped quote
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    csvStringBuilder.Append('"');
                    i++; // skip escaped quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(csvStringBuilder.ToString());
                csvStringBuilder.Clear();
            }
            else
            {
                csvStringBuilder.Append(c);
            }
        }
        fields.Add(csvStringBuilder.ToString());
        return fields.ToArray();
    }

    private IEnumerator OnAnswerSelected(Button clicked)
    {
        // use the precomputed correctness mapping to avoid relying on button text
        bool isCorrect = false;
        if (buttonIsCorrect != null && clicked != null && buttonIsCorrect.ContainsKey(clicked))
            isCorrect = buttonIsCorrect[clicked];

        if (isCorrect)
        {
            // correct
            Debug.Log("Correct answer selected.");
            // Award the remaining points for this question
            ScoreManager.AddPoints(currentQuestionPoints);
            Debug.Log($"Awarded {currentQuestionPoints} points for correct answer.");
            // prevent further input
            answerAccepted = true;

            UnityEngine.Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
            UnityEngine.Cursor.visible = false; // Hide the cursor

            // Update only the clicked button
            clicked.text = "CORRECT!";
            clicked.style.fontSize = FontSize200Percent;

            // disable other buttons to avoid further clicks
            if (button1 != null && button1 != clicked) button1.SetEnabled(false);
            if (button2 != null && button2 != clicked) button2.SetEnabled(false);
            if (button3 != null && button3 != clicked) button3.SetEnabled(false);
            if (button4 != null && button4 != clicked) button4.SetEnabled(false);

            Time.timeScale = 1f; // Resume the game
            yield return new WaitForSeconds(1.5f); // wait a moment to show "Correct!" message
            if (root != null)
                root.style.display = DisplayStyle.None; // Hide the UI

            ResetButtons();
            UIScore.gameIsPaused = false;
            UIFade.SetActive(false);
            answerAccepted = false;
            
            // Destroy the quiz object after correct answer
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Incorrect");
            // Reduce available points for this question by 20
            currentQuestionPoints = Mathf.Max(0, currentQuestionPoints - 20);
            Debug.Log($"Wrong answer! Points for this question reduced to {currentQuestionPoints}.");
            // mark only the clicked button as incorrect
            clicked.text = "INCORRECT!";
            clicked.style.fontSize = FontSize200Percent;
            // keep game paused and UI shown
        }
    }

    // Reset visual state of answer buttons so new questions start clean
    private void ResetButtons()
    {
        // clear previous correctness mapping
        buttonIsCorrect.Clear();
        
        // Optimized button reset using helper method
        ResetSingleButton(button1);
        ResetSingleButton(button2);
        ResetSingleButton(button3);
        ResetSingleButton(button4);
    }
    
    // Helper method to reset individual button properties
    private void ResetSingleButton(Button button)
    {
        if (button == null) return;
        
        button.text = string.Empty;
        button.style.fontSize = FontSize75Percent;
        button.SetEnabled(true);
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

        UIFade.SetActive(true);

        // then show the UI (if not already)
        root.style.display = DisplayStyle.Flex;
    }
    
    // private void OnClickEvent(ClickEvent evt)
    // {

    // }

    // Update method removed for performance - no per-frame updates needed
}
