using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricPlug : GrabbableObject
{
    public string owner;
    public string pluggedInto;

    void Start()
    {
        if (owner == "") owner = "Appliance";
    }

    public override void Use(Transform target)
    {
        transform.SetParent(target.parent);
        pluggedInto = target.parent.name;

        if (target.localRotation.x == 1.0f) // 180 degree rotation on x-axis
        {
            transform.localPosition = new Vector3(target.localPosition.x, 0.04f, -0.077f);
            transform.localEulerAngles = new Vector3(180.0f, -90.0f, -90.0f);
        }
        else
        {
            transform.localPosition = new Vector3(target.localPosition.x, -0.062f, 0.033f);
            transform.localEulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        }

        GetComponent<Collider>().enabled = true;
        isHeld = false;
    }
}

