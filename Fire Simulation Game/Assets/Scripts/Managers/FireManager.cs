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

	[SerializeField] private List<NPCStateMachine> npcStateMachines;

    //sound effect
    public AudioSource audioSource;
    public AudioClip helpAFireClip;

    void Start()
	{
		plugsCount = 0;

		foreach (Transform spawnPoint in FireSpawnPoints)
		{
			if (spawnPoint.GetComponent<ElectricPlug>()) plugsCount++;
		}

		FireTypes = new List<string>{"Electrical", "Grease", "Class A", "Class A"};

		timeBeforeFire = (float) Random.Range(45, 76);

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
			Transform spawnTransform;

			while(true)
			{
				index = Random.Range(0, FireSpawnPoints.Count);
				spawnTransform = FireSpawnPoints[index];

				ElectricPlug plug = spawnTransform.GetComponent<ElectricPlug>();
				if (plug)
				{
					if (plugsCount >= minPlugsCountForFire)
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
						else
						{
							if (plug.owner.name.Equals("ExtensionCord")) break;
							else RemoveSpawnPoint(spawnTransform);
						}
					}
					else
					{
						Debug.Log("No electrical fires possible");
						spawnTransform = null;
						break;
					}
				}
				else
				{
					Fire fire = spawnTransform.GetComponent<Fire>();
					if (fire)
					{
						fire.transform.SetParent(null);
						fire.transform.localScale = Vector3.one * fire.intensityValue;
						ongoingFire = fire;
						ongoingFire.AffectFire(0.35f);
					}
					
					break;
				}
			}

			Vector3 spawnPoint;

			if (spawnTransform)
				spawnPoint = spawnTransform.position;
			else
			{
				Debug.Log("No generated fire\nPlacing a random one now");
				spawnTransform = FireSpawnPoints[0];
				spawnPoint = spawnTransform.position + Vector3.one * 2.0f;
			}

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

			if (index >= 6)
			{
                notificationSystem.notificationMessage = "A fire has emerged! Identify the cause of the fire and put it out accordingly!";
                notificationSystem.disableAfterTimer = true;
	            notificationSystem.disableTimer = 5.0f;
	            notificationSystem.displayNotification();
			}
			else
			{
                audioSource.clip = helpAFireClip;
                audioSource.Play();

                notificationSystem.notificationMessage = "A fire has emerged from your neighbors! Be careful trying to extinguish it since you're uncertain of the cause of fire!";
                notificationSystem.disableAfterTimer = true;
	            notificationSystem.disableTimer = 5.0f;
	            notificationSystem.displayNotification();
			}

			StartCoroutine(SetupForNPC());
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

	IEnumerator SetupForNPC()
	{
		bool isNodeFinalized = false;

		Node fireNode = ongoingFire.GetComponent<Node>();

		if (!fireNode)
			fireNode = ongoingFire.gameObject.AddComponent<Node>();

		try
		{
			if (fireNode.adjacentNodes.Count == 0)
				isNodeFinalized = addAdjacentNodesForFire(fireNode);
		}
		catch
		{
			fireNode.adjacentNodes = new List<Node>();
			fireNode.isEdgeValid = new List<bool>();
			fireNode.edgeWeights = new List<float>();

			isNodeFinalized = addAdjacentNodesForFire(fireNode);
		}

		yield return new WaitUntil(() => isNodeFinalized == true);

		foreach(NPCStateMachine sm in npcStateMachines)
			sm.ongoingFire = ongoingFire;
	}

	bool addAdjacentNodesForFire(Node fireNode)
	{
		if (fireNode.transform.position.y > 5.0f)
			fireNode.floorLevel = 2;
		else
			fireNode.floorLevel = 1;

		foreach(Node node in GameObject.FindObjectsOfType<Node>())
		{
			if (node == fireNode) continue;

			if (!node.GetComponent<FireFightingObject>() && !node.transform.CompareTag("WaterSource") && node.floorLevel == fireNode.floorLevel)
			{
				if (!fireNode.adjacentNodes.Contains(node))
				{
					fireNode.adjacentNodes.Add(node);
					fireNode.isEdgeValid.Add(true);
					fireNode.edgeWeights.Add(0.0f);
				}

				if (!node.adjacentNodes.Contains(fireNode))
				{
					node.adjacentNodes.Add(fireNode);
	                node.isEdgeValid.Add(true);
	                node.edgeWeights.Add(0.0f);	
				}
			}
		}

		return true;
	}

	public void AddSpawnPoint(Transform spawnPoint)
	{
		if (!isFireOngoing)
		{
			FireSpawnPoints.Add(spawnPoint);

			if (spawnPoint.GetComponent<ElectricPlug>()) plugsCount++;
		}
	}

	public void RemoveSpawnPoint(Transform spawnPoint)
	{
		if (!isFireOngoing)
		{
			bool isRemoved = true;

			if (spawnPoint.GetComponent<ElectricPlug>()) plugsCount--;

			while(isRemoved)
			{
				isRemoved = FireSpawnPoints.Remove(spawnPoint);
			}
		}
	}
}