using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    [SerializeField] private List<Node> fireFightingNodes;
    [SerializeField] private List<Node> waterSourceNodes;

    [SerializeField] private Node bottomOfStairs;
    [SerializeField] private Node topOfStairs;
    [SerializeField] private Node exitNode;
    [SerializeField] private Node courtNode;

    void Start()
    {
        fireFightingNodes = new List<Node>();
        waterSourceNodes = new List<Node>();
    }

    public void populateObjectNodes()
    {
        if (fireFightingNodes.Count == 0 && waterSourceNodes.Count == 0)
        {
            foreach(FireFightingObject obj in FindObjectsOfType<FireFightingObject>())
            {
                if (!obj.GetComponent<Foam>())
                    fireFightingNodes.Add(obj.GetComponent<Node>());
            }

            foreach(GameObject obj in GameObject.FindGameObjectsWithTag("WaterSource"))
                waterSourceNodes.Add(obj.GetComponent<Node>());

            foreach(Node node in FindObjectsOfType<Node>())
            {
                if (!node.GetComponent<FireFightingObject>() && !node.transform.CompareTag("WaterSource") &&
                    !node.transform.parent.name.Equals("Outside"))
                {
                    node.addNodes(fireFightingNodes);
                    node.addNodes(waterSourceNodes);
                    node.initValues();
                }
            }
        }
    }

    public List<Node> generatePath(Node current, string target)
    {
        if (target.Equals("Court"))
        {
            List<Node> path = generatePathToTarget(current, exitNode);

            if (path.Count > 0)
            {
                foreach(Node node in generatePathToTarget(exitNode, courtNode))
                    path.Add(node);
            }

            return path;
        }
        else if (target.Equals("FireFightingObject") || target.Equals("WaterSource"))
        {
            Node closest = getClosestNode(current, target);
            List<Node> path = generatePathToTarget(current, closest);

            return path;
        }
        else if (target.Equals("Fire"))
        {
            Node fireNode = GetComponent<NPCStateMachine>().ongoingFire.GetComponent<Node>();

            return generatePathToTarget(current, fireNode);
        }

        return new List<Node>();
    }

    private List<Node> generatePathToTarget(Node current, Node target)
    {
        foreach(Node node in FindObjectsOfType<Node>())
            node.initValues();


        List<Node> path = new List<Node>();
        
        current.cost = 0;
        current.heuristic = GetDistance(current, target);

        List<Node> candidates = new List<Node>{current};

        while (candidates.Count > 0)
        {
            Node nextNode = null;

            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].GetComponent<FireFightingObject>())
                {
                    if (!candidates[i].GetComponent<Collider>().enabled ||
                        !target.GetComponent<FireFightingObject>())
                        continue;
                }
                else if (candidates[i].transform.CompareTag("WaterSource") && !target.transform.CompareTag("WaterSource"))
                    continue;
                else if (candidates[i].GetComponent<Fire>() && !target.GetComponent<Fire>())
                    continue;

                if (nextNode == null)
                    nextNode = candidates[i];
                else if (candidates[i].heuristic < nextNode.heuristic)
                    nextNode = candidates[i];
            }

            if (nextNode == target)
            {
                Node node = target;

                while (node != current)
                {
                    path.Add(node);
                    node = node.previousNode;
                }

                path.Reverse();
                return path;
            }
            else
            {
                candidates.Remove(nextNode);

                try
                {
                    for (int i = 0; i < nextNode.adjacentNodes.Count; i++)
                    {
                        if (nextNode.isEdgeValid[i])
                        {
                            Node node = nextNode.adjacentNodes[i];
                            float newCost = nextNode.cost + GetDistance(nextNode, node);

                            if (newCost < node.cost)
                            {
                                node.updateCost(newCost, nextNode);
                                node.heuristic = node.cost + GetDistance(node, target);

                                if (!candidates.Contains(node))
                                    candidates.Add(node);
                            }
                        }
                    }
                }
                catch { continue; }
            }
        }

        return new List<Node>();
    }

    private Node getClosestNode(Node current, string type)
    {
        Node closestNode = null;

        if (type.Equals("FireFightingObject"))
        {
            closestNode = null;
            float distance = -1.0f;

            for (int i = 0; i < fireFightingNodes.Count; i++)
            {
                if (fireFightingNodes[i].transform.GetComponent<FireFightingObject>())
                {
                    if (!fireFightingNodes[i].transform.GetComponent<Collider>().enabled)
                        continue;
                }

                float newDistance = GetDistance(current, fireFightingNodes[i]);

                if (closestNode == null || newDistance < distance)
                {
                    distance = newDistance;
                    closestNode = fireFightingNodes[i];
                }
            }
        }
        else if (type.Equals("WaterSource"))
        {
            closestNode = null;
            float distance = -1.0f;

            for (int i = 0; i < waterSourceNodes.Count; i++)
            {
                float newDistance = GetDistance(current, waterSourceNodes[i]);

                if (closestNode == null || newDistance < distance)
                {
                    distance = newDistance;
                    closestNode = waterSourceNodes[i];
                }
            }
        }

        return closestNode;
    }

    private float GetDistance(Node start, Node target)
    {
        Vector2 startPos = new Vector2(start.transform.position.x, start.transform.position.z);
        Vector2 targetPos = new Vector2(target.transform.position.x, target.transform.position.z);

        if (start.floorLevel == target.floorLevel)
            return Vector2.Distance(startPos, targetPos);

        
        Vector2 bottomPos = new Vector2(bottomOfStairs.transform.position.x, bottomOfStairs.transform.position.z);
        Vector2 topPos = new Vector2(topOfStairs.transform.position.x, topOfStairs.transform.position.z);

        if (start.floorLevel == 1)
        {
            return Vector2.Distance(startPos, bottomPos) + Vector2.Distance(bottomPos, topPos) +
                    Vector2.Distance(topPos, targetPos);
        }
        else
        {
            return Vector2.Distance(startPos, topPos) + Vector2.Distance(topPos, bottomPos) +
                    Vector2.Distance(bottomPos, targetPos);
        }
    }

    public void SetEdgeInvalid(Node node1, Node node2)
    {
        int i = node1.adjacentNodes.IndexOf(node2);
        if (i > -1)
            node1.isEdgeValid[i] = false;

        i = node2.adjacentNodes.IndexOf(node1);
        if (i > -1)
            node2.isEdgeValid[i] = false;
    }


    public Node getExitNode()
    {
        return exitNode;
    }
}
