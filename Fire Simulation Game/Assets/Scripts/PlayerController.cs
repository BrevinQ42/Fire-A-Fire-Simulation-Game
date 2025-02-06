using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Win Screen
    public WinScreen winScreen;
    public float timeElapsed;
    public bool isGrounded;
    public bool isOnStairs;

    // Notification system reference
    [Header("Notification System")]
    public NotificationTriggerEvent notificationSystem;
    public bool coverNoseMessageDisplayed;
    public bool stoppedCoveringNoseMessageDisplayed;

    // Nose Notification system
    public NoseNotification noseNotificationSystem;

    [Header("Fire Related")]
    public Fire FireOnPlayer;
    [SerializeField] private FireManager fireManager;

    [Header("Player Components")]
    private Rigidbody rb;
    private Transform cameraTransform;

    [Header("Player Info")]
    public string currentState;
    
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float crawlingSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float currentSpeed;
    public Vector3 movementVector;

    [Header("Player Bools")]
    public bool isCoveringNose;
    public bool isOnFire;

    [Header("Player Look Around")]
    private Vector2 cameraRotation;
    [SerializeField] private float mouseSensitivity;
    private bool isVerticalTiltEnabled;
    private bool isHorizontalTiltEnabled;

    [SerializeField] private float closeProximityValue; // distance that is considered to be in close proximity
    private Transform hitTransform;                     // (nearby) object that is being pointed at by the player
    [SerializeField] private GrabbableObject heldObject;
    private GameObject objectLookedAt;

    [Header("Player Roll")]
    private Vector3 previousEulerAngles;
    private float rollingTimeLeft;
    private int rotationCount;
    private float rollRotation;
    [SerializeField] private float rollTimePerRotation;

    [Header("Player Stamina")]
    private PlayerBars playerBars;
    public float staminaRequiredForCrawling;
    public float staminaRequiredForRunning;
    public float staminaRequiredForRolling;

    // Sound Effects
    public AudioSource audioSource;
    public AudioSource audioSource2;
    public AudioClip walkingClip;
    public AudioClip runningClip;
    public AudioClip breathingClip;
    public AudioClip rollingClip;
    public AudioClip crawlingClip;
    public AudioClip burningClip;
    public AudioClip pickUpClip;

    [Header("Misc.")]
    private bool isExtensionsResolved;
    public Collider collidedWith;
    [SerializeField] private float throwForce;

    // Start is called before the first frame update
    void Start()
    {
        isExtensionsResolved = false;

        collidedWith = null;

        rb = GetComponent<Rigidbody>();
        cameraTransform = transform.GetChild(0);

        SetupUprightState();
        currentSpeed = walkingSpeed;

        cameraRotation = Vector2.zero;
        if (fireManager) Cursor.lockState = CursorLockMode.Locked;

        isCoveringNose = false;
        isOnFire = false;

        previousEulerAngles = Vector3.zero;
        rollingTimeLeft = 0.0f;
        rotationCount = 0;
        rollRotation = 5.0f;

        hitTransform = null;
        heldObject = null;

        playerBars = GetComponent<PlayerBars>();

        notificationSystem.notificationMessage = "Try to prevent fires!\nOne way is to avoid plugging extension cords on others";
        notificationSystem.disableAfterTimer = true;
        notificationSystem.disableTimer = 7.5f;
        notificationSystem.displayNotification();

        coverNoseMessageDisplayed = false;
        stoppedCoveringNoseMessageDisplayed = false;

        timeElapsed = 0;

        AudioSource[] audioSources = GetComponents<AudioSource>(); 

        if (audioSources.Length > 1)
        {
            audioSource = audioSources[0];  
            audioSource2 = audioSources[1]; 
        }
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
                    // Debug.Log(hit.transform.name + ": " + hit.distance);

                    hitTransform = hit.transform;
                    if (hitTransform.GetComponent<NonFlammableObject>())    
                    {
                        // Debug.Log("FireResistant Object is Hit");
                        NonFlammableObject fireResistant = hitTransform.GetComponent<NonFlammableObject>();
                        fireResistant.lookedAt = true;
                        
                        if(objectLookedAt != fireResistant.gameObject)
                        {
                            ResetLastObjLookedAt();
                            objectLookedAt = fireResistant.gameObject;
                        }

                        Transform textName = fireResistant.textName.transform;
                        textName.SetPositionAndRotation(textName.position, transform.rotation);
                    }
                    else if (hitTransform.GetComponent<ObjectNamePopUp>())
                    {
                        // Debug.Log("Faucet Object is Hit");  
                        ObjectNamePopUp faucet = hitTransform.GetComponent<ObjectNamePopUp>();
                        faucet.lookedAt = true;
                        
                        if(objectLookedAt != faucet.gameObject)
                        {
                            ResetLastObjLookedAt();
                            objectLookedAt = faucet.gameObject;
                        }

                        Transform textName = faucet.textName.transform;
                        textName.SetPositionAndRotation(textName.position, transform.rotation);
                    }
                    else if (hitTransform.GetComponent<Pail>())
                    {
                        // Debug.Log("Bucket Object is Hit");
                        Pail bucket = hitTransform.GetComponent<Pail>();
                        bucket.lookedAt = true;
                        
                        if(objectLookedAt != bucket.gameObject)
                        {    
                            ResetLastObjLookedAt();
                            objectLookedAt = bucket.gameObject;
                        }

                        Transform textName = bucket.textName.transform;
                        textName.SetPositionAndRotation(textName.position, transform.rotation);
                    }
                    else if (hitTransform.GetComponent<ElectricPlug>())
                    {
                        // Debug.Log("Plug is Hit");
                        ElectricPlug electricPlug = hitTransform.GetComponent<ElectricPlug>();
                        electricPlug.lookedAt = true;
                        
                        if(objectLookedAt != electricPlug.gameObject)
                        {
                            ResetLastObjLookedAt();
                            objectLookedAt = electricPlug.gameObject;
                        }

                        Transform textName = electricPlug.textName.transform;
                        textName.SetPositionAndRotation(textName.position, transform.rotation);
                    }
                    else if (hitTransform.GetComponent<FireExtinguisher>())
                    {
                        // Debug.Log("Extinguisher is hit");
                        FireExtinguisher fireExtinguisher = hitTransform.GetComponent<FireExtinguisher>();
                        fireExtinguisher.lookedAt = true;

                        if(objectLookedAt != fireExtinguisher.gameObject)
                        {
                            ResetLastObjLookedAt();
                            objectLookedAt = fireExtinguisher.gameObject;
                        }

                        Transform textName = fireExtinguisher.textName.transform;
                        textName.SetPositionAndRotation(textName.position, transform.rotation);
                    }
                    else ResetLastObjLookedAt();
                }
            }
            else
            {
                // reset looked at of last object to false
                ResetLastObjLookedAt();

                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * 2.0f, Color.white);
                // Debug.Log("Did not Hit");
                hitTransform = null;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Sound Effects
        if (playerBars.isRunning && !isOnFire) // Running audio
        {
            if (audioSource.clip != runningClip || !audioSource.isPlaying)
            {
                audioSource.clip = runningClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (playerBars.isWalking && !isOnFire) // Walking audio
        {
            if (audioSource.clip != walkingClip || !audioSource.isPlaying)
            {
                audioSource.clip = walkingClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (playerBars.isRolling) // Rolling audio
        {
            if (audioSource.clip != rollingClip || !audioSource.isPlaying)
            {
                audioSource.clip = rollingClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (playerBars.isCrawling && !isOnFire) // Crawling audio
        {
            if (audioSource.clip != crawlingClip || !audioSource.isPlaying)
            {
                audioSource.clip = crawlingClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (isOnFire) // On fire
        {
            if (audioSource.clip != burningClip || !audioSource.isPlaying && !playerBars.isRolling)
            {
                audioSource.clip = burningClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else // Heavy breathing audio when standing still
        {
            if (audioSource.clip != breathingClip || !audioSource.isPlaying && !isOnFire)
            {
                audioSource.clip = breathingClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        // Check if the player is grounded
        isGrounded = CheckIfGrounded();
        if (!isGrounded && !isOnStairs)
        {
            rb.AddForce(new Vector3(0, -20, 0));
            Debug.Log("I am floating");
        }

        // Transitions when stamina is 0
        // Run to walk
        if (playerBars.stamina <= 0 && currentState.Equals("Running"))
        {
            SetupUprightState();
            currentSpeed = walkingSpeed;
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

        timeElapsed += Time.deltaTime;

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
            else if (Input.GetKeyDown(KeyCode.Return)) EndGame();

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

    bool CheckIfGrounded()
    {

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.0f))
        {
            return true; 
        }
        return false; 
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
            SetupUprightState();
            currentSpeed = walkingSpeed;
        }
        else if (!heldObject)
        {
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
            SetupCrawlState();

            isHorizontalTiltEnabled = false;
            previousEulerAngles = transform.eulerAngles;
            cameraTransform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);

            rollingTimeLeft = 5.0f;
            rotationCount = 37;
            rollRotation = 5.0f;

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
                FireExtinguisher extinguisher = hitTransform.GetComponent<FireExtinguisher>();
                NonFlammableObject nonFlammable = hitTransform.GetComponent<NonFlammableObject>();
                if (pail)
                {
                    if(pail.hasWaterInside())
                        notificationSystem.notificationMessage = "[Left Click] to throw water at the fire\n[G] to Drop Bucket";
                    else
                        notificationSystem.notificationMessage = "[Left Click] to open the faucet to fill the bucket with water\n[G] to Drop Bucket";
                    
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 6.0f;
                    notificationSystem.displayNotification();

                    pail.closeProximityValue = closeProximityValue;
                    pail.playerCamera = transform.GetChild(0);
                    hitTransform.SetPositionAndRotation(
                        transform.position + transform.forward + transform.right * 0.7f - transform.up * 0.15f, 
                        transform.rotation);
                }
                else if (extinguisher)
                {
                    if (!extinguisher.isPinPulled)
                    {
                        notificationSystem.notificationMessage = "[Left Click] to PULL the Pin";
                        notificationSystem.disableTimer = 3.0f;
                    }
                    else
                    {
                        notificationSystem.notificationMessage = "AIM at fire, & Hold [Left Click] to SQUEEZE the handle and use extinguisher";
                        notificationSystem.disableTimer = 6.0f;
                    }

                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.displayNotification();

                    hitTransform.SetPositionAndRotation(
                        transform.position + transform.forward + transform.right * 0.7f - transform.up * 0.15f, 
                        transform.rotation);
                }
                else if (nonFlammable)
                {
                    notificationSystem.notificationMessage = "You can [Left Click] to throw this directly at a fire to put it out!\n[G] to Drop Object";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 6.0f;
                    notificationSystem.displayNotification();
                    hitTransform.SetPositionAndRotation(transform.position + transform.forward, transform.rotation);
                }
                else
                    hitTransform.SetPositionAndRotation(transform.position + transform.forward, transform.rotation);

                heldObject = hitTransform.GetComponent<GrabbableObject>();
                heldObject.isHeld = true;

                audioSource2.clip = pickUpClip;
                audioSource2.Play();

                ElectricPlug plug = heldObject.GetComponent<ElectricPlug>();
                if (plug)
                {
                    if(!isExtensionsResolved &&
                        plug.owner.name.Equals("ExtensionCord") && plug.pluggedInto.name.Equals("ExtensionCord"))
                    {
                        notificationSystem.notificationMessage = "Unplug unused appliances as well!\n*Take note that fire may come from other houses despite doing this*";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 7.0f;
                        notificationSystem.displayNotification();

                        isExtensionsResolved = true;
                    }
                    else
                    {
                        notificationSystem.notificationMessage = "[Left Click] to plug it in!\n[G] to Drop Plug";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 7.0f;
                        notificationSystem.displayNotification();
                    }

                    plug.pluggedInto = null;

                    if(fireManager) fireManager.RemoveSpawnPoint(plug.transform);
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
        
        if (currentState.Equals("Crawling") || currentState.Equals("Rolling"))
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
        else if (currentState.Equals(""))
            transform.position = new Vector3(transform.position.x, 1.2f, transform.position.z);

        currentState = "Walking";
    }

    void SetupCrawlState()
    {
        isVerticalTiltEnabled = false;
        isHorizontalTiltEnabled = true;
        transform.eulerAngles = new Vector3(75, transform.eulerAngles.y, transform.eulerAngles.z);
        cameraTransform.eulerAngles = new Vector3(30, transform.eulerAngles.y, transform.eulerAngles.z);

        if (currentState.Equals("Walking") || currentState.Equals("Running"))
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z);

        currentState = "Crawling";
    }

    void EndGame()
    {
        if (collidedWith)
        {
            if (collidedWith.name.Equals("Outside Floor"))
            {            
                // Debug.Log("You Won");
                winScreen.Setup(Mathf.FloorToInt(playerBars.hydrationLevel), Mathf.FloorToInt(timeElapsed));
                Cursor.lockState = CursorLockMode.None;
            }
            else if (collidedWith.name.Equals("Court") && fireManager)
            {           
                // Debug.Log("You Escaped");
                winScreen.oneStar.color = new Color(255f, 255f, 255f);
                if (playerBars.hydrationLevel > 50 && timeElapsed < 540)
                {
                    winScreen.twoStar.color = new Color(255f, 255f, 255f);
                }
                if (playerBars.hydrationLevel > 84 && playerBars.hydrationLevel < 101 && timeElapsed < 420)
                {
                    winScreen.threeStar.color = new Color(255f, 255f, 255f);
                }
                winScreen.Setup(Mathf.FloorToInt(playerBars.hydrationLevel), Mathf.FloorToInt(timeElapsed));
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (fireManager)
        {
            if (fireManager.isPlayerSuccessful && collision.collider.name.Equals("Outside Floor"))
            {
                collidedWith = collision.collider;

                EndGame();
            }
            else if (fireManager.isFireOngoing && collision.collider.name.Equals("Court"))
            {
                collidedWith = collision.collider;

                notificationSystem.notificationMessage = "Press [Enter] to End the Game!";
                notificationSystem.disableAfterTimer = true;
                notificationSystem.disableTimer = 5.0f;
                notificationSystem.displayNotification();
            }
        }
        if (collision.gameObject.tag == "Stairs")
        {
            isOnStairs = true;
        }
        if (collision.gameObject.tag == "SmokeLayer")
        {
            playerBars.oxygen = 0.0f;
        }

        else if (collision.collider.name.Equals("Court"))
            collidedWith = collision.collider;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Stairs")
        {
            isOnStairs = false;
        }
    }

    void ResetLastObjLookedAt()
    {
        if(objectLookedAt)
        {
            if (objectLookedAt.GetComponent<NonFlammableObject>())
                objectLookedAt.GetComponent<NonFlammableObject>().lookedAt = false;
            else if (objectLookedAt.GetComponent<ObjectNamePopUp>())
                objectLookedAt.GetComponent<ObjectNamePopUp>().lookedAt = false;
            else if (objectLookedAt.GetComponent<Pail>())
                objectLookedAt.GetComponent<Pail>().lookedAt = false;
            else if (objectLookedAt.GetComponent<ElectricPlug>())
                objectLookedAt.GetComponent<ElectricPlug>().lookedAt = false;
            else if (objectLookedAt.GetComponent<FireExtinguisher>())
                objectLookedAt.GetComponent<FireExtinguisher>().lookedAt = false;
            
            objectLookedAt = null;
        }
    }

    void TestFunction()
    {
        // key press for testing
        if (Input.GetKeyDown(KeyCode.T))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
