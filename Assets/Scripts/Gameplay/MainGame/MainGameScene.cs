using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.UI;

namespace Game.Gameplay
{
    public class MainGameScene : GameScene
    {
        private GameHUDPanel _gameHUDPanel;

        private void Awake()
        {
            GameManager.instance.gameScene = this;
        }

        private void Start()
        {
            // TODO Show intro like "Game Start" 

            // Show Game HUD, it contains a button to switch between Echo Locating and Planning Mode
            _gameHUDPanel = UIManager.instance.OpenUI(AvailableUI.GameHUDPanel) as GameHUDPanel;

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
    }
}
