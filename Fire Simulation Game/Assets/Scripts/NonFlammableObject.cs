using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonFlammableObject : FireFightingObject
{
    public bool lookedAt;
    public GameObject textName;
    public PlayerController player;

    void Start()
    {
        lookedAt = false;
        textName = transform.Find("FloatingText").gameObject;
    }

    void Update()
    { 
        if (lookedAt == false)
        {
            textName.SetActive(false);
        }
        if (lookedAt == true && isHeld == false)
        {
            textName.SetActive(true);
        }
        if (isHeld == true)
        {
            textName.SetActive(false);
        }
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
}
