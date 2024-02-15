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

            echoLocator.Door = _player.transform;
            echoLocator.Init();
            planController.Init(level.startPoint.position, level.maxMoves, level.maxActions);

            // show intro like "Game Start" 
            gameRuntimeState.SetValue(GameState.Start);
            await OnGameStart();

            gameRuntimeState.SetValue(GameState.EchoLocation);
            // wait for echo location done

            gameRuntimeState.SetValue(GameState.Plan);
            // wait for planning dowe
            await planController.onPlanSet.AsObservable().Take(1);

            gameRuntimeState.SetValue(GameState.Perform);
            // wait for perform
            await planPerformer.PerformPlan(_player);

            // wait for game end
            bool isWin = _level.AreAllEnemiesDead();

            // show end game panel
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
            GameStartPanel startPanel = await UIManager.instance.OpenUIAsync(AvailableUI.GameStartPanel) as GameStartPanel;
            await startPanel.ShowText("Game Start", 1, DG.Tweening.Ease.InOutSine);
            await UIManager.instance.PrevAsync();
        }

        private async UniTask OnGameEnd(bool isWin)
        {
            GameEndPanel endPanel = await UIManager.instance.OpenUIAsync(AvailableUI.GameEndPanel) as GameEndPanel;
            endPanel.SetEndGameState(isWin);
        }

        public async UniTask NavigateToMenu()
        {
            await levelLoader.UnloadCurrentLevel();
            GameManager.instance.SwitchScene(AvailableScene.Menu);
        }

        public class LevelEndEvent : UnityEvent<bool> { }
    }
}
