using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

    /*
    * A script for managing the tutorial messages.
    * This script handles the loading of the tutorial messages, displaying new ones, 
      removing old ones, and finally after all messages have been displayed it retruns the player
      to the menu screen.
    *
    * Author: Cole Herzog
    */

public class TutorialManager : MonoBehaviour
{
    public GameObject WelcomeMsg;
    public GameObject HealthMsg;
    public GameObject BudgetMsg;
    public GameObject MaterialSelectMsg;
    public GameObject BlockSelectMsg;
    public GameObject SetupMsg;
    public GameObject DefendMsg;
    public GameObject ReturnLevelSelectMsg;

    public Button NextButton;

    private GameObject[] tutorialMessages;
    private int currentMessageIndex = 0;

    void Start()
    {
        // Ensure that all required references are assigned
        if (NextButton == null)
        {
            Debug.LogError("NextButton is not assigned in the Inspector!");
        }

        if (WelcomeMsg == null || HealthMsg == null || BudgetMsg == null || MaterialSelectMsg == null || BlockSelectMsg == null || SetupMsg == null || DefendMsg == null || ReturnLevelSelectMsg == null)
        {
            Debug.LogError("One or more tutorial message GameObjects are not assigned in the Inspector!");
        }

        // Initialize the array of messages
        tutorialMessages = new GameObject[]
        {
            WelcomeMsg,
            HealthMsg,
            BudgetMsg,
            MaterialSelectMsg,
            BlockSelectMsg,
            SetupMsg,
            DefendMsg,
            ReturnLevelSelectMsg
        };

        // Hide all messages initially
        foreach (GameObject msg in tutorialMessages)
        {
            if (msg != null) msg.SetActive(false);
        }

        // Display the first message
        if (tutorialMessages.Length > 0 && tutorialMessages[currentMessageIndex] != null)
        {
            tutorialMessages[currentMessageIndex].SetActive(true);
        }

        // Ensure the listener is only added once
        if (NextButton != null)
        {
            NextButton.onClick.RemoveAllListeners();
            NextButton.onClick.AddListener(ShowNextMessage);
        }
        else
        {
            Debug.LogError("NextButton is not assigned!");
        }
    }

    // Function to show the next message when the button is clicked
    public void ShowNextMessage()
    {
        // Hide the current message
        if (currentMessageIndex < tutorialMessages.Length && tutorialMessages[currentMessageIndex] != null)
        {
            tutorialMessages[currentMessageIndex].SetActive(false);
        }

        // Increment the index to show the next message
        currentMessageIndex++;

        // If there are more messages, show the next one
        if (currentMessageIndex < tutorialMessages.Length && tutorialMessages[currentMessageIndex] != null)
        {
            tutorialMessages[currentMessageIndex].SetActive(true);
        }
        else
        {
            // All messages have been shown, show the ReturnLevelSelectMsg first
            if (ReturnLevelSelectMsg != null)
            {
                ReturnLevelSelectMsg.SetActive(true);
            }

            // After showing ReturnLevelSelectMsg, load the MenuScreen
            Invoke("LoadLevelMenuScene", 2f); // Optional: wait for 2 seconds before loading the scene
        }
    }

    // Function to load the LevelSelection scene after ReturnLevelSelectMsg
    private void LoadLevelMenuScene()
    {
        SceneManager.LoadScene("MenuScreen"); // Replace "MenuScreen" with your actual scene name
    }
}
