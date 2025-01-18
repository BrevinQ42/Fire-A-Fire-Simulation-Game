using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
	[SerializeField] private PlayerController player;
	[SerializeField] private NotificationTriggerEvent tutorialNotifSystem;

	[Header("Fire Fighting Objects")]
	[SerializeField] private Pail Bucket;
	[SerializeField] private FireFightingObject FireExtinguisher;
	private Dictionary<string, List<string>> ObjsVsFire;

	[Header("Fire")]
	[SerializeField] private Fire firePrefab;
	[SerializeField] private Vector3 fireLocation;
	[SerializeField] private Fire ongoingFire;
	private bool isNextFireComing;

	void Start()
	{
		Bucket.setFractionFilled(1.0f);

		PopulateObjsVsFire();

		ongoingFire = Instantiate(firePrefab, fireLocation, Quaternion.identity).GetComponent<Fire>();
		// set type of fire here (from PlayerPrefs)
		ongoingFire.Toggle(false);

		isNextFireComing = false;

		string message = "This is a " + ongoingFire.type + " fire.\nPut it out with the items in front of you.";
		tutorialNotifSystem.notificationMessage = message;
		tutorialNotifSystem.disableAfterTimer = false;
		tutorialNotifSystem.displayNotification();

		if (ObjsVsFire.ContainsKey(ongoingFire.type))
		{
			foreach (string name in ObjsVsFire[ongoingFire.type])
			{
				if (name.Equals("Water"))
				{
					Pail newBucket = Instantiate(Bucket, new Vector3(25.5f, 1f, -19f), Quaternion.identity);
				}
				else
				{
					// fireExtinguisher fe =
					Instantiate(FireExtinguisher, new Vector3(28.5f, 1f, -19f), Quaternion.identity);
					// set type of extinguisher accordingly
				}
			}
		}
		else
			Debug.Log(ongoingFire.type + " [key] not in dict");
	}

	void Update()
	{
		if (!ongoingFire && !isNextFireComing)
		{
			tutorialNotifSystem.notificationMessage = "You extinguished the fire! Good job!";
			tutorialNotifSystem.displayNotification();

			isNextFireComing = true;

			// after 5 seconds,
			StartCoroutine(PromptForEvacuation());

			// mention press esc to select another type of fire for tutorial
			// - do this when they step on basketball court (might need to implement this in playercontroller)
			//		- else if collider is court (under the OnCollisionEnter function)
			// - needs a screen for type of fire selection
		}
		else if (ongoingFire.intensityValue > 0.5f && player.collidedWith && player.collidedWith.name.Equals("Court"))
		{
			tutorialNotifSystem.notificationMessage = "You successfully escaped!\nPress [Esc] to Leave Tutorial";
			tutorialNotifSystem.displayNotification();

			if(Input.GetKeyDown(KeyCode.Escape))
			{
        		Cursor.lockState = CursorLockMode.None;
				
				// go back to screen where they select fire type
				// **for now back to starting menu
				SceneManager.LoadScene("Start Menu");
			}
		}
	}

	void PopulateObjsVsFire()
	{
		ObjsVsFire = new Dictionary<string, List<string>>();

		// electrical
		

		// grease

		// class A
		ObjsVsFire["Class A"] = new List<string>{"Water"};
	}

	IEnumerator PromptForEvacuation()
	{
		yield return new WaitForSeconds(3.5f);

		ongoingFire = Instantiate(firePrefab, fireLocation + new Vector3(0f,0f,1.5f), Quaternion.identity).GetComponent<Fire>();
		ongoingFire.type = "Electrical";

		yield return new WaitUntil(() => ongoingFire.type.Equals("Electrical"));
		ongoingFire.Toggle(true);

		tutorialNotifSystem.notificationMessage = "There is a growing, unstoppable fire!\nGo to an open area like the basketball court outside";
		tutorialNotifSystem.displayNotification();
	}
}