using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    public class MenuCharacter : Character
    {
        [Header("Menu Character Settings")]
        public float facingDirectionInDegrees = 0f;
        protected override void Start()
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
        }
    }
}