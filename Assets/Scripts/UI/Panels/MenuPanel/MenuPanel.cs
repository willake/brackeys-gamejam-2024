using UniRx;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.UI
{
    public class MenuPanel : UIPanel
    {
        public override AvailableUI Type => AvailableUI.MenuPanel;

        [Header("References")]
        public WDTextButton btnPlay;
        public WDTextButton btnHowToPlay;
        public WDTextButton btnSettings;
        public WDTextButton btnExit;
        public WDText txtVersion;

        private void Start()
        {
            btnPlay
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ =>
                {
                    GameManager.instance.SwitchScene(AvailableScene.MainGame);
                })
                .AddTo(this);

            btnHowToPlay
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ =>
                {
                    UIManager.instance.Prev();
                    UIManager.instance.OpenUI(AvailableUI.HowToPlayPanel);
                })
                .AddTo(this);

            btnSettings
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ =>
                {
                    UIManager.instance.Prev();
                    UIManager.instance.OpenUI(AvailableUI.SettingsPanel);
                })
                .AddTo(this);

#if UNITY_WEBGL
            btnExit.gameObject.SetActive(false);
#else
            btnExit.gameObject.SetActive(true);
            btnExit
                .OnClickObservable
                .ObserveOnMainThread()
                .Subscribe(_ => GameManager.instance.ExitGame())
                .AddTo(this);
#endif
            txtVersion.text = Consts.VERSION;
        }
        public override WDButton[] GetSelectableButtons()
        {
            return new WDButton[] {
                btnPlay,
                btnSettings,
                btnExit
            };
        }

        public override void PerformCancelAction()
        {

        }

        public override void Open()
        {
            gameObject.SetActive(true);
        }

        public override async UniTask OpenAsync()
        {
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
    }
}