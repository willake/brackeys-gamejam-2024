using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game
{
    [CreateAssetMenu(menuName = "MyGame/Resources/UIPanelResources")]
    public class UIPanelResources : ScriptableObject
    {
        [Header("UI Panels")]
        public GameObject menuPanel;
        public GameObject gameHUDPanel;
        public GameObject settingsPanel;
        public GameObject modalPanel;
        public GameObject pausePanel;
        public GameObject gameStartPanel;
        public GameObject gameEndPanel;
        public GameObject howToPlayPanel;
    }
}