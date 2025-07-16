using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("Graph Setup")]
    public List<Node> adjacentNodes;
    public List<bool> isEdgeValid;
    public List<float> edgeWeights;

    [Header("A* Components")]
    public int floorLevel;
    public float cost;
    public float heuristic;
    public Node previousNode;

    // Start is called before the first frame update
    void Start()
    {
        initValues();

        try
        {
            foreach(Node node in adjacentNodes)
            {
                edgeWeights.Add(0.0f);
            }
        }
        catch {}
    }

    void Update()
    {
        if (transform.position.y > 5.0f)
            floorLevel = 2;
        else
            floorLevel = 1;
    }

    public void initValues()
    {
        cost = float.MaxValue;
        heuristic = float.MaxValue;
    }

    public void addNodes(List<Node> nodes)
    {
        foreach(Node node in nodes)
        {
            if (!adjacentNodes.Contains(node))
            {
                adjacentNodes.Add(node);
                isEdgeValid.Add(true);
                edgeWeights.Add(0.0f);
            }

            if (!node.adjacentNodes.Contains(this))
            {
                node.adjacentNodes.Add(this);
                node.isEdgeValid.Add(true);
                node.edgeWeights.Add(0.0f);
            }
        }
    }

    public void updateCost(float newCost, Node cameFrom)
    {
        if (cost == float.MaxValue || cost > newCost)
        {
            cost = newCost;
            previousNode = cameFrom;
        }
    }

    // private void OnDrawGizmos()
    // {
    //     if(adjacentNodes != null && adjacentNodes.Count > 0)
    //     {
    //         for(int i = 0; i < adjacentNodes.Count; i++)
    //         {
    //             if (isEdgeValid[i])
    //                 Gizmos.color = Color.blue;
    //             else
    //                 Gizmos.color = Color.yellow;

    //             Gizmos.DrawLine(transform.position, adjacentNodes[i].transform.position);
    //         }
    //     }
    // }
}
