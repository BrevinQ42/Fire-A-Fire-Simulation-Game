using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    [Header("Pathfinding")]
    public Transform currentLocation;
    public Transform lastHouseLocation;

    public NavMeshAgent agent;
    public HashSet<string> blacklist;

    public Transform evacuationLocation;

    [Header("Misc.")]
    public FireFightingObject heldObject;
    [SerializeField] private Transform head;
    public Vector3 position;
    public float walkingSpeed;
    public float runningSpeed;
    public float closeProximityValue;
    public float throwForce;
    public Fire FireOnNPC;
    public bool isPanicking;
    public bool hasFailedFireFighting;
    public BaseState lastState;
    public bool isOnStairs;

    [Header("Animation Related")]
    public bool isHoldingObject;
    public bool isRolling;
    public bool coroutinePlaying;
    public Vector3 lastPosition;
    public bool fireOnDoor;
    public bool isRunning;
    public bool isInPanicState;
    public bool isUsingLaptop;
    public bool isSleeping;
    public bool isWatchingTV;
    public Animator NPCAnimator;

    // Start is called before the first frame update
    void Start()
    {
        evacuationLocation = GameObject.Find("Basketball_Court -3D").transform;

        blacklist = new HashSet<string>();

        heldObject = null;

        position = transform.position + new Vector3(0.0f, 0.9203703f, 0.0f);

        FireOnNPC = null;

        isHoldingObject = false;
        isRolling = false;
        coroutinePlaying = false;
        isUsingLaptop = false;
        isSleeping= false;
        isWatchingTV = false;
        NPCAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isGrounded() && !isOnStairs)
            GetComponent<Rigidbody>().AddForce(new Vector3(0, -100, 0));

        position = transform.position + new Vector3(0.0f, 0.9203703f, 0.0f);

        if (isHoldingObject == true)
        {
            NPCAnimator.SetBool("isHoldingObject", true);
        }
        else if (isHoldingObject == false)
        {
            NPCAnimator.SetBool("isHoldingObject", false);
        }

        if (isHoldingObject == true && position == lastPosition)
        {
            NPCAnimator.SetBool("isStandingStill", true);
        }
        else
        {
            NPCAnimator.SetBool("isStandingStill", false);
        }

        if (isRunning == true && isHoldingObject == false && isInPanicState == false)
        {
            NPCAnimator.SetBool("isRunning", true);
        }
        else if (isRunning == false)
        {
            NPCAnimator.SetBool("isRunning", false);
        }

        if (isInPanicState == true)
        {
            NPCAnimator.SetBool("isInPanicState", true);
        }
        else if (isInPanicState == false)
        {
            NPCAnimator.SetBool("isInPanicState", false);
        }

        if (isRolling == true)
        {
            StartCoroutine(RollOver());
            coroutinePlaying = true;
        }
        else if (isRolling == false && coroutinePlaying == true)
        {
            StopCoroutine(RollOver());
            coroutinePlaying = false;
        }

        if (isUsingLaptop == true)
        {
            NPCAnimator.SetBool("isUsingLaptop", true);
        }
        else if (isUsingLaptop == false)
        {
            NPCAnimator.SetBool("isUsingLaptop", false);
        }

        if (isSleeping == true)
        {
            NPCAnimator.SetBool("isSleeping", true);
        }
        else if (isSleeping == false)
        {
            NPCAnimator.SetBool("isSleeping", false);
        }

        if (isWatchingTV == true)
        {
            NPCAnimator.SetBool("isWatchingTV", true);
        }
        else if (isWatchingTV == false)
        {
            NPCAnimator.SetBool("isWatchingTV", false);
        }

        //if (position == lastPosition)
        //{
        //    NPCAnimator.SetBool("isIdle", true);
        //}
        //else
        //{
        //    NPCAnimator.SetBool("isIdle", false);
        //}


        lastPosition = position;
    }

    public void GoTo(Vector3 target, float speed)
    {
        Debug.Log("Moving to " + target);

        agent.speed = speed;
        agent.SetDestination(target);
    }

    public bool isHalted()
    {
        return agent.velocity.magnitude < 0.01f;
    }

    public bool hasReachedTarget()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    public void SetStoppingDistance(float distance)
    {
        agent.stoppingDistance = distance;
    }

    public void InteractWithObject(Transform hitTransform)
    {
        if (hitTransform.CompareTag("WaterSource"))
        {
            Spawner waterSpawner = hitTransform.GetComponentInChildren<Spawner>();
            waterSpawner.Toggle(true);
        }
        else
        {
            Rigidbody hitRB = hitTransform.GetComponent<Rigidbody>();

            if (hitRB && hitTransform.CompareTag("Grabbable"))
            {
                hitRB.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
                hitTransform.GetComponent<Collider>().enabled = false;

                hitTransform.SetParent(transform);

                Pail pail = hitTransform.GetComponent<Pail>();
                FireExtinguisher extinguisher = hitTransform.GetComponent<FireExtinguisher>();
                NonFlammableObject nonFlammable = hitTransform.GetComponent<NonFlammableObject>();
                if (pail)
                {
                    pail.closeProximityValue = closeProximityValue;
                    pail.playerCamera = head;
                    hitTransform.SetPositionAndRotation(
                        transform.position + transform.forward * 0.5f + transform.right * 0.3f, 
                        transform.rotation);
                    isHoldingObject = true; 
                }
                else if (extinguisher)
                {
                    if (!extinguisher.isPinPulled)
                        extinguisher.isPinPulled = true;

                    hitTransform.SetPositionAndRotation(
                        transform.position + transform.forward * 0.5f + transform.right * 0.3f, 
                        transform.rotation);
                    isHoldingObject = true;
                }
                else if (nonFlammable)
                    hitTransform.SetPositionAndRotation(transform.position + transform.forward * 0.55f, transform.rotation);
            }

            hitTransform.GetComponent<GrabbableObject>().isHeld = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Smoke"))
        {
            // Destroy(collision.gameObject);
            isOnStairs = false;
        }
        else if (collision.gameObject.GetComponent<Door>())
        {
            Door door = collision.gameObject.GetComponent<Door>();

            if (!door.isOpen)
                door.toggleDoor();
        }
        else if (collision.gameObject.name.Equals("Stairs"))
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(0, 350, 0));
            isOnStairs = true;
        }
        else
        {
            Transform other = collision.gameObject.transform;
            if (other.parent && other.parent.GetComponent<NavMeshSurface>())
            {
                if (currentLocation != other.parent)
                {
                    other.parent.GetComponent<NavMeshSurface>().BuildNavMesh();

                    if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

                    int areaMask = NavMesh.AllAreas;
                    areaMask -= 1 << NavMesh.GetAreaFromName("Outside");
                    agent.areaMask = areaMask;
                    
                    currentLocation = other.parent;
                    lastHouseLocation = currentLocation;

                    if (heldObject)
                        heldObject.isOutside = false;
                }
            }
            else if ( !GetComponent<NPCStateMachine>().currentStateName.Equals("Roam") &&
                        (other.name.Equals("Outside Floor") || other.name.Equals("Court")) )
            {
                currentLocation = other;

                if (other.GetComponent<NavMeshSurface>())
                {
                    if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

                    int areaMask = NavMesh.AllAreas;
                    areaMask -= 1 << NavMesh.GetAreaFromName("Walkable");
                    agent.areaMask = areaMask;
                }

                if (heldObject)
                    heldObject.isOutside = true;
            }
        
            isOnStairs = false;
        }
    }

    bool isGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.0f);
    }

    public IEnumerator RollOver()
    {
        while (FireOnNPC != null)
        {
            yield return new WaitForSeconds(2.0f);
            FireOnNPC.AffectFire(-0.01f);
            yield return null;
        }
    }
}
