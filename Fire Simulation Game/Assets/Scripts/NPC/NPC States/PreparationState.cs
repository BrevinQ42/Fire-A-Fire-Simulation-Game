using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PreparationState : BaseState
{
    private NPC npc;
    private float speed;

    private Transform nearestObject;

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

        houseExit = null;

        if (npc.heldObject)
        {
            nearestObject = GetNearestWaterSource();
         
            if (Vector3.Distance(nearestObject.position, npc.transform.position) <= 1.25f)
            {
                npc.InteractWithObject(nearestObject);
                return;
            }

            if (nearestObject.GetComponent<ObjectNamePopUp>().isOutside && !npc.currentLocation.name.Equals("Outside Floor"))
            {
                npc.SetStoppingDistance(0.01f);

                houseExit = npc.currentLocation.GetChild(1).GetChild(0);
                npc.GoTo(houseExit.position, speed);
            }
            else
            {
                npc.SetStoppingDistance(1.25f);
                npc.GoTo(nearestObject.position, speed);
            }
        }
        else
        {
            nearestObject = GetNearestObject();

            if (nearestObject.GetComponent<FireFightingObject>().isOutside && !npc.currentLocation.name.Equals("Outside Floor"))
            {
                npc.SetStoppingDistance(0.01f);

                houseExit = npc.currentLocation.GetChild(1).GetChild(0);
                npc.GoTo(houseExit.position, speed);
            }
            else
            {
                npc.SetStoppingDistance(2.0f);
                npc.GoTo(nearestObject.position, speed);
            }            
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

        if (npc.hasReachedTarget())
        {
            if (houseExit)
            {
                houseExit = null;

                if (nearestObject.GetComponent<FireFightingObject>())
                    npc.SetStoppingDistance(2.0f);
                else
                    npc.SetStoppingDistance(1.25f);

                int areaMask = NavMesh.AllAreas;
                areaMask -= 1 << NavMesh.GetAreaFromName("Walkable");
                npc.agent.areaMask = areaMask;

                npc.GoTo(nearestObject.position, speed);

                return;
            }

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

                if (nearestObject.GetComponent<ObjectNamePopUp>().isOutside && !npc.currentLocation.name.Equals("Outside Floor"))
                {
                    npc.SetStoppingDistance(0.01f);

                    houseExit = npc.currentLocation.GetChild(1).GetChild(0);
                    npc.GoTo(houseExit.position, speed);
                }
                else
                {
                    npc.SetStoppingDistance(1.25f);
                    npc.GoTo(nearestObject.position, speed);

                    houseExit = null;
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

        foreach(FireFightingObject obj in npc.currentLocation.parent.GetChild(5).GetComponentsInChildren<FireFightingObject>())
        {
            if (npc.blacklist.Contains(obj.GetObjectType()) || obj.transform.parent.GetComponent<PlayerController>())
            {
                if (obj.GetObjectType().Equals("NonFlammableObject"))
                    NonFlammable = obj.transform;

                continue;
            }

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

        foreach(ObjectNamePopUp obj in npc.currentLocation.parent.GetChild(5).GetComponentsInChildren<ObjectNamePopUp>())
        {
            if (obj.CompareTag("WaterSource"))
            {
                float newDistance = Vector3.Distance(obj.transform.position, npc.transform.position);

                if (minDistance == -1.0f || newDistance < minDistance)
                {
                    nearest = obj.transform;
                    minDistance = newDistance;
                }
            }
        }

        return nearest;
    }
}
