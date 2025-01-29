using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinScreen : MonoBehaviour
{
    public Text healthPointsText;
    public Text timePointsText;
    public Image oneStar;
    public Image twoStar;
    public Image threeStar;
    public void Setup(float hpText, float tpText)
    {
        gameObject.SetActive(true);
        healthPointsText.text = "Health Remaining: " + hpText.ToString();
        timePointsText.text = "Time Completed: " + tpText.ToString() + " Seconds";
    }
}
