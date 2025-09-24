using UnityEngine;

public class ControlsToggle : MonoBehaviour
{
    public GameObject controlsPanel;
    private bool isPaused = false;

    void Start()
    {
        controlsPanel = GameObject.Find("ControlsPage");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (!isPaused)
                ShowControls();
            else
                HideControls();
        }
    }

    void ShowControls()
    {
        controlsPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game
        isPaused = true;
    }

    void HideControls()
    {
        controlsPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
        isPaused = false;
    }
}
