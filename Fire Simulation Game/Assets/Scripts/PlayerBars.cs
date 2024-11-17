using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBars : MonoBehaviour
{
    // Hydration Level
    public float hydrationLevel = 100f;
    public float hydrationLevelDamage = 1000f;
    public Slider hydrationBar;

    // Oxygen
    public float oxygen = 100f;
    public float oxygenDamage = 5f;
    public float collisionCount = 0;
    public bool isLosingOxygen = false;
    public Slider oxygenBar;

    // Stamina
    public float stamina = 100f;
    public Slider staminaBar;

    public bool isRunning;

    private PlayerController playerController;
    private Coroutine fireDamageCoroutine;
    private Coroutine staminaDepletionCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        isRunning = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.isOnFire)
        {
            if (fireDamageCoroutine == null) 
            {
                fireDamageCoroutine = StartCoroutine(FireDamageOverTime());
            }
        }
        else
        {
            if (fireDamageCoroutine != null) 
            {
                StopCoroutine(fireDamageCoroutine);
                fireDamageCoroutine = null; 
            }
        }
        if (hydrationLevel <= 0f)
        {
            Debug.Log("Player has burned to death!");
            // Insert player death code here
        }

        if (oxygen <= 0f)
        {
            Debug.Log("Player has run out of oxygen!");
            // Insert player death code here
        }

        // Stamina Related
        if (playerController.currentState == "Running")
        {
            if (!isRunning) 
            {
                isRunning = true;
                if (staminaDepletionCoroutine == null) // Ensure no coroutine stacking
                {
                    staminaDepletionCoroutine = StartCoroutine(StaminaDepletionOverTime());
                }
            }
        }
        else if (playerController.currentState == "Walking")
        {
            if (isRunning)
            {
                isRunning = false;
                Debug.Log("Player stopped running");
                if (staminaDepletionCoroutine != null)
                {
                    StopCoroutine(staminaDepletionCoroutine);
                    staminaDepletionCoroutine = null;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Smoke"))
        {
            if (playerController.isCoveringNose)
            {
                collisionCount += 0.5f;
                Debug.Log("Covered nose made contact with a smoke sphere! Current collision count: " + collisionCount);
            }
            else
            {
                collisionCount++;
                Debug.Log("Player made contact with a smoke sphere! Current collision count: " + collisionCount);
            }

            if (isLosingOxygen == false)
            {
                StartCoroutine(OxygenDamageOverTime());
            }

            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Fire"))
        {
            // Fire collision logic if needed
        }
    }

    // Coroutine to handle oxygen damage over time
    IEnumerator OxygenDamageOverTime()
    {
        isLosingOxygen = true;

        while (oxygen > 0)
        {
            oxygen -= (oxygenDamage * collisionCount) * Time.deltaTime;
            oxygenBar.value = oxygen / 100;
            Debug.Log("Oxygen Level: " + oxygen);
            yield return new WaitForSeconds(1f);
        }
    }

    // Coroutine to handle fire damage over time
    IEnumerator FireDamageOverTime()
    {
        while (hydrationLevel > 0)
        {
            hydrationLevel -= hydrationLevelDamage * Time.deltaTime;
            hydrationBar.value = hydrationLevel / 100;
            //Debug.Log("Hydration Level: " + hydrationLevel);
            yield return new WaitForSeconds(1f);
        }
    }

    // Coroutine to handle stamina over time
    IEnumerator StaminaDepletionOverTime()
    {
        while (stamina > 0)
        {
            stamina -= 10f;
            staminaBar.value = stamina / 100;
            //Debug.Log("Stamina: " + stamina);
            yield return new WaitForSeconds(1.0f);
        }
    }
}
