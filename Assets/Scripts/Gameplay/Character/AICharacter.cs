using Game.Audios;
using Game.Gameplay;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.UI
{
    public class AICharacter : Character
    {
        [Header("AI Character References")]
        public LineRenderer sightStartRenderer;
        public LineRenderer sightEndRenderer;

        private float _facingDirection = 0f;
        private float _sightRange = 30f;
        private float _sightDistance = 1.5f;
        public LayerMask playerLayermask;

        private Light2D _light2D;
        private bool _isDetected = false;
        public bool IsDetected { get => _isDetected; }

        private Light2D GetLight2D()
        {
            if (_light2D == null) _light2D = GetComponent<Light2D>();

            return _light2D;
        }

        protected override void Start()
        {
            playerPositionState
                .OnValueChanged
                .Where(_ => State != CharacterStates.DeadState)
                .ObserveOnMainThread()
                .Subscribe(pos => AttackIfDetected(pos))
                .AddTo(this);
        }

        public void Init()
        {
            Init(_facingDirection, _sightRange, _sightDistance);
        }

        public void Init(float facingDirection, float sightRange, float sightDistance)
        {
            Vector2 direction = new Vector2(
                Mathf.Cos(facingDirection * Mathf.Deg2Rad),
                Mathf.Sin(facingDirection * Mathf.Deg2Rad)
            );
            GetCharacterAnimator().SetMoveDirection(direction.x, direction.y);

            Vector2 origin = transform.position;
            float minAngle = facingDirection - (sightRange / 2);
            float maxAngle = facingDirection + (sightRange / 2);

            Vector2 minDirection = new Vector3(
                Mathf.Cos(minAngle * Mathf.Deg2Rad),
                Mathf.Sin(minAngle * Mathf.Deg2Rad),
                0
            );
            Vector2 maxDirection = new Vector3(
                Mathf.Cos(maxAngle * Mathf.Deg2Rad),
                Mathf.Sin(maxAngle * Mathf.Deg2Rad),
                0
            );

            Vector2 sightStartTip = origin + minDirection * sightDistance;
            Vector2 sightEndTip = origin + maxDirection * sightDistance;

            sightStartRenderer.SetPositions(new Vector3[] { origin, sightStartTip });
            sightEndRenderer.SetPositions(new Vector3[] { origin, sightEndTip });
            sightStartRenderer.gameObject.SetActive(false);
            sightEndRenderer.gameObject.SetActive(false);

            _facingDirection = facingDirection;
            _sightRange = sightRange;
            _sightDistance = sightDistance;
        }

        public void SetIsDetected(bool isDetected)
        {
            _isDetected = isDetected;
            sightStartRenderer.gameObject.SetActive(isDetected);
            sightEndRenderer.gameObject.SetActive(isDetected);
        }

        public override void Reset()
        {
            base.Reset();

            Vector2 direction = new Vector2(
                Mathf.Cos(_facingDirection * Mathf.Deg2Rad),
                Mathf.Sin(_facingDirection * Mathf.Deg2Rad)
            );
            GetCharacterAnimator().SetMoveDirection(direction.x, direction.y);

            SetIsDetected(false);

            GetLight2D().enabled = false;
        }

        public override void Die(Vector2 attackDirection)
        {
            GetLight2D().enabled = true;

            base.Die(attackDirection);
        }

        private void AttackIfDetected(Vector2 playerPosition)
        {
            if (State.canAttack == false) return;
            // Calculate direction from enemy to player
            Vector2 origin = transform.position;
            Vector2 directionToPlayer = playerPosition - origin;

            Vector3 facingDirection = new Vector3(
                Mathf.Cos(_facingDirection * Mathf.Deg2Rad),
                Mathf.Sin(_facingDirection * Mathf.Deg2Rad),
                0
            );

            // Check if player is within sight distance
            if (directionToPlayer.magnitude < _sightDistance)
            {
                // Calculate angle between forward vector of enemy and direction to player
                float angleToPlayer = Vector2.Angle(facingDirection, directionToPlayer);

                // Check if angle is within sight angle
                if (angleToPlayer <= _sightRange * 0.5f)
                {
                    // Perform a raycast to ensure there are no obstacles blocking the view
                    RaycastHit2D hit = Physics2D.Raycast(origin, directionToPlayer, _sightDistance, playerLayermask);
                    if (hit.collider != null)
                    {
                        GetLight2D().enabled = true;

                        GetCharacterAnimator().SetMoveDirection(directionToPlayer.x, directionToPlayer.y);

                        int random = Random.Range(0, 1);
                        WrappedAudioClip audioClip = random == 0
                            ? ResourceManager.instance.audioResources.gameplayAudios.pistol1
                            : ResourceManager.instance.audioResources.gameplayAudios.pistol2;

                        AudioManager.instance.PlaySFX(audioClip.clip, audioClip.volume, Random.Range(0.8f, 1.2f));

                        Character player = hit.collider.GetComponent<Character>();
                        if (player && player.State != CharacterStates.DeadState) player.Die(directionToPlayer);
                    }
                }
            }
        }

        // private void OnDrawGizmos()
        // {
        //     Vector3 origin = transform.position;
        //     origin.z = -1;
        //     Vector3 direction = new Vector3(
        //         Mathf.Cos(_facingDirection * Mathf.Deg2Rad),
        //         Mathf.Sin(_facingDirection * Mathf.Deg2Rad),
        //         0
        //     );
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawLine(origin, origin + direction * 1.5f);

        //     // green for sight range
        //     Gizmos.color = Color.green;
        //     float minAngle = _facingDirection - (_sightRange / 2);
        //     float maxAngle = _facingDirection + (_sightRange / 2);

        //     Vector3 minDirection = new Vector3(
        //         Mathf.Cos(minAngle * Mathf.Deg2Rad),
        //         Mathf.Sin(minAngle * Mathf.Deg2Rad),
        //         0
        //     );
        //     Vector3 maxDirection = new Vector3(
        //         Mathf.Cos(maxAngle * Mathf.Deg2Rad),
        //         Mathf.Sin(maxAngle * Mathf.Deg2Rad),
        //         0
        //     );

        //     Gizmos.DrawLine(origin, origin + minDirection * _sightDistance);
        //     Gizmos.DrawLine(origin, origin + maxDirection * _sightDistance);
        // }
    }
}