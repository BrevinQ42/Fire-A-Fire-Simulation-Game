using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FryingPan : MonoBehaviour
{
    public GameObject rice; 
    public string npcTag = "NPC";
    public NPC npc;

    private void Start()
    {
        if (rice != null)
        {
            rice.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(npcTag))
        {
            npc = other.GetComponent<NPC>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (rice != null && npc != null && npc.isCooking == true)
        {
            rice.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(npcTag))
        {
            if (rice != null)
            { 
                rice.SetActive(false);
            }
            npc = null;
        }
    }
}
