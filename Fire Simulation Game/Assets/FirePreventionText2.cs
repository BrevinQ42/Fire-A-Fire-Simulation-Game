using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FirePreventionText2 : MonoBehaviour
{
    public PlayerController player;
    public TextMeshProUGUI textUI;

    void OnEnable()
    {
        textUI = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
            return;
        }

        textUI.text = "Fire Prevention Tasks Done: " + player.firePreventionTasksDone;
    }
}