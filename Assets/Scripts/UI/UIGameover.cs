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
    [SerializeField] private RectTransform panelGameover; //panel for the entire gameover
    [SerializeField] private float panelOvershoot = 0.1f;
    [SerializeField] private float panelEntrySpeed = 10f;
    [SerializeField] private float panelCorrectionSpeed = 0.5f;
    [SerializeField] private bool isGameover = false;
    [SerializeField] private bool isPanelEntry = true;
    [SerializeField] private bool isPanelCorrection = true;
    
    // Reference to the player's Movement script
    private Movement playerMovement;
    // Start is called before the first frame update
    void Start()
    {
        isGameover = false;
        isPanelEntry = true;
        isPanelCorrection = true;
        panelGameover.pivot = new Vector2(0.5f, -2.0f); //moves the gameover panel out of frame
        
        // Find the player's Movement script
        playerMovement = FindObjectOfType<Movement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("UIGameover: Could not find Movement script in scene!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if player has died (lives <= 0) or the game is over
        if (playerMovement != null && playerMovement.isGameOver)
        {
            if (!isGameover) // Only log once
            {
                Debug.Log("UIGameover: Detected player game over state - showing game over UI");
            }
            isGameover = true;
            UIScore.gameIsPaused = true;
        }
        // Check if all lives are lost through the LifeManager
        else if (LifeManager.IsGameOver())
        {
            if (!isGameover) // Only log once
            {
                Debug.Log("UIGameover: Detected life system game over - showing game over UI");
            }
            isGameover = true;
            UIScore.gameIsPaused = true;
        }
        // Fallback: also check the old score-based system for compatibility
        else if (Convert.ToInt64(textScoreCount.text) == -1)
        {
            if (!isGameover) // Only log once
            {
                Debug.Log("UIGameover: Detected score-based game over - showing game over UI");
            }
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
            float newY = panelGameover.pivot.y + (panelEntrySpeed * Time.deltaTime);
            panelGameover.pivot = new Vector2(panelGameover.pivot.x, Mathf.Min(newY, 0.5f + panelOvershoot));

            if (panelGameover.pivot.y >= 0.5 + panelOvershoot) 
            {
                isPanelEntry = false; 
            }
        }

        if (isPanelCorrection && !isPanelEntry)
        {
            float newY = panelGameover.pivot.y - (panelCorrectionSpeed * Time.deltaTime);
            panelGameover.pivot = new Vector2(panelGameover.pivot.x, Mathf.Max(newY, 0.5f));
            if (panelGameover.pivot.y <= 0.5)
            {
                isPanelCorrection = false; 
            }
        }

        if (!isPanelCorrection && !isPanelEntry)
        {
            if (Convert.ToInt32(textScoreCount.text) >= 10)
            {
                textScoreCount.text = Convert.ToString(Convert.ToInt32(textScoreCount.text) - 10);
                textGameoverScore.text = Convert.ToString(Convert.ToInt32(textGameoverScore.text) + 10);
            }
        }
    }
}
