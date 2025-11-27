using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateMachine : MonoBehaviour
{
    // STATES
    public string currentStateName;
    public BaseState currentState;
    public RoamState roamState = new RoamState();
    public AlertedState alertedState = new AlertedState();
    public PanicState panicState = new PanicState();
    public PreparationState preparationState = new PreparationState();
    public FireFightingState fireFightingState = new FireFightingState();
    public EvacuateState evacuateState = new EvacuateState();
    public RollState rollState = new RollState();

    // MISC.
    public NPC npc;
    public Fire ongoingFire;

    // Start is called before the first frame update
    void Start()
    {
        npc = GetComponent<NPC>();
        ongoingFire = null;

        if (npc.agent == null)
            currentState = null;
        else
        {
            currentState = roamState;
            SetStateName();
            currentState.EnterState(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == null && npc.agent != null)
        {
            currentState = roamState;
            SetStateName();
            currentState.EnterState(this);
        }
        
        if (currentState != null) currentState.UpdateState(this);
    }

    public void SwitchState(BaseState state)
    {
        currentState = state;
        SetStateName();
        state.EnterState(this);
    }

    void SetStateName()
    {
        if (currentState == roamState)
            currentStateName = "Roam";
        else if (currentState == alertedState)
            currentStateName = "Alerted";
        else if (currentState == panicState)
            currentStateName = "Panic";
        else if (currentState == preparationState)
            currentStateName = "Preparation";
        else if (currentState == fireFightingState)
            currentStateName = "Fire Fighting";
        else if (currentState == evacuateState)
            currentStateName = "Evacuate";
        else if (currentState == rollState)
            currentStateName = "Roll";
        else
            currentStateName = "Null";
    }
}
