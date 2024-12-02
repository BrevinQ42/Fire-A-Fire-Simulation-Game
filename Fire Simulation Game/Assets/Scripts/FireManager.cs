using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
	// Notification system reference
    public NotificationTriggerEvent notificationSystem;

	[SerializeField] private Fire firePrefab;
	[SerializeField] private List<Transform> FireSpawnPoints;

	private List<string> FireTypes;

	[SerializeField] private float timeBeforeFire;
	private bool isFireOngoing;
	private Fire ongoingFire;
	public bool isPlayerSuccessful;

	void Start()
	{
		FireTypes = new List<string>{"Electrical", "Grease", "Class A", "Class A"};

		isFireOngoing = false;
		ongoingFire = null;
		isPlayerSuccessful = false;
	}

	void Update()
	{
		if (timeBeforeFire > 0.0f) timeBeforeFire -= Time.deltaTime;
		else if (!isFireOngoing)
		{
			int index;
			Transform spawnTransform;

			while(true)
			{
				index = Random.Range(0, FireSpawnPoints.Count);
				spawnTransform = FireSpawnPoints[index];

				ElectricPlug plug = spawnTransform.GetComponent<ElectricPlug>();
				if (plug)
				{
					bool isPowered = false;

					while(true)
					{
						if (!plug.pluggedInto) break;

						if (plug.pluggedInto.name.Equals("WallOutlet"))
						{
							isPowered = true;
							break;
						}
						else
						{
							Transform plugOwner = plug.pluggedInto;

							foreach(ElectricPlug electricPlug in FindObjectsByType<ElectricPlug>(FindObjectsSortMode.None))
							{
								if (electricPlug.owner == plugOwner)
								{
									plug = electricPlug;
									break;
								}
							}
						}
					}

					if (!isPowered) RemoveSpawnPoint(spawnTransform);
					else break;
				}
				else break;
			}

			Vector3 spawnPoint = spawnTransform.position;

			ongoingFire = Instantiate(firePrefab, spawnPoint, Quaternion.identity).GetComponent<Fire>();

			if (spawnTransform.GetComponent<ElectricPlug>()) ongoingFire.type = "Electrical";
			else if (spawnTransform.name.Equals("Pan")) ongoingFire.type = "Grease";
			else
			{
				if (index >= FireSpawnPoints.Count - 3)
				{
					int typeIndex = Random.Range(0, FireTypes.Count);
					ongoingFire.type = FireTypes[typeIndex];
				}
				else ongoingFire.type = "Class A";
			}

			ongoingFire.Toggle(true);
			isFireOngoing = true;
		}
		else if (ongoingFire == null && !isPlayerSuccessful)
		{
			notificationSystem.notificationMessage = "You have extinguished the fire!\nHelp your neighbors who might still need it!";
            notificationSystem.disableAfterTimer = true;
            notificationSystem.disableTimer = 4.0f;
            notificationSystem.displayNotification();

			isPlayerSuccessful = true;
		}
	}

	public void AddSpawnPoint(Transform spawnPoint)
	{
		if (!isFireOngoing) FireSpawnPoints.Add(spawnPoint);
	}

	public void RemoveSpawnPoint(Transform spawnPoint)
	{
		if (!isFireOngoing)
		{
			bool isRemoved = true;

			while(isRemoved)
			{
				isRemoved = FireSpawnPoints.Remove(spawnPoint);
			}
		}
	}
}