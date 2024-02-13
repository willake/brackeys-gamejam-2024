namespace Game.Gameplay
{
    public interface IPlanningState
    {
        public bool canPlanMove { get; }
        public bool canPlanAttackPosition { get; }
        public bool canPlanAttackDirection { get; }
        public bool canUndo { get; }
        public string name { get; }
    }

    public static class PlanningStates
    {
        public static readonly IPlanningState IdleState = new PlanIdleState();
        public static readonly IPlanningState PlanMoveState = new PlanMoveState();
        public static readonly IPlanningState PlanAttackPositionState = new PlanAttackPositionState();
        public static readonly IPlanningState PlanAttackActionState = new PlanAttackActionState();
        public static readonly IPlanningState PlanAttackDirectionState = new PlanAttackDirectionState();
    }

    public class PlanMoveState : IPlanningState
    {
        public bool canPlanMove { get => true; }
        public bool canPlanAttackPosition { get => false; }
        public bool canPlanAttackDirection { get => false; }
        public bool canUndo { get => true; }
        public string name { get => "PlanMoveState"; }
    }

    public class PlanAttackPositionState : IPlanningState
    {
        public bool canPlanMove { get => false; }
        public bool canPlanAttackPosition { get => true; }
        public bool canPlanAttackDirection { get => false; }
        public bool canUndo { get => true; }
        public string name { get => "PlanAttackPositionState"; }
    }

    public class PlanAttackActionState : IPlanningState
    {
        public bool canPlanMove { get => false; }
        public bool canPlanAttackPosition { get => false; }
        public bool canPlanAttackDirection { get => false; }
        public bool canUndo { get => true; }
        public string name { get => "PlanAttackActionState"; }
    }

    public class PlanAttackDirectionState : IPlanningState
    {
        public bool canPlanMove { get => false; }
        public bool canPlanAttackPosition { get => false; }
        public bool canPlanAttackDirection { get => true; }
        public bool canUndo { get => true; }
        public string name { get => "PlanAttackDirectionState"; }
    }

    public class PlanIdleState : IPlanningState
    {
        public bool canPlanMove { get => false; }
        public bool canPlanAttackPosition { get => false; }
        public bool canPlanAttackDirection { get => false; }
        public bool canUndo { get => true; }
        public string name { get => "PlanIdleState"; }
    }
}