using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField] private Spawner SmokeSpawner;

    public float intensityValue;
    [SerializeField] private float growingSpeed;

    public string type;

    // Start is called before the first frame update
    void Start()
    {
        type = "Class A";
    }

    // Update is called once per frame
    void Update()
    {
        if (intensityValue < 100.0f) AffectFire(growingSpeed * Time.deltaTime);
        else intensityValue = 100.0f;

        transform.localScale = new Vector3(2.5f, 2.0f, 2.5f) * Math.Max(intensityValue, 0.0f) / 100.0f;
    }

    public void AffectFire (float amt)
    {
        if (amt > 0)
        {
            intensityValue = Math.Min(intensityValue + amt, 100.0f);
            
            if (intensityValue > 5.0f) SmokeSpawner.Toggle(true);
        }
        else
        {
            intensityValue = Math.Max(intensityValue + amt, 0.0f);

            if (intensityValue <= 0.01f)
            {
                SmokeSpawner.Toggle(false);
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (intensityValue > 0.0f)
        {
            if (collision.collider.name.Equals("Player"))
            {
                PlayerController player = collision.collider.GetComponent<PlayerController>();
                Debug.Log("Player is burning!");

                if (!player.FireOnPlayer)
                {
                    player.FireOnPlayer = Instantiate(gameObject,
                                                        player.transform.position + player.transform.forward * 0.5f,
                                                        Quaternion.identity).GetComponent<Fire>();

                    player.FireOnPlayer.transform.SetParent(player.transform);
                    player.FireOnPlayer.intensityValue = intensityValue / 2.0f;

                    player.FireOnPlayer.SmokeSpawner = Instantiate(SmokeSpawner,
                                                        player.transform.position + player.transform.forward * 0.5f + new Vector3(0.0f, 0.5f, 0.0f),
                                                        Quaternion.identity).GetComponent<Spawner>();

                    player.FireOnPlayer.SmokeSpawner.transform.SetParent(player.FireOnPlayer.transform);
                    player.FireOnPlayer.SmokeSpawner.Toggle(true);
                }
                else player.FireOnPlayer.AffectFire(intensityValue / 2.0f);
            }

            else
            {
                FireFightingObject obj = collision.collider.GetComponent<FireFightingObject>();

                if (obj)
                {
                    if (type.Equals("Electrical"))
                    {
                        Debug.Log("Explosion happens");
                        // proceed to game over screen
                    }
                    else
                    {
                        if (type.Equals("Grease"))
                            AffectFire(obj.fireFightingValue);
                        else if (type.Equals("Class A"))
                            AffectFire(-Math.Min(intensityValue, Math.Abs(obj.fireFightingValue-intensityValue)));
                    }

                    Water water = obj.GetComponent<Water>();
                    if(water) Destroy(collision.collider.gameObject);
                }
            }
        }
    }
}
