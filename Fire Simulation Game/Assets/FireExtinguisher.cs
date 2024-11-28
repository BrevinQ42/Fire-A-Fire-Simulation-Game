using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisher : FireFightingObject
{
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
