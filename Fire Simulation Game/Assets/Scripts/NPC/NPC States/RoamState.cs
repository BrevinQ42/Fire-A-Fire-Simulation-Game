using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamState : BaseState
{
    private NPC npc;
    private string target;
    
    private List<Node> path;
    private float speed;

    private int nodesBeforeStop;
    private float timeBeforeNextPath;
    private float timeBeforeAction;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        target = "Any";
        path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);

        speed = npc.walkingSpeed;

        nodesBeforeStop = Random.Range(5, 11);
        timeBeforeNextPath = 0.0f;
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        if (stateMachine.ongoingFire != null &&
            isNpcCloseToFire(stateMachine.npc, stateMachine.ongoingFire))
        {
            if (Random.Range(0, 2) == 0)
            {
                npc.isPanicking = true;
                stateMachine.SwitchState(stateMachine.panicState);
            }
            else
                stateMachine.SwitchState(stateMachine.fireFightingState);
        }

        List<Node> newPath = new List<Node>();

        if (timeBeforeNextPath > 0.0f)
        {
            timeBeforeNextPath -= Time.deltaTime;

            Debug.Log("timeBeforeNextPath:" + timeBeforeNextPath);
        }
        else if (npc.followPath(path, target, speed, false, out newPath))
        {
            Node destination = path[path.Count-1];

            if (npc.getCurrentNode() == destination)
            {
                if (nodesBeforeStop > 0)
                {
                    nodesBeforeStop--;

                    Debug.Log("NodesBeforeStop: " + nodesBeforeStop);

                    if (nodesBeforeStop == 0)
                    {
                        timeBeforeNextPath = Random.Range(3, 8);
                        nodesBeforeStop = Random.Range(5, 11);
                    }
                }

                Debug.Log("NPC has reached " + destination);
            }

            path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);
        }
        else
            if (newPath.Count > 0) path = newPath;
    }

    private bool isNpcCloseToFire(NPC npc, Fire fire)
    {
        return Vector2.Distance(npc.transform.position, fire.transform.position) <= fire.intensityValue * 7.5f;
    }
}
