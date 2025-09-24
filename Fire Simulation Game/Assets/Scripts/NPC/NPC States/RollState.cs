using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollState : BaseState
{
    private NPC npc;
    private float speed;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;// if npc is not panicking, walking speed
        // if npc is panicking, 25% chance of calming down (walking speed)
        if (!npc.isPanicking || Random.Range(0, 4) == 0)
        {
            speed = npc.walkingSpeed;
            npc.isRunning = false;
            npc.isPanicking = false;
        }
        else
        {
            speed = npc.runningSpeed;
            npc.isRunning = true;
        }

        Vector3 offset = (npc.transform.position - stateMachine.ongoingFire.transform.position)
                            * stateMachine.ongoingFire.intensityValue;

        npc.GoTo(npc.transform.position + offset, speed);
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        if (npc.isHalted())
        {
            //play roll animation
            npc.NPCAnimator.SetBool("isRolling", true);
            npc.isRolling = true;
        }

        if (npc.FireOnNPC == null)
        {
            npc.isRolling = false;
            npc.NPCAnimator.SetBool("isRolling", false);
            
            if (npc.coroutinePlaying == false)
                stateMachine.SwitchState(npc.lastState);
        }
    }
}
