using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pail : FireFightingObject
{
    [SerializeField] private GameObject Water;
    [SerializeField] private GameObject WaterInPail;

    [SerializeField] private bool isFilled;
    public float closeProximityValue;
    public Transform playerCamera;
    private Transform hitTransform;

    // Start is called before the first frame update
    void Start()
    {
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
                if (hit.transform.gameObject.name.Equals("WaterSource"))
                {   
                    Debug.DrawRay(playerCamera.position, playerCamera.forward * hit.distance, Color.yellow);
                    Debug.Log(hit.transform.gameObject.name + " found! : " + hit.distance);

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

            WaterInPail.GetComponent<Collider>().enabled = true;
            WaterInPail.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            WaterInPail.transform.SetParent(null);

            WaterInPail.GetComponent<Rigidbody>().AddForce(transform.forward * throwForce);
            WaterInPail = null;
        }
                 // will check tag
        else if (hitTransform.gameObject.name.Equals("WaterSource") && WaterInPail == null)
        {
            isFilled = true;

            WaterInPail = Instantiate(Water, transform.position, transform.rotation);
            WaterInPail.transform.SetParent(transform);

            Rigidbody waterRB = WaterInPail.GetComponent<Rigidbody>();
            waterRB.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

            WaterInPail.GetComponent<Collider>().enabled = false;
        }

        isStillHeld = isHeld;
    }
}
