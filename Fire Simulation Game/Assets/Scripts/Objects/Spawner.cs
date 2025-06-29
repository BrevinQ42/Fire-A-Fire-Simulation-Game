using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private SmokeSystemManager smokeManager;

    public GameObject spawnObject;  
    public Transform spawnPoint;           
    [SerializeField] private float spawnRate = 0.25f;         
    [SerializeField] private bool isRunning = false;              

    // Start is called before the first frame update
    void Start()
    {
        GameObject ssm = GameObject.Find("SmokeSystemManager");

        if (ssm) smokeManager = ssm.GetComponent<SmokeSystemManager>();

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

    public void Toggle()
    {
        isRunning = !isRunning;

        if (smokeManager == null)
        {
            GameObject ssm = GameObject.Find("SmokeSystemManager");
            if(ssm) smokeManager = ssm.GetComponent<SmokeSystemManager>();
        }

        if (smokeManager != null)
        {
            if (isRunning) smokeManager.IncrementCounter();
            else smokeManager.DecrementCounter();
        }

        if (isRunning) StartCoroutine(Spawn());
    }

    public void Toggle(bool hasPermissionToRun)
    {
        bool mustToggle = (isRunning && !hasPermissionToRun) || (!isRunning && hasPermissionToRun);

        if (mustToggle) Toggle();
    }
}
