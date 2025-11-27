using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EvacuateState : BaseState
{
    private NPC npc;
    private float speed;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        // if npc is not panicking, walking speed
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

        npc.GoTo(npc.evacuationLocation.position, speed);
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        if (npc.hasReachedTarget())
        {
            return;
        }
        
        if (npc.FireOnNPC != null)
        {
            npc.lastState = this;
            stateMachine.SwitchState(stateMachine.rollState);
        }

        // if npc has stopped and has not yet reached the evacuation spot
        if (npc.isHalted() && npc.currentLocation != npc.evacuationLocation && !npc.currentLocation.name.Equals("Outside Floor"))
        {
            // if they did not try fighting the fire before, 25% chance of now trying
            if (!npc.hasFailedFireFighting && Random.Range(0, 4) == 0)
                stateMachine.SwitchState(stateMachine.preparationState);
        }
    }
}
