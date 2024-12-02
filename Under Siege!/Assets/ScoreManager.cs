using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
* A script for the score calculation.
* This script handles the calculation for scoring and updates the UI display.
*
* Author: Cole Herzog
*/


/* 
 * Evan Pagani CS347 Project: Under Siege!
 * Completely redid Cole's initial implementation
 */

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;  // UI Text to display the score
    public Goal goal;       // Reference to the Goal script

    private float currentScore;

    private void Start()
    {
        if (goal != null)
        {
            // Initialize score with the Goal's starting health (1000)
            currentScore = goal.maxHealth;  // Start with maxHealth as the initial score
            UpdateScore(currentScore);      // Set initial score in UI
        }
    }

    private void Update()
    {
        if (goal != null)
        {
            // Continuously update the score based on the current health of the goal
            UpdateScore(goal.GetCurrentHealth());  // Use GetCurrentHealth to update the score
        }
        else 
        {
            UpdateScore(0);
        }
    }
    // Method to update the score UI
    private void UpdateScore(float newScore)
    {
        currentScore = newScore;
        if (scoreText != null)
        {
            scoreText.text = "Health: " + Mathf.RoundToInt(currentScore).ToString();
        }
    }
}
