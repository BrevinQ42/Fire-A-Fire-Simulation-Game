using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EvacuateState : BaseState
{
    private NPC npc;
    private float speed;
    private Transform houseExit;

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

        if (npc.currentLocation.name.Equals("Outside Floor"))
        {
            houseExit = null;
            npc.GoTo(npc.evacuationLocation.position, speed);
        }
        else
        {
            houseExit = npc.currentLocation.GetChild(1).GetChild(0);
            npc.GoTo(houseExit.position, speed);
        }
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        if (npc.FireOnNPC != null)
        {
            npc.lastState = this;
            stateMachine.SwitchState(stateMachine.rollState);
        }

        if (npc.hasReachedTarget() && houseExit &&
            Vector3.Distance(npc.transform.position, houseExit.position) <= 0.1f)
        {
            int areaMask = NavMesh.AllAreas;
            areaMask -= 1 << NavMesh.GetAreaFromName("Walkable");
            npc.agent.areaMask = areaMask;

            npc.GoTo(npc.evacuationLocation.position, speed);
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
