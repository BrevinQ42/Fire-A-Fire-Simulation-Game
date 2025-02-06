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

	[SerializeField] private string type;
	public bool isPinPulled;
	private bool isBeingSqueezed;

	[Header("FloatingText")]
	public bool lookedAt;
    public TextMesh textName;

	void Start()
	{
		isHeld = false;
		fireFightingValue = 0.1f;

		foam = GetComponentInChildren<Foam>();
		foam.fireFightingValue = fireFightingValue;

		notificationSystem = GameObject.Find("MainPanel").GetComponent<NotificationTriggerEvent>();

		isPinPulled = false;
		isBeingSqueezed = false;

		lookedAt = false;
        textName = GetComponentInChildren<TextMesh>();
        string text = "[E] to Pick Up ";
        text += type;
        text += " Fire Extinguisher";
        textName.text = text;
	}

	void Update()
	{
		if (isHeld)
		{
			if (Input.GetMouseButtonDown(0) && !isPinPulled)
				isPinPulled = true;
			else if (Input.GetMouseButton(0) && isPinPulled)
			{
				foam.transform.localScale = Vector3.one * foamScale;

				if (!isBeingSqueezed)
				{
					notificationSystem.notificationMessage = "AIM at the fire, & Hold [Left Click] to SQUEEZE the handle.\n SWEEP the nozzle side to side";
					notificationSystem.disableAfterTimer = true;
					notificationSystem.disableTimer = 8.0f;
					notificationSystem.displayNotification();
				}

				isBeingSqueezed = true;
			}
			else
			{
				isBeingSqueezed = false;
				foam.transform.localScale = Vector3.zero;
			}
		}
		else foam.transform.localScale = Vector3.zero;

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
	}

	public void SetType(string newType)
	{
		type = newType;

		if (!foam) foam = GetComponentInChildren<Foam>();
		foam.type = newType;
	}
}