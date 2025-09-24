using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFightingState : BaseState
{
    private NPC npc;
    private float speed;

    private Transform houseExit;

    private FireExtinguisher lastHeldExtinguisher;
    private bool isExtinguisherEffective;

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

        lastHeldExtinguisher = null;
        isExtinguisherEffective = false;

        if (npc.currentLocation.name.Equals("Outside Floor"))
        {
            houseExit = npc.lastHouseLocation.GetChild(1).GetChild(0);
            npc.SetStoppingDistance(0.01f);
            npc.GoTo(houseExit.position, speed);
        }
        else
        {
            houseExit = null;
            npc.SetStoppingDistance(stateMachine.ongoingFire.intensityValue);
            npc.GoTo(stateMachine.ongoingFire.transform.position, speed);
        }
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

        // if on fire, ROLL
        if (npc.FireOnNPC != null)
        {
            // drop any held object
            if (npc.heldObject)
            {
                npc.heldObject.Deattach();
                npc.heldObject = null;
                npc.isHoldingObject = false;
            }

            npc.lastState = this;
            stateMachine.SwitchState(stateMachine.rollState);
        }

        bool isUsingExtinguisher = false;

        if (npc.hasReachedTarget())
        {
            if (houseExit)
            {
                npc.SetStoppingDistance(stateMachine.ongoingFire.intensityValue);
                npc.GoTo(stateMachine.ongoingFire.transform.position, speed);
                houseExit = null;
                return;
            }

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
                            stateMachine.SwitchState(stateMachine.evacuateState);

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

        if (!isUsingExtinguisher)
        {
            if (lastHeldExtinguisher && lastHeldExtinguisher.transform.parent && npc.heldObject && 
                lastHeldExtinguisher == npc.heldObject.GetComponent<FireExtinguisher>())
            {
                lastHeldExtinguisher.isBeingUsed = false;
            }
        }
    }
}
