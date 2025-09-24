using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFightingObject : GrabbableObject
{
    public float fireFightingValue;
    public Transform parent;
    public bool isOutside;

    public virtual void Use(float throwForce, out bool isStillHeld)
    {
        // filler code (for debugging purposes)
        Debug.Log(gameObject.name + " is used!");
        isStillHeld = isHeld;
    }

    public virtual string GetObjectType()
    {
        return "object";
    }

    public override void Deattach()
    {
        GetComponent<Collider>().enabled = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        transform.SetParent(parent);
        isHeld = false;
    }
}
