using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private NotificationTriggerEvent notificationSystem;

    public bool isOpen = false;

    public Vector3 closedPosition;
    public Vector3 openPosition;
    public float speed = 5f;

    public Quaternion closedRotation;
    public Quaternion openRotation;

    public GameObject textName;
    public bool lookedAt;
    public PlayerController playerController;
    public Transform openTargetPosition;

    void Start()
    {
        closedPosition = transform.position;
        closedRotation = transform.rotation;

        openPosition = openTargetPosition.position;
        openRotation = transform.rotation * Quaternion.Euler(0, 270, 0);

        lookedAt = false;
        textName = GetComponentInChildren<TextMesh>().gameObject;
    }

    void Update()
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

        textName.SetActive(lookedAt);
    }

    public void toggleDoor()
    {
        isOpen = !isOpen;
    }
}
