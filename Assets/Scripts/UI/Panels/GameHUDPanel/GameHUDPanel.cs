using UniRx;
using System;
using Cysharp.Threading.Tasks;
using Game.Events;
using UnityEngine;
using Game.Gameplay;
using UnityEngine.UI;
using Game.RuntimeStates;
using UnityEngine.Events;

namespace Game.UI
{
    public class GameHUDPanel : UIPanel
    {
        public override AvailableUI Type => AvailableUI.GameHUDPanel;

        [Header("References")]
        public PlanRuntimeState planRuntimeState;
        public GamePhaseTab gamePhaseTab;
        public GamePhaseState gamePhaseState;
        public WDButton btnPerformPlan;

        public UnityEvent onPerformPlanClickEvent = new();

        private void Start()
        {
            gamePhaseTab
                .OnPhaseSelectObservable
                .ObserveOnMainThread()
                .Subscribe(phase => gamePhaseState.SetValue(phase))
                .AddTo(this);

            gamePhaseState
                .OnValueChanged
                .ObserveOnMainThread()
                .Subscribe(phase => gamePhaseTab.SetPhaseState(phase))
                .AddTo(this);

            btnPerformPlan
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ => onPerformPlanClickEvent.Invoke())
                .AddTo(this);

            planRuntimeState
                .onChangedObservable
                .ObserveOnMainThread()
                .Subscribe(_ => ShowPerformButton(planRuntimeState.moveplans.Count > 0))
                .AddTo(this);
        }

        public override WDButton[] GetSelectableButtons()
        {
            return new WDButton[] { };
        }

        public override void PerformCancelAction()
        {

        }

        public override void Open()
        {
        }

        public override async UniTask OpenAsync()
        {
            await UniTask.CompletedTask;
        }

        public override void Close()
        {
        }

        public override async UniTask CloseAsync()
        {
            await UniTask.CompletedTask;
        }

        public void ShowPerformButton(bool shouldShow)
        {
            btnPerformPlan.gameObject.SetActive(shouldShow);
        }

        private void OnDestroy()
        {
            onPerformPlanClickEvent.RemoveAllListeners();
        }
    }
}