using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    private Pathfinder pathfinder;
    [SerializeField] private Node currentNode;

    [SerializeField] private List<Node> path;
    [SerializeField] private int pathIndex;
    private string target;
    private int raycastLayerMask;

    [SerializeField] private Node exitNode;
    [SerializeField] private Door door;

    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;

    // Start is called before the first frame update
    void Start()
    {
        pathfinder = GetComponent<Pathfinder>();

        path = new List<Node>();
        pathIndex = 0;
        target = "";

        raycastLayerMask = LayerMask.GetMask("Default", "Ignore Raycast", "TransparentFX", "Water", "UI");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            target = "Court";
            path = pathfinder.generatePath(currentNode, target);
        }

        if (pathIndex < path.Count)
        {
            Vector3 direction = path[pathIndex].transform.position - transform.position;

            if (Physics.Raycast(transform.position, Vector3.Normalize(direction), direction.magnitude, raycastLayerMask))
            {
                pathfinder.SetEdgeInvalid(currentNode, path[pathIndex]);

                List<Node> newPath = pathfinder.generatePath(currentNode, target);

                direction = newPath[0].transform.position - transform.position;

                if (transform.position != currentNode.transform.position &&
                    Physics.Raycast(transform.position, Vector3.Normalize(direction), direction.magnitude, raycastLayerMask))
                {
                    path = new List<Node>{currentNode};
                    path.AddRange(newPath);
                }
                else
                    path = newPath;

                pathIndex = 0;
            }
            
            transform.position = Vector3.MoveTowards(transform.position, path[pathIndex].transform.position,
                                    walkingSpeed * Time.deltaTime);

            if (transform.position == path[pathIndex].transform.position)
            {
                if (path[pathIndex] == exitNode && !door.isOpen) door.toggleDoor();

                currentNode = path[pathIndex];

                pathIndex++;
            }
        }
    }
}
