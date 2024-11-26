using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Fire FireOnPlayer;

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
    private FireFightingObject heldObject;

    [SerializeField] private float throwForce;

    public Vector3 movementVector;

    private PlayerBars playerBars;

    // Start is called before the first frame update
    void Start()
    {
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
    }

    void FixedUpdate()
    {
        if (heldObject || currentState.Equals("Crawling") || currentState.Equals("Rolling")) hitTransform = null;
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, closeProximityValue))
            {
                if (hit.transform.CompareTag("Floor") || hit.transform.CompareTag("Fire")) hitTransform = null;
                else
                {
                    Debug.DrawRay(cameraTransform.position, cameraTransform.forward * hit.distance, Color.yellow);
                    Debug.Log(hit.transform.name + ": " + hit.distance);

                    hitTransform = hit.transform;
                }
            }
            else
            {
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

        // basic controls
        Move();
        LookAround();

        if (currentState.Equals("Rolling")) RollOver();

        else
        {
            // actions
            if (Input.GetKeyDown(KeyCode.C)) ToggleCrawl();
            else if (Input.GetKeyDown(KeyCode.F)) ToggleRun();
            else if (Input.GetKeyDown(KeyCode.R)) InitiateRollOver();
            else if (Input.GetKeyDown(KeyCode.E)) InteractWithObject();
            else if (Input.GetKeyDown(KeyCode.G)) DropObject();

            if (!heldObject)
            {
                if (Input.GetMouseButton(0)) CoverNose();
                else isCoveringNose = false;
            }
            else if (heldObject && Input.GetMouseButtonDown(0)) UseObject();
        }

        TestFunction(); // for debugging (i.e. Debug.Log)
    }

    void Move()
    {
        movementVector = Vector3.zero;

        // Prevent movement while crawling with less than 5 stamina
        if (currentState == "Crawling" && playerBars.stamina < 4)
        {
            rb.velocity = Vector3.zero;
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
                FireOnPlayer.AffectFire(-0.1f);

                if (Math.Round(FireOnPlayer.intensityValue, 2) <= 0.01f)
                {
                    FireOnPlayer = null;
                    isOnFire = false;
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
                if (pail)
                {
                    pail.closeProximityValue = closeProximityValue;
                    pail.playerCamera = transform.GetChild(0);
                    hitTransform.SetPositionAndRotation(
                        transform.position + transform.forward + transform.right * 0.7f - transform.up * 0.15f, 
                        transform.rotation);
                }
                else
                    hitTransform.SetPositionAndRotation(transform.position + transform.forward, transform.rotation);

                heldObject = hitTransform.GetComponent<FireFightingObject>();
                heldObject.isHeld = true;
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
        bool isStillHeld;
        heldObject.Use(throwForce, out isStillHeld);

        if (!isStillHeld) heldObject = null;
    }

    void CoverNose()
    {
        Debug.Log("Covering nose");
        isCoveringNose = true;
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

    void TestFunction()
    {
        // key press for testing
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(previousEulerAngles);
        }
    }
}
