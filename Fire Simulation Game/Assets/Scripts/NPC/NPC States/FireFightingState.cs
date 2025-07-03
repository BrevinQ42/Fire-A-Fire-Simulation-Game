using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFightingState : BaseState
{
    private NPC npc;
    private string target;
    private List<Node> path;
    private float speed;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        target = "FireFightingObject";
        path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);

        if (Random.Range(0, 2) == 0)
            speed = npc.walkingSpeed;
        else
            speed = npc.runningSpeed;
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        List<Node> newPath;

        if (npc.followPath(path, target, speed, true, out newPath))
        {
            if (npc.transform.childCount == 1)
            {
                Debug.Log("NPC got item");

                stateMachine.SwitchState(stateMachine.evacuateState);
            }
            else
                Debug.Log("NPC failed");
        }
        
        if (newPath.Count > 0) path = newPath;
    }
}
