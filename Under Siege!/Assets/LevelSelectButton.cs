using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

    /*
    * A script for the level select button on the menu screen.
    * This script handles the loading of the level selection scene.
    * And Several other levels
    * Author: Evan Pagani
    */
    
public class LevelSelectButton : MonoBehaviour
{
    //Load Level select
    public void LevelSelect()
    {
        SceneManager.LoadScene("LevelSelection");
    }
    //Load MainMenu
    public void MainMenuSelect()
    {
        SceneManager.LoadScene("MenuScreen");
    }

    //Load first level
    public void LEVELSELECT1()
    {
        SceneManager.LoadScene("_LEVEL1");
    }

    //Load second level
    public void LEVELSELECT2()
    {
        SceneManager.LoadScene("_LEVEL2");
    }

    //Load third level
    public void LEVELSELECT3()
    {
        SceneManager.LoadScene("_LEVEL3");
    }

    //Load fourth level
    public void LEVELSELECT4()
    {
        SceneManager.LoadScene("_LEVEL4");
    }

    //Load fifth level
    public void LEVELSELECT5()
    {
        SceneManager.LoadScene("_LEVEL5");
    }

    //Load sixth level
    public void LEVELSELECT6()
    {
        SceneManager.LoadScene("_LEVEL6");
    }

    //Load seventh level
    public void LEVELSELECT7()
    {
        SceneManager.LoadScene("_LEVEL7");
    }

    //Load eighth level
    public void LEVELSELECT8()
    {
        SceneManager.LoadScene("_LEVEL8");
    }

}
