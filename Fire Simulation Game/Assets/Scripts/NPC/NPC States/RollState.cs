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
        {
            speed = npc.walkingSpeed;
            npc.isRunning = false;
        }
        else
        {
            speed = npc.runningSpeed;
            npc.isRunning = true;
        }
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
                npc.isRolling = true;
            }
            else if (npc.getCurrentNode().name.Equals("BottomNode"))
                path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);
            else
                Debug.Log("NPC is stuck");
        }

        if (npc.FireOnNPC == null && npc.fireOnDoor != true)
        {
            npc.isRolling = false;
            npc.NPCAnimator.SetBool("isRolling", false);
            if (npc.coroutinePlaying == false)
            {
                stateMachine.SwitchState(stateMachine.evacuateState);
            }
        }
        else if (npc.FireOnNPC == null && npc.fireOnDoor == true) // If Fire is On Door, go back to fire fighting
        {
            npc.isRolling = false;
            npc.NPCAnimator.SetBool("isRolling", false);
            if (npc.coroutinePlaying == false)
            {
                stateMachine.SwitchState(stateMachine.fireFightingState);
            }
        }
    }
}
