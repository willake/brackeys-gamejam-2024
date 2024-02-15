using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.UI;
using Cysharp.Threading.Tasks;
using Game.RuntimeStates;
using UniRx;

namespace Game.Gameplay
{
    public class MainGameScene : GameScene
    {
        private GameHUDPanel _gameHUDPanel;
        private Level _level;
        private Character _player;

        [Header("References")]
        public GameObject prefabPlayer;
        public EchoLocator echoLocator;
        public PlanController planController;
        public PlanPresenter planPresenter;
        public PlanPerformer planPerformer;
        public LevelLoader levelLoader;
        public GamePhaseState phaseState;

        private void Awake()
        {
            GameManager.instance.gameScene = this;

            phaseState
                .OnValueChanged
                .ObserveOnMainThread()
                .Subscribe(phase => ChangeGamePhase(phase))
                .AddTo(this);
        }

        private async void Start()
        {
            if (GameManager.instance)
            {
                await levelLoader.LoadLevel(GameManager.instance.levelToLoad);
            }
            else
            {
                await levelLoader.LoadLevel(AvailableLevel.Test);
            }
        }

        public async UniTask StartLevel(Level level)
        {
            _level = level;

            // spawn character
            _player = GeneratePlayer(level.transform, level.startPoint.position);

            echoLocator.Door = _player.transform;
            echoLocator.Init();

            // TODO Show intro like "Game Start" 
            await OnGameStart();

            // Show Game HUD, it contains a button to switch between Echo Locating and Planning Mode
            _gameHUDPanel = UIManager.instance.OpenUI(AvailableUI.GameHUDPanel) as GameHUDPanel;
            planController.planningPanel = UIManager.instance.OpenUI(AvailableUI.PlanningPanel) as PlanningPanel;

            _gameHUDPanel.onPerformPlanClickEvent.AddListener(PerformPlan);

            Reset();
        }

        public void PerformPlan()
        {
            planPerformer.PerformPlan(_player);
        }

        private void Reset()
        {
            planController.Init(_level.startPoint.position);

            // TODO Setup everything

            // Set EnterLocationMode as default
            phaseState.SetValue(GamePhase.EchoLocation);
        }

        private Character GeneratePlayer(Transform parent, Vector3 position)
        {
            GameObject playerObj = Instantiate(prefabPlayer, parent);
            playerObj.name = "NinjaPlayer";
            playerObj.transform.position = position;

            return playerObj.GetComponent<Character>();
        }

        public void ChangeGamePhase(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.EchoLocation:
                    Debug.Log("Enter EchoLocation Phase");
                    planController.canPlan = false;
                    planPresenter.SetVisisble(false);
                    break;
                case GamePhase.Planning:
                    Debug.Log("Enter Planning Phase");
                    planController.canPlan = true;
                    planPresenter.SetVisisble(true);
                    break;
            }
        }

        private async UniTask OnGameStart()
        {
            await UniTask.CompletedTask;
        }

        private async UniTask OnGameEnd()
        {
            await UniTask.CompletedTask;
        }

        public async UniTask NavigateToMenu()
        {
            await levelLoader.UnloadCurrentLevel();
            GameManager.instance.SwitchScene(AvailableScene.Menu);
        }
    }
}
