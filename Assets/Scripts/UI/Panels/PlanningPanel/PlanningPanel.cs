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

        [Header("References")]
        public PlanningActionList planningActionList;

        private void Start()
        {
            // planningActionList.onSelectActionObservable
            //     .ObserveOnMainThread()
            //     .Subscribe(_ => planningActionList.gameObject.SetActive(false))
            //     .AddTo(this);
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
            OpenAsync().Forget();
        }

        public override async UniTask OpenAsync()
        {
            planningActionList.gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }

        public override void Close()
        {
        }

        public override async UniTask CloseAsync()
        {
            await UniTask.CompletedTask;
        }

        public void SetActionListVisible(bool isVisible)
        {
            planningActionList.gameObject.SetActive(isVisible);
        }

        public void HighlightAction(int idx)
        {
            planningActionList.Highlight(idx);
            Debug.Log($"Highlight {idx}");
        }
    }
}