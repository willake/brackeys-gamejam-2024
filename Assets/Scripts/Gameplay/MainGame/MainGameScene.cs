using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.UI;
using Cysharp.Threading.Tasks;

namespace Game.Gameplay
{
    public class MainGameScene : GameScene
    {
        private GameHUDPanel _gameHUDPanel;

        [Header("References")]
        public LevelLoader levelLoader;

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

            // TODO Show intro like "Game Start" 
            await OnGameStart();

            // Show Game HUD, it contains a button to switch between Echo Locating and Planning Mode
            _gameHUDPanel = UIManager.instance.OpenUI(AvailableUI.GameHUDPanel) as GameHUDPanel;

            // TODO Setup everything

            // Set EnterLocationMode as default
            EnterEchoLocationMode();
        }

        public void EnterEchoLocationMode()
        {
            // TODO: change game state to echo location mode
        }

        public void EnterPlanningMode()
        {
            // TODO: change game state to planning mode
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
