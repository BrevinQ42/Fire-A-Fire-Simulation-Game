using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollState : BaseState
{
    private NPC npc;
    private float speed;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        npc.GoTo(npc.transform.position, 0.0f);
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        //play roll animation
        npc.NPCAnimator.SetBool("isRolling", true);
        npc.isRolling = true;

        if (npc.FireOnNPC == null)
        {
            npc.isRolling = false;
            npc.NPCAnimator.SetBool("isRolling", false);
            
            if (npc.coroutinePlaying == false)
            {
                if ((npc.lastState == null || npc.lastState != stateMachine.evacuateState) &&
                    Random.Range(0f, 1f) <= 0.25f)
                {
                    stateMachine.SwitchState(stateMachine.preparationState);
                }
                else
                {
                    npc.lastState = null;
                    stateMachine.SwitchState(stateMachine.evacuateState);
                }
            }
        }
    }
}
