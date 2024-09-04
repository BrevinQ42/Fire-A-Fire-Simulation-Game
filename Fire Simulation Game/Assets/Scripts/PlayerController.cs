using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Transform cameraTransform;

    private string currentState;

    [SerializeField] private float walkingSpeed;
    [SerializeField] private float crawlingSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float currentSpeed;

    private Vector3 previousMousePosition;
    [SerializeField] private float mouseSensitivity;
    private bool isVerticalTiltEnabled;
    private bool isHorizontalTiltEnabled;

    private Vector3 previousEulerAngles;
    private float rollingTimeLeft;
    private int rotationCount;
    private float rollRotation;

    [SerializeField] private float closeProximityValue; // distance that is considered to be in close proximity
    private Transform hitTransform;                        // (nearby) object that is being pointed at by the player
    bool isHoldingObject;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform = transform.GetChild(0);

        currentState = "Walking";
        SetupUprightState();
        currentSpeed = walkingSpeed;

        previousMousePosition = Input.mousePosition;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        previousEulerAngles = Vector3.zero;
        rollingTimeLeft = 0.0f;
        rotationCount = 0;
        rollRotation = 5.0f;

        hitTransform = null;
        isHoldingObject = false;
    }

    void FixedUpdate()
    {
        if(isHoldingObject || currentState.Equals("Crawling") || currentState.Equals("Rolling")) hitTransform = null;
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, closeProximityValue))
            {
                if (hit.transform.gameObject.name.Equals("Floor")) hitTransform = null;
                else
                {
                    Debug.DrawRay(cameraTransform.position, cameraTransform.forward * hit.distance, Color.yellow);
                    Debug.Log(hit.transform.gameObject.name + ": " + hit.distance);

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
            
            if (Input.GetMouseButton(0)) CoverNose(); // holding of left click button
        }

        TestFunction(); // for debugging (i.e. Debug.Log)
    }

    void Move()
    {
        Vector3 movementVector = Vector3.zero;

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

    void LookAround()
    {
        // adjusting camera when looking around
        Vector3 currentMousePosition = Input.mousePosition;

        if (isHorizontalTiltEnabled)
        {
            transform.Rotate(0, (currentMousePosition.x - previousMousePosition.x) * mouseSensitivity, 0, Space.World);

            // configure x rotation
            float xRotation = transform.eulerAngles.x;

            if (xRotation > 180) xRotation -= 360;

            // limit the x rotation between -75 and 75 degrees
            if (xRotation > 75.0f) transform.Rotate(-(xRotation - 75.0f), 0, 0, Space.Self);
            else if (xRotation < -75.0f) transform.Rotate(-(xRotation + 75.0f), 0, 0, Space.Self);
        }

        if (isVerticalTiltEnabled)
            transform.Rotate(-(currentMousePosition.y - previousMousePosition.y) * mouseSensitivity, 0, 0, Space.Self);

        previousMousePosition = currentMousePosition;
    }

    void ToggleCrawl()
    {
        if (currentState.Equals("Crawling"))
        {
            currentState = "Walking";
            SetupUprightState();
            currentSpeed = walkingSpeed;
        }
        else if (!isHoldingObject)
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
        if(!isHoldingObject)
        {
            isVerticalTiltEnabled = false;
            isHorizontalTiltEnabled = false;
            rollingTimeLeft = 5.0f;
            rotationCount = 37;
            rollRotation = 5.0f;

            transform.position = new Vector3(transform.position.x, 0.63f, transform.position.z);
            transform.eulerAngles = new Vector3(75, transform.eulerAngles.y, transform.eulerAngles.z);
            previousEulerAngles = transform.eulerAngles;
            cameraTransform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
            
            currentSpeed = 0;
            currentState = "Rolling";
        }
    }

    void RollOver()
    {
        transform.Rotate(0.0f, rollRotation, 0.0f, Space.Self);
        rotationCount--;

        if (rotationCount == 0)
        {
            rotationCount = 74;
            rollRotation = -rollRotation;
        }

        rollingTimeLeft -= Time.deltaTime;

        if (rollingTimeLeft <= 0.0f)
        {
            transform.eulerAngles = previousEulerAngles;
            ToggleCrawl();
        }
    }

    void InteractWithObject()
    {
        if (hitTransform != null)
        {
            Rigidbody hitRB = hitTransform.GetComponent<Rigidbody>();
            hitRB.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            
            hitTransform.SetParent(transform);
            hitTransform.SetLocalPositionAndRotation(new Vector3(0.0f, 0.0f, 1.0f), Quaternion.identity);
            
            isHoldingObject = true;
        }
    }

    void DropObject()
    {
        if(isHoldingObject)
        {
            transform.GetChild(1).GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            transform.GetChild(1).SetParent(null);
           
            isHoldingObject = false;
        }
    }

    void CoverNose()
    {
        Debug.Log("Covering nose");
    }

    void SetupUprightState()
    {
        isVerticalTiltEnabled = true;
        isHorizontalTiltEnabled = true;
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
        cameraTransform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    void SetupCrawlState()
    {
        transform.position = new Vector3(transform.position.x, 0.63f, transform.position.z);
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
