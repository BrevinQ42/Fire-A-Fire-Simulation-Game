using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeSystemManager : MonoBehaviour
{
    // Notification system reference
    public NotificationTriggerEvent notificationSystem;

    private int spawnersActive = 0; // updated in spawner script

    public GameObject smokeLayer1;      
    public float timeToActivateLayer1 = 20f;
    public GameObject smokeLayer2;
    public float timeToActivateLayer2 = 40f;
    public GameObject smokeLayer3;
    public float timeToActivateLayer3 = 60f;
    public GameObject smokeLayer4;
    public float timeToActivateLayer4 = 80f;
    public GameObject smokeLayer5;
    public float timeToActivateLayer5 = 100f;
    public GameObject smokeLayer6;
    public float timeToActivateLayer6 = 120f;

    private float elapsedTime = 0f;    
    private bool isSmokeLayer1Activated = false;
    private bool isSmokeLayer2Activated = false;
    private bool isSmokeLayer3Activated = false;
    private bool isSmokeLayer4Activated = false;
    private bool isSmokeLayer5Activated = false;
    private bool isSmokeLayer6Activated = false;

    // Fire Started
    public bool isFireAssigned;

    void Start()
    {
        smokeLayer1.SetActive(false);
        smokeLayer2.SetActive(false);
        smokeLayer3.SetActive(false);
        smokeLayer4.SetActive(false);
        smokeLayer5.SetActive(false);
        smokeLayer6.SetActive(false);
        isFireAssigned = false;
    }

    void Update()
    {
        if (spawnersActive > 0)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= timeToActivateLayer1 && isSmokeLayer1Activated == false)
            {
                DeleteSmokeSpheres();     
                ActivateSmokeLayer1();      
                isSmokeLayer1Activated = true;

                notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                notificationSystem.disableAfterTimer = true;
                notificationSystem.disableTimer = 5.0f;
                notificationSystem.displayNotification();
            }

            if (elapsedTime >= timeToActivateLayer2 && isSmokeLayer2Activated == false)
            {
                DeleteSmokeSpheres();
                ActivateSmokeLayer2();
                isSmokeLayer2Activated = true;

                notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't don't go near it or  let the smoke orbs get to you!\n[C] to crawl / stand back up";
                notificationSystem.disableAfterTimer = true;
                notificationSystem.disableTimer = 5.0f;
                notificationSystem.displayNotification();
            }

            if (elapsedTime >= timeToActivateLayer3 && isSmokeLayer3Activated == false)
            {
                DeleteSmokeSpheres();
                ActivateSmokeLayer3();
                isSmokeLayer3Activated = true;

                notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't don't go near it or  let the smoke orbs get to you!\n[C] to crawl / stand back up";
                notificationSystem.disableAfterTimer = true;
                notificationSystem.disableTimer = 5.0f;
                notificationSystem.displayNotification();
            }

            if (elapsedTime >= timeToActivateLayer4 && isSmokeLayer4Activated == false)
            {
                DeleteSmokeSpheres();
                ActivateSmokeLayer4();
                isSmokeLayer4Activated = true;

                notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't don't go near it or  let the smoke orbs get to you!\n[C] to crawl / stand back up";
                notificationSystem.disableAfterTimer = true;
                notificationSystem.disableTimer = 5.0f;
                notificationSystem.displayNotification();
            }

            if (elapsedTime >= timeToActivateLayer5 && isSmokeLayer5Activated == false)
            {
                DeleteSmokeSpheres();
                ActivateSmokeLayer5();
                isSmokeLayer5Activated = true;

                notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't don't go near it or  let the smoke orbs get to you!\n[C] to crawl / stand back up";
                notificationSystem.disableAfterTimer = true;
                notificationSystem.disableTimer = 5.0f;
                notificationSystem.displayNotification();
            }

            if (elapsedTime >= timeToActivateLayer6 && isSmokeLayer6Activated == false)
            {
                DeleteSmokeSpheres();
                ActivateSmokeLayer6();
                isSmokeLayer6Activated = true;

                notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't don't go near it or  let the smoke orbs get to you!\n[C] to crawl / stand back up";
                notificationSystem.disableAfterTimer = true;
                notificationSystem.disableTimer = 5.0f;
                notificationSystem.displayNotification();
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

    void ActivateSmokeLayer3()
    {

        smokeLayer3.SetActive(true);
        Debug.Log("Smoke layer3 activated.");
    }

    void ActivateSmokeLayer4()
    {

        smokeLayer4.SetActive(true);
        Debug.Log("Smoke layer4 activated.");
    }

    void ActivateSmokeLayer5()
    {

        smokeLayer5.SetActive(true);
        Debug.Log("Smoke layer5 activated.");
    }

    void ActivateSmokeLayer6()
    {

        smokeLayer6.SetActive(true);
        Debug.Log("Smoke layer6 activated.");
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
