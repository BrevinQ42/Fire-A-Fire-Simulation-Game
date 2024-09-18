using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    private float intensityValue;

    // Start is called before the first frame update
    void Start()
    {
        intensityValue = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name.Equals("Player"))
        {
            Debug.Log("Player is burning!");
        }

        else
        {
            FireFightingObject obj = collision.collider.GetComponent<FireFightingObject>();

            if (obj)
            {
                if (obj.fireFightingValue >= intensityValue) Destroy(gameObject);
                else Debug.Log(intensityValue + " > " + obj.fireFightingValue);

                Water water = obj.GetComponent<Water>();
                if(water) Destroy(collision.collider.gameObject);
            }
        }
    }
}
