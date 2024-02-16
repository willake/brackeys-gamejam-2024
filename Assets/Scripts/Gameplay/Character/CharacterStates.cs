namespace Game.Gameplay
{
    public interface ICharacterState
    {
        public bool canMove { get; }
        public bool canAttack { get; }
        public bool isMoving { get; }
        public string name { get; }
    }

    public static class CharacterStates
    {
        public static readonly ICharacterState IdleState = new IdleState();
        public static readonly ICharacterState MoveState = new MoveState();
        public static readonly ICharacterState AttackState = new AttackState();
        public static readonly ICharacterState DeadState = new DeadState();
    }

    public class IdleState : ICharacterState
    {
        public bool canMove { get => true; }
        public bool canAttack { get => true; }
        public bool isMoving { get => false; }
        public string name { get => "IdleState"; }
    }

    public class MoveState : ICharacterState
    {
        public bool canMove { get => true; }
        public bool canAttack { get => true; }
        public bool isMoving { get => true; }
        public string name { get => "MoveState"; }
    }

    public class AttackState : ICharacterState
    {
        public bool canMove { get => true; }
        public bool canAttack { get => false; }
        public bool isMoving { get => false; }
        public string name { get => "AttackState"; }
    }

    public class DeadState : ICharacterState
    {
        public bool canMove { get => false; }
        public bool canAttack { get => false; }
        public bool isMoving { get => false; }
        public string name { get => "DeadState"; }
    }
}