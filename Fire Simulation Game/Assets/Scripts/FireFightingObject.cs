using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFightingObject : MonoBehaviour
{
    public bool isHeld;

    public virtual void Use(float throwForce, out bool isStillHeld)
    {
        // filler code (for debugging purposes)
        Debug.Log(gameObject.name + " is used!");
        isStillHeld = isHeld;
    }

    public void Deattach()
    {
        GetComponent<Collider>().enabled = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        transform.SetParent(null);
        isHeld = false;
    }
}
