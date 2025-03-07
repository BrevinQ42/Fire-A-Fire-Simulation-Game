using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isOpen = false;

    public Vector3 closedPosition;
    public Vector3 openPosition;
    public float speed = 5f;

    public Quaternion closedRotation;
    public Quaternion openRotation;

    public Coroutine moveCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        closedPosition = transform.position;
        openPosition = new Vector3(19.0f, -0.3f, -22.1f);

        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(0, 270, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (isOpen)
        {
            transform.position = Vector3.Lerp(transform.position, openPosition, Time.deltaTime * speed);
            transform.rotation = Quaternion.Lerp(transform.rotation, openRotation, Time.deltaTime * speed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, closedPosition, Time.deltaTime * speed);
            transform.rotation = Quaternion.Lerp(transform.rotation, closedRotation, Time.deltaTime * speed);
        }
    }
    
    public void toggleDoor()
    {
        isOpen = !isOpen;
    }
}
