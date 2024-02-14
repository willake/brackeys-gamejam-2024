using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    public class Character : MonoBehaviour
    {
        private CharacterAnimator _animator;
        [Header("Settings")]
        public float speed;
        public bool isDead = false;

        private CharacterAnimator GetCharacterAnimator()
        {
            if (_animator == null) _animator = GetComponent<CharacterAnimator>();

            return _animator;
        }

        public void MoveTo(Vector2 destination)
        {
            Vector2 direction = (destination - new Vector2(transform.position.x, transform.position.y));
            GetCharacterAnimator().SetMoveSpeed(direction.x, direction.y, speed);
        }

        public void Attack(Vector2 direction)
        {
            GetCharacterAnimator().TriggerAttack();
        }

        public void Die()
        {
            GetCharacterAnimator().TriggerDead();
            isDead = true;
        }
    }
}