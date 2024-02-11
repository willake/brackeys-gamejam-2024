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
            UIManager.instance.OpenUI(AvailableUI.GameHUDPanel);
        }
    }
}
