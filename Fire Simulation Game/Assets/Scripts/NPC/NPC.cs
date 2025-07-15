using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("Pathfinding")]
    public Pathfinder pathfinder;
    [SerializeField] private Node currentNode;
    
    // temp
    [SerializeField] private List<Node> generatedPath;
    [SerializeField] private string pathTarget;
    
    [SerializeField] private int pathIndex;
    private Vector2 nextPos;
    private int raycastLayerMask;

    public HashSet<Node> blacklist;

    [Header("Misc.")]
    [SerializeField] private Transform head;
    public Vector3 position;
    public float gravityForce;
    public float walkingSpeed;
    public float runningSpeed;
    public float closeProximityValue;
    public float throwForce;
    public Fire FireOnNPC;

    // Animation Related
    public bool isHoldingObject;
    public bool isRolling;
    public bool coroutinePlaying;
    public Vector3 lastPosition;
    public Animator NPCAnimator;

    // Start is called before the first frame update
    void Start()
    {
        pathfinder = GetComponent<Pathfinder>();

        resetPathfindingValues();
        raycastLayerMask = LayerMask.GetMask("Default", "Ignore Raycast", "TransparentFX", "Water", "UI", "Overlay");

        blacklist = new HashSet<Node>();

        position = transform.position + new Vector3(0.0f, 0.9203703f, 0.0f);

        FireOnNPC = null;

        isHoldingObject = false;
        isRolling = false;
        coroutinePlaying = false;
        NPCAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isGrounded())
            GetComponent<Rigidbody>().AddForce(new Vector3(0, -gravityForce, 0));

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

        lastPosition = position;
    }

    public Node getCurrentNode()
    {
        return currentNode;
    }

    public void setCurrentNode(Node node)
    {
        currentNode = node;
    }

    public bool followPath(List<Node> path, string target, float speed, bool willInteractWithTarget, out List<Node> newPath)
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

        newPath = new List<Node>();

        generatedPath = path;
        pathTarget = target;

        if (pathIndex < path.Count)
        {
            try
            {
                nextPos = new Vector2(path[pathIndex].transform.position.x, path[pathIndex].transform.position.z);
            }
            catch
            {
                if (pathIndex == path.Count-1 && target.Equals("Fire"))
                {
                    newPath = pathfinder.generatePath(currentNode, "Court");
                    return true;
                }
            }

                                                                        // z-coord
            transform.LookAt(new Vector3(nextPos.x, transform.position.y, nextPos.y));

            Vector3 direction = path[pathIndex].transform.position - transform.position;

            Debug.DrawRay(transform.position, direction, Color.white); 

            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.Normalize(direction), out hit, direction.magnitude, raycastLayerMask))
            {
                Debug.Log(pathIndex + " / " + hit.transform.name);
                Debug.DrawRay(transform.position, direction, Color.white); 

                bool isValid = (target.Equals("WaterSource") && hit.transform.CompareTag(target)) ||
                                (target.Equals("FireFightingObject") && hit.transform.GetComponent<FireFightingObject>()) ||
                                !hit.transform.GetComponent<Collider>().enabled;
                if (!isValid)
                {
                    if (target.Equals("Fire") &&
                        (hit.transform.GetComponent<Fire>() || hit.transform.CompareTag("Smoke")) )
                    {
                        bool isFireFound = false;

                        if (hit.transform.GetComponent<Fire>() && hit.transform.GetComponent<Fire>() == FireOnNPC)
                        {
                            RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.Normalize(direction), direction.magnitude, raycastLayerMask);

                            foreach(RaycastHit obj in hits)
                            {
                                if (obj.transform.GetComponent<Fire>())
                                {
                                    if (obj.transform.GetComponent<Fire>() != FireOnNPC)
                                        isFireFound = true;
                                }
                                else
                                {
                                    pathfinder.SetEdgeInvalid(currentNode, path[pathIndex]);
                                    newPath = findNewPath(target);
                                    return false;
                                }
                            }
                        }
                        else
                            isFireFound = true;
                        
                        if (isFireFound && hit.distance <= closeProximityValue)
                        {
                            Vector3 newDirection = hit.transform.position - position;

                            Vector3 tempPosition = transform.position + transform.forward * 0.15f;

                            int tempLayerMask = LayerMask.GetMask("Default", "Person", "TransparentFX", "Water", "UI", "Overlay");

                            if (Vector3.Normalize(direction) != Vector3.Normalize(newDirection) &&
                                !Physics.Raycast(tempPosition, Vector3.Normalize(newDirection), newDirection.magnitude, tempLayerMask))
                            {
                                Vector3 firePos = hit.transform.position;

                                transform.LookAt(new Vector3(firePos.x, transform.position.y, firePos.z));
                            }

                            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;
                            
                            return true;
                        }
                    }
                    else
                    {
                        pathfinder.SetEdgeInvalid(currentNode, path[pathIndex]);

                        newPath = findNewPath(target);
                        if (newPath.Count == 0)
                        {
                            NPCStateMachine stateMachine = GetComponent<NPCStateMachine>();
                            stateMachine.SwitchState(stateMachine.panicState);
                        }
                    }
                }
                else if ( willInteractWithTarget && pathIndex == path.Count-1 &&
                            ( direction.magnitude <= closeProximityValue ||
                            Vector2.Distance(nextPos, new Vector2(position.x, position.z)) <= closeProximityValue ) )
                {
                    InteractWithObject(hit.transform);
                    
                    return true;
                }
            }
            
            Vector2 currentPos = new Vector2(position.x, position.z);

            Vector2 newPos = Vector2.MoveTowards(currentPos, nextPos, speed * Time.deltaTime);
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.y);

            if (newPos == nextPos)
            {
                if (target.Equals("Court") && path[pathIndex] == pathfinder.getExitNode())
                {
                    Door door = GameObject.FindObjectOfType<Door>();

                    if (!door.isOpen) door.toggleDoor();
                }

                currentNode = path[pathIndex];

                pathIndex++;

                if (pathIndex < path.Count)
                {
                    nextPos = new Vector2(path[pathIndex].transform.position.x, path[pathIndex].transform.position.z);
                    return false;
                }
                else
                {
                    resetPathfindingValues();
                    return true;
                }
            }
            return false;
        }
        else
        {
            resetPathfindingValues();
            return true;
        }
    }

    List<Node> findNewPath(string target)
    {
        List<Node> newPath = new List<Node>();
        List<Node> temp;
        if (target.Equals("FireFightingObject"))
            temp = pathfinder.generatePath(currentNode, target, blacklist);
        else
            temp = pathfinder.generatePath(currentNode, target);

        if (temp.Count > 0)
        {
            Vector3 direction = temp[0].transform.position - position;

            if (position != currentNode.transform.position &&
                Physics.Raycast(position, Vector3.Normalize(direction), direction.magnitude, raycastLayerMask))
            {
                newPath.Add(currentNode);
                newPath.AddRange(temp);
            }
            else
                newPath = temp;

            pathIndex = 0;
        }
        
        return newPath;
    }

    void resetPathfindingValues()
    {
        pathIndex = 0;
        nextPos = new Vector2(position.x, position.z);
    }

    void InteractWithObject(Transform hitTransform)
    {
        if (hitTransform.CompareTag("WaterSource"))
        {
            Spawner waterSpawner = hitTransform.GetComponentInChildren<Spawner>();
            waterSpawner.Toggle(true);
        }
        else
        {
            resetPathfindingValues();

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

    bool isGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.0f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Smoke"))
        {
            Destroy(collision.gameObject);
        }
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
