using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationTriggerEvent : MonoBehaviour
{
    [Header("UI Content")]
    public Text notificationTextUI;

    [Header("Message Customisation")]
    public string notificationMessage;

    [Header("Notification Removal")]
    public bool removeAfterEXIT = false;
    public bool disableAfterTimer = false;
    public float disableTimer;

    [Header("Notification Animation")]
    public Animator notificationAnim;

    public void displayNotification()
    {
        StartCoroutine(EnableNotification());
    }

    IEnumerator EnableNotification()
    {
        notificationAnim.Play("NotificationFadeIn");
        notificationTextUI.text = notificationMessage;

        if(disableAfterTimer)
        {
            yield return new WaitForSeconds(disableTimer);
            RemoveNotification();
        }
    }

    void RemoveNotification()
    {
        notificationAnim.Play("NotificationFadeOut");
    }
}