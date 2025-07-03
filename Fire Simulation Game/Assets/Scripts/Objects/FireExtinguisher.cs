using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisher : FireFightingObject
{
	private Foam foam;
	[SerializeField] private float foamScale;

	[Header("Notification")]
	[SerializeField] private NotificationTriggerEvent notificationSystem;

	public string type;
	public bool isPinPulled;
	private bool areNextInstructionsSent;

	[Header("FloatingText")]
	public bool lookedAt;
    public TextMesh textName;

    public AudioSource audioSource;
    public AudioClip pullingClip;
    public AudioClip sweepingClip;

    void Start()
	{
		isHeld = false;
		fireFightingValue = 0.1f;

		foam = GetComponentInChildren<Foam>();
		foam.fireFightingValue = fireFightingValue;
		foam.GetComponent<Collider>().enabled = false;

		notificationSystem = GameObject.Find("MainPanel").GetComponent<NotificationTriggerEvent>();

		isPinPulled = false;
		areNextInstructionsSent = false;

		lookedAt = false;
        textName = GetComponentInChildren<TextMesh>();
        string text = "[E] to Pick Up\n";
        text += type;
        text += " Fire Ext.";
        textName.text = text;

        audioSource = GetComponent<AudioSource>();
    }

	void Update()
	{
		if (isHeld)
		{
			if (Input.GetMouseButtonDown(0) && !isPinPulled)
			{
                isPinPulled = true;
                audioSource.clip = pullingClip;
                audioSource.Play();
            }
            else if (Input.GetMouseButton(0) && isPinPulled)
			{
				foam.transform.localScale = Vector3.one * foamScale;
                foam.GetComponent<Collider>().enabled = true;

                if (audioSource.clip != sweepingClip || !audioSource.isPlaying)
                {
                    audioSource.clip = sweepingClip;
                    audioSource.Play();
                }
            }
			else
			{
				if (isPinPulled && !areNextInstructionsSent)
				{
					areNextInstructionsSent = true;
					
					notificationSystem.notificationMessage = "AIM at the fire, & Hold [Left Click] to SQUEEZE the handle.\n SWEEP the nozzle side to side";
					notificationSystem.disableAfterTimer = true;
					notificationSystem.disableTimer = 8.0f;
					notificationSystem.displayNotification();
				}

                foam.GetComponent<Collider>().enabled = false;
				foam.transform.localScale = Vector3.zero;

                if (audioSource.clip == sweepingClip || audioSource.isPlaying)
                {
					audioSource.Stop();
                }
            }
		}
		else
		{
			foam.transform.localScale = Vector3.zero;
			areNextInstructionsSent = false;
		}

		if (lookedAt == false)
        {
            textName.gameObject.SetActive(false);
        }
        if (lookedAt == true)
        {
            textName.gameObject.SetActive(true);
        }
        if (isHeld == true)
        {
            textName.gameObject.SetActive(false);
        }

        if (transform.position.y > 5.0f)
        	GetComponent<Node>().floorLevel = 2;
        else
        	GetComponent<Node>().floorLevel = 1;
	}

	public void SetType(string newType)
	{
		type = newType;

		if (!foam) foam = GetComponentInChildren<Foam>();
		foam.type = newType;
    }
}