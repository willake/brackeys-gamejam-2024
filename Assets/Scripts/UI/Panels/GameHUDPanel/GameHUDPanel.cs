using UniRx;
using System;
using Cysharp.Threading.Tasks;
using Game.Events;
using UnityEngine;
using Game.Gameplay;
using UnityEngine.UI;
using Game.RuntimeStates;

namespace Game.UI
{
    public class GameHUDPanel : UIPanel
    {
        public override AvailableUI Type => AvailableUI.GameHUDPanel;

        [Header("References")]
        public GamePhaseTab gamePhaseTab;
        public GamePhaseState gamePhaseState;

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
    }
}