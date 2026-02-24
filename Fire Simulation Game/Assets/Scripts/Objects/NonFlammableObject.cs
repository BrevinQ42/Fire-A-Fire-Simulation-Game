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

    public override string GetObjectType()
    {
        return "NonFlammableObject";
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
    
        notifMessage = "none";
        messageDuration = 0.0f;

        if (pan.childCount == 5)
        {
            Destroy(pan.GetChild(2).gameObject);

            notifMessage = "Fire got taken out!";
            messageDuration = 3.0f;

            if (fireManager)
                fireManager.RemoveSpawnPoint(pan.GetChild(1));
        }

        isOnPan = true;
    }
}
