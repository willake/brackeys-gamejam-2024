using UniRx;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Audios;
using Game.Gameplay;
using Game.Saves;
using System.Linq;
using System.Collections.Generic;

namespace Game.UI
{
    public class GameEndPanel : UIPanel
    {
        public override AvailableUI Type => AvailableUI.GameEndPanel;

        [Header("References")]
        public GameEndTitle title;
        public WDButton btnRetry;
        public WDButton btnNextLevel;
        public WDTextButton btnMenu;

        private CanvasGroup _canvasGroup;

        [Header("Fade animation settings")]
        public float fadeDuration = 0.2f;
        public Ease fadeEase = Ease.InSine;

        private void Start()
        {
            btnMenu
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ => GoMainMenu())
                .AddTo(this);

            btnRetry
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ => Retry())
                .AddTo(this);

            btnNextLevel
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ => GoNextLevel())
                .AddTo(this);
        }

        public override WDButton[] GetSelectableButtons()
        {
            return new WDButton[] {
                btnMenu
            };
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
            GetCanvasGroup().alpha = 0;
            gameObject.SetActive(true);
            await UniTask.CompletedTask;
        }

        public override void Close()
        {
            gameObject.SetActive(false);
        }

        public override async UniTask CloseAsync()
        {
            gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }

        private string ParseTimeText(long time)
        {
            long minutes = time / 60;
            long seconds = time % 60;
            string minutesText = minutes > 9 ? $"{minutes}" : $"0{minutes}";
            string secondsText = seconds > 9 ? $"{seconds}" : $"0{seconds}";
            return $"{minutesText}:{secondsText}";
        }

        private void Retry()
        {
            UIManager.instance.Prev();
            (GameManager.instance.gameScene as MainGameScene).RetryCurrentLevel();
        }

        private void GoNextLevel()
        {
            UIManager.instance.Prev();
            (GameManager.instance.gameScene as MainGameScene).NextLevel();
        }

        private void GoMainMenu()
        {
            (GameManager.instance.gameScene as MainGameScene).NavigateToMenu().Forget();
        }

        private void OnDestroy()
        {
            btnMenu.StopAnimation();
        }

        private CanvasGroup GetCanvasGroup()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

            return _canvasGroup;
        }

        public void SetEndGameState(bool isWin, bool isPlayerDead, bool areEnemiesKilled, bool hasNextLevel)
        {
            WrappedAudioClip endSFX = isWin
                ? ResourceManager.instance.audioResources.gameplayAudios.stingWin
                : ResourceManager.instance.audioResources.gameplayAudios.stingLose;
            AudioManager.instance.PlayUI(endSFX.clip, endSFX.volume);

            if (isWin)
            {
                string winText = hasNextLevel
                    ? ResourceManager.instance.dialogueResources.winAndMoreLevel
                    : ResourceManager.instance.dialogueResources.winAndLastLevel;
                title.PlayWinAnimation(winText);
                GetCanvasGroup().DOFade(1, fadeDuration).SetEase(fadeEase).SetUpdate(true);
            }
            else
            {
                string loseText = isPlayerDead
                    ? ResourceManager.instance.dialogueResources.loseCuzGetKilled
                    : ResourceManager.instance.dialogueResources.loseCuzMoreEnemies;
                title.PlayLoseAnimation(loseText);
                GetCanvasGroup().DOFade(1, fadeDuration).SetEase(fadeEase).SetUpdate(true);
            }

            btnRetry.gameObject.SetActive(!isWin);
            btnNextLevel.gameObject.SetActive(isWin && hasNextLevel);
        }

        public enum EndState
        {
            Win,
            Lose
        }
    }
}