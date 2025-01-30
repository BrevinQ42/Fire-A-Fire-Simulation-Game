using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectionManager : MonoBehaviour
{
	[SerializeField] private GameObject player;
	[SerializeField] private TutorialManager tm;
	[SerializeField] private GameObject Canvas;
	[SerializeField] private GameObject NotifCanvas;

	void Start()
	{
		player.GetComponent<PlayerController>().enabled = false;
		Canvas.SetActive(false);
		NotifCanvas.SetActive(false);

		Cursor.lockState = CursorLockMode.None;
	}

	public void ChooseFireType (string type)
	{
		NotifCanvas.SetActive(true);
		Canvas.SetActive(true);
		player.GetComponent<PlayerController>().enabled = true;

		tm.SetupFireType(type);
		
		gameObject.SetActive(false);
	}

	public void BackToMain()
	{
		SceneManager.LoadScene("Start Menu");
	}
}