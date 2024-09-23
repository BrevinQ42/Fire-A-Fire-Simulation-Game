using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBars : MonoBehaviour
{
    //Oxygen
    public float oxygen = 100f;
    public float oxygenDamage = 5f;
    public int collisionCount = 0;
    public bool isLosingOxygen = false;

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


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Smoke"))  
        {
            collisionCount++;

            Debug.Log("Player made contact with a smoke sphere! Current collision count: " + collisionCount);


            if (!isLosingOxygen)
            {
                StartCoroutine(OxygenDamageOverTime());
            }

            Destroy(collision.gameObject);

        }
    }

    // Coroutine to handle oxygen damage over time
    IEnumerator OxygenDamageOverTime()
    {
        isLosingOxygen = true;

        while (oxygen > 0)
        {
            oxygen -= (oxygenDamage * collisionCount) * Time.deltaTime;
            Debug.Log("Oxygen Level: " + oxygen);

            yield return new WaitForSeconds(1f);
        }
    }
}
