namespace Game.Gameplay
{
    public interface IPlanningState
    {
        public bool canPlanMove { get; }
        public bool canPlanAttackDirection { get; }
        public bool canUndo { get; }
    }

    public static class PlanningStates
    {
        public static readonly IPlanningState IdleState = new PlanIdleState();
        public static readonly IPlanningState PlanMoveState = new PlanMoveState();
        public static readonly IPlanningState PlanAttackState = new PlanAttackState();
        public static readonly IPlanningState PlanAttackDirectionState = new PlanAttackDirectionState();
    }

    public class PlanMoveState : IPlanningState
    {
        public bool canPlanMove { get => true; }
        public bool canPlanAttackDirection { get => false; }
        public bool canUndo { get => true; }
    }

    public class PlanAttackState : IPlanningState
    {
        public bool canPlanMove { get => false; }
        public bool canPlanAttackDirection { get => false; }
        public bool canUndo { get => true; }
    }

    public class PlanAttackDirectionState : IPlanningState
    {
        public bool canPlanMove { get => false; }
        public bool canPlanAttackDirection { get => true; }
        public bool canUndo { get => true; }
    }

    public class PlanIdleState : IPlanningState
    {
        public bool canPlanMove { get => false; }
        public bool canPlanAttackDirection { get => false; }
        public bool canUndo { get => true; }
    }
}