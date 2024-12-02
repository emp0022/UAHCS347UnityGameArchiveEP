using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    /*
    * A script manager for driving the game 'phases'.
    *
    * Note: Alot of this is stubbed for attack phase. Will need to come back and implement.
    * Author: Parker Clark
    */

    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Enum for 'categorizing' the game phases
    public enum GamePhase 
    { 
        BUILD, 
        ATTACK 
    }

    // The current phase of the game
    public GamePhase currentPhase;

    // Build budget on placing components
    // Note: Come back to this, might vary on level
    public int buildBudget;

    // UI element for displaying build-centric buttons
    public GameObject buildUI;

    // Instantiate shot object
    public Shot[] cannonShots;


    // Goal object to control
    public Goal goal;

    // UI element for displaying lose condition
    public GameObject losePanel;
    public GameObject winPanel;

    // Restart button
    public Button[] restartButtons;

    // Audio manager
    public AudioManager audioManager;
    
    // Defend and Reset buttons
    public Button phaseButton;

    // Flag to track game lockout
    private bool isGameOver = false;

    // Start is called before the first frame update
    void Start()
    {   
        // As the game starts, set the phase to build initially 
        SetPhase(GamePhase.BUILD);

        // Add the RestartGame listener to each restart button in the list
        foreach (Button button in restartButtons)
        {
            if (button != null)
            {
                button.onClick.AddListener(RestartGame);
            }
        }
    }

     private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPhase(GamePhase phase)
    {
        // Set the current phase
        currentPhase = phase;

        if (currentPhase == GamePhase.ATTACK)
        {
            // Hide the build UI
            buildUI.SetActive(false);
            audioManager.PlayAttackPhaseMusic();
            BeginAttackPhase();

            // Update the phaseButton to be 'Reset' and call RestartGame when clicked
            if (phaseButton != null)
            {
                // Change the text to 'Reset'
                UpdateButtonText("Reset");

                // Remove all existing listeners
                phaseButton.onClick.RemoveAllListeners();

                // Add the RestartGame listener
                phaseButton.onClick.AddListener(RestartGame);
            }
        }
        else if (currentPhase == GamePhase.BUILD)
        {
            // Show the build UI
            buildUI.SetActive(true);
            audioManager.PlayBuildPhaseMusic();

            // Update the phaseButton to be 'Defend!' and call OnDefendButtonClicked when clicked
            if (phaseButton != null)
            {
                // Change the text to 'Defend!'
                UpdateButtonText("Defend!");

                // Remove all existing listeners
                phaseButton.onClick.RemoveAllListeners();

                // Add the OnDefendButtonClicked listener
                phaseButton.onClick.AddListener(OnDefendButtonClicked);
            }
        }
    }

    private void UpdateButtonText(string newText)
    {
        // Try to get the TextMeshPro component
        TMP_Text tmpText = phaseButton.GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = newText;
            return;
        }

        Debug.LogError("No Text component found on the phaseButton.");
    }


    private void EnableBuildControls()
    {
        /*
        * Stub for helper function to enable the build controls for the player.
        */

        // Logic to enable drag-and-drop functionality and track the budget
        Debug.Log("Build Phase Active. Budget: " + buildBudget);
    }

    private void BeginAttackPhase()
    {
        /*
        * Stub for helper function to begin the attack on the base.
        */

        Debug.Log("Attack Phase Active!");

        // Activate gravity on the goal
        goal.ActivateGravity();

        // Call the cannon to shoot
        foreach (Shot cannon in cannonShots)
        {
            cannon.ShootShot();
        }
    }

    public void OnDefendButtonClicked()
    {
        /*
        * Event handler for the 'Defend!' button.
        */

        if (currentPhase == GamePhase.BUILD)
        {   
            // Switch to attack phase when the player clicks 'Defend!'
            SetPhase(GamePhase.ATTACK); 
        }
    }

    public void TriggerLoseCondition()
    {
        /*
        * Code for displaying the lose condition UI.
        */
        
        if (!isGameOver)
        {
            isGameOver = true;
            Debug.Log("You Lose!");
            losePanel.SetActive(true);
        }
        
    }

    public void TriggerWinCondition()
    {   
        /*
        * Code for triggering the win condition UI.
        */

        if (!isGameOver)
        {
            isGameOver = true;
            Debug.Log("You Win!");

            // Display the win panel
            winPanel.SetActive(true);

            // Show win panel
            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }
        }
    }

    public void RestartGame()
    {
        /*
        * Event handler for the 'Restart' button.
        */
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    
}