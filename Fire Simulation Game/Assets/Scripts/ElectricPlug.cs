using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricPlug : GrabbableObject
{
    private FireManager fireManager;

    public Transform owner;
    public Transform pluggedInto;

    public bool lookedAt;
    public GameObject textName;

    void Start()
    {
        lookedAt = false;
        textName = transform.Find("FloatingText").gameObject;

        if (transform.parent && !transform.parent.name.Equals("Main House")) pluggedInto = transform.parent;
        InitializeFireManager();
    }

    private void Update()
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
        }
    }

    void InitializeFireManager()
    {
        GameObject fm = GameObject.Find("FireManager");
        
        if (fm) fireManager = fm.GetComponent<FireManager>();
    }

    public override void Use(Transform target)
    {
        pluggedInto = target.parent;
        transform.SetParent(pluggedInto);

        if (target.localRotation.x == 1.0f) // extension cord
        {
            transform.localPosition = new Vector3(target.localPosition.x, 0.04f, -0.077f);
            transform.localEulerAngles = new Vector3(180.0f, -90.0f, -90.0f);
        }
        else // wall outlet
        {
            transform.localPosition = new Vector3(target.localPosition.x, -0.062f, 0.033f);
            transform.localEulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        }

        GetComponent<Collider>().enabled = true;
        isHeld = false;

        if(!fireManager) InitializeFireManager();

        if(fireManager)
        {
            fireManager.AddSpawnPoint(transform);

            if (owner.name.Equals("ExtensionCord") && pluggedInto.name.Equals("ExtensionCord"))
            {
                // additional chance of being set on fire
                fireManager.AddSpawnPoint(pluggedInto.GetComponentInChildren<ElectricPlug>().transform);
            }
        }
    }
}

