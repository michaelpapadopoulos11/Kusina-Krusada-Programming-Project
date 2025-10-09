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

    // cached UI elements
    private Label questionLabel;
    private Button button1;
    private Button button2;
    private Button button3;
    private Button button4;

    private string currentCorrectAnswer;
    private bool handlersAttached = false;
    private List<Question> parsedQuestions = new List<Question>();
    private int lastQuestionIndex = -1;

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

        // Parse all questions from the CSV at startup (independent from UI visual tree)
        ParseAllQuestionsFromCsv();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Time.timeScale = 0f; // Pause the game
            UnityEngine.Cursor.lockState = CursorLockMode.None; // Unlock the cursor
            UnityEngine.Cursor.visible = true; // Make the cursor visible
            // Ensure UI elements are cached
            if (root == null && uIDocument != null)
                root = uIDocument.rootVisualElement;

            if (root == null)
            {
                Debug.LogWarning("Cannot show quiz UI because the UIDocument root is null.");
                return;
            }

            if (!handlersAttached)
                CacheUIElements();

            // Load a random question from parsed CSV and populate UI
            LoadRandomQuestionFromCsv();

            // Show UI
            root.style.display = DisplayStyle.Flex;
        }
    }

    private void CacheUIElements()
    {
        questionLabel = root.Q<Label>("question_text");
        button1 = root.Q<Button>("Button1");
        button2 = root.Q<Button>("Button2");
        button3 = root.Q<Button>("Button3");
        button4 = root.Q<Button>("Button4");

        // attach handlers once
        if (button1 != null) button1.clicked += () => OnAnswerSelected(button1.text);
        if (button2 != null) button2.clicked += () => OnAnswerSelected(button2.text);
        if (button3 != null) button3.clicked += () => OnAnswerSelected(button3.text);
        if (button4 != null) button4.clicked += () => OnAnswerSelected(button4.text);

        handlersAttached = true;
    }

    private void LoadFirstQuestionFromCsv()
    {
        // Deprecated: previously loaded only the first CSV row. Kept for compatibility but not used.
    }

    private void ParseAllQuestionsFromCsv()
    {
        parsedQuestions.Clear();
        if (questionsCsv == null)
        {
            Debug.LogError("questionsCsv TextAsset not assigned. Assign the questions.csv file in the Inspector.");
            return;
        }

        var lines = questionsCsv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            Debug.LogError("questions.csv is empty or couldn't be read.");
            return;
        }

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
            parsedQuestions.Add(q);
        }

        if (parsedQuestions.Count == 0)
            Debug.LogError("No valid question rows found in questions.csv");
    }

    private void LoadRandomQuestionFromCsv()
    {
        if (parsedQuestions == null || parsedQuestions.Count == 0)
        {
            Debug.LogError("No parsed questions available to load.");
            return;
        }

        int index = UnityEngine.Random.Range(0, parsedQuestions.Count);
        // avoid immediate repeat when possible
        if (parsedQuestions.Count > 1 && index == lastQuestionIndex)
        {
            int attempts = 0;
            while (index == lastQuestionIndex && attempts < 5)
            {
                index = UnityEngine.Random.Range(0, parsedQuestions.Count);
                attempts++;
            }
        }

        lastQuestionIndex = index;
        var item = parsedQuestions[index];
        currentCorrectAnswer = item.correct;

        if (questionLabel != null) questionLabel.text = item.q;
        if (button1 != null) button1.text = item.a1;
        if (button2 != null) button2.text = item.a2;
        if (button3 != null) button3.text = item.a3;
        if (button4 != null) button4.text = item.a4;
    }

    private string[] SplitCsvLine(string line)
    {
        // Parse CSV line handling quoted fields (fields may contain commas inside quotes)
        var fields = new List<string>();
        bool inQuotes = false;
        var cur = new System.Text.StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                // peek next for escaped quote
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    cur.Append('"');
                    i++; // skip escaped quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(cur.ToString());
                cur.Clear();
            }
            else
            {
                cur.Append(c);
            }
        }
        fields.Add(cur.ToString());
        return fields.ToArray();
    }

    private void OnAnswerSelected(string selectedText)
    {
        if (string.Equals(selectedText.Trim(), currentCorrectAnswer.Trim(), StringComparison.Ordinal))
        {
            // correct
            Debug.Log("Correct answer selected: " + selectedText);
            Time.timeScale = 1f; // Resume the game
            UnityEngine.Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
            UnityEngine.Cursor.visible = false; // Hide the cursor
            if (root != null)
                root.style.display = DisplayStyle.None; // Hide the UI
        }
        else
        {
            Debug.Log("Incorrect");
            // keep game paused and UI shown
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
