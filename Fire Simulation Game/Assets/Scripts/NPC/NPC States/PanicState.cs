using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanicState : BaseState
{
    private NPC npc;
    private float speed;

    private float panicDuration;
    private float panicTimer;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;
        speed = npc.runningSpeed;

        npc.isRunning = true;
        npc.isInPanicState = true;

        panicDuration = Random.Range(10f, 15f);
        panicTimer = 0f;

        float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
        npc.SetStoppingDistance(0.01f);
        npc.GoTo(npc.transform.position + new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle)) * 2.5f, speed);
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        panicTimer += Time.deltaTime;

        List<Node> newPath = new List<Node>();

        if (npc.isHalted() || npc.hasReachedTarget())
        {
            float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
            npc.GoTo(npc.transform.position + new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle)) * 2.5f, speed);
        }
        
        if (npc.FireOnNPC != null)
        {
            npc.lastState = this;

            npc.isInPanicState = false;
            stateMachine.SwitchState(stateMachine.rollState);
        }

        if (panicTimer >= panicDuration)
        {
            float roll = Random.Range(0f, 1f);
            if (roll <= 0.25f)
            {
                Debug.Log($"{npc.name} decides to fight the fire.");
                npc.isInPanicState = false;
                stateMachine.SwitchState(stateMachine.preparationState);
            }
            else
            {
                Debug.Log($"{npc.name} decides to evacuates.");
                npc.isInPanicState = false;
                stateMachine.SwitchState(stateMachine.evacuateState);
            }
        }
    }
}
