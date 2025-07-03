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
    
    [SerializeField] private int pathIndex;
    private Vector2 nextPos;
    private int raycastLayerMask;

    [Header("Misc.")]
    public float walkingSpeed;
    public float runningSpeed;
    public float closeProximityValue;

    // Start is called before the first frame update
    void Start()
    {
        pathfinder = GetComponent<Pathfinder>();

        resetPathfindingValues();
        raycastLayerMask = LayerMask.GetMask("Default", "Ignore Raycast", "TransparentFX", "Water", "UI");
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
        newPath = new List<Node>();

        generatedPath = path;

        if (pathIndex < path.Count)
        {
            Vector3 direction = path[pathIndex].transform.position - transform.position;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.Normalize(direction), out hit, direction.magnitude, raycastLayerMask))
            {
                bool isValid = (target.Equals("WaterSource") && hit.transform.CompareTag(target)) ||
                                (target.Equals("FireFightingObject") && hit.transform.GetComponent<FireFightingObject>()) ||
                                !hit.transform.GetComponent<Collider>().enabled;
                if (!isValid)
                {
                    pathfinder.SetEdgeInvalid(currentNode, path[pathIndex]);

                    List<Node> temp = pathfinder.generatePath(currentNode, target);

                    direction = temp[0].transform.position - transform.position;

                    if (transform.position != currentNode.transform.position &&
                        Physics.Raycast(transform.position, Vector3.Normalize(direction), direction.magnitude, raycastLayerMask))
                    {
                        newPath.Add(currentNode);
                        newPath.AddRange(temp);
                    }
                    else
                        newPath = temp;

                    pathIndex = 0;
                }
                else if (willInteractWithTarget && pathIndex == path.Count-1 && direction.magnitude <= closeProximityValue)
                {
                    InteractWithObject(hit.transform);

                    resetPathfindingValues();
                    return true;
                }
            }
            else if (willInteractWithTarget && pathIndex == path.Count-1 && direction.magnitude <= closeProximityValue)
            {
                InteractWithObject(hit.transform);

                resetPathfindingValues();
                return true;
            }

            nextPos = new Vector2(path[pathIndex].transform.position.x, path[pathIndex].transform.position.z);
            
            Vector2 currentPos = new Vector2(transform.position.x, transform.position.z);

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
        nextPos = new Vector2(transform.position.x, transform.position.z);
    }

    void InteractWithObject(Transform hitTransform)
    {
        if (hitTransform.CompareTag("WaterSource"))
        {
            Spawner waterSpawner = hitTransform.GetComponentInChildren<Spawner>();
            waterSpawner.Toggle();
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
                    // if(pail.getWaterInside() >= GetComponent<NPCStateMachine>().ongoingFire.intensityValue)
                    // {
                    //     // GO TO FIRE
                    // }
                    // else
                    // {
                    //     // GO TO FAUCET
                    // }
                    
                    pail.closeProximityValue = closeProximityValue;
                    pail.playerCamera = transform;
                    hitTransform.SetPositionAndRotation(
                        transform.position + transform.forward + transform.right * 0.7f - transform.up * 0.15f, 
                        transform.rotation);
                }
                else if (extinguisher)
                {
                    // GO TO FIRE

                    if (!extinguisher.isPinPulled)
                        extinguisher.isPinPulled = true;

                    hitTransform.SetPositionAndRotation(
                        transform.position + transform.forward + transform.right * 0.7f - transform.up * 0.15f, 
                        transform.rotation);
                }
                else if (nonFlammable)
                {
                    // GO TO FIRE

                    hitTransform.SetPositionAndRotation(transform.position + transform.forward, transform.rotation);
                }
            }
        }
    }
}
