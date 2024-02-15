using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Gameplay
{
    public class CharacterAnimator : MonoBehaviour
    {
        private Animator _animator;

        [Header("Settings")]
        public float attackAnimationTime = 0.5f;
        public float damageAnimationTime = 0.5f;

        public UnityEvent attackEndedEvent = new();
        public UnityEvent damageEndedEvent = new();

        private Coroutine _attackCoroutine = null;
        private Coroutine _damageCoroutine = null;

        private Animator GetAnimator()
        {
            if (_animator == null) _animator = GetComponent<Animator>();

            return _animator;
        }

        public void SetMoveSpeed(float speed)
        {
            GetAnimator().SetFloat("Speed", speed);
        }

        public void SetMoveDirection(float horizontal, float vertical)
        {
            GetAnimator().SetFloat("Horizontal", horizontal);
            GetAnimator().SetFloat("Vertical", vertical);
        }

        public void TriggerAttack()
        {
            GetAnimator().SetTrigger("Attack");

            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
            _attackCoroutine = StartCoroutine(CountdownAttackAnimation());
        }

        public void TriggerDamage()
        {
            GetAnimator().SetTrigger("Damage");

            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
                _damageCoroutine = null;
            }
            _damageCoroutine = StartCoroutine(CountdownDamageAnimation());
        }

        public void TriggerDead()
        {
            GetAnimator().SetTrigger("Dead");
        }

        IEnumerator CountdownAttackAnimation()
        {
            yield return new WaitForSecondsRealtime(attackAnimationTime);
            attackEndedEvent.Invoke();
        }

        IEnumerator CountdownDamageAnimation()
        {
            yield return new WaitForSecondsRealtime(damageAnimationTime);
            damageEndedEvent.Invoke();
        }

        private void OnDestroy()
        {
            attackEndedEvent.RemoveAllListeners();
            damageEndedEvent.RemoveAllListeners();
        }
    }
}