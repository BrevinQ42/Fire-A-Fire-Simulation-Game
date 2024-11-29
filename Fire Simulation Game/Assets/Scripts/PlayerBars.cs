using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBars : MonoBehaviour
{
    // Notification system reference
    public NotificationTriggerEvent notificationSystem;

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
    public bool isWalking;
    public bool isRolling;
    public bool isCrawling;

    private PlayerController playerController;
    private Coroutine fireDamageCoroutine;
    private Coroutine staminaRunDepletionCoroutine;
    private Coroutine staminaWalkRegenerationCoroutine;
    private Coroutine staminaRollDepletionCoroutine;
    private Coroutine staminaCrawlDepletionCoroutine;
    private Coroutine staminaRegenerationCoroutine;
    private Coroutine staminaCrawlRegenerationCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        ResetMovementStates();
    }

    // Update is called once per frame
    void Update()
    {
        HandleFireDamage();

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

        HandleStaminaRegeneration();
    }

    private void HandleFireDamage()
    {
        if (playerController.isOnFire)
        {
            if (fireDamageCoroutine == null)
            {
                fireDamageCoroutine = StartCoroutine(FireDamageOverTime());
            }
        }
        else if (playerController.isOnFire == false)
        {
            if (fireDamageCoroutine != null)
            {
                StopCoroutine(fireDamageCoroutine);
                fireDamageCoroutine = null;
            }
        }
    }

    private void HandleStaminaRegeneration()
    {
        if (playerController.movementVector == Vector3.zero)
        {
            if (playerController.currentState == "Rolling")
            {
                // Special case for rolling
                if (!isRolling)
                {
                    if (staminaRegenerationCoroutine != null)
                    {
                        StopCoroutine(staminaRegenerationCoroutine);
                        staminaRegenerationCoroutine = null;
                    }
                    ResetMovementStates();
                    isRolling = true;
                    staminaRollDepletionCoroutine = StartCoroutine(StaminaRollDepletionOverTime());
                }
            }
            else if (playerController.currentState == "Crawling")
            {
                // Special case for crawling state
                ResetMovementStates();
                if (staminaCrawlRegenerationCoroutine == null)
                {
                    staminaCrawlRegenerationCoroutine = StartCoroutine(StaminaCrawlRegenerationOverTime());
                }
            }
            else
            {
                // Default case when not moving and not rolling
                ResetMovementStates();
                if (staminaCrawlRegenerationCoroutine != null)
                {
                    StopCoroutine(staminaCrawlRegenerationCoroutine);
                    staminaCrawlRegenerationCoroutine = null;
                }
                if (staminaRegenerationCoroutine == null)
                {
                    staminaRegenerationCoroutine = StartCoroutine(StaminaRegenerationOverTime());
                }
            }
        }
        else
        {
            // Player is moving
            if (staminaRegenerationCoroutine != null)
            {
                StopCoroutine(staminaRegenerationCoroutine);
                staminaRegenerationCoroutine = null;
            }
            if (staminaCrawlRegenerationCoroutine != null)
            {
                StopCoroutine(staminaCrawlRegenerationCoroutine);
                staminaCrawlRegenerationCoroutine = null;
            }

            HandleMovementState();
        }
    }


    private void HandleMovementState()
    {
        if (playerController.currentState == "Running" && !isRunning)
        {
            ResetMovementStates();
            isRunning = true;
            staminaRunDepletionCoroutine = StartCoroutine(StaminaRunDepletionOverTime());
        }
        else if (playerController.currentState == "Walking" && !isWalking)
        {
            ResetMovementStates();
            isWalking = true;
            staminaWalkRegenerationCoroutine = StartCoroutine(WalkRegenerationOverTime());
        }
        else if (playerController.currentState == "Rolling" && !isRolling)
        {
            ResetMovementStates();
            isRolling = true;
            staminaRollDepletionCoroutine = StartCoroutine(StaminaRollDepletionOverTime());
        }
        else if (playerController.currentState == "Crawling" && !isCrawling)
        {
            ResetMovementStates();
            isCrawling = true;
            staminaCrawlDepletionCoroutine = StartCoroutine(StaminaCrawlDepletionOverTime());
        }
    }

    private void ResetMovementStates()
    {
        isRunning = false;
        isWalking = false;
        isRolling = false;
        isCrawling = false;

        if (staminaRunDepletionCoroutine != null)
        {
            StopCoroutine(staminaRunDepletionCoroutine);
            staminaRunDepletionCoroutine = null;
        }
        if (staminaWalkRegenerationCoroutine != null)
        {
            StopCoroutine(staminaWalkRegenerationCoroutine);
            staminaWalkRegenerationCoroutine = null;
        }
        if (staminaRollDepletionCoroutine != null)
        {
            StopCoroutine(staminaRollDepletionCoroutine);
            staminaRollDepletionCoroutine = null;
        }
        if (staminaCrawlDepletionCoroutine != null)
        {
            StopCoroutine(staminaCrawlDepletionCoroutine);
            staminaCrawlDepletionCoroutine = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Smoke"))
        {
            collisionCount += playerController.isCoveringNose ? 0.5f : 1f;
            Debug.Log("Player made contact with a smoke sphere! Current collision count: " + collisionCount);

            if (!isLosingOxygen)
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
        isLosingOxygen = false;
    }

    // Coroutine to handle fire damage over time
    IEnumerator FireDamageOverTime()
    {
        while (hydrationLevel > 0)
        {
            hydrationLevel -= hydrationLevelDamage * Time.deltaTime;
            hydrationBar.value = hydrationLevel / 100;
            yield return new WaitForSeconds(1f);
        }
    }

    // Prevent stamina bar from exceeding 100 and reaching below 0
    private void ClampStamina()
    {
        stamina = Mathf.Clamp(stamina, 0f, 100f);
        staminaBar.value = stamina / 100f;

        // Display Stamina Notification
        if (stamina == 0)
        {
            notificationSystem.notificationMessage = "Stamina Depleted!";
            notificationSystem.disableAfterTimer = true;
            notificationSystem.disableTimer = 3.0f;
            notificationSystem.displayNotification();
        }
    }

    public float GetCurrentOxygenMultiplier()
    {
        return Mathf.Lerp(1.0f, 2.0f, 1 - (oxygen / 100f));
    }

    // Coroutines to handle stamina over time
    IEnumerator StaminaRunDepletionOverTime()
    {
        while (stamina > 0)
        {
            float multiplier = GetCurrentOxygenMultiplier();
            stamina -= 10f * multiplier; 
            ClampStamina();
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator WalkRegenerationOverTime()
    {
        while (stamina < 100)
        {
            stamina += 2f;
            ClampStamina();
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator StaminaRollDepletionOverTime()
    {
        while (stamina > 0)
        {
            float multiplier = GetCurrentOxygenMultiplier();
            stamina -= 20f * multiplier; 
            ClampStamina();
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator StaminaCrawlDepletionOverTime()
    {
        while (stamina > 0)
        {
            float multiplier = GetCurrentOxygenMultiplier();
            stamina -= 4f * multiplier; 
            ClampStamina();
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator StaminaRegenerationOverTime()
    {
        while (stamina < 100)
        {
            stamina += 10f;
            ClampStamina();
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator StaminaCrawlRegenerationOverTime()
    {
        while (stamina < 100)
        {
            stamina += 3.0f; 
            ClampStamina();
            yield return new WaitForSeconds(1f);
        }
    }

}
