using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertedState : BaseState
{
    private NPC npc;
    
    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        npc.currentSpeed = npc.walkingSpeed;

        npc.GoTo(stateMachine.ongoingFire.transform.position, npc.walkingSpeed);
        npc.SetStoppingDistance(Mathf.Min(stateMachine.ongoingFire.intensityValue / 2.0f, 2.5f));
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        if (npc.GetComponent<Collider>().enabled)
        {
            if (stateMachine.ongoingFire == null)
                stateMachine.SwitchState(stateMachine.evacuateState);

            npc.SetStoppingDistance(Mathf.Min(stateMachine.ongoingFire.intensityValue / 2.0f, 2.5f));
            
            npc.StuckCheck();

            // if on fire, PANIC
            if (npc.FireOnNPC != null)
                stateMachine.SwitchState(stateMachine.panicState);

            if (canNpcSeeFire(stateMachine.ongoingFire))
            {
                if (Random.Range(0, 2) == 0)
                {
                    npc.isPanicking = true;
                    stateMachine.SwitchState(stateMachine.panicState);
                }
                else
                    stateMachine.SwitchState(stateMachine.preparationState);
            }
        }
    }

    private bool canNpcSeeFire(Fire fire)
    {
        if (fire == null)
            return false;

        Vector3 forwardVector = fire.transform.position - npc.position;
        if (!Physics.Raycast(npc.position, forwardVector, forwardVector.magnitude - fire.intensityValue / 2.0f))
            return true;

        Debug.DrawRay(npc.position, forwardVector, Color.yellow);
        
        Vector3 dirToHighestPoint = forwardVector + Vector3.up * fire.intensityValue * 2.0f;
        if (!Physics.Raycast(npc.position, dirToHighestPoint, dirToHighestPoint.magnitude))
            return true;

        Debug.DrawRay(npc.position, dirToHighestPoint, Color.yellow);
        return false;
    }
}
