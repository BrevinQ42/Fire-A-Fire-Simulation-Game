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

        // if npc is not panicking, walking speed
        // if npc is panicking, 25% chance of calming down (walking speed)
        if (!npc.isPanicking || Random.Range(0, 4) == 0)
        {
            speed = npc.walkingSpeed;
            npc.isRunning = false;
        }
        else
        {
            speed = npc.runningSpeed;
            npc.isRunning = true;
        }

        npc.pathfinder.justUsedStairs = false;
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        List<Node> newPath;

        if (npc.followPath(path, target, speed, false, out newPath))
        {
            if (npc.pathfinder.justUsedStairs)
                npc.pathfinder.justUsedStairs = false;
            else if (npc.getCurrentNode().name.Equals("EvacuationNode"))
            {
                Debug.Log("NPC has evacuated");
                path = new List<Node>{npc.getCurrentNode()};
            }
            else if (npc.getCurrentNode().name.Equals("BottomNode") ||
                        npc.getCurrentNode() == npc.pathfinder.getExitNode())
            {
                path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);
            }
            else
            {
                Debug.Log("NPC is stuck");
                stateMachine.SwitchState(stateMachine.panicState);
            }
        }
        else if (npc.FireOnNPC != null)
        {
            stateMachine.SwitchState(stateMachine.rollState);
        }

        if (newPath.Count > 0) path = newPath;
    }
}
