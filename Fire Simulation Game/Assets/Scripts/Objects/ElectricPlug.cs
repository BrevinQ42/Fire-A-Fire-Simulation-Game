using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricPlug : GrabbableObject
{
    private FireManager fireManager;

    public Transform owner;
    public Transform pluggedInto;

    public bool lookedAt;
    public GameObject textName;

    [SerializeField] private List<ElectricPlug> ExtensionCordPlugs;

    void Start()
    {
        lookedAt = false;
        textName = GetComponentInChildren<TextMesh>().gameObject;

        InitializeFireManager();
    }

    private void Update()
    {
        if (lookedAt == false)
        {
            textName.SetActive(false);
        }
        if (lookedAt == true)
        {
            textName.SetActive(true);
        }
        if (isHeld == true)
        {
            textName.SetActive(false);
        }
    }

    bool isPowered()
    {
        Transform currentPlug = pluggedInto;

        HashSet<Transform> plugs = new HashSet<Transform>();
        plugs.Add(currentPlug);

        if (currentPlug == null) return false;
        if (currentPlug.name.Equals("WallOutlet")) return true;

        while(true)
        {
            bool isPlugFound = false;

            foreach(ElectricPlug plug in ExtensionCordPlugs)
            {
                if (plug.owner == currentPlug)
                {
                    if (plugs.Contains(plug.pluggedInto)) return false;

                    currentPlug = plug.pluggedInto;

                    if (currentPlug == null) return false;
                    if (currentPlug.name.Equals("WallOutlet")) return true;

                    isPlugFound = true;
                    break;
                }
            }

            if (!isPlugFound) return false;
        }
    }

    void InitializeFireManager()
    {
        GameObject fm = GameObject.Find("FireManager");
        
        if (fm) fireManager = fm.GetComponent<FireManager>();
    }

    public override void Use(Transform target)
    {
        pluggedInto = target.parent;
        transform.SetParent(pluggedInto);

        if (target.localRotation.x == 1.0f) // extension cord
        {
            transform.localPosition = new Vector3(target.localPosition.x, 0.04f, -0.077f);
            transform.localEulerAngles = new Vector3(180.0f, -90.0f, -90.0f);
        }
        else // wall outlet
        {
            transform.localPosition = new Vector3(target.localPosition.x, -0.062f, 0.033f);
            transform.localEulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
        }

        GetComponent<Collider>().enabled = true;
        isHeld = false;

        if(!fireManager) InitializeFireManager();

        if(fireManager && isPowered())
        {
            fireManager.AddSpawnPoint(transform, false);

            if (owner.name.Equals("ExtensionCord"))
            {
                if (pluggedInto.name.Equals("ExtensionCord"))
                    fireManager.AddSpawnPoint(transform, true); // additional chance of being set on fire

                Transform currentOwner = owner;

                // WARNING: will have issue when there are more than two extension cords
                while(currentOwner.name.Equals("ExtensionCord"))
                {
                    bool isNewOwnerFound = false;

                    foreach(ElectricPlug plug in currentOwner.GetComponentsInChildren<ElectricPlug>())
                    {
                        if (plug.owner != currentOwner && plug.owner.name.Equals("ExtensionCord"))
                        {
                            currentOwner = plug.owner;
                            isNewOwnerFound = true;
                        }

                        fireManager.AddSpawnPoint(plug.transform, false);
                    }

                    if (!isNewOwnerFound) break;
                }
            }
        }
    }

    public void Unplug()
    {
        if (pluggedInto)
        {
            pluggedInto = null;
            fireManager.RemoveSpawnPoint(transform);

            Transform currentOwner = owner;

            // WARNING: will have issue when there are more than two extension cords
            while(currentOwner.name.Equals("ExtensionCord"))
            {
                bool isNewOwnerFound = false;

                foreach(ElectricPlug plug in currentOwner.GetComponentsInChildren<ElectricPlug>())
                {
                    if (plug.owner != currentOwner && plug.owner.name.Equals("ExtensionCord"))
                    {
                        currentOwner = plug.owner;
                        isNewOwnerFound = true;
                    }

                    fireManager.RemoveSpawnPoint(plug.transform);
                }

                if (!isNewOwnerFound) break;
            }
        }
    }
}

