using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.UI;
using Cysharp.Threading.Tasks;
using Game.RuntimeStates;
using UniRx;
using UnityEngine.Events;
using Game.Events;
using System;
using Game.Audios;
using System.Threading;

namespace Game.Gameplay
{
    public class MainGameScene : GameScene
    {
        private Lazy<EventManager> _eventManager = new Lazy<EventManager>(
            () => DIContainer.instance.GetObject<EventManager>(),
            true
        );
        protected EventManager EventManager { get => _eventManager.Value; }

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

        private CancellationTokenSource _performPhasecCts = new CancellationTokenSource();

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
        }

        public async UniTask PlayLevel(Level level)
        {
            gameRuntimeState.SetValue(GameState.Loading);

            _level = level;

            WrappedAudioClip musicPlan = ResourceManager.instance.audioResources.backgroundAudios.musicPlan;
            AudioManager.instance.PlayMusic(musicPlan.clip, musicPlan.volume);

            // delete player from previous level
            if (_player) Destroy(_player.gameObject);

            // spawn character
            _player = GeneratePlayer(level.transform, level.spawnPoint.position);

            echoLocator.Door = _level.echolocatorPoint;
            echoLocator.Init();
            planController.Init(level.doorEntrance.transform.position, level.maxMoves, level.maxActions);

            await _player.MoveToAsync(_level.doorFront.position);

            // show intro like "Game Start" 
            gameRuntimeState.SetValue(GameState.Start);
            await OnGameStart();

            gameRuntimeState.SetValue(GameState.EchoLocation);
            // ninja talk
            EventManager.Publish(
                    EventNames.presentDialogue,
                    new Payload() { args = new object[] { ResourceManager.instance.dialogueResources.enterEchoLocator } }
            );
            // wait for echo location done
            await echoLocator.LocationEndEvent.AsObservable().Take(1);

            gameRuntimeState.SetValue(GameState.Plan);
            // ninja talk
            EventManager.Publish(
                    EventNames.presentDialogue,
                    new Payload() { args = new object[] { ResourceManager.instance.dialogueResources.enterPlan } }
            );
            // wait for planning dowe
            await planController.Plan();

            gameRuntimeState.SetValue(GameState.Perform);

            _level.doorEntrance.Open();

            await _player.MoveToAsync(_level.doorEntrance.transform.position);
            // ninja talk
            EventManager.Publish(
                    EventNames.presentDialogue,
                    new Payload() { args = new object[] { ResourceManager.instance.dialogueResources.enterPerform } }
            );

            // cancel the perform phase if player die
            _player.onDie.AsObservable().Take(1).Do(_ => _performPhasecCts.Cancel()).Subscribe(_ => planPerformer.Cancel());

            AudioManager.instance.PauseMusic();

            WrappedAudioClip combat = ResourceManager.instance.audioResources.backgroundAudios.Combat;
            int combatAudioToken = AudioManager.instance.PlaySFXLoop(combat.clip, combat.volume);

            planPerformer.PerformPlan(_player);
            await planPerformer.onPerformPlanFinish.AsObservable().Take(1);

            AudioManager.instance.StopSFXLoop(combatAudioToken);

            // wait for game end
            bool isWin = _level.AreAllEnemiesDead();

            await UniTask.Delay(TimeSpan.FromSeconds(2));

            // show end game panel
            gameRuntimeState.SetValue(GameState.End);
            await OnGameEnd(isWin);

            AudioManager.instance.PauseMusic();
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
            WrappedAudioClip endSFX = isWin
                ? ResourceManager.instance.audioResources.gameplayAudios.stingWin
                : ResourceManager.instance.audioResources.gameplayAudios.stingLose;
            AudioManager.instance.PlaySFX(endSFX.clip, endSFX.volume);
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
