using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


/*  Leonard Hochmuth
 *  This script controls the shooting of the cannon. Shot Parameters are set in an array in the shotPoint game object that is part of the cannon.
 *  Sequence defined by shot parameters will start when linked button is pressed (currently called 'Shoot').
 *  Cannon can be moved and shot will still originate from the end of the cannon.
 */

 // Evan Pagani: added size and mass to the shots

public class Shot : MonoBehaviour
{
    public GameObject shotPrefab;
    public bool shoot = false;
    public int waitTime = 7;
    public ShotParam[] shotParams;
    private int index = 0;
    public Button startButton;

    // Total number of shots max
    public int totalShots = 3;
    
    // Counter for shots fired
    private int shotsFired = 0;


    public void ShootShot()
    {
        GameObject shot = Instantiate<GameObject>(shotPrefab);
        Rigidbody shotRb = shot.GetComponent<Rigidbody>();
        shot.transform.position = transform.position;

        // Set the size of the shot
        shot.transform.localScale = Vector3.one * shotParams[index].size;

        // Set the mass of the shot
        shotRb.mass = shotParams[index].mass;

        shotRb.velocity = new Vector3(Mathf.Cos(shotParams[index].angle * Mathf.Deg2Rad) * shotParams[index].speed,
                                      Mathf.Sin(shotParams[index].angle * Mathf.Deg2Rad) * shotParams[index].speed, 0);     // Calculates initial velocity vector of the shot
        index++;
        shotsFired++;

        if(shotParams.Length > index)
        {
            Invoke("ShootShot", shotParams[index].delay);

            Debug.Log("Shots Fired: " + shotsFired); 
        }
        else
        {
            CheckWinCondition();  
        }
    }
    void ButtonClicked()
    {
        ShootShot();
    }

    // Update is called once per frame
    void Update()
    {
        // Uncomment folowing to use checkbox to start firing sequence
        //if (shoot)
        //{
        //    shoot = false;
        //    ShootShot();
        //}
    }

    void CheckWinCondition()
    {
        /*
        * Check if the goal is still alive and trigger the win condition if it is.
        */

        if (GameManager.Instance.goal.isAlive())
        {
            // Sleep for 7 seconds then trigger win condition
            StartCoroutine(WaitAndTriggerWinCondition(waitTime));
        }
    }

    IEnumerator WaitAndTriggerWinCondition(int seconds)
    {
        /*
        * Wait for a specified number of seconds before triggering the win condition.
        */

        yield return new WaitForSeconds(seconds);
        GameManager.Instance.TriggerWinCondition();
    }


}

[System.Serializable]
public struct ShotParam
{
    public float speed;
    public float angle; // 0 = horizontal 90 = vertical
    public float delay; // delay before shot
    public float size;  // size of shot
    public float mass;  // mass of shot
}

