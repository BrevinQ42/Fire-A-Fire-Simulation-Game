using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamState : BaseState
{
    private NPC npc;

    private Transform tasksList;
    private int tasksCount;
    private int taskIndex;

    private float timeBeforeNextAction;

    private Vector3 positionBeforeWarp;

    public override void EnterState(NPCStateMachine stateMachine)
    {
        npc = stateMachine.npc;
        npc.currentSpeed = npc.walkingSpeed;

        npc.SetStoppingDistance(2.0f);

        tasksList = null;

        timeBeforeNextAction = 0;

        positionBeforeWarp = Vector3.one * 100;
    }

    public override void UpdateState(NPCStateMachine stateMachine)
    {
        if (tasksList == null)
        {
            GetFirstTask();
            return;
        }

        if (stateMachine.ongoingFire != null &&
            (canNpcSenseFire(stateMachine.ongoingFire) || stateMachine.hasBellRung))
        {
            if (npc.isSleeping && positionBeforeWarp.x != 100)
            {
                Debug.Log(npc.transform.position.x + ", " + npc.transform.position.y + ", " + npc.transform.position.z +
                            " warped to " + positionBeforeWarp.x + ", " + positionBeforeWarp.y + ", " + positionBeforeWarp.z);
                npc.WarpTo(positionBeforeWarp, false);

                positionBeforeWarp = Vector3.one * 100;
            }

            //Idle Animation Stuff
            npc.isUsingLaptop = false;
            npc.isSleeping = false;
            npc.isWatchingTV= false;
            npc.isCooking = false;

            stateMachine.SwitchState(stateMachine.alertedState);
        }

        if (timeBeforeNextAction > 0)
        {
            timeBeforeNextAction -= Time.deltaTime;

            if (timeBeforeNextAction <= 0)
            {
                npc.WarpTo(positionBeforeWarp, false);
                positionBeforeWarp = Vector3.one * 100;

                taskIndex = (taskIndex + Random.Range(1, tasksCount)) % tasksCount;
                npc.GoTo(tasksList.GetChild(taskIndex).position, npc.currentSpeed);

                npc.ResetBuffer();

                //Idle Animation Stuff
                npc.isUsingLaptop = false;
                npc.isSleeping = false;
                npc.isWatchingTV= false;
                npc.isCooking = false;
            }
        }
        else if (npc.hasReachedTarget())
        {
            timeBeforeNextAction = Random.Range(25, 35);

            positionBeforeWarp = npc.transform.position;

            npc.WarpTo(tasksList.GetChild(taskIndex).position, true);

            Debug.Log(positionBeforeWarp.x + ", " + positionBeforeWarp.y + ", " + positionBeforeWarp.z +
                            " warped to " + npc.transform.position.x + ", " + npc.transform.position.y + ", " + npc.transform.position.z);

            npc.transform.rotation = tasksList.GetChild(taskIndex).rotation;

            //Idle Animation Stuff
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
        else npc.StuckCheck();
    }

    private bool canNpcSenseFire(Fire fire)
    {
        return Vector3.Distance(npc.transform.position, fire.transform.position) <= fire.intensityValue * 7.5f;
    }

    private void GetFirstTask()
    {
        try
        {
            tasksList = npc.currentLocation.parent.GetChild(0);
            tasksCount = tasksList.childCount;
            taskIndex = Random.Range(0, tasksCount);

            npc.GoTo(tasksList.GetChild(taskIndex).position, npc.currentSpeed);
        }
        catch {}
    }
}
