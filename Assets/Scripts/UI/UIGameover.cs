using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.UIElements;

public class UIGameover : MonoBehaviour
{
    [SerializeField] private Text textGameoverScore;
    [SerializeField] private Text textScoreCount;
    [SerializeField] private int lifeCount = 3;
    [SerializeField] private RectTransform panelGameover; //panel for the entire gameover
    [SerializeField] private float panelOvershoot = 0.1f;
    [SerializeField] private float panelEntrySpeed = 10f;
    [SerializeField] private float panelCorrectionSpeed = 0.5f;
    [SerializeField] private bool isGameover = false;
    [SerializeField] private bool isPanelEntry = true;
    [SerializeField] private bool isPanelCorrection = true;
    // Start is called before the first frame update
    void Start()
    {
        isGameover = false;
        isPanelEntry = true;
        isPanelCorrection = true;
        panelGameover.pivot = new Vector2(0.5f, -2.0f); //moves the gameover panel out of frame
    }

    // Update is called once per frame
    void Update()
    {

        //TODO change this to the quiz life system <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        if (Convert.ToInt64(textScoreCount.text) == -1)
        {
            isGameover = true;
            UIScore.gameIsPaused = true;
        }
        displayGameover();
        //panelGameover.pivot += new Vector2(0, panelEntrySpeed * Time.deltaTime);
    }

    private void displayGameover()
    {
        if (!isGameover)
        {
            return;
        }

        if (isPanelEntry)
        {
            panelGameover.pivot += new Vector2(0, panelEntrySpeed * Time.deltaTime);
            if (panelGameover.pivot.y >= 0.5 + panelOvershoot) { isPanelEntry = false; }
        }

        if (isPanelCorrection && !isPanelEntry)
        {
            panelGameover.pivot -= new Vector2(0, panelCorrectionSpeed * Time.deltaTime);
            if (panelGameover.pivot.y <= 0.5) { isPanelCorrection = false; }
        }

        if (!isPanelCorrection && !isPanelCorrection)
        {
            if (Convert.ToInt32(textScoreCount.text) >= 10)
            {
                textScoreCount.text = Convert.ToString(Convert.ToInt32(textScoreCount.text) - 10);
                textGameoverScore.text = Convert.ToString(Convert.ToInt32(textGameoverScore.text) + 10);
            }
        }
    }
}
