using System;
using System.Collections;
using System.Collections.Generic;
using Game.Gameplay;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Game.UI
{
    public class PlanningActionList : MonoBehaviour
    {
        [Header("References")]
        public WDButton btnAttack;
        public WDButton btnIdle;

        public OnSelectActionEvent onSelectActionEvent = new OnSelectActionEvent();
        public IObservable<PlanActionType> onSelectActionObservable
        {
            get => onSelectActionEvent.AsObservable();
        }

        private void Start()
        {
            btnAttack
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ => onSelectActionEvent.Invoke(PlanActionType.Attack))
                .AddTo(this);

            btnIdle
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ => onSelectActionEvent.Invoke(PlanActionType.Idle))
                .AddTo(this);
        }

        public class OnSelectActionEvent : UnityEvent<PlanActionType> { }
    }
}