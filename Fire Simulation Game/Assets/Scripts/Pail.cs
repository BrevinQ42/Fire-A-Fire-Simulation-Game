using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pail : FireFightingObject
{
    [SerializeField] private GameObject WaterObject;
    [SerializeField] private GameObject WaterInPail;

    [SerializeField] private bool isFilled;
    public float closeProximityValue;
    public Transform playerCamera;
    private Transform hitTransform;
    [SerializeField] private Vector3 throwHeightOffset;

    // Start is called before the first frame update
    void Start()
    {
        WaterObject.GetComponent<Water>().fireFightingValue = fireFightingValue;

        isFilled = false;
        closeProximityValue = 0.0f;
    }

    void FixedUpdate()
    {
        if(!isFilled && isHeld)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, closeProximityValue))
            {
                if (hit.transform.name.Equals("WaterSource"))
                {   
                    Debug.DrawRay(playerCamera.position, playerCamera.forward * hit.distance, Color.yellow);
                    Debug.Log(hit.transform.name + " found! : " + hit.distance);

                    hitTransform = hit.transform;
                }
            }
            else
            {
                Debug.DrawRay(playerCamera.position, playerCamera.forward * 2.0f, Color.white);
                Debug.Log("Did not Hit");

                hitTransform = null;
            }
        }
    }

    public override void Use(float throwForce, out bool isStillHeld)
    {
        if (isFilled)
        {
            isFilled = false;

            WaterInPail.GetComponent<Water>().Deattach();
            WaterInPail.GetComponent<Rigidbody>().AddForce(transform.forward * throwForce + throwHeightOffset);
            WaterInPail = null;
        }
                                        // will check tag
        else if (hitTransform && hitTransform.name.Equals("WaterSource") && !WaterInPail)
        {
            isFilled = true;

            WaterInPail = Instantiate(WaterObject, transform.position, transform.rotation);
            WaterInPail.transform.SetParent(transform);

            Rigidbody waterRB = WaterInPail.GetComponent<Rigidbody>();
            waterRB.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

            WaterInPail.GetComponent<Collider>().enabled = false;
        }

        isStillHeld = isHeld;
    }
}
