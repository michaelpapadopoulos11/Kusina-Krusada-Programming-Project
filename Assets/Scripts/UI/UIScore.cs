using UnityEngine;
using UnityEngine.UI;
using System;


//[ExecuteInEditMode()]
public class UIScore : MonoBehaviour
{
    [SerializeField] private Text txtScore;
    [SerializeField] private float score = 0;
    [SerializeField] private Image container;
    [SerializeField] private Image mask;
    [SerializeField] private float max = 100f;
    [SerializeField] private float cur = 0f;
    [SerializeField] private float drainRate = 14.5f;

    public static bool gameIsPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        //text.text = "test";
    }

    // Update is called once per frame
    void Update()
    {
        drainBar();
        setScoreText();
    }

    private void drainBar()
    {
        cur -= drainRate * Time.deltaTime; //decreases by the drainRate/per second
        cur = Mathf.Clamp(cur, 0, max);

        updateBar();
    }

    private void updateBar()
    {
        float fillAmount = cur / max;
        mask.fillAmount = fillAmount; //changes the fill mask on the inner soap bar
    }

    // Time-based scoring removed. Score is now driven by pickups (ScoreManager).
    private void setScoreText()
    {
        // Use ScoreManager.Score (static). If ScoreManager isn't set up, fallback to the local score field.
        int display = ScoreManager.Score != 0 ? ScoreManager.Score : Mathf.RoundToInt(score);
        txtScore.text = Convert.ToString(display);
    }
}
