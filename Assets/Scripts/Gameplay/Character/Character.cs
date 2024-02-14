using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Gameplay
{
    public class Character : MonoBehaviour
    {
        private CharacterAnimator _animator;
        [Header("Settings")]
        public float speed;
        public bool isDead = false;
        public bool isMoving = false;
        private Vector2 _destination = Vector2.zero;

        private UnityEvent _onArriveDestination = new();

        private CharacterAnimator GetCharacterAnimator()
        {
            if (_animator == null) _animator = GetComponent<CharacterAnimator>();

            return _animator;
        }

        public void MoveTo(Vector2 destination)
        {
            Vector2 direction = (destination - new Vector2(transform.position.x, transform.position.y));
            GetCharacterAnimator().SetMoveSpeed(direction.x, direction.y, speed);
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
            GetCharacterAnimator().TriggerAttack();
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
                Vector2.MoveTowards(transform.position, _destination, step);

                float distanceToDestination = Vector2.Distance(transform.position, _destination);

                if (distanceToDestination < 0.01f)
                {
                    isMoving = false;
                    _onArriveDestination.Invoke();
                }
            }
        }
    }
}