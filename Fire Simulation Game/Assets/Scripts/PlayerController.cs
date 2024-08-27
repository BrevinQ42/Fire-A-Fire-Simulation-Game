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

    private Vector3 previousMousePosition;
    [SerializeField] private float mouseSensitivity;
    private bool isVerticalTiltEnabled;
    private bool isHorizontalTiltEnabled;

    private Vector3 previousEulerAngles;
    private float rollingTimeLeft;
    private int rotationCount;
    private float rollRotation;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform = transform.GetChild(0);

        currentState = "Walking";

        previousMousePosition = Input.mousePosition;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        isVerticalTiltEnabled = true;
        isHorizontalTiltEnabled = true;

        previousEulerAngles = Vector3.zero;
        rollingTimeLeft = 0.0f;
        rotationCount = 0;
        rollRotation = 5.0f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movementVector = Vector3.zero;

        // movement controls
        if (Input.GetKey(KeyCode.W))
            movementVector = new Vector3(transform.forward.x, 0.0f, transform.forward.z);
        else if (Input.GetKey(KeyCode.S))
            movementVector = new Vector3(-transform.forward.x, 0.0f, -transform.forward.z);
        
        if (Input.GetKey(KeyCode.A))
            movementVector += new Vector3(-transform.right.x, 0.0f, -transform.right.z);
        else if (Input.GetKey(KeyCode.D))
            movementVector += new Vector3(transform.right.x, 0.0f, transform.right.z);

        // move character accordingly
        if (currentState.Equals("Crawling"))
            rb.velocity = movementVector.normalized * crawlingSpeed;
        else if (currentState.Equals("Running"))
            rb.velocity = movementVector.normalized * runningSpeed;
        else
            rb.velocity = movementVector.normalized * walkingSpeed;


        // toggle for crawling
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (currentState.Equals("Crawling"))
            {
                currentState = "Walking";

                isVerticalTiltEnabled = true;
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);

                cameraTransform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
            }
            else
            {
                currentState = "Crawling";

                transform.position = new Vector3(transform.position.x, 0.63f, transform.position.z);
                isVerticalTiltEnabled = false;
                transform.eulerAngles = new Vector3(75, transform.eulerAngles.y, transform.eulerAngles.z);

                cameraTransform.eulerAngles = new Vector3(30, transform.eulerAngles.y, transform.eulerAngles.z);
            }
        }


        // toggle for running
        else if (Input.GetKeyDown(KeyCode.F))
        {
            if (currentState.Equals("Running"))
                currentState = "Walking";
            else
                currentState = "Running";
        }


        // key press to start rolling over
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentState.Equals("Rolling"))
            {
                currentState = "Crawling";
                isHorizontalTiltEnabled = true;
                transform.eulerAngles = previousEulerAngles;

                cameraTransform.eulerAngles = new Vector3(30, transform.eulerAngles.y, transform.eulerAngles.z);
            }
            else
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
                
                currentState = "Rolling";
            }
        }


        if (currentState.Equals("Rolling"))
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
                currentState = "Crawling";
                isHorizontalTiltEnabled = true;
                transform.eulerAngles = previousEulerAngles;

                cameraTransform.eulerAngles = new Vector3(30, transform.eulerAngles.y, transform.eulerAngles.z);
            }
        }


        // holding of left click button
        if (Input.GetMouseButton(0))
        {
            // if not holding anything, is covering nose
            // else using item (when applicable)
        }
        

        // key press for testing
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(transform.eulerAngles);
        }


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
}
