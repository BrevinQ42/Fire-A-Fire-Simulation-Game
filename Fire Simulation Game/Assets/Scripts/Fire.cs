using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    // Notification system reference
    public NotificationTriggerEvent notificationSystem;

    [SerializeField] private Spawner SmokeSpawner;

    public float intensityValue;
    private bool isGrowing;
    [SerializeField] private float growingSpeed;
    private float maxGrowingSpeed;

    public string type;

    // Start is called before the first frame update
    void Start()
    {
        notificationSystem = FindAnyObjectByType<NotificationTriggerEvent>();

        SmokeSpawner = GetComponentInChildren<Spawner>();
        SmokeSpawner.transform.SetLocalPositionAndRotation(new Vector3(0.0f, 1.5f, 0.0f), transform.rotation);

        intensityValue = 0.0f;
        growingSpeed = 0.05f;
        maxGrowingSpeed = 0.25f;

        transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrowing)
        {
            AffectFire(growingSpeed * Time.deltaTime);

            SmokeSpawner.transform.position = new Vector3(SmokeSpawner.transform.position.x,
                                                        Math.Min(intensityValue + 1.5f, 2.5f),
                                                        SmokeSpawner.transform.position.z);
        }
    }

    public void AffectFire(float amt)
    {
        Vector3 newScale;

        if (amt > 0)
        {
            intensityValue += amt;

            if (intensityValue > 0.5f) SmokeSpawner.Toggle(true);

            if (transform.localScale == Vector3.zero) newScale = transform.localScale + Vector3.one * amt;
            else newScale = transform.localScale * intensityValue / (intensityValue - amt);

            growingSpeed = Math.Min(growingSpeed + 0.00001f, maxGrowingSpeed);
        }
        else
        {
            intensityValue = Math.Max(intensityValue + amt, 0.0f);

            if (intensityValue <= 0.01f)
            {
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
        if (intensityValue > 0.0f)
        {
            if (collider.name.Equals("Player"))
            {
                PlayerController player = collider.GetComponent<PlayerController>();

                notificationSystem.notificationMessage = "You are burning!\n[R] to roll side to side to put it out!";
                notificationSystem.disableAfterTimer = true;
                notificationSystem.disableTimer = 4.0f;
                notificationSystem.displayNotification();

                if (!player.FireOnPlayer)
                {
                    player.FireOnPlayer = Instantiate(gameObject,
                                                        player.transform.position + player.transform.forward * 0.5f,
                                                        Quaternion.identity).GetComponent<Fire>();

                    player.FireOnPlayer.transform.SetParent(player.transform);
                    player.FireOnPlayer.intensityValue = intensityValue / 1.2f;
                    player.isOnFire = true;
                }
                else player.FireOnPlayer.AffectFire(intensityValue / 1.2f);
            }

            else
            {
                FireFightingObject obj = collider.GetComponent<FireFightingObject>();

                if (obj)
                {
                    Water water = obj.GetComponent<Water>();
                    if (water)
                    {
                        Destroy(collider.gameObject);

                        bool isEligibleForNotif = !water.CompareTag("WaterDroplet") && water.GetComponent<Rigidbody>().velocity != Vector3.zero;

                        if (type.Equals("Electrical"))
                        {
                            AffectFire(obj.fireFightingValue);
                            maxGrowingSpeed = 0.5f;
                            growingSpeed = Math.Min(growingSpeed * 2, maxGrowingSpeed);

                            if (isEligibleForNotif)
                            {
                                notificationSystem.notificationMessage = "The fire grew! Water is ineffective because that is an electrical fire.\nThere might be something else more effective";
                                notificationSystem.disableAfterTimer = true;
                                notificationSystem.disableTimer = 8.0f;
                                notificationSystem.displayNotification();
                            }
                        }
                        else
                        {
                            if (type.Equals("Grease"))
                            {
                                AffectFire(obj.fireFightingValue);
                                growingSpeed = Math.Min(growingSpeed + 0.0001f * obj.fireFightingValue, maxGrowingSpeed);

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
                                growingSpeed = Math.Max(growingSpeed - 0.1f * obj.fireFightingValue, 0.05f);
                            }
                        }
                    }
                    else if (obj.GetComponent<Rigidbody>().velocity != Vector3.zero)
                        AffectFire(-obj.fireFightingValue);
                }

                else if (collider.CompareTag("Outlet") && !type.Equals("Electrical"))
                    type = "Electrical";
            }
        }
    }
}
