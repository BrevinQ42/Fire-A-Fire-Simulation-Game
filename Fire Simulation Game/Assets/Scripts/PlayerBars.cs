using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBars : MonoBehaviour
{
    //Oxygen
    public float oxygen = 100f;
    public float oxygenDamage = 10f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (oxygen <= 0f)
        {
            Debug.Log("Player has run out of oxygen!");
            // Insert player death code here
        }
    }

    // Called when a particle system's particles collide with this object
    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Smoke")) 
        {

            Debug.Log("Player made contact with smoke!");

        }
    }
}
