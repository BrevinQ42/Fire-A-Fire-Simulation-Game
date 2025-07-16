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

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;

        target = "Any";
        path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);

        speed = npc.runningSpeed;
        npc.isRunning = true;

        panicDuration = Random.Range(10f, 15f);
        panicTimer = 0f;

        Debug.Log($"{npc.name} has entered PANIC STATE!");
        // npc.NPCAnimator.SetTrigger("Panic");
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        panicTimer += Time.deltaTime;

        List<Node> newPath = new List<Node>();

        if (npc.followPath(path, target, speed, false, out newPath))
        {
            path = npc.pathfinder.generatePath(npc.getCurrentNode(), target);
        }
        else if (npc.FireOnNPC != null)
        {
            stateMachine.SwitchState(stateMachine.rollState);
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
