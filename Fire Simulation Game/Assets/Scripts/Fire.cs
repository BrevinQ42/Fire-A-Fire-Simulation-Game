using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public float intensityValue;
    private float growingSpeed;

    // Start is called before the first frame update
    void Start()
    {
        intensityValue = 1.0f;
        growingSpeed = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if(intensityValue == 0.0f) gameObject.SetActive(false);
        else if (intensityValue < 100.0f) AffectFire(growingSpeed * Time.deltaTime);
        else intensityValue = 100.0f;

        transform.localScale = new Vector3(2.5f, 2.0f, 2.5f) * (intensityValue / 100.0f);
    }

    public void AffectFire (float amt)
    {
        if (amt > 0) intensityValue = Math.Min(intensityValue + amt, 100.0f);
        else intensityValue = Math.Max(intensityValue + amt, 0.0f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name.Equals("Player"))
        {
            Debug.Log("Player is burning!");
        }

        else
        {
            FireFightingObject obj = collision.collider.GetComponent<FireFightingObject>();

            if (obj)
            {
                if (obj.fireFightingValue >= intensityValue) AffectFire(-Math.Min(intensityValue, obj.fireFightingValue-intensityValue));
                else AffectFire(Math.Min(obj.fireFightingValue, intensityValue-obj.fireFightingValue));

                Water water = obj.GetComponent<Water>();
                if(water) Destroy(collision.collider.gameObject);
                // else burn fire resistant material ?
            }
        }
    }
}
