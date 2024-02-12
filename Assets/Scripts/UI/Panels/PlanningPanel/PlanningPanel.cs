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
    public class PlanningPanel : UIPanel
    {
        public override AvailableUI Type => AvailableUI.PlanningPanel;

        private void Start()
        {

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

        public async UniTask OpenActionList(Vector2 mousePosition)
        {
            await UniTask.CompletedTask;
        }

        public async UniTask WaitForSelectAction()
        {
            await UniTask.CompletedTask;
            // await UniTask<PlanActionType>.;
        }
    }
}