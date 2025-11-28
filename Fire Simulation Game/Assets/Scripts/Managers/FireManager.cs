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
    public int index;

    public float timeBeforeFire;
	public bool isFireOngoing;
	public Fire ongoingFire;
	public bool isPlayerSuccessful;

	public List<NPCStateMachine> npcStateMachines;

	private float timeBeforeBell;

    //sound effect
    public AudioSource audioSource;
    public AudioClip helpAFireClip;
	public AudioClip bellClip;

    void Start()
	{
		FireTypes = new List<string>{"Electrical", "Grease", "Class A", "Class A"};

		timeBeforeFire = (float) Random.Range(45, 76);
		// timeBeforeFire = 20;

		isFireOngoing = false;
		ongoingFire = null;
		isPlayerSuccessful = false;

		if (npcStateMachines.Count < 2) npcStateMachines = new List<NPCStateMachine>();

		timeBeforeBell = 5.0f;

        audioSource = GetComponent<AudioSource>();
    }

	void Update()
	{
		if (timeBeforeFire > 0.0f) timeBeforeFire -= Time.deltaTime;
		else if (!isFireOngoing)
		{
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

				if (spawnTransform.GetComponent<ElectricPlug>()) ongoingFire.SetType("Electrical");
				else if (spawnTransform.name.Equals("Pan"))
				{
					ongoingFire.SetType("Grease");
					ongoingFire.isOnPan = true;
				}
				else
					ongoingFire.SetType("Class A");
			}

			ongoingFire.Toggle(true);
			isFireOngoing = true;

			if (audioSource != null && helpAFireClip != null)
			{
				audioSource.clip = helpAFireClip;
				audioSource.Play();
			}

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

		if (isFireOngoing && timeBeforeBell > 0.0f)
		{
			timeBeforeBell -= Time.deltaTime;

			if (timeBeforeBell <= 0.0f)
			{
				if (audioSource != null && bellClip != null)
				{
					audioSource.PlayOneShot(bellClip);
				}

				foreach (NPCStateMachine sm in npcStateMachines)
				{
					sm.hasBellRung = true;
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.T))
        {
        	foreach (NPCStateMachine sm in npcStateMachines)
            	Debug.Log (sm.currentState + " / " + sm.npc.agent + " => " + (sm.currentState == null && sm.npc.agent != null));
        }
	}

	public void AddSpawnPoint(Transform spawnPoint, bool isDuplicate)
	{
		if (!isFireOngoing)
		{
			FireSpawnPoints.Add(spawnPoint);
		}
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