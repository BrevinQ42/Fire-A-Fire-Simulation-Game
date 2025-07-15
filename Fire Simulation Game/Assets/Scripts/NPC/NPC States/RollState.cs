using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollState : BaseState
{
    private NPC npc;
    private string target;
    private List<Node> path;
    private float speed;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        target = "Roll";
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
            if (npc.getCurrentNode().name.Equals("RollNode"))
            {
                //play roll animation
                npc.NPCAnimator.SetBool("isRolling", true);
            }
            else
            {
                Debug.Log("NPC is stuck");
            }
        }
    }
}
