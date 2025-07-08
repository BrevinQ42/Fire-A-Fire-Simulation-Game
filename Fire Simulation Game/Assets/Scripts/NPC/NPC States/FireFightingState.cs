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
    private HashSet<Node> blacklist;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        target = "FireFightingObject";
        path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);

        if (Random.Range(0, 2) == 0)
            speed = npc.walkingSpeed;
        else
            speed = npc.runningSpeed;

        heldObject = null;
        blacklist = new HashSet<Node>();
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        List<Node> newPath;

        if (npc.followPath(path, target, speed, true, out newPath))
        {
            if (path.Count == 0) Debug.Log("Path to " + target + ": Fail");
            else Debug.Log("Path to " + target + ": Success");

            if (heldObject == null && npc.transform.GetComponentInChildren<FireFightingObject>())
            {
                heldObject = npc.transform.GetComponentInChildren<FireFightingObject>();

                if (heldObject.GetComponent<FireExtinguisher>() || heldObject.GetComponent<NonFlammableObject>())
                    target = "Fire";
                else
                {
                    Pail pail = heldObject.GetComponent<Pail>();
                    if (pail)
                    {
                        if (pail.getWaterInside() > stateMachine.ongoingFire.intensityValue ||
                            pail.getFractionFilled() >= 1.0f)
                            target = "Fire";
                        else
                            target = "WaterSource";
                    }
                }

                path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);

                Debug.Log("Got " + heldObject.transform.name + " / Next Target: " + target);
            }
            else if (heldObject != null)
            {
                if (target.Equals("Fire"))
                {
                    Debug.Log("Using " + heldObject.transform.name + " to extinguish fire");

                    bool isStillHeld;

                    heldObject.Use(npc.throwForce, out isStillHeld);

                    if (stateMachine.ongoingFire == null)
                        stateMachine.SwitchState(stateMachine.evacuateState);
                    else
                    {
                        if (!isStillHeld)
                        {
                            heldObject = null;
                            target = "FireFightingObject";
                        }
                        else if (heldObject.GetComponent<Pail>())
                        {
                            target = "WaterSource";
                        }

                        path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);
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
                        heldObject.transform.SetPositionAndRotation(
                            npc.transform.position + npc.transform.forward * 0.5f + npc.transform.right * 0.3f, 
                            npc.transform.rotation);

                        heldObject.transform.SetParent(npc.transform);

                        target = "Fire";
                        path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);

                        Debug.Log(heldObject.transform.name + " is ready to take out " + target);
                    }
                }
            }
            else Debug.Log("NPC failed");
        }
        
        if (newPath.Count > 0) path = newPath;
    }
}
