using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
	[SerializeField] private FireManager fireManager;

	[Header("Fire Extinguisher")]
	[SerializeField] private List<Transform> extinguisherSpawnPoints;
	[SerializeField] private List<string> extinguisherTypes;

	[SerializeField] private FireExtinguisher extinguisherPrefab;

	[Header("Candle")]
	[SerializeField] private List<Transform> candleSpawnPoints;
	[SerializeField] private GameObject candlePrefab;

	[Header("Characters")]
	[SerializeField] private PlayerController playerPrefab;
	[SerializeField] private NPC npcPrefab;
	[SerializeField] private List<Transform> characterSpawnPoints;

	[Header("Misc.")]
	[SerializeField] private GameObject OutsideFloor;
	public int typeIndex;
	public bool isClassCExtinguisher;
	
	void Start()
	{
		OutsideFloor.GetComponent<NavMeshSurface>().BuildNavMesh();

		extinguisherTypes = new List<string>{"Class A", "Class C", "Class C", "Class K", "Class K"};

		RandomizeExtinguishers();
		RandomizeCandles();
		RandomizeCharacterSpawns();
	}

	void RandomizeExtinguishers()
	{
		for (int i = 0; i < 9; i++)
		{
			int extinguisherIndex = Random.Range(0, extinguisherSpawnPoints.Count);
			Vector3 extinguisherSpawnPoint = extinguisherSpawnPoints[extinguisherIndex].position;

			FireExtinguisher extinguisher = Instantiate(extinguisherPrefab, extinguisherSpawnPoint, Quaternion.identity).GetComponent<FireExtinguisher>();

			typeIndex = Random.Range(0, extinguisherTypes.Count);
			if (typeIndex == 1 || typeIndex == 2)
			{
				isClassCExtinguisher = true;
			}
			else
			{
				isClassCExtinguisher = false;
			}
			extinguisher.SetType(extinguisherTypes[typeIndex]);

			extinguisherSpawnPoints.RemoveAt(extinguisherIndex);
		}
	}

	void RandomizeCandles()
	{
		for (int i = 0; i < 9; i++)
		{
			int index = Random.Range(0, candleSpawnPoints.Count);
			Vector3 candleSpawnPoint = candleSpawnPoints[index].position;

			GameObject candle = Instantiate(candlePrefab, candleSpawnPoint, Quaternion.identity);
			Fire FireOnCandle = candle.GetComponentInChildren<Fire>();
			
			FireOnCandle.intensityValue += 0.14f;
			FireOnCandle.transform.localScale = Vector3.one * FireOnCandle.intensityValue;

			fireManager.AddSpawnPoint(FireOnCandle.transform, false);

			candleSpawnPoints.RemoveAt(index);
		}
	}

	void RandomizeCharacterSpawns()
	{
		int index = Random.Range(0, characterSpawnPoints.Count);

		Instantiate(playerPrefab, characterSpawnPoints[index].position, characterSpawnPoints[index].rotation);
		characterSpawnPoints.RemoveAt(index);

		for (int i = 0; i < 2; i++)
		{
			index = Random.Range(0, characterSpawnPoints.Count);

			NPC npc = Instantiate(npcPrefab, characterSpawnPoints[index].position, characterSpawnPoints[index].rotation);
			fireManager.npcStateMachines.Add(npc.GetComponent<NPCStateMachine>());
			characterSpawnPoints.RemoveAt(index);
		}
	}
}