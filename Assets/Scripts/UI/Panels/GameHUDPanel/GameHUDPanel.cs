using UniRx;
using System;
using Cysharp.Threading.Tasks;
using Game.Events;
using UnityEngine;
using Game.Gameplay;
using UnityEngine.UI;
using Game.RuntimeStates;
using UnityEngine.Events;
using UnityEditorInternal;

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
        public GameRuntimeState gameRuntimeState;
        public Dialogue dialogue;

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
                })
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
    }
}