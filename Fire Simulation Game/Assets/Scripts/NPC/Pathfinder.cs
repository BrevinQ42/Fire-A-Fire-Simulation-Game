using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    [SerializeField] private List<Node> firstFloorPathNodes;
    [SerializeField] private List<Node> secondFloorPathNodes;
    [SerializeField] private List<Node> fireFightingNodes;
    [SerializeField] private List<Node> waterSourceNodes;

    [SerializeField] private Node bottomOfStairs;
    [SerializeField] private Node topOfStairs;
    [SerializeField] private Node exitNode;
    [SerializeField] private Node courtNode;
    [SerializeField] private Node rollNode;

    public bool justUsedStairs;

    void Start()
    {
        firstFloorPathNodes = new List<Node>();
        secondFloorPathNodes = new List<Node>();
        foreach(Node node in FindObjectsOfType<Node>())
        {
            if (!node.GetComponent<FireFightingObject>() && !node.transform.CompareTag("WaterSource") &&
                !node.transform.parent.name.Equals("Outside"))
            {
                if (node.floorLevel == 1) firstFloorPathNodes.Add(node);
                else secondFloorPathNodes.Add(node);
            }
        }

        fireFightingNodes = new List<Node>();
        waterSourceNodes = new List<Node>();

        justUsedStairs = false;
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
        else if (target.Equals("WaterSource"))
        {
            Node closest = getClosestNode(current, target, new HashSet<Node>());

            return generatePathToTarget(current, closest);
        }
        else if (target.Equals("Fire"))
        {
            Node fireNode = GetComponent<NPCStateMachine>().ongoingFire.GetComponent<Node>();

            return generatePathToTarget(current, fireNode);
        }
        else if (target.Equals("Roll"))
        {
            List<Node> path = generatePathToTarget(current, rollNode);
            return path;
        }
        else if (target.Equals("Any"))
        {
            int index = -1;
            Node targetNode = null;

            if (current.floorLevel == 1)
                firstFloorPathNodes.Remove(current);
            else
                secondFloorPathNodes.Remove(current);

            if (justUsedStairs)
            {
                if (current == topOfStairs)
                {
                    index = Random.Range(0, secondFloorPathNodes.Count);
                    targetNode = secondFloorPathNodes[index];

                    secondFloorPathNodes.Add(current);
                }
                else // if bottomOfStairs
                {
                    index = Random.Range(0, firstFloorPathNodes.Count);
                    targetNode = firstFloorPathNodes[index];

                    firstFloorPathNodes.Add(current);
                }

                justUsedStairs = false;

                return generatePathToTarget(current, targetNode);
            }

            index = Random.Range(0, firstFloorPathNodes.Count + secondFloorPathNodes.Count);
            if (index >= firstFloorPathNodes.Count)
                targetNode = secondFloorPathNodes[index-firstFloorPathNodes.Count];
            else
                targetNode = firstFloorPathNodes[index];

            if (current.floorLevel == 1)
                firstFloorPathNodes.Add(current);
            else
                secondFloorPathNodes.Add(current);

            return generatePathToTarget(current, targetNode);
        }

        return new List<Node>();
    }

    public List<Node> generatePath(Node current, string target, HashSet<Node> blacklist)
    {
        if (target.Equals("FireFightingObject"))
        {
            Node closest = getClosestNode(current, target, blacklist);

            if (closest == null) return new List<Node>();

            return generatePathToTarget(current, closest);
        }

        return new List<Node>();
    }

    private List<Node> generatePathToTarget(Node current, Node targetNode)
    {
        Node target = targetNode;
        Node additionalNode = null;

        if (current.floorLevel != targetNode.floorLevel)
        {
            justUsedStairs = true;

            if (current.floorLevel == 1)
            {
                target = bottomOfStairs;
                additionalNode = topOfStairs;
            }
            else
            {
                target = topOfStairs;
                additionalNode = bottomOfStairs;
            }
        }

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
                        !target.GetComponent<FireFightingObject>() ||
                        GetComponent<NPC>().blacklist.Contains(candidates[i]))
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

                if (additionalNode != null)
                    path.Add(additionalNode);
                    
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

    private Node getClosestNode(Node current, string type, HashSet<Node> blacklist)
    {
        Node closestNode = null;

        if (type.Equals("FireFightingObject"))
        {
            closestNode = null;
            float distance = -1.0f;

            for (int i = 0; i < fireFightingNodes.Count; i++)
            {
                if (!fireFightingNodes[i].transform.GetComponent<Collider>().enabled ||
                    blacklist.Contains(fireFightingNodes[i]))
                    continue;

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
