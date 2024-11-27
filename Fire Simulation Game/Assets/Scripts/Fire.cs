using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField] private Spawner SmokeSpawner;

    public float intensityValue;
    private Vector3 maxScale;
    private bool isGrowing;
    [SerializeField] private float growingSpeed;
    private float maxGrowingSpeed;

    public string type;

    // Start is called before the first frame update
    void Start()
    {
        intensityValue = 0.0f;
        growingSpeed = 0.05f;
        maxGrowingSpeed = 0.25f;

        if (type == "") type = "Class A";

        maxScale = new Vector3(35.0f, 1.1f, 35.0f);
        isGrowing = false;
        Toggle(true);       // to be removed

        transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrowing)
        {
            AffectFire(growingSpeed * Time.deltaTime);

            SmokeSpawner.transform.position = new Vector3(SmokeSpawner.transform.position.x, intensityValue + 1.5f,
                                                        SmokeSpawner.transform.position.z);
        }

        if (transform.localScale == maxScale) isGrowing = false;
        else isGrowing = true;
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

            if (maxScale.x > 0.0f) newScale.x = Math.Min(newScale.x, maxScale.x);
            if (maxScale.y > 0.0f) newScale.y = Math.Min(newScale.y, maxScale.y);
            if (maxScale.z > 0.0f) newScale.z = Math.Min(newScale.z, maxScale.z);

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
        if (mustGrow) isGrowing = true;
        else isGrowing = false;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (intensityValue > 0.0f)
        {
            if (collider.name.Equals("Player"))
            {
                PlayerController player = collider.GetComponent<PlayerController>();
                Debug.Log("Player is burning!");

                if (!player.FireOnPlayer)
                {
                    player.FireOnPlayer = Instantiate(gameObject,
                                                        player.transform.position + player.transform.forward * 0.5f,
                                                        Quaternion.identity).GetComponent<Fire>();

                    player.FireOnPlayer.transform.SetParent(player.transform);
                    player.FireOnPlayer.intensityValue = intensityValue / 2.0f;
                    player.isOnFire = true;
                }
                else player.FireOnPlayer.AffectFire(intensityValue / 2.0f);
            }

            else
            {
                FireFightingObject obj = collider.GetComponent<FireFightingObject>();

                if (obj)
                {
                    Water water = obj.GetComponent<Water>();
                    if(water)
                    {
                        Destroy(collider.gameObject);

                        if (type.Equals("Electrical"))
                        {
                            AffectFire(obj.fireFightingValue);
                            maxGrowingSpeed = 0.5f;
                            growingSpeed = Math.Min(growingSpeed * 2, maxGrowingSpeed);
                        }
                        else
                        {
                            if (type.Equals("Grease"))
                            {
                                AffectFire(obj.fireFightingValue);
                                growingSpeed = Math.Min(growingSpeed + 0.0001f * obj.fireFightingValue, maxGrowingSpeed);
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
            }
        }
    }
}
