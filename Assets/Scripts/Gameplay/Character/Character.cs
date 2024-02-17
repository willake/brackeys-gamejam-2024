using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Audios;
using Game.Events;
using Game.RuntimeStates;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
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
        private Lazy<EventManager> _eventManager = new Lazy<EventManager>(
            () => DIContainer.instance.GetObject<EventManager>(),
            true
        );
        protected EventManager EventManager { get => _eventManager.Value; }
        public UnityEvent onDie = new();

        private NavMeshAgent _navmeshAgent;
        private SpriteRenderer _renderer;
        private CharacterAnimator _animator;

        [Header("References")]
        public Vector3State playerPositionState;
        public ParticleSystem deathVFX;
        [Header("Settings")]
        public CharacterType characterType = CharacterType.Enemy;
        public float speed;
        public float attackRadius = 1;
        public float attackDetectionRadius = 1;
        public float footstepsInterval = 0.2f;
        private Vector2 _destination = Vector2.zero;
        private UnityEvent _onArriveDestination = new();
        private float _lastFootstepsTime = 0;
        private ICharacterState _state = CharacterStates.IdleState;
        public ICharacterState State { get => _state; }


        protected SpriteRenderer GetRenderer()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();

            return _renderer;
        }

        protected CharacterAnimator GetCharacterAnimator()
        {
            if (_animator == null) _animator = GetComponent<CharacterAnimator>();

            return _animator;
        }

        protected NavMeshAgent GetNavMeshAgent()
        {
            if (_navmeshAgent == null) _navmeshAgent = GetComponent<NavMeshAgent>();

            return _navmeshAgent;
        }

        protected virtual void Start()
        {
            GetNavMeshAgent().updateRotation = false;
            GetNavMeshAgent().updateUpAxis = false;
            transform.rotation = Quaternion.identity;
        }

        public async UniTask MoveToAsync(Vector2 destination)
        {
            if (_state.canMove == false) return;
            SetState(CharacterStates.MoveState);
            GetCharacterAnimator().SetMoveSpeed(speed);
            _destination = destination;

            GetNavMeshAgent().destination = destination;

            await _onArriveDestination.AsObservable().Take(1);
        }

        private void SetState(ICharacterState state)
        {
            _state = state;
        }

        public async UniTask AttackAsync(Vector2 direction)
        {
            if (_state.canAttack == false) return;

            SetState(CharacterStates.AttackState);
            GetCharacterAnimator().SetMoveDirection(direction.x, direction.y);
            GetCharacterAnimator().TriggerAttack();

            WrappedAudioClip audioClip =
                ResourceManager.instance.audioResources.gameplayAudios.attack;

            AudioManager.instance?.PlaySFX(
                audioClip.clip,
                audioClip.volume,
                UnityEngine.Random.Range(0.6f, 1f)
            );

            await GetCharacterAnimator().attackEndedEvent.AsObservable().Take(1);

            Vector2 attackTip = new Vector2(transform.position.x, transform.position.y) + direction * attackRadius;
            Debug.DrawCircle(new Vector3(attackTip.x, attackTip.y, -1), attackDetectionRadius, 32, Color.red);
            RaycastHit2D[] hits = Physics2D.CircleCastAll(attackTip, attackDetectionRadius, direction);

            bool kill = false;

            foreach (var hit in hits)
            {
                if (hit.collider != null)
                {
                    Character character = hit.collider.GetComponent<Character>();
                    // check if from different groups
                    if (character && character.characterType != characterType && character.State != CharacterStates.DeadState)
                    {
                        character.Die();
                        kill = true;
                    }
                }
            }

            if (kill && characterType == CharacterType.Player)
            {
                EventManager.Publish(
                    EventNames.presentDialogue,
                    new Payload() { args = new object[] { ResourceManager.instance.dialogueResources.onKillEnemy } }
            );
            }
        }

        public virtual void Die()
        {
            SetState(CharacterStates.DeadState);
            GetCharacterAnimator().TriggerDead();

            WrappedAudioClip audioClip =
                ResourceManager.instance.audioResources.gameplayAudios.dead;

            AudioManager.instance?.PlaySFX(
                audioClip.clip,
                audioClip.volume,
                UnityEngine.Random.Range(0.6f, 1f)
            );

            StartCoroutine(DeathEffect());
            onDie.Invoke();
        }

        private IEnumerator DeathEffect()
        {
            deathVFX.Play();
            yield return new WaitForSeconds(1.5f);
            gameObject.SetActive(false);
        }

        public virtual void Reset()
        {
            gameObject.SetActive(true);
            SetState(CharacterStates.IdleState);
        }

        private void Update()
        {
            if (_state.isMoving)
            {
                float step = speed * Time.deltaTime;
                // transform.position = Vector2.MoveTowards(transform.position, _destination, step);

                // Vector2 direction = (_destination - new Vector2(transform.position.x, transform.position.y));
                GetCharacterAnimator().SetMoveDirection(GetNavMeshAgent().velocity.x, GetNavMeshAgent().velocity.y);


                // float distanceToDestination = Vector2.Distance(transform.position, _destination);

                // if (distanceToDestination < 0.01f)
                // {
                //     SetState(CharacterStates.IdleState);
                //     GetCharacterAnimator().SetMoveSpeed(0);
                //     _onArriveDestination.Invoke();
                // }

                if (_navmeshAgent.hasPath == false)
                {
                    SetState(CharacterStates.IdleState);
                    GetCharacterAnimator().SetMoveSpeed(0);
                    _onArriveDestination.Invoke();
                }

                if (Time.time - _lastFootstepsTime > footstepsInterval)
                {
                    float r = UnityEngine.Random.value;
                    WrappedAudioClip audioClip;

                    if (r > 0.66) audioClip = ResourceManager.instance?.audioResources.gameplayAudios.footstepsWood1;
                    else if (r > 0.33) audioClip = ResourceManager.instance?.audioResources.gameplayAudios.footstepsWood2;
                    else audioClip = ResourceManager.instance?.audioResources.gameplayAudios.footstepsWood3;

                    AudioManager.instance?.PlaySFX(
                        audioClip.clip,
                        audioClip.volume,
                        UnityEngine.Random.Range(0.6f, 1f)
                    );
                    _lastFootstepsTime = Time.time;
                }

                if (characterType == CharacterType.Player) playerPositionState.SetValue(transform.position);
            }
        }
    }
}