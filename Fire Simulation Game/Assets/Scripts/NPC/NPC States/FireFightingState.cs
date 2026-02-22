using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFightingState : BaseState
{
    private NPC npc;

    private FireExtinguisher lastHeldExtinguisher;
    private bool isExtinguisherEffective;

    private float proximity_value;

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

        lastHeldExtinguisher = null;
        isExtinguisherEffective = false;

        proximity_value = stateMachine.ongoingFire.intensityValue / 2.0f;

        npc.SetStoppingDistance(2.0f);
        npc.GoTo(stateMachine.ongoingFire.transform.position, npc.currentSpeed);
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        // if fire got extinguished,
        if (stateMachine.ongoingFire == null)
        {
            // drop any held object
            if (npc.heldObject)
            {
                npc.heldObject.Deattach();
                npc.heldObject = null;
                npc.isHoldingObject = false;
            }

            // proceed to evacuation
            stateMachine.SwitchState(stateMachine.evacuateState);
        }

        // else, if current extinguisher is effective, and npc is not on fire
        else if (isExtinguisherEffective && npc.FireOnNPC == null)
        {
            // continue using extinguisher

            bool isStillHeld;
            npc.heldObject.Use(npc.throwForce, out isStillHeld);
            npc.heldObject.GetComponent<FireExtinguisher>().isBeingUsed = true;

            return;
        }

        // if on fire, PANIC
        if (npc.FireOnNPC != null)
        {
            // drop any held object
            if (npc.heldObject)
            {
                npc.heldObject.Deattach();
                npc.heldObject = null;
                npc.isHoldingObject = false;
            }

            stateMachine.SwitchState(stateMachine.panicState);
        }

        proximity_value = Mathf.Max(stateMachine.ongoingFire.intensityValue / 2.0f + stateMachine.ongoingFire.growingSpeed * 5.0f, 3.5f);

        bool isUsingExtinguisher = false;

        Debug.Log(Vector3.Distance(stateMachine.ongoingFire.transform.position, npc.position) + " vs " +
                    proximity_value);

        if (isFireWithinRange(stateMachine.ongoingFire))
        {
            npc.ResetBuffer();

            Vector3 firePos = stateMachine.ongoingFire.transform.position;
            npc.transform.LookAt(new Vector3(firePos.x, npc.transform.position.y, firePos.z));

            bool isStillHeld;
            npc.heldObject.Use(npc.throwForce, out isStillHeld);

            if (npc.heldObject.GetComponent<FireExtinguisher>())
            {
                npc.heldObject.GetComponent<FireExtinguisher>().isBeingUsed = true;
                isUsingExtinguisher = true;
            }

            if (stateMachine.ongoingFire == null)
            {
                if (npc.heldObject.GetComponent<FireExtinguisher>())
                    npc.heldObject.GetComponent<FireExtinguisher>().isBeingUsed = false;

                npc.heldObject.Deattach();
                npc.heldObject = null;
                npc.isHoldingObject = false;

                stateMachine.SwitchState(stateMachine.evacuateState);
            }
            else
            {
                Pail bucket = npc.heldObject.GetComponent<Pail>();

                if (!isStillHeld) // if not held anymore, it is the non-flammable object
                {
                    // prevent spam of non-flammable object
                    npc.blacklist.Add(npc.heldObject.GetObjectType());
                    npc.heldObject = null;

                    stateMachine.SwitchState(stateMachine.preparationState);
                }
                else
                {
                    Fire fire = stateMachine.ongoingFire;

                    if ( (bucket && !fire.EffectivityTable[fire.type].Equals("Class A")) ||
                         (!bucket && !fire.EffectivityTable[fire.type].Equals(npc.heldObject.GetComponent<FireExtinguisher>().type)) )
                    {
                        npc.blacklist.Add(npc.heldObject.GetObjectType());

                        if (bucket) // fire is most likely pretty big if they used a bucket full of water wrongly
                        {
                            npc.hasFailedFireFighting = true;
                            stateMachine.SwitchState(stateMachine.evacuateState);
                        }

                        npc.heldObject.Deattach();
                        npc.heldObject = null;
                        npc.isHoldingObject = false;
                        
                        stateMachine.SwitchState(stateMachine.preparationState);
                    }
                    else if (npc.heldObject.GetComponent<Pail>())
                        stateMachine.SwitchState(stateMachine.preparationState);
                    else if (npc.heldObject.GetComponent<FireExtinguisher>())
                        isExtinguisherEffective = true;
                }
            }
        }
        else npc.StuckCheck();

        if (!isUsingExtinguisher)
        {
            if (lastHeldExtinguisher && lastHeldExtinguisher.transform.parent && npc.heldObject && 
                lastHeldExtinguisher == npc.heldObject.GetComponent<FireExtinguisher>())
            {
                lastHeldExtinguisher.isBeingUsed = false;
            }
        }
    }

    bool isFireWithinRange(Fire fire)
    {
        Vector3 forwardVector = fire.transform.position - npc.position;

        if (forwardVector.magnitude - fire.intensityValue / 2.0f > proximity_value)
            return false;

        if (!Physics.Raycast(npc.position, forwardVector, forwardVector.magnitude - fire.intensityValue / 2.0f))
            return true;

        Debug.DrawRay(npc.position, forwardVector, Color.yellow);

        Vector3 forward2DVector = new Vector3(forwardVector.x, 0.0f, forwardVector.z);

        if (!Physics.Raycast(npc.position, forward2DVector, forward2DVector.magnitude - fire.intensityValue / 2.0f))
            return true;

        Debug.DrawRay(npc.position, forward2DVector, Color.yellow);
        return false;
    }
}
