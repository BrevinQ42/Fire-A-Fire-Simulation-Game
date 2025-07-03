using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateMachine : MonoBehaviour
{
    // STATES
    private BaseState currentState;
    public RoamState roamState = new RoamState();
    public PanicState panicState = new PanicState();
    public FireFightingState fireFightingState = new FireFightingState();
    public EvacuateState evacuateState = new EvacuateState();

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
            currentState = fireFightingState;
            currentState.EnterState(this);
        }

        if (currentState != null)
            currentState.UpdateState(this);
    }

    public void SwitchState(BaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }
}
