using UnityEngine;
using UnityEngine.UI;
using System;


//[ExecuteInEditMode()]
public class UIScore : MonoBehaviour
{
    [SerializeField] private Text txtScore;
    [SerializeField] private float score = 0;

    public static bool gameIsPaused = false;
    
    // Performance optimization: cache last displayed score to avoid unnecessary updates
    private int lastDisplayedScore = -1;

    void Awake()
    {
        // Set soapbar icon to hidden by default as early as possible
        ScoreManager.ResetScore();
        gameIsPaused = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        setScoreText();
    }


    // Time-based scoring removed. Score is now driven by pickups (ScoreManager).
    private void setScoreText()
    {
        // Use ScoreManager.Score (static). If ScoreManager isn't set up, fallback to the local score field.
        int display = ScoreManager.Score != 0 ? ScoreManager.Score : Mathf.RoundToInt(score);
        
        // Performance optimization: only update text when score actually changes
        if (display != lastDisplayedScore)
        {
            txtScore.text = PerformanceHelper.FormatScore(display);
            lastDisplayedScore = display;
        }
    }
}
