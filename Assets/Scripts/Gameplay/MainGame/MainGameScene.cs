using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.UI;
using Cysharp.Threading.Tasks;
using Game.RuntimeStates;
using UniRx;
using UnityEngine.Events;

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
        public GameState gameState;
        public GamePhaseState phaseState;

        private LevelEndEvent _onLevelEnd = new();

        private void Awake()
        {
            GameManager.instance.gameScene = this;
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

            // show Game HUD, it contains a button to switch between Echo Locating and Planning phase
            _gameHUDPanel = UIManager.instance.OpenUI(AvailableUI.GameHUDPanel) as GameHUDPanel;
            planController.planningPanel = UIManager.instance.OpenUI(AvailableUI.PlanningPanel) as PlanningPanel;

            _gameHUDPanel.onPerformPlanClickEvent.AsObservable().ObserveOnMainThread().Subscribe(_ => PerformPlan()).AddTo(this);

            phaseState.OnValueChanged.ObserveOnMainThread().Subscribe(phase => SetGamePhase(phase)).AddTo(this);
        }

        public async UniTask PlayLevel(Level level)
        {
            _level = level;

            // delete player from previous level
            if (_player) Destroy(_player.gameObject);

            // spawn character
            _player = GeneratePlayer(level.transform, level.startPoint.position);

            echoLocator._door = _player.transform;
            echoLocator.Init();
            planController.Init(level.startPoint.position, level.maxMoves, level.maxActions);

            // TODO Show intro like "Game Start" 
            SetGameState(GameState.Start);
            await OnGameStart();

            SetGameState(GameState.InGame);

            // wait for game end
            bool isWin = await _onLevelEnd.AsObservable().Take(1);

            SetGameState(GameState.End);
            await OnGameEnd(isWin);
        }

        public async void PerformPlan()
        {
            phaseState.SetValue(GamePhase.Perform);
            await planPerformer.PerformPlan(_player);
            // check if all enemies are dead, if yes, the game win
            _onLevelEnd.Invoke(_level.AreAllEnemiesDead());
        }

        private Character GeneratePlayer(Transform parent, Vector3 position)
        {
            GameObject playerObj = Instantiate(prefabPlayer, parent);
            playerObj.name = "NinjaPlayer";
            playerObj.transform.position = position;

            return playerObj.GetComponent<Character>();
        }

        public void SetGameState(GameState state)
        {
            switch (state)
            {
                case GameState.Start:
                    break;
                case GameState.InGame:
                    // set EnterLocationMode as default
                    phaseState.SetValue(GamePhase.EchoLocation);
                    break;
                case GameState.End:
                    break;
            }

            gameState = state;
        }

        public void SetGamePhase(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.EchoLocation:
                    planController.canPlan = false;
                    planPresenter.SetVisisble(false);
                    break;
                case GamePhase.Planning:
                    planController.canPlan = true;
                    planPresenter.SetVisisble(true);
                    break;
                case GamePhase.Perform:
                    planController.canPlan = false;
                    planPresenter.SetVisisble(false);
                    break;
            }
        }

        private async UniTask OnGameStart()
        {
            await UniTask.CompletedTask;
        }

        private async UniTask OnGameEnd(bool isWin)
        {
            await UniTask.CompletedTask;
        }

        public async UniTask NavigateToMenu()
        {
            await levelLoader.UnloadCurrentLevel();
            GameManager.instance.SwitchScene(AvailableScene.Menu);
        }

        public class LevelEndEvent : UnityEvent<bool> { }
    }
}
