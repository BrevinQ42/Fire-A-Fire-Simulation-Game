using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PreparationState : BaseState
{
    private NPC npc;
    private float speed;

    private Transform nearestObject;

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

        if (npc.heldObject)
        {
            nearestObject = GetNearestWaterSource();
         
            if (Vector3.Distance(nearestObject.position, npc.transform.position) <= 1.25f)
            {
                npc.InteractWithObject(nearestObject);
                return;
            }

            npc.SetStoppingDistance(1.25f);
            npc.GoTo(nearestObject.position, speed);
        }
        else
        {
            nearestObject = GetNearestObject();

            npc.SetStoppingDistance(2.0f);
            npc.GoTo(nearestObject.position, speed);            
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

        if ( nearestObject.parent &&
            (nearestObject.GetComponent<PlayerController>() || nearestObject.GetComponent<NPC>()) )
        {
            nearestObject = GetNearestObject();
        }

        if (npc.hasReachedTarget() || canSeeObject(nearestObject) ||
            Vector3.Distance(nearestObject.position, npc.position) <= 4.0f)
        {
            npc.InteractWithObject(nearestObject);

            if (!npc.heldObject && nearestObject.GetComponent<Pail>())
            {
                npc.heldObject = nearestObject.GetComponent<FireFightingObject>();
                npc.isHoldingObject = true;

                npc.SetStoppingDistance(1.25f);
                
                nearestObject = GetNearestWaterSource();

                if (Vector3.Distance(nearestObject.position, npc.transform.position) <= 1.25f)
                {
                    npc.InteractWithObject(nearestObject);
                    return;
                }

                npc.GoTo(nearestObject.position, speed);
            }
            else if (nearestObject.CompareTag("WaterSource"))
            {
                Transform faucet = nearestObject.transform;
                Pail pail = npc.heldObject.GetComponent<Pail>();

                if (pail.getWaterInside() > stateMachine.ongoingFire.intensityValue ||
                    pail.getFractionFilled() >= 1.0f)
                {
                    faucet.GetComponentInChildren<Spawner>().Toggle(false);

                    pail.transform.SetParent(npc.transform);

                    pail.transform.SetPositionAndRotation(
                        npc.transform.position + npc.transform.forward * 0.5f + npc.transform.right * 0.3f, 
                        npc.transform.rotation);

                    stateMachine.SwitchState(stateMachine.fireFightingState);
                }
                else if (pail.transform.parent != faucet)
                {
                    pail.transform.SetParent(faucet);
                    pail.transform.SetLocalPositionAndRotation(new Vector3(1.15f, -4.0f, 0.0f),
                                                                        Quaternion.identity);
                }
            }
            else
            {
                npc.heldObject = nearestObject.GetComponent<FireFightingObject>();

                npc.blacklist.Remove("NonFlammableObject");
                stateMachine.SwitchState(stateMachine.fireFightingState);
            }
        }
    }

    private Transform GetNearestObject()
    {
        Transform nearest = null;
        float minDistance = -1.0f;

        Transform NonFlammable = null;

        foreach(GameObject thing in GameObject.FindGameObjectsWithTag("Grabbable"))
        {
            if (thing.GetComponent<ElectricPlug>()) continue;

            FireFightingObject obj = thing.GetComponent<FireFightingObject>();
            if (obj == null) continue;

            if (npc.blacklist.Contains(obj.GetObjectType()))
            {
                if (obj.GetObjectType().Equals("NonFlammableObject"))
                    NonFlammable = obj.transform;

                continue;
            }

            if (obj.transform.parent && (obj.transform.parent.GetComponent<PlayerController>() || obj.transform.parent.GetComponent<NPC>()))
                continue;

            float newDistance = Vector3.Distance(obj.transform.position, npc.transform.position);

            if (minDistance == -1.0f || newDistance < minDistance)
            {
                nearest = obj.transform;
                minDistance = newDistance;
            }
        }

        if (nearest == null) return NonFlammable;

        return nearest;
    }

    private Transform GetNearestWaterSource()
    {
        Transform nearest = null;
        float minDistance = -1.0f;

        foreach(GameObject thing in GameObject.FindGameObjectsWithTag("WaterSource"))
        {
            ObjectNamePopUp obj = thing.GetComponent<ObjectNamePopUp>();

            float newDistance = Vector3.Distance(obj.transform.position, npc.transform.position);

            if (minDistance == -1.0f || newDistance < minDistance)
            {
                nearest = obj.transform;
                minDistance = newDistance;
            }
        }

        return nearest;
    }

    bool canSeeObject(Transform obj)
    {
        Vector3 forwardVector = obj.transform.position - npc.position;

        if (forwardVector.magnitude > 1.25f)
            return false;

        int layerMask =~ LayerMask.GetMask("Ignore Navmesh");

        return !Physics.Raycast(npc.position, forwardVector, forwardVector.magnitude, layerMask);
    }
}
