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
    private float targetFractionFilled;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        target = "FireFightingObject";
        path = npc.pathfinder.generatePath(npc.getCurrentNode(), target, npc.blacklist);

        if (Random.Range(0, 2) == 0)
            speed = npc.walkingSpeed;
        else
            speed = npc.runningSpeed;

        heldObject = null;
        lastHeldExtinguisher = null;
        targetFractionFilled = 0.5f;
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        List<Node> newPath;
        bool isUsingExtinguisher = false;

        if (npc.followPath(path, target, speed, true, out newPath))
        {
            if (heldObject == null && npc.transform.GetComponentInChildren<FireFightingObject>())
            {
                heldObject = npc.transform.GetComponentInChildren<FireFightingObject>();

                if (heldObject.GetComponent<FireExtinguisher>() || heldObject.GetComponent<NonFlammableObject>())
                {
                    target = "Fire";
                    npc.closeProximityValue = 1.25f;

                    if (heldObject.GetComponent<FireExtinguisher>())
                        lastHeldExtinguisher = heldObject.GetComponent<FireExtinguisher>();
                }
                else
                {
                    Pail pail = heldObject.GetComponent<Pail>();
                    if (pail)
                    {
                        if (pail.getFractionFilled() >= targetFractionFilled || targetFractionFilled - pail.getFractionFilled() <= 0.05f)
                        {
                            target = "Fire";
                            targetFractionFilled = 1.0f;
                        }
                        else
                            target = "WaterSource";

                        npc.closeProximityValue = 1.25f;
                    }
                }

                path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);

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
                    
                    stateMachine.SwitchState(stateMachine.evacuateState);
                }

                if (target.Equals("Fire"))
                {
                    Debug.Log("Using " + heldObject.transform.name + " to extinguish fire");

                    Pail bucket = heldObject.GetComponent<Pail>();

                    bool isStillHeld;

                    if (bucket)
                    {
                        float yOffset = stateMachine.ongoingFire.transform.position.y - npc.position.y;
                        if (yOffset <= 0.0f) yOffset = 0.0f;

                        bucket.Use(npc.throwForce, out isStillHeld, yOffset);
                    }
                    else
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
                    if(pail.getWaterInside() > stateMachine.ongoingFire.intensityValue ||
                        pail.getFractionFilled() >= 1.0f)
                    {
                        heldObject.transform.SetParent(npc.transform);

                        heldObject.transform.SetPositionAndRotation(
                            npc.transform.position + npc.transform.forward * 0.5f + npc.transform.right * 0.3f, 
                            npc.transform.rotation);

                        target = "Fire";
                        npc.closeProximityValue = 1.25f;
                        path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);

                        faucet.GetComponentInChildren<Spawner>().Toggle(false);

                        Debug.Log(heldObject.transform.name + " is ready to take out " + target);
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
