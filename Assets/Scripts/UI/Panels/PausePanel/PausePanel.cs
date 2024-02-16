using UniRx;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Game.Gameplay;
using Game.Audios;

namespace Game.UI
{
    public class PausePanel : UIPanel
    {
        public override AvailableUI Type { get => AvailableUI.PausePanel; }

        [Header("References")]
        public WDButton btnResume;
        public WDButton btnRetry;
        public WDButton btnMenu;

        [Header("Settings")]
        public float fadeDuration = 0.2f;
        public Ease fadeEase = Ease.InSine;
        private CanvasGroup _canvasGroup;

        public CanvasGroup GetCanvasGroup()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

            return _canvasGroup;
        }

        private void Start()
        {
            btnResume
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(async _ =>
                {
                    await UIManager.instance.PrevAsync();
                    GameManager.instance.ResumeGame();
                })
                .AddTo(this);

            btnRetry
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(async _ =>
                {
                    GameManager.instance.ResumeGame();
                    MainGameScene mainGameScene = GameManager.instance.gameScene as MainGameScene;
                    mainGameScene.RetryCurrentLevel();
                    await UIManager.instance.PrevAsync();
                })
                .AddTo(this);

            btnMenu
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ =>
                {
                    GameManager.instance.ResumeGame();
                    MainGameScene mainGameScene = GameManager.instance.gameScene as MainGameScene;
                    mainGameScene.NavigateToMenu().Forget();
                })
                .AddTo(this);
        }

        public override WDButton[] GetSelectableButtons()
        {
            return new WDButton[0];
        }

        public override void PerformCancelAction()
        {

        }

        public override void Open()
        {
            GetCanvasGroup().alpha = 1;
            gameObject.SetActive(true);
            GameManager.instance.PauseGame();
            WrappedAudioClip audioClip = ResourceManager.instance.audioResources.uiAudios.UIOpen;
            AudioManager.instance.PlaySFX(audioClip.clip, audioClip.volume);
        }

        public override async UniTask OpenAsync()
        {
            gameObject.SetActive(true);
            GameManager.instance.PauseGame();
            GetCanvasGroup().alpha = 0;

            WrappedAudioClip audioClip = ResourceManager.instance.audioResources.uiAudios.UIOpen;
            AudioManager.instance.PlaySFX(audioClip.clip, audioClip.volume);

            await GetCanvasGroup()
                .DOFade(1, fadeDuration)
                .SetEase(fadeEase).SetUpdate(true).AsyncWaitForCompletion();
        }

        public override void Close()
        {
            gameObject.SetActive(false);
            GameManager.instance.ResumeGame();

            WrappedAudioClip audioClip = ResourceManager.instance.audioResources.uiAudios.UIClose;
            AudioManager.instance.PlaySFX(audioClip.clip, audioClip.volume);
        }

        public override async UniTask CloseAsync()
        {
            GetCanvasGroup().alpha = 1;

            WrappedAudioClip audioClip = ResourceManager.instance.audioResources.uiAudios.UIClose;
            AudioManager.instance.PlaySFX(audioClip.clip, audioClip.volume);

            await GetCanvasGroup()
                .DOFade(0, fadeDuration)
                .SetEase(fadeEase).SetUpdate(true).AsyncWaitForCompletion();
            gameObject.SetActive(false);
            GameManager.instance.ResumeGame();
        }
    }
}