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
    public Vector3 position;
    public float walkingSpeed;
    public float runningSpeed;
    public float closeProximityValue;
    public float throwForce;
    public Fire FireOnNPC;

    // Start is called before the first frame update
    void Start()
    {
        pathfinder = GetComponent<Pathfinder>();

        resetPathfindingValues();
        raycastLayerMask = LayerMask.GetMask("Default", "Ignore Raycast", "TransparentFX", "Water", "UI", "Overlay");

        blacklist = new HashSet<Node>();

        position = transform.position + new Vector3(0.0f, 0.9203703f, 0.0f);

        FireOnNPC = null;
    }

    void Update()
    {
        position = transform.position + new Vector3(0.0f, 0.9203703f, 0.0f);
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

            Vector3 direction = path[pathIndex].transform.position - position;

            RaycastHit hit;
            if (Physics.Raycast(position, Vector3.Normalize(direction), out hit, direction.magnitude, raycastLayerMask))
            {
                Debug.Log(pathIndex + " / " + hit.transform.name);

                bool isValid = (target.Equals("WaterSource") && hit.transform.CompareTag(target)) ||
                                (target.Equals("FireFightingObject") && hit.transform.GetComponent<FireFightingObject>()) ||
                                !hit.transform.GetComponent<Collider>().enabled;
                if (!isValid)
                {
                    if (target.Equals("Fire") &&
                        (hit.transform.GetComponent<Fire>() || hit.transform.CompareTag("Smoke")) )
                    {
                        if (hit.distance <= closeProximityValue)
                        {
                            Vector3 newDirection = hit.transform.position - position;

                            Vector3 tempPosition = position + transform.forward * 0.15f;
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

                        List<Node> temp;
                        if (target.Equals("FireFightingObject"))
                            temp = pathfinder.generatePath(currentNode, target, blacklist);
                        else
                            temp = pathfinder.generatePath(currentNode, target);

                        if (temp.Count > 0)
                        {
                            direction = temp[0].transform.position - position;

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
                        else
                        {
                            NPCStateMachine stateMachine = GetComponent<NPCStateMachine>();
                            stateMachine.SwitchState(stateMachine.panicState);
                        }
                    }
                }
                else if (willInteractWithTarget && pathIndex == path.Count-1 && direction.magnitude <= closeProximityValue)
                {
                    InteractWithObject(hit.transform);

                    resetPathfindingValues();
                    return true;
                }
            }
            
            Vector2 currentPos = new Vector2(position.x, position.z);

            Vector2 newPos = Vector2.MoveTowards(currentPos, nextPos, speed * Time.deltaTime);
            transform.position = new Vector3(newPos.x, transform.position.y, newPos.y);

            if (newPos == nextPos)
            {
                if (path[pathIndex] == pathfinder.getExitNode())
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
            Rigidbody hitRB = hitTransform.GetComponent<Rigidbody>();

            if (hitRB && hitTransform.CompareTag("Grabbable"))
            {
                NonFlammableObject nonFlammable = hitTransform.GetComponent<NonFlammableObject>();
                if (nonFlammable && nonFlammable.GetComponent<Rigidbody>().GetAccumulatedForce().magnitude >= 100.0f)
                {
                    return;
                }

                hitRB.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
                hitTransform.GetComponent<Collider>().enabled = false;

                hitTransform.SetParent(transform);

                Pail pail = hitTransform.GetComponent<Pail>();
                FireExtinguisher extinguisher = hitTransform.GetComponent<FireExtinguisher>();
                if (pail)
                {
                    pail.closeProximityValue = closeProximityValue;
                    pail.playerCamera = transform;
                    hitTransform.SetPositionAndRotation(
                        transform.position + transform.forward * 0.5f + transform.right * 0.3f, 
                        transform.rotation);
                }
                else if (extinguisher)
                {
                    if (!extinguisher.isPinPulled)
                        extinguisher.isPinPulled = true;

                    hitTransform.SetPositionAndRotation(
                        transform.position + transform.forward * 0.5f + transform.right * 0.3f, 
                        transform.rotation);
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
            Destroy(collision.gameObject);
        }
    }
}
