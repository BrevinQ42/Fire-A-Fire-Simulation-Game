using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateMachine : MonoBehaviour
{
    // STATES
    public string currentStateName;
    private BaseState currentState;
    public RoamState roamState = new RoamState();
    public PanicState panicState = new PanicState();
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

        // currentState = roamState;
        // currentState.EnterState(this);
        currentState = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == null && Input.GetKeyDown(KeyCode.M))
        {
            currentState = rollState;
            currentState.EnterState(this);
        }

        if (currentState != null)
        {
            currentState.UpdateState(this);

            if (currentState == roamState)
                currentStateName = "Roam";
            else if (currentState == panicState)
                currentStateName = "Panic";
            else if (currentState == fireFightingState)
                currentStateName = "Fire Fighting";
            else if (currentState == evacuateState)
                currentStateName = "Evacuate";
            else if (currentState == rollState)
                currentStateName = "Roll";
        }
    }

    public void SwitchState(BaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }
}
