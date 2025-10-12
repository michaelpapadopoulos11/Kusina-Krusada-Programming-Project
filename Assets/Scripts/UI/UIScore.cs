using UnityEngine;
using UnityEngine.UI;
using System;


//[ExecuteInEditMode()]
public class UIScore : MonoBehaviour
{
    [SerializeField] private Text txtScore;
    [SerializeField] private int multiplier = 100;
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
        increaseScore();
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

    private void increaseScore()
    {
        score += multiplier * Time.deltaTime * (cur/100);
    }
    private void setScoreText()
    {
        txtScore.text = Convert.ToString(Math.Round(score, 0));
    }
}
