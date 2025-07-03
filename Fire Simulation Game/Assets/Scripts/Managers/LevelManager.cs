using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

	public int typeIndex;
	public bool isClassCExtinguisher;
	void Start()
	{
		extinguisherTypes = new List<string>{"Class A", "Class C", "Class C", "Class K", "Class K"};

		RandomizeExtinguisher();

		GameObject.FindObjectOfType<Pathfinder>().populateObjectNodes();

		RandomizeCandles();
	}

	void Update()
	{

	}

	void RandomizeExtinguisher()
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
	}

	void RandomizeCandles()
	{
		int candleCount = Random.Range(1, candleSpawnPoints.Count);

		for (int i = 0; i < candleCount; i++)
		{
			int index = Random.Range(0, candleSpawnPoints.Count);
			Vector3 candleSpawnPoint = candleSpawnPoints[index].position;

			GameObject candle = Instantiate(candlePrefab, candleSpawnPoint, Quaternion.identity);
			Fire FireOnCandle = candle.GetComponentInChildren<Fire>();
			
			FireOnCandle.intensityValue += 0.14f;
			FireOnCandle.transform.localScale = Vector3.one * FireOnCandle.intensityValue;

			fireManager.AddSpawnPoint(FireOnCandle.transform);

			candleSpawnPoints.RemoveAt(index);
		}
	}
}