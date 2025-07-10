using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvacuateState : BaseState
{
    private NPC npc;
    private string target;
    private List<Node> path;
    private float speed;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        target = "Court";
        path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);

        if (Random.Range(0, 2) == 0)
            speed = npc.walkingSpeed;
        else
            speed = npc.runningSpeed;
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        List<Node> newPath;

        if (npc.followPath(path, target, speed, false, out newPath))
        {
            if (npc.getCurrentNode().name.Equals("EvacuationNode"))
            {
                Debug.Log("NPC has evacuated");
                path = new List<Node>{npc.getCurrentNode()};
            }
            else
            {
                Debug.Log("NPC is stuck");
            }
        }

        if (newPath.Count > 0) path = newPath;
    }
}
