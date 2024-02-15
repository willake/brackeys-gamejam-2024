using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Gameplay
{
    public enum CharacterType
    {
        Player,
        Enemy
    }
    public class Character : MonoBehaviour
    {
        private CharacterAnimator _animator;
        [Header("States")]
        public bool isDead = false;
        public bool isMoving = false;
        [Header("Settings")]
        public CharacterType characterType = CharacterType.Enemy;
        public float speed;
        public float attackRadius = 1;
        public float attackDetectionRadius = 1;
        private Vector2 _destination = Vector2.zero;

        private UnityEvent _onArriveDestination = new();

        private CharacterAnimator GetCharacterAnimator()
        {
            if (_animator == null) _animator = GetComponent<CharacterAnimator>();

            return _animator;
        }

        public void MoveTo(Vector2 destination)
        {
            GetCharacterAnimator().SetMoveSpeed(speed);
            isMoving = true;
            _destination = destination;
        }

        public async UniTask MoveToAsync(Vector2 destination)
        {
            MoveTo(destination);

            await _onArriveDestination.AsObservable().Take(1);
        }

        public void Attack(Vector2 direction)
        {
            GetCharacterAnimator().SetMoveDirection(direction.x, direction.y);
            GetCharacterAnimator().TriggerAttack();

            Vector2 attackTip = new Vector2(transform.position.x, transform.position.y) + direction * attackRadius;
            Debug.DrawCircle(new Vector3(attackTip.x, attackTip.y, -1), attackDetectionRadius, 32, Color.red);
            RaycastHit2D[] hits = Physics2D.CircleCastAll(attackTip, attackDetectionRadius, direction);

            foreach (var hit in hits)
            {
                if (hit.collider != null)
                {
                    Character character = hit.collider.GetComponent<Character>();
                    // check if from different groups
                    if (character && character.characterType != characterType) character.Die();
                }
            }
        }

        public async UniTask AttackAsync(Vector2 direction)
        {
            Attack(direction);
            await GetCharacterAnimator().attackEndedEvent.AsObservable().Take(1);
        }

        public void Die()
        {
            GetCharacterAnimator().TriggerDead();
            isDead = true;
        }

        private void Update()
        {
            if (isMoving)
            {
                float step = speed * Time.deltaTime;
                transform.position = Vector2.MoveTowards(transform.position, _destination, step);

                Vector2 direction = (_destination - new Vector2(transform.position.x, transform.position.y));
                GetCharacterAnimator().SetMoveDirection(direction.x, direction.y);


                float distanceToDestination = Vector2.Distance(transform.position, _destination);

                if (distanceToDestination < 0.01f)
                {
                    isMoving = false;
                    GetCharacterAnimator().SetMoveSpeed(0);
                    _onArriveDestination.Invoke();
                }
            }
        }
    }
}