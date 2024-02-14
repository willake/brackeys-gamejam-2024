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

        [Header("References")]
        public GameObject prefabPlayer;
        public PlanningController planningController;
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
            GeneratePlayer(level.transform, level.startPoint.position);

            // TODO Show intro like "Game Start" 
            await OnGameStart();

            // Show Game HUD, it contains a button to switch between Echo Locating and Planning Mode
            _gameHUDPanel = UIManager.instance.OpenUI(AvailableUI.GameHUDPanel) as GameHUDPanel;
            planningController.planningPanel = UIManager.instance.OpenUI(AvailableUI.PlanningPanel) as PlanningPanel;
            planningController.Init(level.startPoint.position);

            // TODO Setup everything

            // Set EnterLocationMode as default
            phaseState.SetValue(GamePhase.EchoLocation);
        }

        public void GeneratePlayer(Transform parent, Vector3 position)
        {
            GameObject playerObj = Instantiate(prefabPlayer, parent);
            playerObj.name = "NinjaPlayer";
            playerObj.transform.position = position;
        }

        public void ChangeGamePhase(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.EchoLocation:
                    Debug.Log("Enter EchoLocation Phase");
                    planningController.canPlan = false;
                    break;
                case GamePhase.Planning:
                    Debug.Log("Enter Planning Phase");
                    planningController.canPlan = true;
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
