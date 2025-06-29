using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonFlammableObject : FireFightingObject
{
    private FireManager fireManager;

    public bool lookedAt;
    public GameObject textName;

    public bool isOnPan;

    void Start()
    {
        lookedAt = false;
        textName = GetComponentInChildren<TextMesh>().gameObject;

        isOnPan = false;

        InitializeFireManager();
    }

    void Update()
    { 
        if (lookedAt == false)
        {
            textName.SetActive(false);
        }

        if (lookedAt == true)
        {
            textName.SetActive(true);
        }

        if (isHeld == true)
        {
            textName.SetActive(false);
            isOnPan = false;
        }
    }

    void InitializeFireManager()
    {
        GameObject fm = GameObject.Find("FireManager");
        
        if (fm) fireManager = fm.GetComponent<FireManager>();
    }

    public override void Use(float throwForce, out bool isStillHeld)
    {
        Throw(throwForce);
        isStillHeld = isHeld;
    }

    void Throw(float throwForce)
    {
        Deattach();
        transform.position += Vector3.up * 0.67f;
        GetComponent<Rigidbody>().AddForce(transform.forward * throwForce);
    }

    public void PutOnPan(Transform pan, out string notifMessage, out float messageDuration)
    {
        Deattach();
        transform.SetParent(pan);
        transform.localPosition = pan.GetChild(1).localPosition;
    
        string message = "none";
        float duration = 0.0f;

        foreach (Transform child in pan)
        {
            if (child.CompareTag("Fire"))
            {
                if (!child.GetComponent<Fire>())
                {
                    Destroy(child.gameObject);

                    if (message.Equals("none"))
                    {
                        message = "Fire got taken out!";
                        duration = 3.0f;
                    }

                    if (transform.childCount > 1 && !transform.GetChild(1).GetComponent<Fire>())
                        fireManager.RemoveSpawnPoint(transform.GetChild(1));
                }
            }
        }

        notifMessage = message;
        messageDuration = duration;

        isOnPan = true;
    }
}
