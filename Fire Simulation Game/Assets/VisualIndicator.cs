using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualIndicator : MonoBehaviour
{
    public GameObject player;
    public GameObject fire;
    public GameObject pivot;
    private RectTransform rt;

    void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (fire)
        {
            // Based on Source:
            // https://www.youtube.com/watch?v=knb3nNAuTy8&t=192s

            Vector3 playerToFireVector = new Vector3(fire.transform.position.x - player.transform.position.x, 0.0f,
                                                        fire.transform.position.z - player.transform.position.z);

            Vector3 playerForwardVector = new Vector3(player.transform.forward.x, 0.0f, player.transform.forward.z);

            float angle = Vector3.SignedAngle(playerToFireVector.normalized, playerForwardVector, Vector3.up);

            pivot.transform.localEulerAngles = new Vector3(0, 0, angle);

            playerToFireVector.y = fire.transform.position.y - player.transform.position.y;

            float size = Mathf.Clamp((15.0f - playerToFireVector.magnitude), 2.5f, 5.0f) * 10.0f;
            rt.sizeDelta = new Vector2(size, size);
        }
        else
            gameObject.SetActive(false);

        if (Input.GetKey(KeyCode.T))
        {
            Debug.Log("Distance = " + new Vector3(fire.transform.position.x - player.transform.position.x,
                                            fire.transform.position.y - player.transform.position.y,
                                            fire.transform.position.z - player.transform.position.z).magnitude);
        }
    }
}
