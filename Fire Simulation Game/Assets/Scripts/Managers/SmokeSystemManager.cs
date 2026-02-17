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

    //JoshHalfHouse2
    public GameObject joshHalfHouse2SmokeLayer1;
    public GameObject joshHalfHouse2SmokeLayer2;

    //JoshHalfHouse3
    public GameObject joshHalfHouse3SmokeLayer1;
    public GameObject joshHalfHouse3SmokeLayer2;

    //HalfHouse1
    public GameObject halfHouse1SmokeLayer1;
    public GameObject halfHouse1SmokeLayer2;
    public GameObject halfHouse1SmokeLayer3;
    public GameObject halfHouse1SmokeLayer4;
    public GameObject halfHouse1SmokeLayer5;
    public GameObject halfHouse1SmokeLayer6;

    //SmallHouse1
    public GameObject smallHouse1SmokeLayer1;
    public GameObject smallHouse1SmokeLayer2;

    //JoshSmallHouse1
    public GameObject joshSmallHouse1SmokeLayer1;
    public GameObject joshSmallHouse1SmokeLayer2;

    //JoshMainHouse
    public GameObject joshMainHouseSmokeLayer1;
    public GameObject joshMainHouseSmokeLayer2;

    //MainHouse1
    public GameObject mainHouse1SmokeLayer1;
    public GameObject mainHouse1SmokeLayer2;

    //SmallHouse2
    public GameObject smallHouse2SmokeLayer1;
    public GameObject smallHouse2SmokeLayer2;

    //SmallHouse3
    public GameObject smallHouse3SmokeLayer1;
    public GameObject smallHouse3SmokeLayer2;
    public GameObject smallHouse3SmokeLayer3;
    public GameObject smallHouse3SmokeLayer4;
    public GameObject smallHouse3SmokeLayer5;
    public GameObject smallHouse3SmokeLayer6;

    //JoshSmallHouse2
    public GameObject joshSmallHouse2SmokeLayer1;
    public GameObject joshSmallHouse2SmokeLayer2;

    //HalfHouse2
    public GameObject halfHouse2SmokeLayer1;
    public GameObject halfHouse2SmokeLayer2;

    private float elapsedTime = 0f;    

    // Fire Started
    public bool isFireAssigned;
    public string houseName;

    // SmokeLayer Message Displayed
    public bool isLayer1MessageDisplayed;
    public bool isLayer2MessageDisplayed;
    public bool isLayer3MessageDisplayed;
    public bool isLayer4MessageDisplayed;
    public bool isLayer5MessageDisplayed;
    public bool isLayer6MessageDisplayed;

    void Start()
    {
        isFireAssigned = false;
        isLayer1MessageDisplayed = false;
        isLayer2MessageDisplayed = false;
        isLayer3MessageDisplayed = false;
        isLayer4MessageDisplayed = false;
        isLayer5MessageDisplayed = false;
        isLayer6MessageDisplayed = false;

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

        //JoshHalfHouse2
        joshHalfHouse2SmokeLayer1.SetActive(false);
        joshHalfHouse2SmokeLayer2.SetActive(false);

        //JoshHalfHouse3
        joshHalfHouse3SmokeLayer1.SetActive(false);
        joshHalfHouse3SmokeLayer2.SetActive(false);

        //HalfHouse1
        halfHouse1SmokeLayer1.SetActive(false);
        halfHouse1SmokeLayer2.SetActive(false);
        halfHouse1SmokeLayer3.SetActive(false);
        halfHouse1SmokeLayer4.SetActive(false);
        halfHouse1SmokeLayer5.SetActive(false);
        halfHouse1SmokeLayer6.SetActive(false);

        //SmallHouse1
        smallHouse1SmokeLayer1.SetActive(false);
        smallHouse1SmokeLayer2.SetActive(false);

        //JoshSmallHouse1
        joshSmallHouse1SmokeLayer1.SetActive(false);
        joshSmallHouse1SmokeLayer2.SetActive(false);

        //JoshMainHouse
        joshMainHouseSmokeLayer1.SetActive(false);
        joshMainHouseSmokeLayer2.SetActive(false);

        //MainHouse1
        mainHouse1SmokeLayer1.SetActive(false);
        mainHouse1SmokeLayer2.SetActive(false);

        //SmallHouse2
        smallHouse2SmokeLayer1.SetActive(false);
        smallHouse2SmokeLayer2.SetActive(false);

        //SmallHouse3
        smallHouse3SmokeLayer1.SetActive(false);
        smallHouse3SmokeLayer2.SetActive(false);
        smallHouse3SmokeLayer3.SetActive(false);
        smallHouse3SmokeLayer4.SetActive(false);
        smallHouse3SmokeLayer5.SetActive(false);
        smallHouse3SmokeLayer6.SetActive(false);

        //JoshSmallHouse2
        joshSmallHouse2SmokeLayer1.SetActive(false);
        joshSmallHouse2SmokeLayer2.SetActive(false);

        //HalfHouse2
        halfHouse2SmokeLayer1.SetActive(false);
        halfHouse2SmokeLayer2.SetActive(false);
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

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    mainHouseSmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer3)
                {
                    DeleteSmokeSpheres();
                    mainHouseSmokeLayer3.SetActive(true);

                    if (isLayer3MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer3MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer4)
                {
                    DeleteSmokeSpheres();
                    mainHouseSmokeLayer4.SetActive(true);

                    if (isLayer4MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer4MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer5)
                {
                    DeleteSmokeSpheres();
                    mainHouseSmokeLayer5.SetActive(true);

                    if (isLayer5MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer5MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer6)
                {
                    DeleteSmokeSpheres();
                    mainHouseSmokeLayer6.SetActive(true);

                    if (isLayer6MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer6MessageDisplayed = true;
                    }
                }
            }

            if (houseName == "JoshHalfHouse1")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    joshHalfHouse1SmokeLayer1.SetActive(true);

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    joshHalfHouse1SmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
                }
            }

            if (houseName == "JoshHalfHouse2")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    joshHalfHouse2SmokeLayer1.SetActive(true);

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    joshHalfHouse2SmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
                }
            }

            if (houseName == "JoshHalfHouse3")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    joshHalfHouse3SmokeLayer1.SetActive(true);

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    joshHalfHouse3SmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
                }
            }

            if (houseName == "HalfHouse1")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    halfHouse1SmokeLayer1.SetActive(true);

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    halfHouse1SmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer3)
                {
                    DeleteSmokeSpheres();
                    halfHouse1SmokeLayer3.SetActive(true);

                    if (isLayer3MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer3MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer4)
                {
                    DeleteSmokeSpheres();
                    halfHouse1SmokeLayer4.SetActive(true);

                    if (isLayer4MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer4MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer5)
                {
                    DeleteSmokeSpheres();
                    halfHouse1SmokeLayer5.SetActive(true);

                    if (isLayer5MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer5MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer6)
                {
                    DeleteSmokeSpheres();
                    halfHouse1SmokeLayer6.SetActive(true);

                    if (isLayer6MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer6MessageDisplayed = true;
                    }
                }
            }

            if (houseName == "SmallHouse1")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    smallHouse1SmokeLayer1.SetActive(true);

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    smallHouse1SmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
                }
            }

            if (houseName == "JoshSmallHouse1")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    joshSmallHouse1SmokeLayer1.SetActive(true);

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    joshSmallHouse1SmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
                }
            }

            if (houseName == "JoshMainHouse")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    joshMainHouseSmokeLayer1.SetActive(true);

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    joshMainHouseSmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
                }
            }

            if (houseName == "MainHouse1")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    mainHouse1SmokeLayer1.SetActive(true);

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    mainHouse1SmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
                }
            }

            if (houseName == "SmallHouse2")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    smallHouse2SmokeLayer1.SetActive(true);

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    smallHouse2SmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
                }
            }

            if (houseName == "SmallHouse3")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    smallHouse3SmokeLayer1.SetActive(true);

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    smallHouse3SmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer3)
                {
                    DeleteSmokeSpheres();
                    smallHouse3SmokeLayer3.SetActive(true);

                    if (isLayer3MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer3MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer4)
                {
                    DeleteSmokeSpheres();
                    smallHouse3SmokeLayer4.SetActive(true);

                    if (isLayer4MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer4MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer5)
                {
                    DeleteSmokeSpheres();
                    smallHouse3SmokeLayer5.SetActive(true);

                    if (isLayer5MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer5MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer6)
                {
                    DeleteSmokeSpheres();
                    smallHouse3SmokeLayer6.SetActive(true);

                    if (isLayer6MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer6MessageDisplayed = true;
                    }
                }
            }

            if (houseName == "JoshSmallHouse2")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    joshSmallHouse2SmokeLayer1.SetActive(true);

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    joshSmallHouse2SmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
                }
            }

            if (houseName == "HalfHouse2")
            {
                if (elapsedTime >= timeToActivateLayer1)
                {
                    DeleteSmokeSpheres();
                    halfHouse2SmokeLayer1.SetActive(true);

                    if (isLayer1MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer1MessageDisplayed = true;
                    }
                }

                if (elapsedTime >= timeToActivateLayer2)
                {
                    DeleteSmokeSpheres();
                    halfHouse2SmokeLayer2.SetActive(true);

                    if (isLayer2MessageDisplayed == false)
                    {
                        notificationSystem.notificationMessage = "The smoke orbs have filled up and become a full layer, \n don't go near it or let the smoke orbs get to you!\n[C] to crawl / stand back up";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                        isLayer2MessageDisplayed = true;
                    }
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
