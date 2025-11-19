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

        tasksList = null;
        GetTask();

        timeBeforeNextAction = Random.Range(25, 35);
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        if (tasksList == null) GetTask();
        else return;

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
            try
            {
                npc.transform.rotation = tasksList.GetChild(taskIndex).rotation;
            }
            catch {}

            if (timeBeforeNextAction > 0)
            {
                timeBeforeNextAction -= Time.deltaTime;
                //Idle Animation Stuff
                try
                {
                    if (tasksList.GetChild(taskIndex).name == "Laptop")
                    {
                        npc.isUsingLaptop = true;
                    }

                    if (tasksList.GetChild(taskIndex).name == "Sleep")
                    {
                        npc.isSleeping = true;
                    }

                    if (tasksList.GetChild(taskIndex).name == "Watch TV")
                    {
                        npc.isWatchingTV = true;
                    }

                    if (tasksList.GetChild(taskIndex).name == "Cook")
                    {
                        npc.isCooking = true;
                    }
                }
                catch {}
            }
            else
            {
                taskIndex = (taskIndex + Random.Range(1, tasksCount)) % tasksCount;
                npc.GoTo(tasksList.GetChild(taskIndex).position, speed);

                timeBeforeNextAction = Random.Range(25, 35);
                //Idle Animation Stuff
                npc.isUsingLaptop = false;
                npc.isSleeping = false;
                npc.isWatchingTV= false;
                npc.isCooking = false;
            }

        }
    }

    private bool canNpcSenseFire(Fire fire)
    {
        return Vector3.Distance(npc.transform.position, fire.transform.position) <= fire.intensityValue * 7.5f;
    }

    private void GetTask()
    {
        try
        {
            tasksList = npc.currentLocation.parent.GetChild(0);
            tasksCount = tasksList.childCount;
            taskIndex = Random.Range(0, tasksCount);

            npc.SetStoppingDistance(0.01f);
            npc.GoTo(tasksList.GetChild(taskIndex).position, speed);
        }
        catch {}
    }
}
