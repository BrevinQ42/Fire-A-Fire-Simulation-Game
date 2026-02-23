using System;
using System.Collections;
using System.Collections.Generic;
// using UnityEditor.Search;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // Level Manager
    public LevelManager levelManager;

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
    public FireManager fireManager;

    [Header("Player Components")]
    private Rigidbody rb;
    [SerializeField] private Transform cameraTransform;

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
    [SerializeField] private TextMeshProUGUI mouseSensText;
    private Vector2 cameraRotation;
    [SerializeField] private float mouseSensitivity;
    private bool isVerticalTiltEnabled;
    private bool isHorizontalTiltEnabled;

    private int layerMask;
    [SerializeField] private float closeProximityValue; // distance that is considered to be in close proximity
    [SerializeField] private Transform hitTransform;    // (nearby) object that is being pointed at by the player
    [SerializeField] private GrabbableObject heldObject;
    [SerializeField] private GameObject objectLookedAt;

    [Header("Player Roll")]
    private Vector3 previousEulerAngles;
    private float rollingTimeLeft;
    private int rotationCount;
    private float rollRotation;
    [SerializeField] private float rollTimePerRotation;

    [Header("Player Stamina")]
    [SerializeField] private PlayerBars playerBars;
    public float staminaRequiredForCrawling;
    public float staminaRequiredForRunning;
    public float staminaRequiredForRolling;

    [Header("Sound Effects")]
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
    [SerializeField] private float gravityForce;
    private bool isGameEnded;

    // Start is called before the first frame update
    void Start()
    {
        ConfigureReferences();

        isExtensionsResolved = false;

        collidedWith = null;

        rb = GetComponent<Rigidbody>();
        cameraTransform = transform.GetChild(0);

        SetupUprightState();
        currentSpeed = walkingSpeed;

        cameraRotation = Vector2.zero;
        if (fireManager) Cursor.lockState = CursorLockMode.Locked;
        layerMask = LayerMask.GetMask("Default", "TransparentFX", "Water", "UI", "Door", "Ignore Navmesh");

        isCoveringNose = false;
        isOnFire = false;

        previousEulerAngles = Vector3.zero;
        rollingTimeLeft = 0.0f;
        rotationCount = 0;
        rollRotation = 5.0f;

        hitTransform = null;
        heldObject = null;

        playerBars = GetComponent<PlayerBars>();

        isGameEnded = false;

        if (fireManager)
        {
            notificationSystem.notificationMessage = "Try to prevent fires!\nOne way is to avoid plugging extension cords on others";
            notificationSystem.disableAfterTimer = true;
            notificationSystem.disableTimer = 7.5f;
            notificationSystem.displayNotification();
        }
        else
        {
            notificationSystem.notificationMessage = "WELCOME TO THE TUTORIAL";
            notificationSystem.disableAfterTimer = true;
            notificationSystem.disableTimer = 6.0f;
            notificationSystem.displayNotification();
        }

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
        ResetLastObjLookedAt();

        if (currentState.Equals("Rolling")) hitTransform = null;
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, closeProximityValue, layerMask))
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
                    if (hitTransform.CompareTag("WaterSource"))
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
                    else if (heldObject && heldObject.GetComponent<NonFlammableObject>())
                    {
                        // Debug.Log("Frying Pan Object is Hit");  
                        ObjectNamePopUp fryingPan = hitTransform.GetComponent<ObjectNamePopUp>();
                        fryingPan.lookedAt = true;
                        
                        if(objectLookedAt != fryingPan.gameObject)
                        {
                            ResetLastObjLookedAt();
                            objectLookedAt = fryingPan.gameObject;
                        }

                        Transform textName = fryingPan.textName.transform;
                        textName.SetPositionAndRotation(textName.position, transform.rotation);
                    }
                    else ResetLastObjLookedAt();
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
                    textName.SetPositionAndRotation(hit.point - transform.forward * 0.25f, transform.rotation);
                }
                else if (hitTransform.GetComponent<Candle>())
                {
                    // Debug.Log("Candle is hit");
                    Candle candle = hitTransform.GetComponent<Candle>();
                    candle.lookedAt = true;

                    if(objectLookedAt != candle.gameObject)
                    {
                        ResetLastObjLookedAt();
                        objectLookedAt = candle.gameObject;
                    }

                    Transform textName = candle.textName.transform;
                    textName.SetPositionAndRotation(textName.position, transform.rotation);
                }
                else if (hitTransform.GetComponent<Door>())
                {
                    Door door = hitTransform.GetComponent<Door>();
                    door.lookedAt = true;

                    if (objectLookedAt != door.gameObject)
                    {
                        ResetLastObjLookedAt();
                        objectLookedAt = door.gameObject;
                    }

                    Transform textName = door.textName.transform;
                    textName.SetPositionAndRotation(textName.position, transform.rotation);
                }
                // else ResetLastObjLookedAt();
            }
            else
            {
                // reset looked at of last object to false
                ResetLastObjLookedAt();

                // Debug.DrawRay(cameraTransform.position, cameraTransform.forward * 2.0f, Color.white);
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
            rb.AddForce(new Vector3(0, -gravityForce, 0));
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

        // end game
        if (fireManager.isPlayerSuccessful && !isGameEnded) EndGame();

        timeElapsed += Time.deltaTime;

        if (currentState.Equals("Rolling"))
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                // switch them to crawling state
                transform.eulerAngles = previousEulerAngles;
                ToggleCrawl();
            }
        }
        else
        {
            // basic controls
            Move();
            LookAround();

            // actions
            if (Input.GetKeyDown(KeyCode.C)) ToggleCrawl();
            else if (Input.GetKeyDown(KeyCode.LeftShift))
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
            else
            {
                if (Input.GetMouseButton(0)) UseObject();
                else if (heldObject.GetComponent<FireExtinguisher>())
                    heldObject.GetComponent<FireExtinguisher>().isBeingUsed = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            mouseSensitivity = (float) Math.Round(Math.Max(mouseSensitivity - 0.05f, 0.05f), 2);
            mouseSensText.text = "SENS: " + mouseSensitivity;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            mouseSensitivity = (float) Math.Round(Math.Min(mouseSensitivity + 0.05f, 2.00f), 2);
            mouseSensText.text = "SENS: " + mouseSensitivity;
        }

        // TestFunction(); // for debugging (i.e. Debug.Log)
    }

    void ConfigureReferences()
    {
        try
        {
            GameObject lm = GameObject.Find("LevelManager");
            levelManager = lm.GetComponent<LevelManager>();
            GameObject fm = GameObject.Find("FireManager");
            fireManager = fm.GetComponent<FireManager>();
        }
        catch { Debug.Log("Tutorial"); }

        winScreen = GameObject.Find("WinScreen").GetComponent<WinScreen>();
        winScreen.gameObject.SetActive(false);

        notificationSystem = GameObject.Find("MainPanel").GetComponent<NotificationTriggerEvent>();
        noseNotificationSystem = GameObject.Find("NoseIcon").GetComponent<NoseNotification>();

        mouseSensText = GameObject.Find("MouseSensText").GetComponent<TextMeshProUGUI>();
    }

    bool CheckIfGrounded()
    {

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.0f))
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                Debug.Log(hit.collider.gameObject.name);
            }
            if (hit.collider.gameObject.name != "FryingPan") // HalfHouse1 Stairs Fix
            {
                return true;
            }
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

        // move character accordingly
        rb.velocity = movementVector.normalized * currentSpeed;

        if (movementVector != Vector3.zero)
            rb.velocity = new Vector3(rb.velocity.x, 0.05f, rb.velocity.z);
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
            else if (objectLookedAt.GetComponent<Candle>())
                objectLookedAt.GetComponent<Candle>().lookedAt = false;
            else if (objectLookedAt.GetComponent<Door>())
                objectLookedAt.GetComponent<Door>().lookedAt = false;

            objectLookedAt = null;
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
            Rigidbody hitRB = hitTransform.GetComponent<Rigidbody>();

            if (hitRB && hitTransform.CompareTag("Grabbable"))
            {
                if (heldObject)
                {
                    notificationSystem.notificationMessage = "[G] to Drop Held Object first!";
                    notificationSystem.disableTimer = 3.0f;
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.displayNotification();

                    // disregard when holding an object
                    return;
                }

                hitRB.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

                hitTransform.GetComponent<Collider>().enabled = false;

                hitTransform.SetParent(transform);

                Pail pail = hitTransform.GetComponent<Pail>();
                FireExtinguisher extinguisher = hitTransform.GetComponent<FireExtinguisher>();
                NonFlammableObject nonFlammable = hitTransform.GetComponent<NonFlammableObject>();
                ElectricPlug plug = hitTransform.GetComponent<ElectricPlug>();
                if (pail)
                {
                    if(pail.getWaterInside() > 0.0f)
                    {
                        notificationSystem.notificationMessage = "[Left Click] to throw water at the fire\n[G] to Drop Bucket";
                        notificationSystem.disableTimer = 6.0f;
                    }
                    else
                    {
                        notificationSystem.notificationMessage = "[G] to Drop Bucket and [E] to Open Faucet\nGrab Bucket & align it to catch the water. You will see it fill up to the brim";
                        notificationSystem.disableTimer = 10.0f;
                    }
                    
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.displayNotification();

                    pail.closeProximityValue = closeProximityValue;
                    pail.playerCamera = transform.GetChild(0);
                    hitTransform.SetPositionAndRotation(
                        transform.position + transform.forward + transform.right * 0.7f + transform.up * 0.65f, 
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
                        notificationSystem.notificationMessage = "AIM at the fire, & Hold [Left Click] to SQUEEZE the handle.\n SWEEP the nozzle side to side";
                        notificationSystem.disableTimer = 8.0f;
                    }

                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.displayNotification();

                    hitTransform.SetPositionAndRotation(
                        transform.position + transform.forward + transform.right * 0.7f + transform.up * 0.65f, 
                        transform.rotation);
                }
                else
                {
                    if (nonFlammable)
                    {
                        notificationSystem.notificationMessage = "You can [Left Click] to throw this directly at a fire to put it out!\n[G] to Drop Object";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 6.0f;
                        notificationSystem.displayNotification();
                    }

                    hitTransform.SetPositionAndRotation(transform.position + transform.forward + transform.up * 0.8f, transform.rotation);
                }

                heldObject = hitTransform.GetComponent<GrabbableObject>();
                heldObject.isHeld = true;

                audioSource2.clip = pickUpClip;
                audioSource2.Play();

                if (plug)
                {
                    if(!isExtensionsResolved && plug.owner.name.Equals("ExtensionCord") &&
                        plug.pluggedInto && plug.pluggedInto.name.Equals("ExtensionCord"))
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

                    plug.Unplug();
                }
            }
            else if (hitTransform.CompareTag("WaterSource"))
            {
                Spawner ws = hitTransform.GetChild(0).GetComponent<Spawner>();
                ws.Toggle();
            }
            else if (hitTransform.GetComponent<Candle>())
            {
                Candle candle = hitTransform.GetComponent<Candle>();
                if (candle.transform.childCount > 1)
                {
                    Fire FireOnCandle = candle.transform.GetChild(0).GetComponent<Fire>();
                    if (FireOnCandle.intensityValue > 0.15f)
                    {
                        FireOnCandle.AffectFire(0.01f);

                        notificationSystem.notificationMessage = "Fire is too big to be blown out!\nTry another way to put it out.";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 7.0f;
                        notificationSystem.displayNotification();
                    }
                    else
                    {
                        fireManager.RemoveSpawnPoint(FireOnCandle.transform);
                        Destroy(FireOnCandle.gameObject);

                        notificationSystem.notificationMessage = "You have blown out the fire on the candle!\nPut out the fires on other unused candles as well!";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 8.0f;
                        notificationSystem.displayNotification();
                    }
                }
            }
            else if (hitTransform.GetComponent<Door>())
            {
                if (heldObject)
                {
                    notificationSystem.notificationMessage = "[G] to Drop Held Object first!";
                    notificationSystem.disableTimer = 3.0f;
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.displayNotification();

                    // disregard when holding an object
                    return;
                }
                
                Door door = hitTransform.GetComponent<Door>();
                door.toggleDoor();
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
            NonFlammableObject fireResistant = heldObject.GetComponent<NonFlammableObject>();
            if (fireResistant && hitTransform && hitTransform.name.Equals("FryingPan"))
            {
                string message;
                float duration;

                fireResistant.PutOnPan(hitTransform, out message, out duration);
                heldObject = null;

                if (!message.Equals("none"))
                {
                    notificationSystem.notificationMessage = message;
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = duration;
                    notificationSystem.displayNotification();
                }
            }
            else
            {
                bool isStillHeld;
                obj.Use(throwForce, out isStillHeld);

                if (obj.GetComponent<FireExtinguisher>())
                    obj.GetComponent<FireExtinguisher>().isBeingUsed = true;

                if (!isStillHeld) heldObject = null;
            }
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
        isGameEnded = true;

        try
        {
            StopCoroutine(playerBars.FireDamageOverTime());
            Destroy(FireOnPlayer.gameObject);
        }
        catch {}

        try
        {
            StopCoroutine(playerBars.OxygenDamageOverTime());
        }
        catch {}

        // Debug.Log("You Won");
        winScreen.oneStar.color = new Color(255f, 255f, 255f);
        if (playerBars.hydrationLevel > 50 && timeElapsed < 300)
        {
            winScreen.twoStar.color = new Color(255f, 255f, 255f);
        }
        if (playerBars.hydrationLevel > 84 && playerBars.hydrationLevel < 101 && timeElapsed < 180 && fireManager.isPlayerSuccessful) //3 Stars if they put out the fire 
        {
            winScreen.threeStar.color = new Color(255f, 255f, 255f);
        }
        if (playerBars.hydrationLevel > 84 && playerBars.hydrationLevel < 101 && timeElapsed < 180 && levelManager.isClassCExtinguisher == false) //3 Stars if they escape from an electrical fire and there is no electrical extinguisher 
        {
            winScreen.threeStar.color = new Color(255f, 255f, 255f);
        }
        if (playerBars.hydrationLevel > 84 && playerBars.hydrationLevel < 101 && timeElapsed < 180 && levelManager.isClassKExtinguisher == false) //3 Stars if they escape from a grease fire and there is no grease extinguisher 
        {
            winScreen.threeStar.color = new Color(255f, 255f, 255f);
        }
        winScreen.Setup(Mathf.FloorToInt(playerBars.hydrationLevel), Mathf.FloorToInt(timeElapsed));
        Cursor.lockState = CursorLockMode.None;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (fireManager)
        {
            if (fireManager.isPlayerSuccessful && collision.collider.name.Equals("Outside Floor"))
            {
                collidedWith = collision.collider;

                EndGame();
                
                return;
            }
            else if (fireManager.isFireOngoing && collision.collider.name.Equals("Court"))
            {
                collidedWith = collision.collider;

                notificationSystem.notificationMessage = "You successfully escaped!\n";
                notificationSystem.disableAfterTimer = false;
                notificationSystem.displayNotification();

                EndGame();

                return;
            }
        }

        if (collision.gameObject.tag == "Stairs")
        {
            isOnStairs = true;
        }
        else if (collision.gameObject.tag == "SmokeLayer")
        {
            playerBars.oxygen = 0.0f;
        }
        else if (collision.collider.GetComponent<NPC>())
        {
            NPC npc = collision.collider.GetComponent<NPC>();

            if (FireOnPlayer)
            {
                if (npc.FireOnNPC)
                {
                    npc.FireOnNPC.AffectFire(FireOnPlayer.intensityValue);
                    FireOnPlayer.AffectFire(npc.FireOnNPC.intensityValue);
                }
                else
                {
                    npc.FireOnNPC = Instantiate(FireOnPlayer.gameObject,
                                            npc.transform.position,
                                            Quaternion.identity).GetComponent<Fire>();

                    npc.FireOnNPC.GetComponent<Collider>().enabled = false;

                    npc.FireOnNPC.intensityValue = FireOnPlayer.intensityValue;

                    npc.FireOnNPC.transform.SetParent(npc.transform);
                }
            }
            else if (npc.FireOnNPC)
            {
                FireOnPlayer = Instantiate(npc.FireOnNPC.gameObject,
                                            transform.position + transform.forward * 0.5f,
                                            Quaternion.identity).GetComponent<Fire>();

                FireOnPlayer.GetComponent<Collider>().enabled = false;

                FireOnPlayer.transform.SetParent(transform);
                FireOnPlayer.intensityValue = npc.FireOnNPC.intensityValue;
                isOnFire = true;
            }
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

    // void TestFunction()
    // {
    //     // key press for testing
    //     if (Input.GetKeyDown(KeyCode.T))
    //     {
    //         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    //     }
    // }
}
