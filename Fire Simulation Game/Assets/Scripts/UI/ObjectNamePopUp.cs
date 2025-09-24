using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectNamePopUp : MonoBehaviour
{
    public bool lookedAt;
    public GameObject textName;
    public bool isOutside;

    // Start is called before the first frame update
    void Start()
    {
        lookedAt = false;
        textName = GetComponentInChildren<TextMesh>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (lookedAt == false)
        {
            textName.SetActive(false);
        }
        if (lookedAt == true)
        {
            textName.SetActive(true);
        }
    }
}
