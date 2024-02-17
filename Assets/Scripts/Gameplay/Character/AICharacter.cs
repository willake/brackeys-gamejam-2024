using System;
using System.Collections;
using System.Collections.Generic;
using Game.Gameplay;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.UI
{
    public class AICharacter : Character
    {
        [Header("AI Settings")]
        public float facingDirectionInDegrees = 0f;
        public float sightRangeInDegree = 30f;
        public float sightDistance = 1.5f;
        public LayerMask playerLayermask;

        private Light2D _light2D;

        private Light2D GetLight2D()
        {
            if (_light2D == null) _light2D = GetComponent<Light2D>();

            return _light2D;
        }

        protected override void Start()
        {
            Vector2 direction = new Vector2(
                Mathf.Cos(facingDirectionInDegrees * Mathf.Deg2Rad),
                Mathf.Sin(facingDirectionInDegrees * Mathf.Deg2Rad)
            );
            GetCharacterAnimator().SetMoveDirection(direction.x, direction.y);

            playerPositionState
                .OnValueChanged
                .Where(_ => State != CharacterStates.DeadState)
                .ObserveOnMainThread()
                .Subscribe(pos => AttackIfDetected(pos))
                .AddTo(this);
        }

        public override void Reset()
        {
            base.Reset();

            GetLight2D().enabled = false;
        }

        public override void Die()
        {
            GetLight2D().enabled = true;

            base.Die();
        }

        private void AttackIfDetected(Vector2 playerPosition)
        {
            // Calculate direction from enemy to player
            Vector2 origin = transform.position;
            Vector2 directionToPlayer = playerPosition - origin;

            Vector3 facingDirection = new Vector3(
                Mathf.Cos(facingDirectionInDegrees * Mathf.Deg2Rad),
                Mathf.Sin(facingDirectionInDegrees * Mathf.Deg2Rad),
                0
            );

            // Check if player is within sight distance
            if (directionToPlayer.magnitude < sightDistance)
            {
                // Calculate angle between forward vector of enemy and direction to player
                float angleToPlayer = Vector2.Angle(facingDirection, directionToPlayer);

                // Check if angle is within sight angle
                if (angleToPlayer <= sightRangeInDegree * 0.5f)
                {
                    // Perform a raycast to ensure there are no obstacles blocking the view
                    RaycastHit2D hit = Physics2D.Raycast(origin, directionToPlayer, sightDistance, playerLayermask);
                    if (hit.collider != null)
                    {
                        GetLight2D().enabled = true;

                        Character player = hit.collider.GetComponent<Character>();
                        if (player && player.State != CharacterStates.DeadState) player.Die();
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 origin = transform.position;
            origin.z = -1;
            Vector3 direction = new Vector3(
                Mathf.Cos(facingDirectionInDegrees * Mathf.Deg2Rad),
                Mathf.Sin(facingDirectionInDegrees * Mathf.Deg2Rad),
                0
            );
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + direction * 1.5f);

            // green for sight range
            Gizmos.color = Color.green;
            float minAngle = facingDirectionInDegrees - (sightRangeInDegree / 2);
            float maxAngle = facingDirectionInDegrees + (sightRangeInDegree / 2);

            Vector3 minDirection = new Vector3(
                Mathf.Cos(minAngle * Mathf.Deg2Rad),
                Mathf.Sin(minAngle * Mathf.Deg2Rad),
                0
            );
            Vector3 maxDirection = new Vector3(
                Mathf.Cos(maxAngle * Mathf.Deg2Rad),
                Mathf.Sin(maxAngle * Mathf.Deg2Rad),
                0
            );

            Gizmos.DrawLine(origin, origin + minDirection * sightDistance);
            Gizmos.DrawLine(origin, origin + maxDirection * sightDistance);
        }
    }
}