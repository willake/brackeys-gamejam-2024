using System;
using Game.Gameplay;
using UniRx;
using UnityEngine;

namespace Game.RuntimeStates
{
    [CreateAssetMenu(fileName = "PlanRuntimeState", menuName = "MyGame/RuntimeStates/PlanRuntimeState", order = 0)]
    public class PlanRuntimeState : ScriptableObject
    {
        public ReactiveCollection<PlanNode> Value;

        public IObservable<Unit> onChangedObservable
        {
            get => Value.ObserveCountChanged().AsUnitObservable()
                .Merge(Value.ObserveReplace().AsUnitObservable());
        }
    }
}