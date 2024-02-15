using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Audios;
using Game.Events;
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
        private Lazy<EventManager> _eventManager = new Lazy<EventManager>(
            () => DIContainer.instance.GetObject<EventManager>(),
            true
        );
        protected EventManager EventManager { get => _eventManager.Value; }

        private SpriteRenderer _renderer;
        private CharacterAnimator _animator;

        [Header("References")]
        public ParticleSystem deathVFX;
        [Header("States")]
        public bool isDead = false;
        public bool isMoving = false;
        [Header("Settings")]
        public CharacterType characterType = CharacterType.Enemy;
        public float speed;
        public float attackRadius = 1;
        public float attackDetectionRadius = 1;
        public float footstepsInterval = 0.2f;
        private Vector2 _destination = Vector2.zero;
        private UnityEvent _onArriveDestination = new();
        private float _lastFootstepsTime = 0;


        private SpriteRenderer GetRenderer()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();

            return _renderer;
        }

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

            WrappedAudioClip audioClip =
                ResourceManager.instance.audioResources.gameplayAudios.attack;

            AudioManager.instance?.PlaySFX(
                audioClip.clip,
                audioClip.volume,
                UnityEngine.Random.Range(0.6f, 1f)
            );
        }

        public async UniTask AttackAsync(Vector2 direction)
        {
            Attack(direction);
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
                    if (character && character.characterType != characterType)
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

        public void Die()
        {
            GetCharacterAnimator().TriggerDead();
            deathVFX.Play();

            WrappedAudioClip audioClip =
                ResourceManager.instance.audioResources.gameplayAudios.dead;

            AudioManager.instance?.PlaySFX(
                audioClip.clip,
                audioClip.volume,
                UnityEngine.Random.Range(0.6f, 1f)
            );

            GetRenderer().enabled = false;
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
            }
        }
    }
}