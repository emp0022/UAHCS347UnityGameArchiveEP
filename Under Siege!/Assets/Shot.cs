using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


/*  Leonard Hochmuth
 *  This script controlls the shooting of the cannon. Shot Parameters are set in an array in the shotPoint game object that is part of the cannon.
 *  Sequence defined by shot parameters will start when linked button is pressed (currently called 'Shoot').
 *  Cannon can be moved and shot will still originate from the end of the cannon.
 */



public class Shot : MonoBehaviour
{
    public GameObject shotPrefab;
    public bool shoot = false;
    public ShotParam[] shotParams;
    private int index = 0;
    public Button startButton;

    // Start is called before the first frame update
    void Start()
    {
        startButton.onClick.AddListener(ButtonClicked);
    }

    public void ShootShot()
    {
        GameObject shot = Instantiate<GameObject>(shotPrefab);
        Rigidbody shotRb = shot.GetComponent<Rigidbody>();
        shot.transform.position = transform.position;
        shotRb.velocity = new Vector3(Mathf.Cos(shotParams[index].angle * Mathf.Deg2Rad) * shotParams[index].speed,
                                      Mathf.Sin(shotParams[index].angle * Mathf.Deg2Rad) * shotParams[index].speed, 0);     // Calculates initial velocity vector of the shot
        index++;
        if(shotParams.Length > index)
        {
            Invoke("ShootShot", shotParams[index].delay);
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
}

[System.Serializable]
public struct ShotParam
{
    public float speed; 
    public float angle; // 0 = horizontal 90 = verticle
    public float delay; // delay before shot
}
