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
        public GameRuntimeState gameRuntimeState;
        public GameObject prefabPlayer;
        public EchoLocator echoLocator;
        public PlanController planController;
        public PlanPerformer planPerformer;
        public LevelLoader levelLoader;

        public GameState GameState { get => gameRuntimeState.Value; }

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
            gameRuntimeState.SetValue(GameState.Start);
            await OnGameStart();

            gameRuntimeState.SetValue(GameState.EchoLocation);
            // wait for echo location done

            gameRuntimeState.SetValue(GameState.Plan);
            await planController.onPlanSet.AsObservable().Take(1);
            // wait for planning down

            gameRuntimeState.SetValue(GameState.Perform);
            await planPerformer.PerformPlan(_player);
            // wait for perform

            // wait for game end
            bool isWin = _level.AreAllEnemiesDead();

            gameRuntimeState.SetValue(GameState.End);
            await OnGameEnd(isWin);
        }

        private Character GeneratePlayer(Transform parent, Vector3 position)
        {
            GameObject playerObj = Instantiate(prefabPlayer, parent);
            playerObj.name = "NinjaPlayer";
            playerObj.transform.position = position;

            return playerObj.GetComponent<Character>();
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
