using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoseNotification : MonoBehaviour
{
    private Text coveredText;

    void Start()
    {
        coveredText = GetComponentInChildren<Text>();
        coveredText.text = "";
    }

    [Header("Notification Animation")]
    public Animator noseNotificationAnim;

    public void EnableNotification()
    {
        noseNotificationAnim.Play("NoseAnimation");
        coveredText.text = "COVERED";
    }

    public void RemoveNotification()
    {
        noseNotificationAnim.Play("NoseAnimationExit");
        coveredText.text = "";
    }
}
