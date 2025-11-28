using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertedState : BaseState
{
    private NPC npc;
    private float speed;

    private int layerMask;
    
    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        npc.GoTo(stateMachine.ongoingFire.transform.position, npc.walkingSpeed);
        npc.SetStoppingDistance(Mathf.Min(stateMachine.ongoingFire.intensityValue / 2.0f, 2.5f));

        layerMask =~ LayerMask.GetMask("Ignore Raycast");
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        if (npc.GetComponent<Collider>().enabled)
        {
            npc.SetStoppingDistance(Mathf.Min(stateMachine.ongoingFire.intensityValue / 2.0f, 2.5f));
            
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
        Vector3 forwardVector = fire.transform.position - npc.position;
        Vector3 dirToHighestPoint = forwardVector + Vector3.up * fire.intensityValue * 2.0f;

        bool noObstructionsFound = true;

        if (Physics.Raycast(npc.position, forwardVector, forwardVector.magnitude, layerMask))
        {
            Debug.DrawRay(npc.position, forwardVector, Color.red);
            noObstructionsFound = false;
        }
        else
        {
            Debug.DrawRay(npc.position, forwardVector, Color.yellow);
            noObstructionsFound = true;
        }

        if (Physics.Raycast(npc.position, dirToHighestPoint, dirToHighestPoint.magnitude, layerMask))
        {
            Debug.DrawRay(npc.position, dirToHighestPoint, Color.red);
            noObstructionsFound = noObstructionsFound || false;
        }
        else
        {
            Debug.DrawRay(npc.position, dirToHighestPoint, Color.yellow);
            noObstructionsFound = noObstructionsFound || true;
        }

        return noObstructionsFound;
    }
}
