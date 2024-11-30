using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoseNotification : MonoBehaviour
{

    [Header("Notification Animation")]
    public Animator noseNotificationAnim;

    public void EnableNotification()
    {
        noseNotificationAnim.Play("NoseAnimation");
    }

    public void RemoveNotification()
    {
        noseNotificationAnim.Play("NoseAnimationExit");
    }
}
