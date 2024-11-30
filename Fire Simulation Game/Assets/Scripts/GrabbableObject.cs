using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    public bool isHeld;
    
    public virtual void Use(Transform target)
    {
        // filler code (for debugging purposes)
        Debug.Log(gameObject.name + " is used on " + target.name);
    }

    public void Deattach()
    {
        GetComponent<Collider>().enabled = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        transform.SetParent(null);
        isHeld = false;
    }
}
