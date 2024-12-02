using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    /*
    * An audio manager for controlling the game's audio.
    *
    * Author: Parker Clark
    */

    // Audio variables
    public AudioSource audioSource;
    public AudioClip buildPhaseMusic;
    public AudioClip attackPhaseMusic;

    private void Start()
    {
        // Start with build phase music
        PlayBuildPhaseMusic();
    }

    public void PlayBuildPhaseMusic()
    {
        /*
        * Play the build phase music.
        */

        if (audioSource.isPlaying) audioSource.Stop(); 
        audioSource.clip = buildPhaseMusic; 
        audioSource.Play(); 
    }

    public void PlayAttackPhaseMusic()
    {   
        /*
        * Play the attack phase music.
        */
        if (audioSource.isPlaying) audioSource.Stop(); // Stop current audio
        audioSource.clip = attackPhaseMusic; // Assign the attack phase clip
        audioSource.Play(); // Play the audio
    }
}

