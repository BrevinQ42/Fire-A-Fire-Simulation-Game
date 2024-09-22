using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpawn : MonoBehaviour
{
    public GameObject waterDroplet;  
    public Transform spawnPoint;           
    private float spawnRate = 0.1f;         
    private bool isRunning = true;        

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnWater());
    }

    // Update is called once per frame
    void Update()
    {
    }
    IEnumerator SpawnWater()
    {
        while (isRunning)
        {
            // Instantiate a new water droplet
            Instantiate(waterDroplet, spawnPoint.position, Quaternion.identity);

            // Wait for the next droplet spawn based on spawnRate
            yield return new WaitForSeconds(spawnRate);
        }
    }
}
