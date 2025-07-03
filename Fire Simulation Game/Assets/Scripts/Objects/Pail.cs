using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pail : FireFightingObject
{
    [SerializeField] private GameObject WaterObject;
    [SerializeField] private GameObject WaterInPail;

    private float fractionFilled;
    private float maxFireFightingValue;

    public float closeProximityValue;
    public Transform playerCamera;

    public bool lookedAt;
    public GameObject textName;

    //sound effect
    public AudioSource audioSource;
    public AudioClip waterThrowClip;

    // Start is called before the first frame update
    void Start()
    {
        maxFireFightingValue = fireFightingValue;

        if(fractionFilled != 1.0f) fractionFilled = 0.0f;
        UpdateWaterInPail();

        closeProximityValue = 0.0f;

        lookedAt = false;

        audioSource = GetComponent<AudioSource>();
        textName = GetComponentInChildren<TextMesh>().gameObject;
    }

    private void Update()
    {
        if (lookedAt == false)
        {
            textName.SetActive(false);
        }
        if (lookedAt == true)
        {
            textName.SetActive(true);
        }
        if (isHeld == true)
        {
            textName.SetActive(false);
        }

        if (transform.position.y > 5.0f)
            GetComponent<Node>().floorLevel = 2;
        else
            GetComponent<Node>().floorLevel = 1;
    }

    public override void Use(float throwForce, out bool isStillHeld)
    {
        if (fractionFilled > 0) StartCoroutine(ThrowWater(throwForce));

        isStillHeld = isHeld;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("WaterDroplet"))
        {
            if (fractionFilled < 1.0f) fractionFilled = Math.Min(fractionFilled + 0.05f, 1.0f);
            UpdateWaterInPail();

            Destroy(collision.collider.gameObject);
        }
    }

    void UpdateWaterInPail()
    {
        WaterInPail.transform.localScale = new Vector3(125.0f, 75.0f * fractionFilled, 125.0f);
    }

    IEnumerator ThrowWater(float throwForce)
    {
        transform.SetPositionAndRotation(
                    playerCamera.position + playerCamera.forward - playerCamera.up * 0.67f, 
                    transform.rotation);

        GameObject ThrownWater = Instantiate(WaterObject, playerCamera.position + playerCamera.forward, playerCamera.rotation);
        ThrownWater.transform.SetParent(transform);

        Water water = ThrownWater.GetComponent<Water>();
        water.fireFightingValue = maxFireFightingValue * fractionFilled;
        water.Deattach();
        ThrownWater.GetComponent<Rigidbody>().AddForce(playerCamera.forward * throwForce);
        audioSource.clip = waterThrowClip;
        audioSource.Play();

        fractionFilled = 0.0f;
        UpdateWaterInPail();

        yield return new WaitForSeconds(0.2f);

        transform.SetPositionAndRotation(
            transform.position + playerCamera.right * 0.7f - playerCamera.up * 0.15f,
            transform.rotation);
    }

    public bool hasWaterInside()
    {
        return fractionFilled > 0.0f;
    }

    public void setFractionFilled(float fraction)
    {
        fractionFilled = Math.Min(1.0f, fraction);
        UpdateWaterInPail();
    }
}
