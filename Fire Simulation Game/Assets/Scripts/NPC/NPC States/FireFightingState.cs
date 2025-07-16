using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFightingState : BaseState
{
    private NPC npc;
    private string target;
    private List<Node> path;
    private float speed;

    private FireFightingObject heldObject;
    private FireExtinguisher lastHeldExtinguisher;
    private bool isExtinguisherEffective;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        target = "FireFightingObject";
        path = npc.pathfinder.generatePath(npc.getCurrentNode(), target, npc.blacklist);

        // if npc is not panicking, walking speed
        // if npc is panicking, 25% chance of calming down (walking speed)
        if (!npc.isPanicking || Random.Range(0, 4) == 0)
        {
            speed = npc.walkingSpeed;
            npc.isRunning = false;
        }
        else
        {
            speed = npc.runningSpeed;
            npc.isRunning = true;
        }

        heldObject = null;
        lastHeldExtinguisher = null;
        isExtinguisherEffective = false;

        npc.pathfinder.justUsedStairs = false;
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        if (stateMachine.ongoingFire == null)
        {
            if (heldObject != null)
            {
                heldObject.Deattach();
                heldObject = null;
                npc.isHoldingObject = false;
            }

            stateMachine.SwitchState(stateMachine.evacuateState);
        }
        else if (isExtinguisherEffective && npc.FireOnNPC == null)
        {
            bool isStillHeld;
            heldObject.Use(npc.throwForce, out isStillHeld);
            heldObject.GetComponent<FireExtinguisher>().isBeingUsed = true;

            return;
        }
        //Roll
        else if (npc.FireOnNPC != null)
        {
            if (heldObject != null)
            {
                heldObject.Deattach();
                heldObject = null;
                npc.isHoldingObject = false;
            }
            stateMachine.SwitchState(stateMachine.rollState);
        }

        List<Node> newPath;
        bool isUsingExtinguisher = false;

        if (npc.followPath(path, target, speed, true, out newPath))
        {
            if (npc.pathfinder.justUsedStairs)
                npc.pathfinder.justUsedStairs = false;
            else if (heldObject == null && npc.transform.GetComponentInChildren<FireFightingObject>())
            {
                heldObject = npc.transform.GetComponentInChildren<FireFightingObject>();

                if (heldObject.GetComponent<FireExtinguisher>() || heldObject.GetComponent<NonFlammableObject>())
                {
                    target = "Fire";
                    npc.closeProximityValue = 1.25f;

                    if (heldObject.GetComponent<FireExtinguisher>())
                        lastHeldExtinguisher = heldObject.GetComponent<FireExtinguisher>();

                    path = new List<Node>{npc.getCurrentNode()};
                }
                else
                {
                    Pail pail = heldObject.GetComponent<Pail>();
                    if (pail)
                    {
                        if (pail.getWaterInside() > stateMachine.ongoingFire.intensityValue ||
                            pail.getFractionFilled() >= 1.0f)
                        {
                            path = new List<Node>{npc.getCurrentNode()};
                            target = "Fire";
                        }
                        else
                        {
                            path = new List<Node>();
                            target = "WaterSource";
                        }

                        npc.closeProximityValue = 1.25f;
                    }
                }

                path.AddRange(npc.pathfinder.generatePath(npc.getCurrentNode(), target));

                Debug.Log("Got " + heldObject.transform.name + " / Next Target: " + target);

                foreach(Node node in npc.blacklist)
                {
                    if (node.GetComponent<NonFlammableObject>())
                    {
                        npc.blacklist.Remove(node);
                        break;
                    }
                }
            }
            else if (heldObject != null)
            {
                if (path.Count == 0 && newPath.Count == 0)
                {
                    Debug.Log("NPC failed to reach " + target);

                    heldObject.Deattach();
                    heldObject = null;
                    npc.isHoldingObject = false;

                    stateMachine.SwitchState(stateMachine.evacuateState);
                }

                if (target.Equals("Fire"))
                {
                    Debug.Log("Using " + heldObject.transform.name + " to extinguish fire");

                    Pail bucket = heldObject.GetComponent<Pail>();

                    bool isStillHeld;
                    heldObject.Use(npc.throwForce, out isStillHeld);

                    if (heldObject.GetComponent<FireExtinguisher>())
                    {
                        heldObject.GetComponent<FireExtinguisher>().isBeingUsed = true;
                        isUsingExtinguisher = true;
                    }

                    if (stateMachine.ongoingFire == null)
                    {
                        if (heldObject.GetComponent<FireExtinguisher>())
                            heldObject.GetComponent<FireExtinguisher>().isBeingUsed = false;

                        heldObject.Deattach();
                        heldObject = null;
                        npc.isHoldingObject = false;

                        stateMachine.SwitchState(stateMachine.evacuateState);
                    }
                    else
                    {
                        if (!isStillHeld)
                        {
                            npc.blacklist.Add(heldObject.GetComponent<Node>());
                            heldObject = null;
                            target = "FireFightingObject";
                            npc.closeProximityValue = 2.0f;
                        }
                        else
                        {
                            Fire fire = stateMachine.ongoingFire;

                            if ( (bucket && !fire.EffectivityTable[fire.type].Equals("Class A")) ||
                                 (!bucket && !fire.EffectivityTable[fire.type].Equals(heldObject.GetComponent<FireExtinguisher>().type)) )
                            {
                                heldObject.Deattach();
                                heldObject = null;
                                npc.isHoldingObject = false;

                                if (bucket) // fire is most likely pretty big if they used a bucket full of water wrongly
                                    stateMachine.SwitchState(stateMachine.evacuateState);
                                
                                foreach(FireExtinguisher extinguisher in GameObject.FindObjectsOfType<FireExtinguisher>())
                                    npc.blacklist.Add(extinguisher.GetComponent<Node>());
                                
                                target = "FireFightingObject";
                                npc.closeProximityValue = 2.0f;
                            }
                            else if (heldObject.GetComponent<Pail>())
                            {
                                target = "WaterSource";
                                npc.closeProximityValue = 1.25f;
                            }
                            else if (heldObject.GetComponent<FireExtinguisher>())
                                isExtinguisherEffective = true;
                        }

                        if (heldObject && bucket)
                            path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);
                        else if (!heldObject)
                            path = npc.pathfinder.generatePath(npc.getCurrentNode(), target, npc.blacklist);
                    }
                }
                else if (target.Equals("WaterSource"))
                {
                    Transform faucet = path[path.Count-1].transform;
                    heldObject.transform.SetParent(faucet);
                    heldObject.transform.SetLocalPositionAndRotation(new Vector3(1.15f, -4.0f, 0.0f),
                                                                        Quaternion.identity);

                    Debug.Log("Filling up " + heldObject.transform.name);

                    Pail pail = heldObject.GetComponent<Pail>();
                    try
                    {
                        if(pail.getWaterInside() > stateMachine.ongoingFire.intensityValue ||
                            pail.getFractionFilled() >= 1.0f)
                        {
                            heldObject.transform.SetParent(npc.transform);

                            heldObject.transform.SetPositionAndRotation(
                                npc.transform.position + npc.transform.forward * 0.5f + npc.transform.right * 0.3f, 
                                npc.transform.rotation);

                            target = "Fire";
                            npc.closeProximityValue = 1.25f;

                            if (faucet.GetComponent<Node>().floorLevel == 1)
                            {
                                path = new List<Node>{npc.pathfinder.crDoorNode};
                                path.AddRange(npc.pathfinder.generatePath(npc.pathfinder.crDoorNode, target));
                            }
                            else
                                path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);

                            faucet.GetComponentInChildren<Spawner>().Toggle(false);

                            Debug.Log(heldObject.transform.name + " is ready to take out " + target);
                        }
                    }
                    catch
                    {
                        Debug.Log("Fire has been taken out by player");

                        heldObject.Deattach();
                        heldObject = null;
                        npc.isHoldingObject = false;

                        stateMachine.SwitchState(stateMachine.evacuateState);
                    }
                }
            }
            else if (path.Count == 0 && newPath.Count == 0)
            {
                Debug.Log("NPC failed to get fire fighting object");
                stateMachine.SwitchState(stateMachine.evacuateState);
            }
        }
        
        if (newPath.Count > 0) path = newPath;

        if (!isUsingExtinguisher)
        {
            if (lastHeldExtinguisher && lastHeldExtinguisher.transform.parent && heldObject && 
                lastHeldExtinguisher == heldObject.GetComponent<FireExtinguisher>())
            {
                lastHeldExtinguisher.isBeingUsed = false;
            }
        }
    }
}
