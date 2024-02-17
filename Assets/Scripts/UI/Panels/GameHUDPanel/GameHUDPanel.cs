using UniRx;
using System;
using Cysharp.Threading.Tasks;
using Game.Events;
using UnityEngine;
using Game.Gameplay;
using Game.RuntimeStates;


namespace Game.UI
{
    public class GameHUDPanel : UIPanel
    {
        public override AvailableUI Type => AvailableUI.GameHUDPanel;

        public const string IDENTITY = "GAME_HUD_PANEL";
        private Lazy<EventManager> _eventManager = new Lazy<EventManager>(
            () => DIContainer.instance.GetObject<EventManager>(),
            true
        );
        protected EventManager EventManager { get => _eventManager.Value; }

        [Header("References")]
        public EcholocationRuntimeState echolocationRuntimeState;
        public PlanRuntimeState planRuntimeState;
        public GameRuntimeState gameRuntimeState;
        public WDText echoIndicator;
        public WDText movePlansIndicator;
        public WDText actionPlansIndicator;
        public Dialogue dialogue;
        public PhaseIndicator phaseIndicator;

        private Subscription _dialogueEventSubscription;
        private CanvasGroup _canvasGroup;

        public CanvasGroup GetCanvasGroup()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

            return _canvasGroup;
        }

        private void Start()
        {
            dialogue.gameObject.SetActive(false);

            _dialogueEventSubscription =
                EventManager.Subscribe(IDENTITY, EventNames.presentDialogue,
                (payload) =>
                {
                    string dialogueText = (string)payload.args[0];

                    if (dialogueText == string.Empty)
                    {
                        dialogue.gameObject.SetActive(false);
                    }
                    else
                    {
                        dialogue.gameObject.SetActive(true);
                        dialogue.Present(dialogueText);
                    }
                });

            gameRuntimeState
                .OnValueChanged
                .ObserveOnMainThread()
                .Subscribe(state =>
                {
                    if (state == GameState.EchoLocation || state == GameState.Plan || state == GameState.Perform)
                    {
                        GetCanvasGroup().alpha = 1;
                    }
                    else
                    {
                        GetCanvasGroup().alpha = 0;
                    }

                    echoIndicator.gameObject.SetActive(state == GameState.EchoLocation);
                    movePlansIndicator.gameObject.SetActive(state == GameState.Plan);
                    actionPlansIndicator.gameObject.SetActive(state == GameState.Plan);

                    phaseIndicator.SetState(state);
                })
                .AddTo(this);

            planRuntimeState
                .onChangedObservable
                .DoOnSubscribe(() => UpdatePlanInfo())
                .ObserveOnMainThread()
                .Subscribe(_ => UpdatePlanInfo())
                .AddTo(this);

            echolocationRuntimeState
                .OnShotRaysChanged
                .DoOnSubscribe(() => UpdateEcholocationInfo())
                .ObserveOnMainThread()
                .Subscribe(_ => UpdateEcholocationInfo())
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
            gameObject.SetActive(true);
            GetCanvasGroup().alpha = 0;
        }

        public override async UniTask OpenAsync()
        {
            Open();
            await UniTask.CompletedTask;
        }

        public override void Close()
        {
            gameObject.SetActive(false);
        }

        public override async UniTask CloseAsync()
        {
            Close();
            await UniTask.CompletedTask;
        }

        private void UpdateEcholocationInfo()
        {
            echoIndicator.text = $"Rays {echolocationRuntimeState.ShotRays}/{echolocationRuntimeState.maxRays}";
        }

        private void UpdatePlanInfo()
        {
            movePlansIndicator.text = $"Moves {planRuntimeState.moveplans.Count}/{planRuntimeState.maxMoves}";
            actionPlansIndicator.text = $"Actions {planRuntimeState.actionPlans.Count}/{planRuntimeState.maxActions}";
        }

        private void OnDestroy()
        {
            EventManager.CancelSubscription(EventNames.presentDialogue, _dialogueEventSubscription);
        }
    }
}