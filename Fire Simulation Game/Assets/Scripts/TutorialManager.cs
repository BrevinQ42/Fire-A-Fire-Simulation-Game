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
	[SerializeField] private NonFlammableObject FireResistant;
	[SerializeField] private FireExtinguisher FireExtinguisher;

	[Header("Fire")]
	public Fire firePrefab;
	[SerializeField] private Vector3 fireLocation;
	public Fire ongoingFire;
	[SerializeField] private string fireType;
	[SerializeField] private float previousIntensityValue;
	private bool isNextFireComing;

	void Start()
	{
		Bucket.setFractionFilled(1.0f);

		ongoingFire = Instantiate(firePrefab, fireLocation, Quaternion.identity).GetComponent<Fire>();
		ongoingFire.Toggle(false);

		previousIntensityValue = 0.5f;
		isNextFireComing = false;

		if (fireType != "")
		{
			SetupFireType(fireType);
		}
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
		}
		else if (ongoingFire.intensityValue > 0.5f && player.collidedWith && player.collidedWith.name.Equals("Court"))
		{
			tutorialNotifSystem.notificationMessage = "You successfully escaped!\nPress [Esc] to Call the Fire Department and Leave Tutorial";
			tutorialNotifSystem.displayNotification();

			if(Input.GetKeyDown(KeyCode.Escape))
			{
        		Cursor.lockState = CursorLockMode.None;
				
				// reload scene
				SceneManager.LoadScene("Tutorial Scene");
			}
		}
		
		if (!isNextFireComing)
		{
			if (previousIntensityValue > ongoingFire.intensityValue)
			{
				tutorialNotifSystem.notificationMessage = "The fire got smaller!\nYou found the right object!";
				tutorialNotifSystem.displayNotification();

				previousIntensityValue = ongoingFire.intensityValue;
			}
			else if (previousIntensityValue < ongoingFire.intensityValue)
			{
				tutorialNotifSystem.notificationMessage = "The fire got bigger!\nTry another object to put it out!";
				tutorialNotifSystem.displayNotification();

				previousIntensityValue = ongoingFire.intensityValue;
			}
		}
	}

	IEnumerator PromptForEvacuation()
	{
		yield return new WaitForSeconds(3.5f);

		ongoingFire = Instantiate(firePrefab, fireLocation + new Vector3(0f,0f,1.5f), Quaternion.identity).GetComponent<Fire>();
		ongoingFire.type = "UNSTOPPABLE";

		yield return new WaitUntil(() => ongoingFire.type.Equals("UNSTOPPABLE"));
		ongoingFire.Toggle(true);

		tutorialNotifSystem.notificationMessage = "There is a growing, unstoppable fire!\nGo to an open area like the basketball court outside";
		tutorialNotifSystem.displayNotification();
	}

	void SpawnFireFightingObjects()
	{
		Vector3 objPosition = new Vector3(24.0f, 1.0f, -18.5f);
		
		Pail newBucket = Instantiate(Bucket, objPosition, Quaternion.identity);
		newBucket.setFractionFilled(1.0f);
		objPosition = new Vector3(objPosition.x + 1.5f, objPosition.y, objPosition.z);

		NonFlammableObject fireResistant = Instantiate(FireResistant, objPosition, Quaternion.identity);
		objPosition = new Vector3(objPosition.x + 1.5f, objPosition.y, objPosition.z);

		List<string> fireExtinguisherTypes = new List<string>{"Class A", "Class C", "Class K"};

		foreach (string type in fireExtinguisherTypes)
		{
			FireExtinguisher extinguisher = Instantiate(FireExtinguisher, objPosition, Quaternion.identity);
			extinguisher.SetType(type);

			objPosition = new Vector3(objPosition.x + 1.5f, objPosition.y, objPosition.z);
		}
	}

	public void SetupFireType(string type)
	{
		fireType = type;
		ongoingFire.type = fireType;

		string message = "This is a " + ongoingFire.type + " fire.\nFigure out which of these can put it out.";
		tutorialNotifSystem.notificationMessage = message;
		tutorialNotifSystem.disableAfterTimer = false;
		tutorialNotifSystem.displayNotification();

		SpawnFireFightingObjects();

		Cursor.lockState = CursorLockMode.Locked;
	}
}