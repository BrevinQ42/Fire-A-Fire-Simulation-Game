using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonFlammableObject : FireFightingObject
{
    public override void Use(float throwForce, out bool isStillHeld)
    {
        Throw(throwForce);
        isStillHeld = isHeld;
    }

    void Throw(float throwForce)
    {
        Deattach();
        GetComponent<Rigidbody>().AddForce(transform.forward * throwForce);
    }
}
