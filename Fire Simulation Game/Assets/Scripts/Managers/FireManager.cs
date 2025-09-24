using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
	// Notification system reference
    public NotificationTriggerEvent notificationSystem;

	[SerializeField] private Fire firePrefab;
	[SerializeField] private List<Transform> FireSpawnPoints;

	[SerializeField] private int plugsCount;
	[SerializeField] private int minPlugsCountForFire;

	private List<string> FireTypes;
    public int index;

    public float timeBeforeFire;
	public bool isFireOngoing;
	private Fire ongoingFire;
	public bool isPlayerSuccessful;

	public List<NPCStateMachine> npcStateMachines;

    //sound effect
    public AudioSource audioSource;
    public AudioClip helpAFireClip;

	public NPC npc;

    void Start()
	{
		plugsCount = 0;

		foreach (Transform spawnPoint in FireSpawnPoints)
		{
			if (spawnPoint.GetComponent<ElectricPlug>()) plugsCount++;
		}

		FireTypes = new List<string>{"Electrical", "Grease", "Class A", "Class A"};

		timeBeforeFire = (float) Random.Range(45, 76);
		// timeBeforeFire = 20;

		isFireOngoing = false;
		ongoingFire = null;
		isPlayerSuccessful = false;

        audioSource = GetComponent<AudioSource>();
    }

	void Update()
	{
		if (timeBeforeFire > 0.0f) timeBeforeFire -= Time.deltaTime;
		else if (!isFireOngoing)
		{
			if (plugsCount < minPlugsCountForFire)
			{
				foreach(ElectricPlug plug in FindObjectsOfType<ElectricPlug>())
				{
					if (FireSpawnPoints.Contains(plug.transform))
						FireSpawnPoints.Remove(plug.transform);
				}
			}

			index = Random.Range(0, FireSpawnPoints.Count);
			Transform spawnTransform = FireSpawnPoints[index];

			if (!spawnTransform.GetComponent<ElectricPlug>())
			{
				Fire fire = spawnTransform.GetComponent<Fire>();
				if (fire)
				{
					fire.transform.SetParent(null);
					fire.transform.localScale = Vector3.one * fire.intensityValue;
					ongoingFire = fire;
					ongoingFire.AffectFire(0.35f);
				}
			}

			Vector3 spawnPoint = spawnTransform.position;

			if (!ongoingFire)
			{
				ongoingFire = Instantiate(firePrefab, spawnPoint, Quaternion.identity).GetComponent<Fire>();

				if (spawnTransform.GetComponent<ElectricPlug>()) ongoingFire.type = "Electrical";
				else if (spawnTransform.name.Equals("Pan"))
				{
					ongoingFire.type = "Grease";
					ongoingFire.isOnPan = true;
				}
				else
				{
					if (index < 6)
					{
						int typeIndex = Random.Range(0, FireTypes.Count);
						ongoingFire.type = FireTypes[typeIndex];
					}
					else ongoingFire.type = "Class A";
				}
			}

			ongoingFire.Toggle(true);
			isFireOngoing = true;

			audioSource.clip = helpAFireClip;
            audioSource.Play();

            notificationSystem.notificationMessage = "A fire has emerged! Identify the cause of the fire and put it out quickly!";
            notificationSystem.disableAfterTimer = true;
            notificationSystem.disableTimer = 5.0f;
            notificationSystem.displayNotification();

            foreach(NPCStateMachine sm in npcStateMachines)
            	sm.ongoingFire = ongoingFire;
		}
		else if (ongoingFire == null && !isPlayerSuccessful)
		{
			notificationSystem.notificationMessage = "You have extinguished the fire!";
            notificationSystem.disableAfterTimer = true;
            notificationSystem.disableTimer = 4.0f;
            notificationSystem.displayNotification();

			isPlayerSuccessful = true;
		}
	}

	public void AddSpawnPoint(Transform spawnPoint, bool isDuplicate)
	{
		if (!isFireOngoing)
		{
			FireSpawnPoints.Add(spawnPoint);

			if (spawnPoint.GetComponent<ElectricPlug>() && !isDuplicate) plugsCount++;
		}
	}

	public void RemoveSpawnPoint(Transform spawnPoint)
	{
		if (!isFireOngoing)
		{
			bool isRemoved = true;

			if (FireSpawnPoints.Contains(spawnPoint) && spawnPoint.GetComponent<ElectricPlug>())
				plugsCount--;

			while(isRemoved)
			{
				isRemoved = FireSpawnPoints.Remove(spawnPoint);
			}
		}
	}
}