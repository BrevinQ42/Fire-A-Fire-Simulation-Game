using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Notification system reference
    public NotificationTriggerEvent notificationSystem;
    public bool coverNoseMessageDisplayed;
    public bool stoppedCoveringNoseMessageDisplayed;

    private bool isExtensionsResolved;

    [SerializeField] private Collider collidedWith;

    // Nose Notification system
    public NoseNotification noseNotificationSystem;

    public Fire FireOnPlayer;
    [SerializeField] private FireManager fireManager;

    private Rigidbody rb;
    private Transform cameraTransform;

    public string currentState;

    [SerializeField] private float walkingSpeed;
    [SerializeField] private float crawlingSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float currentSpeed;

    private Vector2 cameraRotation;
    [SerializeField] private float mouseSensitivity;
    private bool isVerticalTiltEnabled;
    private bool isHorizontalTiltEnabled;

    public bool isCoveringNose;
    public bool isOnFire;

    private Vector3 previousEulerAngles;
    private float rollingTimeLeft;
    private int rotationCount;
    private float rollRotation;
    [SerializeField] private float rollTimePerRotation;

    [SerializeField] private float closeProximityValue; // distance that is considered to be in close proximity
    private Transform hitTransform;                     // (nearby) object that is being pointed at by the player
    [SerializeField] private GrabbableObject heldObject;

    [SerializeField] private float throwForce;

    public Vector3 movementVector;

    private PlayerBars playerBars;
    public float staminaRequiredForCrawling;
    public float staminaRequiredForRunning;
    public float staminaRequiredForRolling;

    // Start is called before the first frame update
    void Start()
    {
        isExtensionsResolved = false;

        collidedWith = null;

        rb = GetComponent<Rigidbody>();
        cameraTransform = transform.GetChild(0);

        currentState = "Walking";
        SetupUprightState();
        currentSpeed = walkingSpeed;

        cameraRotation = Vector2.zero;
        Cursor.lockState = CursorLockMode.Locked;

        isCoveringNose = false;
        isOnFire = false;

        previousEulerAngles = Vector3.zero;
        rollingTimeLeft = 0.0f;
        rotationCount = 0;
        rollRotation = 5.0f;

        hitTransform = null;
        heldObject = null;

        playerBars = GetComponent<PlayerBars>();

        notificationSystem.notificationMessage = "There are ways to prevent fires from happening, one way is to avoid octopus wiring. Look for an extension cord that is plugged into another and plug it into an outlet instead.\n*Maybe it's behind you*";
        notificationSystem.disableAfterTimer = true;
        notificationSystem.disableTimer = 7.5f;
        notificationSystem.displayNotification();

        coverNoseMessageDisplayed = false;
        stoppedCoveringNoseMessageDisplayed = false;
    }

    void FixedUpdate()
    {
        if ((heldObject && !heldObject.GetComponent<ElectricPlug>()) || currentState.Equals("Rolling")) hitTransform = null;
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, closeProximityValue))
            {
                if (hit.transform.CompareTag("Floor") || hit.transform.CompareTag("Fire")) hitTransform = null;
                else
                {
                    Debug.Log(hit.transform.name + ": " + hit.distance);

                    hitTransform = hit.transform;
                    if (hitTransform.name == "FireResistant")    
                    {
                        Debug.Log("FireResistant Object is Hit");
                        NonFlammableObject fireResistant = hitTransform.GetComponent<NonFlammableObject>();
                        if (fireResistant != null)
                        {
                            fireResistant.lookedAt = true;
                        }
                    }
                    if (hitTransform.name == "bibb_faucet")
                    {
                        Debug.Log("Faucet Object is Hit");  
                        ObjectNamePopUp faucet = hitTransform.GetComponent<ObjectNamePopUp>();
                        if (faucet != null)
                        {
                            faucet.lookedAt = true;
                        }
                    }
                    if (hitTransform.name == "Bucket")
                    {
                        Debug.Log("Bucket Object is Hit");
                        Pail bucket = hitTransform.GetComponent<Pail>();
                        if (bucket != null )
                        {
                            bucket.lookedAt = true;
                        }
                    }
                }
            }
            else
            {
                // Reset lookedAt for all NonFlammableObjects
                foreach (var obj in FindObjectsOfType<NonFlammableObject>())
                {
                    obj.lookedAt = false;
                }
                // Reset lookedAt for all Faucets
                foreach (var obj in FindObjectsOfType<ObjectNamePopUp>())
                {
                    obj.lookedAt = false;
                }
                // Reset lookedAt for all Buckets
                foreach (var obj in FindObjectsOfType<Pail>())
                {
                    obj.lookedAt = false;
                }


                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * 2.0f, Color.white);
                Debug.Log("Did not Hit");
                hitTransform = null;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Transitions when stamina is 0
        // Run to walk
        if (playerBars.stamina <= 0 && currentState.Equals("Running"))
        {
            currentState = "Walking";
            currentSpeed = walkingSpeed;
            SetupUprightState();
        }
        // Roll to crawl
        if (playerBars.stamina <= 0 && currentState.Equals("Rolling"))
        {
            // switch them to crawling state
            transform.eulerAngles = previousEulerAngles;
            ToggleCrawl();
        }
        //Stamina needed updates
        staminaRequiredForCrawling = 4f * playerBars.GetCurrentOxygenMultiplier();
        staminaRequiredForRunning = 10f * playerBars.GetCurrentOxygenMultiplier();
        staminaRequiredForRolling = 20f * playerBars.GetCurrentOxygenMultiplier();

        // If they are on fire
        if (FireOnPlayer == null)
        {
            isOnFire = false;
        }

        // basic controls
        Move();
        LookAround();

        if (currentState.Equals("Rolling")) RollOver();

        else
        {
            // actions
            if (Input.GetKeyDown(KeyCode.C)) ToggleCrawl();
            else if (Input.GetKeyDown(KeyCode.F))
            {
                if (playerBars.stamina > staminaRequiredForRunning)
                {
                    ToggleRun();
                }
                else
                {
                    notificationSystem.notificationMessage = "Not enough stamina to run!";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 3.0f;
                    notificationSystem.displayNotification();
                }
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                if (playerBars.stamina > staminaRequiredForRolling)
                {
                    InitiateRollOver();
                }
                else
                {
                    notificationSystem.notificationMessage = "Not enough stamina to roll!";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 3.0f;
                    notificationSystem.displayNotification();
                }
            }
            else if (Input.GetKeyDown(KeyCode.E)) InteractWithObject();
            else if (Input.GetKeyDown(KeyCode.G)) DropObject();

            if (!heldObject)
            {
                if (Input.GetMouseButton(0))
                {
                    CoverNose();
                }
                else
                {
                    isCoveringNose = false;
                    noseNotificationSystem.RemoveNotification();
                    if (coverNoseMessageDisplayed == true && stoppedCoveringNoseMessageDisplayed == false)
                    {
                        stoppedCoveringNoseMessageDisplayed = true;
                        notificationSystem.notificationMessage = "Stopped Covering nose!";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 3.0f;
                        notificationSystem.displayNotification();
                    }
                }
            }
            else if (heldObject && Input.GetMouseButtonDown(0)) UseObject();
        }

        TestFunction(); // for debugging (i.e. Debug.Log)
    }

    void Move()
    {
        movementVector = Vector3.zero;

        // Prevent movement while crawling if player does not have enough stamina
        if (currentState == "Crawling" && playerBars.stamina < staminaRequiredForCrawling)
        {
            rb.velocity = Vector3.zero;
            notificationSystem.notificationMessage = "Not enough stamina to crawl!";
            notificationSystem.disableAfterTimer = true;
            notificationSystem.disableTimer = 3.0f;
            notificationSystem.displayNotification();
            return;
        }

        // movement controls
        if (Input.GetKey(KeyCode.W))
        {
            movementVector = new Vector3(transform.forward.x, 0.0f, transform.forward.z);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            movementVector = new Vector3(-transform.forward.x, 0.0f, -transform.forward.z);
        }

        if (Input.GetKey(KeyCode.A))
        {
            movementVector += new Vector3(-transform.right.x, 0.0f, -transform.right.z);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            movementVector += new Vector3(transform.right.x, 0.0f, transform.right.z);
        }

        // if the player attempts to move when they are currently rolling,
        if (movementVector != Vector3.zero && currentState.Equals("Rolling"))
        {
            // switch them to crawling state
            transform.eulerAngles = previousEulerAngles;
            ToggleCrawl();
        }

        // move character accordingly
        rb.velocity = movementVector.normalized * currentSpeed;
    }

    // adjusting camera when looking around
    void LookAround()
    {
        if (isHorizontalTiltEnabled)
        {
            cameraRotation.x += Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, cameraRotation.x, transform.eulerAngles.z);
        }

        if (isVerticalTiltEnabled)
        {
            cameraRotation.y += Input.GetAxis("Mouse Y") * mouseSensitivity;
            transform.eulerAngles = new Vector3(-cameraRotation.y, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }

    void ToggleCrawl()
    {
        if (currentState.Equals("Crawling"))
        {
            currentState = "Walking";
            SetupUprightState();
            currentSpeed = walkingSpeed;
        }
        else if (!heldObject)
        {
            currentState = "Crawling";
            SetupCrawlState();
            currentSpeed = crawlingSpeed;
        }
    }

    void ToggleRun()
    {
        if (currentState.Equals("Running"))
        {
            currentState = "Walking";
            currentSpeed = walkingSpeed;
        }
        else
        {
            if (!currentState.Equals("Walking")) SetupUprightState();

            currentState = "Running";
            currentSpeed = runningSpeed;
        }
    }

    void InitiateRollOver()
    {
        if (!heldObject)
        {
            isVerticalTiltEnabled = false;
            isHorizontalTiltEnabled = false;
            rollingTimeLeft = 5.0f;
            rotationCount = 37;
            rollRotation = 5.0f;

            transform.position = new Vector3(transform.position.x, 1.0f, transform.position.z);
            transform.eulerAngles = new Vector3(75, transform.eulerAngles.y, transform.eulerAngles.z);
            previousEulerAngles = transform.eulerAngles;
            cameraTransform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);

            currentSpeed = 0;
            currentState = "Rolling";

            StartCoroutine(RollOver());
        }
    }

    IEnumerator RollOver()
    {
        while (rollingTimeLeft > 0.0f && currentState.Equals("Rolling"))
        {
            transform.Rotate(0.0f, rollRotation, 0.0f, Space.Self);
            rotationCount--;

            if (rotationCount == 0)
            {
                rotationCount = 74;
                rollRotation = -rollRotation;
            }

            if (FireOnPlayer)
            {
                FireOnPlayer.AffectFire(-0.01f);

                if (Math.Round(FireOnPlayer.intensityValue, 2) <= 0.01f)
                {
                    FireOnPlayer = null;
                    Debug.Log("IsOnFire: " + isOnFire);
                    Destroy(transform.GetChild(1).gameObject);
                }
            }

            yield return new WaitForSeconds(rollTimePerRotation);

            rollingTimeLeft -= rollTimePerRotation;

            if (rollingTimeLeft <= 0.0f)
            {
                transform.eulerAngles = previousEulerAngles;
                ToggleCrawl();
            }
        }
    }

    void InteractWithObject()
    {
        if (hitTransform)
        {
            // for this functionality, will add checker if object is grabbable (fire fighting object)
            Rigidbody hitRB = hitTransform.GetComponent<Rigidbody>();

            if (hitRB && hitTransform.CompareTag("Grabbable"))
            {
                hitRB.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

                hitTransform.GetComponent<Collider>().enabled = false;

                hitTransform.SetParent(transform);

                Pail pail = hitTransform.GetComponent<Pail>();
                NonFlammableObject nonFlammable = hitTransform.GetComponent<NonFlammableObject>();
                if (pail)
                {
                    notificationSystem.notificationMessage = "Use the faucet to fill the bucket with water and then throw it at the fire!";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 3.0f;
                    notificationSystem.displayNotification();

                    pail.closeProximityValue = closeProximityValue;
                    pail.playerCamera = transform.GetChild(0);
                    hitTransform.SetPositionAndRotation(
                        transform.position + transform.forward + transform.right * 0.7f - transform.up * 0.15f, 
                        transform.rotation);
                }
                if (nonFlammable)
                {
                    notificationSystem.notificationMessage = "You can throw this directly at a fire to put it out!";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 3.0f;
                    notificationSystem.displayNotification();
                    hitTransform.SetPositionAndRotation(transform.position + transform.forward, transform.rotation);
                }
                else
                    hitTransform.SetPositionAndRotation(transform.position + transform.forward, transform.rotation);

                heldObject = hitTransform.GetComponent<GrabbableObject>();
                heldObject.isHeld = true;

                ElectricPlug plug = heldObject.GetComponent<ElectricPlug>();
                if (plug)
                {
                    if(!isExtensionsResolved &&
                        plug.owner.name.Equals("ExtensionCord") && plug.pluggedInto.name.Equals("ExtensionCord"))
                    {
                        notificationSystem.notificationMessage = "Unplug unused appliances as well!\n*Take note that fire may come from other houses despite doing this";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 7.0f;
                        notificationSystem.displayNotification();

                        isExtensionsResolved = true;
                    }

                    plug.pluggedInto = null;
                    fireManager.RemoveSpawnPoint(plug.transform);
                }
            }
            else if (hitTransform.CompareTag("WaterSource"))
            {
                Spawner ws = hitTransform.GetChild(0).GetComponent<Spawner>();
                ws.Toggle();
            }
        }
    }

    void DropObject()
    {
        if (heldObject)
        {
            heldObject.Deattach();
            heldObject = null;
        }
    }

    void UseObject()
    {
        FireFightingObject obj = heldObject.GetComponent<FireFightingObject>();

        if (obj)
        {
            bool isStillHeld;
            obj.Use(throwForce, out isStillHeld);

            if (!isStillHeld) heldObject = null;
        }
        else if (hitTransform && hitTransform.CompareTag("Outlet"))
        {
            heldObject.Use(hitTransform);
            
            if (!heldObject.isHeld) heldObject = null;
        }
    }

    void CoverNose()
    {
        if (coverNoseMessageDisplayed == false)
        {
            notificationSystem.notificationMessage = "Covering nose!";
            notificationSystem.disableAfterTimer = true;
            notificationSystem.disableTimer = 3.0f;
            notificationSystem.displayNotification();
                
            coverNoseMessageDisplayed = true;
        }

        isCoveringNose = true;
        noseNotificationSystem.EnableNotification();
    }

    void SetupUprightState()
    {
        isVerticalTiltEnabled = true;
        isHorizontalTiltEnabled = true;
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
        cameraTransform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
        transform.position = new Vector3(transform.position.x, 1.2f, transform.position.z);
    }

    void SetupCrawlState()
    {
        transform.position = new Vector3(transform.position.x, 1.0f, transform.position.z);
        isVerticalTiltEnabled = false;
        isHorizontalTiltEnabled = true;
        transform.eulerAngles = new Vector3(75, transform.eulerAngles.y, transform.eulerAngles.z);
        cameraTransform.eulerAngles = new Vector3(30, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    void EndGame()
    {
        if (collidedWith)
        {
            if (collidedWith.name.Equals("Outside Floor"))
            {
                notificationSystem.notificationMessage = "YOU WON!\nYou successfully fought the fire!";
                notificationSystem.disableAfterTimer = true;
                notificationSystem.disableTimer = 10.0f;
                notificationSystem.displayNotification();
                
                Debug.Log("You Won");
            }
            else if (collidedWith.name.Equals("Court"))
            {
                notificationSystem.notificationMessage = "YOU WON!\nYou successfully escaped the fire!";
                notificationSystem.disableAfterTimer = true;
                notificationSystem.disableTimer = 10.0f;
                notificationSystem.displayNotification();
            
                Debug.Log("You Escaped");
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (fireManager.isPlayerSuccessful)
        {    
            if (collision.collider.name.Equals("Outside Floor"))
            {
                collidedWith = collision.collider;

                EndGame();
            }
        }
        else if (collision.collider.name.Equals("Court"))
        {
            collidedWith = collision.collider;

            notificationSystem.notificationMessage = "Press Enter to End the Game!";
            notificationSystem.disableAfterTimer = true;
            notificationSystem.disableTimer = 5.0f;
            notificationSystem.displayNotification();

            EndGame();
        }
    }

    void TestFunction()
    {
        // key press for testing
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(previousEulerAngles);
        }
    }
}
