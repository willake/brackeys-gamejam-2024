using System;
using System.Collections;
using System.Collections.Generic;
using Game.Gameplay;
using UnityEngine;

namespace Game.UI
{
    public class AICharacter : Character
    {
        [Header("AI Settings")]
        public float facingDirectionInDegrees = 0f;
        public float sightRangeInDegree = 30f;

        private void Start()
        {
            Vector2 direction = new Vector2(
                Mathf.Cos(facingDirectionInDegrees * Mathf.Deg2Rad),
                Mathf.Sin(facingDirectionInDegrees * Mathf.Deg2Rad)
            );
            GetCharacterAnimator().SetMoveDirection(direction.x, direction.y);
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

            Gizmos.DrawLine(origin, origin + minDirection * 1.5f);
            Gizmos.DrawLine(origin, origin + maxDirection * 1.5f);
        }
    }
}