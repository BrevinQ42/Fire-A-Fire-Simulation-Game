using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EvacuateState : BaseState
{
    private NPC npc;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        // if npc is not panicking, walking speed
        // if npc is panicking, 25% chance of calming down (walking speed)
        if (!npc.isPanicking || Random.Range(0, 4) == 0)
        {
            npc.currentSpeed = npc.walkingSpeed;
            npc.isRunning = false;
            npc.isPanicking = false;
        }
        else
        {
            npc.currentSpeed = npc.runningSpeed;
            npc.isRunning = true;
        }

        npc.GoTo(npc.evacuationLocation.position, npc.currentSpeed);
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        if (npc.hasReachedTarget())
        {
            npc.isRunning = false;
            return;
        }

        if (npc.FireOnNPC != null){
            stateMachine.SwitchState(stateMachine.panicState);
        }

        npc.StuckCheck();

        // if npc has stopped and has not yet reached the evacuation spot
        if (npc.isHalted() && npc.currentLocation != npc.evacuationLocation && !npc.currentLocation.name.Equals("Outside Floor"))
        {
            // if they did not try fighting the fire before, 25% chance of now trying
            if (!npc.hasFailedFireFighting && Random.Range(0, 4) == 0)
                stateMachine.SwitchState(stateMachine.preparationState);
        }
    }
}
