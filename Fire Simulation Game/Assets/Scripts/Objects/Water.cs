using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : FireFightingObject
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Fire") && !collision.collider.GetComponent<Pail>()
            && !collision.collider.CompareTag("Smoke") && !collision.collider.CompareTag("SmokeLayer"))
        {
            Destroy(gameObject);
            Debug.Log("Water hit " + collision.collider.name);
        }
    }
}
