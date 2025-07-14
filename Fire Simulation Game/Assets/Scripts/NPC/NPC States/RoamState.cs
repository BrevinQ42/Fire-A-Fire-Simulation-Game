using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamState : BaseState
{
    private NPC npc;
    private string target;
    
    private List<Node> path;
    private float speed;

    private float timeBeforeNextPath;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        target = "Any";
        path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);

        if (Random.Range(0, 2) == 0)
            speed = npc.walkingSpeed;
        else
            speed = npc.runningSpeed;

        timeBeforeNextPath = 0.0f;
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        if (stateMachine.ongoingFire != null)
            stateMachine.SwitchState(stateMachine.fireFightingState);

        List<Node> newPath = new List<Node>();

        if (timeBeforeNextPath > 0.0f)
        {
            timeBeforeNextPath -= Time.deltaTime;

            Debug.Log(timeBeforeNextPath);
        }
        else if (npc.followPath(path, target, speed, false, out newPath))
        {
            Node destination = path[path.Count-1];

            if (npc.getCurrentNode() == destination)
            {
                Debug.Log("NPC has reached " + destination);
                timeBeforeNextPath = Random.Range(5, 11);
            }

            path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);
        }
        else
            if (newPath.Count > 0) path = newPath;
    }
}
