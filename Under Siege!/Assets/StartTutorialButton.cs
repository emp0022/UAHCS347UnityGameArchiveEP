using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

    /*
    * A script for the start tutorial button on the menu screen.
    * This script handles the loading of the tutorial walkthrough scene.
    *
    * Author: Cole Herzog
    */
    
public class StartTutorialButton : MonoBehaviour
{
    //Load first level
    public void StartTutorial()
    {
        SceneManager.LoadScene("TutorialWalkthrough");
    }
}
