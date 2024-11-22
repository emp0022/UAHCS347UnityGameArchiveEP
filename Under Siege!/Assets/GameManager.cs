using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /*
    * A script manager for driving the game 'phases'.
    *
    * Note: Alot of this is stubbed for attack phase. Will need to come back and implement.
    * Author: Parker Clark
    */

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

    // TODO: Might need to add attack phasing in here too

    // Start is called before the first frame update
    void Start()
    {   
        // As the game starts, set the phase to build initially 
        SetPhase(GamePhase.BUILD);
    }

    public void SetPhase(GamePhase phase)
    {
        /*
        * Set the current phase of the game.
        *
        * Parameters:
        * phase: The phase to set the game to
        */

        // Set the current phase
        currentPhase = phase;

        // Hide the build UI if the phase is attack
        if (currentPhase == GamePhase.ATTACK)
        {
            // Hide the build UI
            buildUI.SetActive(false);
            BeginAttackPhase();
        }
        else
        {   
            // Otherwise, show the build UI and enable controls
            buildUI.SetActive(true);
        }
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

        // Logic to begin enemy attack
        Debug.Log("Attack Phase Active!");
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
}
