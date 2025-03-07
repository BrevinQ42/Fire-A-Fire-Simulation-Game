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

    public GameObject textName;
    public bool lookedAt;

    public PlayerController playerController;
    // Start is called before the first frame update
    void Start()
    {
        closedPosition = transform.position;
        openPosition = new Vector3(19.0f, -0.3f, -22.1f);

        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(0, 270, 0);

        lookedAt = false;
        textName = GetComponentInChildren<TextMesh>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.timeElapsed >= 60)
        {
            if (isOpen)
            {
                transform.position = Vector3.Lerp(transform.position, openPosition, Time.deltaTime * speed);
                transform.rotation = Quaternion.Lerp(transform.rotation, openRotation, Time.deltaTime * speed);
                textName.GetComponent<TextMesh>().text = "[E] to close door";
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, closedPosition, Time.deltaTime * speed);
                transform.rotation = Quaternion.Lerp(transform.rotation, closedRotation, Time.deltaTime * speed);
                textName.GetComponent<TextMesh>().text = "[E] to open door";
            }
        }

        if (lookedAt == false)
        {
            textName.SetActive(false);
        }
        if (lookedAt == true)
        {
            textName.SetActive(true);
        }
    }
    
    public void toggleDoor()
    {
        isOpen = !isOpen;
    }
}
