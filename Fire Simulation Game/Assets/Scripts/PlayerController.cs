using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;

    private bool isRunning;
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;

    private Vector3 previousMousePosition;
    [SerializeField] private float mouseSensitivity;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        isRunning = false;

        previousMousePosition = Input.mousePosition;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
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

        // toggle for running
        if (Input.GetKeyDown(KeyCode.R))
            isRunning = !isRunning;
        else if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(transform.eulerAngles);
            Debug.Log(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z));
        }

        // move character accordingly
        if (isRunning)
            rb.velocity = movementVector.normalized * runningSpeed;
        else
            rb.velocity = movementVector.normalized * walkingSpeed;

        // adjusting camera when looking around
        Vector3 currentMousePosition = Input.mousePosition;

        transform.Rotate(0, (currentMousePosition.x - previousMousePosition.x) * mouseSensitivity, 0, Space.World);
        transform.Rotate(-(currentMousePosition.y - previousMousePosition.y) * mouseSensitivity, 0, 0, Space.Self);

        // configure x rotation
        float xRotation = transform.eulerAngles.x;

        if (xRotation > 180) xRotation -= 360;

        // limit the x rotation between -75 and 75 degrees
        if (xRotation > 75) transform.Rotate(-(xRotation - 75), 0, 0, Space.Self);
        else if (xRotation < -75) transform.Rotate(-(xRotation + 75), 0, 0, Space.Self);

        previousMousePosition = currentMousePosition;
    }
}
