using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candle : MonoBehaviour
{
    private Fire FireOnCandle;
    private AudioSource fireAudioSource;

    [Header("FloatingText")]
    public bool lookedAt;
    public GameObject textName;

    // Start is called before the first frame update
    void Start()
    {
        FireOnCandle = GetComponentInChildren<Fire>();

        if (FireOnCandle != null)
        {
            fireAudioSource = FireOnCandle.GetComponent<AudioSource>();

            if (fireAudioSource != null)
            {
                fireAudioSource.mute = true;
            }
        }

        lookedAt = false;
        textName = GetComponentInChildren<TextMesh>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (lookedAt == true)
            textName.SetActive(true);
        if (!lookedAt || !FireOnCandle)
            textName.SetActive(false);
    }
}
