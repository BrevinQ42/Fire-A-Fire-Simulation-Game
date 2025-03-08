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

    public Coroutine moveCoroutine;

    public GameObject textName;
    public bool lookedAt;

    private bool isMessageSent;

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

        isMessageSent = false;
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
        else if (lookedAt && !isMessageSent)
        {
            isMessageSent = true;
            StartCoroutine(SendMessage());
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

    IEnumerator SendMessage()
    {
        notificationSystem.notificationMessage = "Try to prevent or extinguish fires first!";
        notificationSystem.disableAfterTimer = true;
        notificationSystem.disableTimer = 5.0f;
        notificationSystem.displayNotification();

        yield return new WaitForSeconds(5.0f);

        isMessageSent = false;
    }
    
    public void toggleDoor()
    {
        isOpen = !isOpen;
    }
}
