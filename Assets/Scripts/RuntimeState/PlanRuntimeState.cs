using System;
using Game.Gameplay;
using UniRx;
using UnityEngine;

namespace Game.RuntimeStates
{
    [CreateAssetMenu(fileName = "PlanRuntimeState", menuName = "MyGame/RuntimeStates/PlanRuntimeState", order = 0)]
    public class PlanRuntimeState : ScriptableObject
    {
        public ReactiveCollection<MovePlanNode> moveplans;
        public ReactiveCollection<ActionPlanNode> actionPlans;
        public ReactiveProperty<bool> isPlanFilled;

        public IObservable<Unit> onChangedObservable
        {
            get => moveplans.ObserveCountChanged().AsUnitObservable()
                .Merge(moveplans.ObserveReplace().AsUnitObservable())
                .Merge(actionPlans.ObserveCountChanged().AsUnitObservable())
                .Merge(actionPlans.ObserveReplace().AsUnitObservable());
        }
    }
}