using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    /*
    * A script for the button sound effect.
    * This script handles the audio playing when the button is clicked.
    *
    * Author: Cole Herzog
    */

public class ButtonSoundEffect : MonoBehaviour
{
    public AudioClip clickSound; // The sound to play
    private AudioSource audioSource; // Audio source component

    void Start()
    {
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();

        // Get the Button component and add a listener for clicks
        Button button = GetComponent<Button>();
        button.onClick.AddListener(PlaySound); 
    }

    void PlaySound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound); // Play the click sound
        }
    }
}
