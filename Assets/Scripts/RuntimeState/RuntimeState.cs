using System;
using UniRx;
using UnityEngine;

namespace Game.RuntimeStates
{
    public class RuntimeState<T> : ScriptableObject
    {
        // public T value;
        [SerializeField]
        private ReactiveProperty<T> _value;

        public IObservable<T> OnValueChanged
        {
            get => _value.AsObservable();
        }

        public void SetValue(T v)
        {
            _value.Value = v;
        }
    }
}