using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpawn : MonoBehaviour
{
    public GameObject waterDroplet;  
    public Transform spawnPoint;           
    private float spawnRate = 0.25f;         
    [SerializeField] private bool isRunning = false;              

    // Start is called before the first frame update
    void Start()
    {
        
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

    public void Toggle()
    {
        isRunning = !isRunning;
        if (isRunning) StartCoroutine(SpawnWater());
    }
}
