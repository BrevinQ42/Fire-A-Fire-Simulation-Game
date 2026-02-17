using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    private Fire activeFire;
    public SmokeSystemManager smokeSystemManager;

    void Update()
    {
        //if (activeFire != null)
        //{
            //Debug.Log("Fire is in this house: " + gameObject.name);
        //}

    }

    public void RegisterFire(Fire fire)
    {
        if (smokeSystemManager.isFireAssigned == false)
        {
            activeFire = fire;
            smokeSystemManager.isFireAssigned = true;
            smokeSystemManager.houseName = gameObject.name;
        }
        else
        {
            Debug.Log("Fire already assigned." );
        }
    }

    void IncreaseSmoke()
    {

    }

}
