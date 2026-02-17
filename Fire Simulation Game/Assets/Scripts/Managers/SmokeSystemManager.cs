using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeSystemManager : MonoBehaviour
{
    // Notification system reference
    public NotificationTriggerEvent notificationSystem;

    private int spawnersActive = 0; // updated in spawner script
    public float timeToActivateLayer1 = 20f;
    public float timeToActivateLayer2 = 40f;
    public float timeToActivateLayer3 = 60f;
    public float timeToActivateLayer4 = 80f;
    public float timeToActivateLayer5 = 100f;
    public float timeToActivateLayer6 = 120f;

    //Main House
    public GameObject mainHouseSmokeLayer1;      
    public GameObject mainHouseSmokeLayer2;
    public GameObject mainHouseSmokeLayer3;
    public GameObject mainHouseSmokeLayer4;
    public GameObject mainHouseSmokeLayer5;
    public GameObject mainHouseSmokeLayer6;

    //JoshHalfHouse1
    public GameObject joshHalfHouse1SmokeLayer1;
    public GameObject joshHalfHouse1SmokeLayer2;

    private float elapsedTime = 0f;    

    // Fire Started
    public bool isFireAssigned;
    public string houseName;

    void Start()
    {
        isFireAssigned = false;
        //Main House
        mainHouseSmokeLayer1.SetActive(false);
        mainHouseSmokeLayer2.SetActive(false);
        mainHouseSmokeLayer3.SetActive(false);
        mainHouseSmokeLayer4.SetActive(false);
        mainHouseSmokeLayer5.SetActive(false);
        mainHouseSmokeLayer6.SetActive(false);

        //JoshHalfHouse1
        joshHalfHouse1SmokeLayer1.SetActive(false);
        joshHalfHouse1SmokeLayer2.SetActive(false);
    }

    void Update()
    {
        if (spawnersActive > 0)
        {
            elapsedTime += Time.deltaTime;

            if (houseName == "Main House")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    mainHouseSmokeLayer1.SetActive(true);

                    notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 5.0f;
                    notificationSystem.displayNotification();
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    mainHouseSmokeLayer2.SetActive(true);

                    notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't don't go near it or  let the smoke orbs get to you!\n[C] to crawl / stand back up";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 5.0f;
                    notificationSystem.displayNotification();
                }

                if (elapsedTime >= timeToActivateLayer3)
                {
                    DeleteSmokeSpheres();
                    mainHouseSmokeLayer3.SetActive(true);

                    notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't don't go near it or  let the smoke orbs get to you!\n[C] to crawl / stand back up";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 5.0f;
                    notificationSystem.displayNotification();
                }

                if (elapsedTime >= timeToActivateLayer4)
                {
                    DeleteSmokeSpheres();
                    mainHouseSmokeLayer4.SetActive(true);

                    notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't don't go near it or  let the smoke orbs get to you!\n[C] to crawl / stand back up";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 5.0f;
                    notificationSystem.displayNotification();
                }

                if (elapsedTime >= timeToActivateLayer5)
                {
                    DeleteSmokeSpheres();
                    mainHouseSmokeLayer5.SetActive(true);

                    notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't don't go near it or  let the smoke orbs get to you!\n[C] to crawl / stand back up";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 5.0f;
                    notificationSystem.displayNotification();
                }

                if (elapsedTime >= timeToActivateLayer6)
                {
                    DeleteSmokeSpheres();
                    mainHouseSmokeLayer6.SetActive(true);

                    notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't don't go near it or  let the smoke orbs get to you!\n[C] to crawl / stand back up";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 5.0f;
                    notificationSystem.displayNotification();
                }
            }

            if (houseName == "JoshHalfHouse1")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    joshHalfHouse1SmokeLayer1.SetActive(true);

                    notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 5.0f;
                    notificationSystem.displayNotification();
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    joshHalfHouse1SmokeLayer2.SetActive(true);

                    notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't don't go near it or  let the smoke orbs get to you!\n[C] to crawl / stand back up";
                    notificationSystem.disableAfterTimer = true;
                    notificationSystem.disableTimer = 5.0f;
                    notificationSystem.displayNotification();
                }
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

    public void IncrementCounter()
    {
        spawnersActive++;
    }

    public void DecrementCounter()
    {
        spawnersActive--;
    }
}
