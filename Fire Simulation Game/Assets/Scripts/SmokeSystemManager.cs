using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeSystemManager : MonoBehaviour
{
    private int spawnersActive = 0; // updated in spawner script

    public GameObject smokeLayer1;      
    public float timeToActivateLayer1 = 180f;
    public GameObject smokeLayer2;
    public float timeToActivateLayer2 = 360f;

    private float elapsedTime = 0f;    
    private bool isSmokeLayer1Activated = false;
    private bool isSmokeLayer2Activated = false;

    void Start()
    {
        smokeLayer1.SetActive(false);
        smokeLayer2.SetActive(false);
    }

    void Update()
    {
        if (spawnersActive > 0)
        {
            elapsedTime += Time.deltaTime;

            // After 3 minutes, activate layer 1
            if (elapsedTime >= timeToActivateLayer1 && isSmokeLayer1Activated == false)
            {
                DeleteSmokeSpheres();     
                ActivateSmokeLayer1();      
                isSmokeLayer1Activated = true; 
            }

            // After 6 minutes, activate layer 1
            if (elapsedTime >= timeToActivateLayer2 && isSmokeLayer2Activated == false)
            {
                DeleteSmokeSpheres();
                ActivateSmokeLayer2();
                isSmokeLayer2Activated = true;
            }
        }
    }


    void DeleteSmokeSpheres()
    {
        GameObject[] smokeSpheres = GameObject.FindGameObjectsWithTag("Smoke");

        foreach (GameObject sphere in smokeSpheres)
        {
            if (sphere != null)
            {
                Destroy(sphere);  
            }
        }

        Debug.Log("All smoke spheres deleted.");
    }


    void ActivateSmokeLayer1()
    {

        smokeLayer1.SetActive(true);
        Debug.Log("Smoke layer1 activated.");

    }

    void ActivateSmokeLayer2()
    {

        smokeLayer2.SetActive(true);
        Debug.Log("Smoke layer2 activated.");
    }

    public void IncrementCounter()
    {
        spawnersActive++;
    }

    public void DecrementCounter()
    {
        spawnersActive--;
    }
}
