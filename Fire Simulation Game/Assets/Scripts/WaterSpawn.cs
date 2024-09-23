using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject spawnObject;  
    public Transform spawnPoint;           
    private float spawnRate = 0.1f;         
    private bool isRunning = true;              

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Spawn());
    }

    // Update is called once per frame
    void Update()
    {
    }
    IEnumerator Spawn()
    {
        while (isRunning)
        {
            Instantiate(spawnObject, spawnPoint.position, Quaternion.identity);

            yield return new WaitForSeconds(spawnRate);
        }
    }
}
