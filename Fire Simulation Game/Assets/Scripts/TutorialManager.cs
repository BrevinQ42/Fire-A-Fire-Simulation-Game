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
	[SerializeField] private FireExtinguisher FireExtinguisher;
	private Dictionary<string, List<string>> ObjsVsFire;

	[Header("Fire")]
	[SerializeField] private Fire firePrefab;
	[SerializeField] private Vector3 fireLocation;
	[SerializeField] private Fire ongoingFire;
	[SerializeField] private string fireType;
	private bool isNextFireComing;

	void Start()
	{
		Bucket.setFractionFilled(1.0f);

		PopulateObjsVsFire();

		ongoingFire = Instantiate(firePrefab, fireLocation, Quaternion.identity).GetComponent<Fire>();
		ongoingFire.Toggle(false);

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
	}

	void PopulateObjsVsFire()
	{
		ObjsVsFire = new Dictionary<string, List<string>>();

		// electrical
		ObjsVsFire["Electrical"] = new List<string>{"Class C"};

		// grease
		ObjsVsFire["Grease"] = new List<string>{"Class K"};

		// class A
		ObjsVsFire["Class A"] = new List<string>{"Water", "Class A"};
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

	public void SetupFireType(string type)
	{
		fireType = type;
		ongoingFire.type = fireType;

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
					Pail newBucket = Instantiate(Bucket, new Vector3(25.5f, 1f, -18.5f), Quaternion.identity);
					newBucket.setFractionFilled(1.0f);
				}
				else
				{
					FireExtinguisher extinguisher = Instantiate(FireExtinguisher, new Vector3(28.5f, 1f, -18.5f), Quaternion.identity);
					extinguisher.SetType(name);
				}
			}
		}
		else
			Debug.Log(ongoingFire.type + " [key] not in dict");

		Cursor.lockState = CursorLockMode.Locked;
	}
}