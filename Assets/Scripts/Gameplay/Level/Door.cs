using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    public class Door : MonoBehaviour
    {
        private Collider2D _col;
        private Animator _animator;

        private Collider2D GetCollider2D()
        {
            if (_col == null) _col = GetComponent<Collider2D>();

            return _col;
        }
        private Animator GetAnimator()
        {
            if (_animator == null) _animator = GetComponent<Animator>();

            return _animator;
        }
        public void Open()
        {
            GetCollider2D().enabled = false;
            GetAnimator().SetTrigger("Open");
        }

        public void Close()
        {
            GetCollider2D().enabled = true;
            GetAnimator().SetTrigger("Close");
        }
    }
}