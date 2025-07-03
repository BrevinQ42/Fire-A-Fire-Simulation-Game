using UnityEngine;

public abstract class BaseState
{
    public abstract void EnterState(NPCStateMachine stateMachine);
    public abstract void UpdateState(NPCStateMachine stateMachine);
}
