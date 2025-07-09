using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    // Notification system reference
    public NotificationTriggerEvent notificationSystem;

    [SerializeField] private Spawner SmokeSpawner;

    [Header("Characteristics")]
    public float intensityValue;
    private bool isGrowing;
    [SerializeField] private float growingSpeed;
    private float maxGrowingSpeed;

    public string type;
    public bool isOnPan;

    public Dictionary<string, string> EffectivityTable;

    [Header("Sound Effects")]
    // Sound effect
    public AudioSource audioSource;
    public AudioClip fireClip;

    // Start is called before the first frame update
    void Start()
    {
        GameObject mainPanel = GameObject.Find("MainPanel");

        if (mainPanel)
            notificationSystem = mainPanel.GetComponent<NotificationTriggerEvent>();

        SmokeSpawner = GetComponentInChildren<Spawner>();
        SmokeSpawner.transform.SetLocalPositionAndRotation(new Vector3(0.0f, 1.5f, 0.0f), transform.rotation);

        intensityValue += 0.01f;

        transform.localScale = Vector3.one * intensityValue;

        growingSpeed = 0.05f;
        maxGrowingSpeed = 0.25f;

        isOnPan = false;

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = fireClip;
        audioSource.loop = true;
        audioSource.Play();

        PopulateEffectivityTable();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrowing)
        {
            AffectFire(growingSpeed * Time.deltaTime);

            SmokeSpawner.transform.SetLocalPositionAndRotation(new Vector3(0.0f,
                                                                0.0f,
                                                                0.0f),
                                                                transform.rotation);
        }
        if (intensityValue < 6)
        {
            audioSource.volume = intensityValue / 10;
        }
    }

    void PopulateEffectivityTable()
    {
        EffectivityTable = new Dictionary<string, string>();

        EffectivityTable["Class A"] = "Class A";
        EffectivityTable["Electrical"] = "Class C";
        EffectivityTable["Grease"] = "Class K";
    }

    public void AffectFire(float amt)
    {
        Vector3 newScale;

        if (amt > 0)
        {
            intensityValue += amt;

            if (intensityValue > 0.5f) SmokeSpawner.Toggle(true);

            if (transform.localScale == Vector3.zero) newScale = transform.localScale + Vector3.one * intensityValue;
            else newScale = transform.localScale * intensityValue / (intensityValue - amt);

            growingSpeed = Math.Min(growingSpeed + 0.00001f, maxGrowingSpeed);
        }
        else
        {
            intensityValue = Math.Max(intensityValue + amt, 0.0f);

            if (intensityValue <= 0.01f)
            {
                if (isOnPan)
                {
                    Transform fryingPan = GameObject.Find("FryingPan").transform;

                    if (fryingPan.childCount > 2)
                    {
                        GameObject innateFire = fryingPan.GetChild(2).gameObject;
                        
                        if (innateFire.name.Equals("Fire")) Destroy(innateFire);
                    }
                }

                SmokeSpawner.Toggle(false);
                Destroy(gameObject);
            }

            newScale = transform.localScale * intensityValue / (intensityValue - amt);
            if (newScale.x < 0.0f) newScale.x = 0.0f;
            if (newScale.y < 0.0f) newScale.y = 0.0f;
            if (newScale.z < 0.0f) newScale.z = 0.0f;
        }

        transform.localScale = newScale;
    }

    public void Toggle(bool mustGrow)
    {
        isGrowing = mustGrow;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.GetComponent<PlayerController>())
        {
            PlayerController player = collider.GetComponent<PlayerController>();

            notificationSystem.notificationMessage = "You are burning!\n[R] to roll side to side to put it out!";
            notificationSystem.disableAfterTimer = true;
            notificationSystem.disableTimer = 4.0f;
            notificationSystem.displayNotification();

            if (player.FireOnPlayer)
            {
                if (player.FireOnPlayer != this)
                    player.FireOnPlayer.AffectFire(intensityValue / 1.05f);
            }
            else
            {
                player.FireOnPlayer = Instantiate(gameObject,
                                                    player.transform.position + player.transform.forward * 0.5f,
                                                    Quaternion.identity).GetComponent<Fire>();

                player.FireOnPlayer.transform.SetParent(player.transform);
                player.FireOnPlayer.intensityValue = intensityValue / 1.05f;
                player.isOnFire = true;
            }
        }
        else if (collider.GetComponent<NPC>())
        {
            NPC npc = collider.GetComponent<NPC>();

            if (npc.FireOnNPC)
            {
                if (npc.FireOnNPC != this)
                    npc.FireOnNPC.AffectFire(intensityValue / 1.05f);
            }
            else
            {
                npc.FireOnNPC = Instantiate(gameObject,
                                        npc.transform.position,
                                        Quaternion.identity).GetComponent<Fire>();

                npc.FireOnNPC.intensityValue = intensityValue / 1.05f;

                npc.FireOnNPC.transform.SetParent(npc.transform);
            }
        }
        else
        {
            FireFightingObject obj = collider.GetComponent<FireFightingObject>();

            if (obj)
            {
                if (type.Equals("UNSTOPPABLE"))
                {
                    AffectFire(obj.fireFightingValue);
                    return;
                }

                Water water = obj.GetComponent<Water>();
                Foam foam = obj.GetComponent<Foam>();
                if (water)
                {
                    Destroy(collider.gameObject);

                    bool isEligibleForNotif = !water.CompareTag("WaterDroplet") && water.GetComponent<Rigidbody>().velocity != Vector3.zero;

                    if (type.Equals("Electrical"))
                    {
                        maxGrowingSpeed = 0.5f;
                        AffectFire(obj.fireFightingValue);

                        if (isEligibleForNotif)
                        {
                            notificationSystem.notificationMessage = "The fire grew! Water is ineffective because that is an electrical fire.\nThere might be something else more effective";
                            notificationSystem.disableAfterTimer = true;
                            notificationSystem.disableTimer = 8.0f;
                            notificationSystem.displayNotification();
                        }
                    }
                    else if (type.Equals("Grease"))
                    {
                        AffectFire(obj.fireFightingValue);

                        if (isEligibleForNotif)
                        {
                            notificationSystem.notificationMessage = "The fire grew! Water is ineffective because that is a grease fire.\nThere might be something else more effective";
                            notificationSystem.disableAfterTimer = true;
                            notificationSystem.disableTimer = 8.0f;
                            notificationSystem.displayNotification();
                        }
                    }
                    else if (type.Equals("Class A"))
                    {
                        AffectFire(-obj.fireFightingValue);

                        if (isEligibleForNotif)
                        {
                            notificationSystem.notificationMessage = "The fire got smaller!\nTake it out before it's too late!";
                            notificationSystem.disableAfterTimer = true;
                            notificationSystem.disableTimer = 5.0f;
                            notificationSystem.displayNotification();
                        }
                    }
                    else
                        AffectFire(obj.fireFightingValue);
                }
                else if (foam)
                {
                    if (EffectivityTable[type].Equals(foam.type))
                    {
                        AffectFire(-obj.fireFightingValue);
                    
                        notificationSystem.notificationMessage = "The fire got smaller!\nTake it out before it's too late!";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                    }
                    else
                    {
                        AffectFire(obj.fireFightingValue);
                        
                        string message = "The fire grew! That is because that is a ";
                        message += type;
                        message += " fire.\nCheck the type of extinguisher you are using.";
                        notificationSystem.notificationMessage = message;
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 8.0f;
                        notificationSystem.displayNotification();
                    }
                }

                else if (obj.GetComponent<NonFlammableObject>())
                {
                    NonFlammableObject fireResistant = obj.GetComponent<NonFlammableObject>();
                    if (fireResistant.isOnPan)
                    {
                        if (intensityValue > 0.25f)
                        {
                            notificationSystem.notificationMessage = "The fire got smaller, but is still burning!\nMaybe try something else.";
                            notificationSystem.disableAfterTimer = true;
                            notificationSystem.disableTimer = 7.0f;
                            notificationSystem.displayNotification();
                        }

                        AffectFire(-0.25f);
                    }
                    else if (obj.GetComponent<Rigidbody>().velocity != Vector3.zero)
                    {
                        AffectFire(-obj.fireFightingValue);

                        notificationSystem.notificationMessage = "The fire got smaller by a bit!\nMaybe try another way.";
                        notificationSystem.disableAfterTimer = true;
                        notificationSystem.disableTimer = 5.0f;
                        notificationSystem.displayNotification();
                    }
                }
            }

            else if (collider.CompareTag("Outlet") && !type.Equals("Electrical"))
                type = "Electrical";
        }
    }

    void OnTriggerStay(Collider collider)
    {
        Foam foam = collider.GetComponent<Foam>();
        if (foam)
        {
            Debug.Log("Foam collided with fire");

            if (EffectivityTable[type].Equals(foam.type))
                AffectFire(-foam.fireFightingValue * foam.affectFireMult);
            else
                AffectFire(foam.fireFightingValue * foam.affectFireMult);
        }
    }
}
