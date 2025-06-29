using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFightingObject : GrabbableObject
{
    public float fireFightingValue;

    public virtual void Use(float throwForce, out bool isStillHeld)
    {
        // filler code (for debugging purposes)
        Debug.Log(gameObject.name + " is used!");
        isStillHeld = isHeld;
    }
}
