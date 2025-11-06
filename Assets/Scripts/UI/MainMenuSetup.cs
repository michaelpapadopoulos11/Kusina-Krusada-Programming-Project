using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// This script helps set up the main menu UI system.
/// Place this on a GameObject in your scene, then assign the references in the inspector.
/// </summary>
public class MainMenuSetup : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private UIDocument mainMenuUIDocument;
    [SerializeField] private UIDocument quizHistoryUIDocument;
    
    [Header("CSV Files")]
    [SerializeField] private TextAsset questionsCsv;
    [SerializeField] private TextAsset questionsTlCsv;
    
    [Header("Other UI Documents to Hide")]
    [SerializeField] private UIDocument[] otherUIDocuments;
    
    private QuizHistoryManager quizHistoryManager;
    private UIMainMenuButtons menuController;

    void Start()
    {
        SetupComponents();
    }

    private void SetupComponents()
    {
        // Add UIMainMenuButtons if it doesn't exist
        menuController = GetComponent<UIMainMenuButtons>();
        if (menuController == null)
        {
            menuController = gameObject.AddComponent<UIMainMenuButtons>();
        }

        // Add QuizHistoryManager if it doesn't exist
        quizHistoryManager = GetComponent<QuizHistoryManager>();
        if (quizHistoryManager == null)
        {
            quizHistoryManager = gameObject.AddComponent<QuizHistoryManager>();
        }

        // Use reflection to set private fields (since they're SerializeField)
        SetPrivateField(menuController, "quizHistoryManager", quizHistoryManager);
        
        SetPrivateField(quizHistoryManager, "uiDocument", quizHistoryUIDocument);
        SetPrivateField(quizHistoryManager, "questionsCsv", questionsCsv);
        SetPrivateField(quizHistoryManager, "questionsTlCsv", questionsTlCsv);
        SetPrivateField(quizHistoryManager, "otherUIDocuments", otherUIDocuments);

        Debug.Log("Main Menu setup completed!");
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogWarning($"Field '{fieldName}' not found in {target.GetType().Name}");
        }
    }

    [ContextMenu("Auto-Find UIDocuments")]
    public void AutoFindUIDocuments()
    {
        // Try to find UIDocuments in the scene
        UIDocument[] allUIDocuments = FindObjectsOfType<UIDocument>();
        
        foreach (var uiDoc in allUIDocuments)
        {
            if (uiDoc.visualTreeAsset != null)
            {
                string assetName = uiDoc.visualTreeAsset.name;
                
                if (assetName.Contains("Menu") && mainMenuUIDocument == null)
                {
                    mainMenuUIDocument = uiDoc;
                    Debug.Log($"Found main menu UI document: {assetName}");
                }
                else if (assetName.Contains("Quiz_History") && quizHistoryUIDocument == null)
                {
                    quizHistoryUIDocument = uiDoc;
                    Debug.Log($"Found quiz history UI document: {assetName}");
                }
            }
        }
        
        // Collect other UI documents (excluding the main ones)
        var otherDocs = new System.Collections.Generic.List<UIDocument>();
        foreach (var uiDoc in allUIDocuments)
        {
            if (uiDoc != mainMenuUIDocument && uiDoc != quizHistoryUIDocument)
            {
                otherDocs.Add(uiDoc);
            }
        }
        otherUIDocuments = otherDocs.ToArray();
        
        Debug.Log($"Auto-found {allUIDocuments.Length} UI documents in scene");
    }
}