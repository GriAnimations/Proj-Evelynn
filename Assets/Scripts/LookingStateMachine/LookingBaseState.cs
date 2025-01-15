namespace LookingStateMachine
{
    public abstract class LookingBaseState
    {
        public abstract void EnterState(LookingStateManager looking);
        public abstract void UpdateState(LookingStateManager looking);
        public abstract void DoAction(LookingStateManager looking);
    }
}
