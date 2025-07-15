using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanicState : BaseState
{
    private NPC npc;
    private string target;

    private List<Node> path;
    private float speed;

    private float panicDuration;
    private float panicTimer;
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

        panicDuration = Random.Range(10f, 15f);
        panicTimer = 0f;

        Debug.Log($"{npc.name} has entered PANIC STATE!");
        npc.animator.SetTrigger("Panic");
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        panicTimer += Time.deltaTime;

        List<Node> newPath = new List<Node>();

        if (timeBeforeNextPath > 0.0f)
        {
            timeBeforeNextPath -= Time.deltaTime;
        }
        else if (npc.followPath(path, target, speed, false, out newPath))
        {
            Node destination = path[path.Count - 1];

            if (npc.getCurrentNode() == destination)
            {
                if (npc.pathfinder.justUsedStairs)
                    timeBeforeNextPath = 0f;
                else
                    timeBeforeNextPath = Random.Range(1f, 3f); 
            }

            path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);
        }

        if (newPath.Count > 0)
            path = newPath;

        if (panicTimer >= panicDuration)
        {
            float roll = Random.Range(0f, 1f);
            if (roll <= 0.25f)
            {
                Debug.Log($"{npc.name} decides to fight the fire.");
                stateMachine.SwitchState(stateMachine.fireFightingState);
            }
            else
            {
                Debug.Log($"{npc.name} decides to evacuates.");
                stateMachine.SwitchState(stateMachine.evacuateState);
            }
        }
    }
}
