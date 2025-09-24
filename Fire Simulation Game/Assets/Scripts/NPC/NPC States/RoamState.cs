using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamState : BaseState
{
    private NPC npc;
    private float speed;

    private Transform tasksList;
    private int tasksCount;
    private int taskIndex;

    private float timeBeforeNextAction;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;
        speed = npc.walkingSpeed;

        tasksList = npc.currentLocation.parent.GetChild(0);
        tasksCount = tasksList.childCount;
        taskIndex = Random.Range(0, tasksCount);

        npc.SetStoppingDistance(0.01f);
        npc.GoTo(tasksList.GetChild(taskIndex).position, speed);

        timeBeforeNextAction = Random.Range(25, 35);
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        if (stateMachine.ongoingFire != null &&
            canNpcSenseFire(stateMachine.ongoingFire))
        {
            if (Random.Range(0, 2) == 0)
            {
                npc.isPanicking = true;
                stateMachine.SwitchState(stateMachine.panicState);
            }
            else
                stateMachine.SwitchState(stateMachine.preparationState);
        }

        if (npc.hasReachedTarget())
        {
            npc.transform.rotation = tasksList.GetChild(taskIndex).rotation;

            if (timeBeforeNextAction > 0)
            {
                timeBeforeNextAction -= Time.deltaTime;
            }
            else
            {
                taskIndex = (taskIndex + Random.Range(1, tasksCount)) % tasksCount;
                npc.GoTo(tasksList.GetChild(taskIndex).position, speed);

                timeBeforeNextAction = Random.Range(25, 35);
            }

        }
    }

    private bool canNpcSenseFire(Fire fire)
    {
        Debug.Log(Vector3.Distance(npc.transform.position, fire.transform.position) + " vs " + fire.intensityValue * 7.5f);
        return Vector3.Distance(npc.transform.position, fire.transform.position) <= fire.intensityValue * 7.5f;
    }
}
