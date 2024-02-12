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
            planningActionList.onSelectActionObservable
                .ObserveOnMainThread()
                .Subscribe(_ => planningActionList.gameObject.SetActive(false))
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

        public async UniTask OpenActionList(Vector2 mousePosition)
        {
            await UniTask.CompletedTask;
            planningActionList.gameObject.SetActive(true);
            RectTransform listTransform = planningActionList.GetComponent<RectTransform>();
            listTransform.anchoredPosition = mousePosition + (Vector2.up * 10) - (Vector2.right * listTransform.sizeDelta.x / 2);
        }

        public async UniTask<PlanActionType> WaitForSelectAction()
        {
            PlanActionType actionType = await planningActionList.onSelectActionObservable;
            planningActionList.gameObject.SetActive(false);
            return actionType;
        }
    }
}