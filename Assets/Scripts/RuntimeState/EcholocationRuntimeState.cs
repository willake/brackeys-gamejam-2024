using System;
using Game.Gameplay;
using UniRx;
using UnityEngine;

namespace Game.RuntimeStates
{
    [CreateAssetMenu(fileName = "EcholocationRuntimeState", menuName = "MyGame/RuntimeStates/EcholocationRuntimeState", order = 0)]
    public class EcholocationRuntimeState : ScriptableObject
    {
        [SerializeField]
        private ReactiveProperty<int> _shotRays;
        public int maxRays;
        public int ShotRays { get => _shotRays.Value; }

        public IObservable<int> OnShotRaysChanged
        {
            get => _shotRays.AsObservable();
        }

        public void SetShotRays(int v)
        {
            _shotRays.Value = v;
        }
    }
}