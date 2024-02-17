using System.Collections;
using UnityEngine;
using Game.UI;
using Cysharp.Threading.Tasks;
using Game.RuntimeStates;
using UniRx;
using Game.Events;
using System;
using Game.Audios;
using System.Threading.Tasks;


// I was using UniTask to handle the whole game state but I found
// that I would be unable to control the lifecycle of the tasks
// which means that it is hard to load next level or reload the level.
// because I will just throw the tasks to no where  
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
        public PlanPresenter planPresenter;
        public LevelLoader levelLoader;

        public GameState GameState { get => gameRuntimeState.Value; }

        int _combatAudioToken = -1;
        private Coroutine _coroutine;
        private IDisposable _playerDeadEventDisposable;
        private int _currentLevelIndex = -1;

        private void Awake()
        {
            GameManager.instance.gameScene = this;
        }

        private async void Start()
        {
            if (GameManager.instance)
            {
                await levelLoader.LoadLevel(GameManager.instance.levelOption, GameManager.instance.levelIndex);
                _currentLevelIndex = GameManager.instance.levelIndex;
            }
            else
            {
                await levelLoader.LoadLevel(LevelOption.Test, 0);
            }

            // show Game HUD, it contains a button to switch between Echo Locating and Planning phase
            _gameHUDPanel = UIManager.instance.OpenUI(AvailableUI.GameHUDPanel) as GameHUDPanel;


            echoLocator.LocationEndEvent
                .AsObservable()
                .Where(_ => GameState == GameState.EchoLocation)
                .ObserveOnMainThread()
                .Subscribe(_ => SetState(GameState.Plan))
                .AddTo(this);

            planController.onPlanSet
                .AsObservable()
                .Where(_ => GameState == GameState.Plan)
                .ObserveOnMainThread()
                .Subscribe(_ => SetState(GameState.Perform))
                .AddTo(this);

            planPerformer.onPerformPlanFinish
                .AsObservable()
                .Where(_ => GameState == GameState.Perform)
                .ObserveOnMainThread()
                .Subscribe(_ => SetState(GameState.End))
                .AddTo(this);

            SetState(GameState.Idle);
        }

        private void Update()
        {
            if ((Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) &&
                (GameState == GameState.EchoLocation || GameState == GameState.Plan))
            {
                if (GameManager.instance.IsPaused == false) UIManager.instance.OpenUI(AvailableUI.PausePanel);
            }
        }

        private Character GeneratePlayer(Transform parent, Vector3 position)
        {
            GameObject playerObj = Instantiate(prefabPlayer, parent);
            playerObj.name = "NinjaPlayer";
            playerObj.transform.position = position;

            return playerObj.GetComponent<Character>();
        }

        private void SetState(GameState state)
        {
            if (state == gameRuntimeState.Value) return;
            gameRuntimeState.SetValue(state);
            switch (state)
            {
                case GameState.Loading:
                    _coroutine = StartCoroutine(RunLoadingPhase());
                    break;
                case GameState.Start:
                    _coroutine = StartCoroutine(RunStartPhase());
                    break;
                case GameState.EchoLocation:
                    RunEcholocationPhase();
                    break;
                case GameState.Plan:
                    RunPlanPhase();
                    break;
                case GameState.Perform:
                    _coroutine = StartCoroutine(RunPerformPhase());
                    break;
                case GameState.End:
                    _coroutine = StartCoroutine(RunEndPhase());
                    break;
            }
        }

        private IEnumerator RunLoadingPhase()
        {
            WrappedAudioClip musicPlan = ResourceManager.instance.audioResources.backgroundAudios.musicPlan;
            AudioManager.instance.PlayMusic(musicPlan.clip, musicPlan.volume);

            // delete player from previous level
            if (_player) Destroy(_player.gameObject);

            // spawn character
            _player = GeneratePlayer(_level.transform, _level.spawnPoint.position);

            _playerDeadEventDisposable = _player.onDie.AsObservable().Subscribe(_ => SetState(GameState.End)).AddTo(this);

            echoLocator.Door = _level.echolocatorPoint;
            echoLocator.Init();
            planController.Init(_level.doorEntrance.transform.position, _level.maxMoves, _level.maxActions);
            planPresenter.Init(_level.maxMoves, _level.maxActions);

            yield return _player.MoveToAsync(_level.doorFront.position).ToCoroutine();

            SetState(GameState.Start);
        }

        private IEnumerator RunStartPhase()
        {
            // terrible code but works well
            Task<UIPanel> openPanelTask;
            openPanelTask = UIManager.instance.OpenUIAsync(AvailableUI.GameStartPanel).AsTask();

            while (openPanelTask.IsCompleted == false)
            {
                yield return null;
            }

            GameStartPanel startPanel = openPanelTask.Result as GameStartPanel;

            yield return startPanel.ShowText("Game Starts", 2, DG.Tweening.Ease.InOutSine).ToCoroutine();
            yield return UIManager.instance.PrevAsync();

            SetState(GameState.EchoLocation);
        }

        private void RunEcholocationPhase()
        {
            // ninja talk
            EventManager.Publish(
                    EventNames.presentDialogue,
                    new Payload() { args = new object[] { ResourceManager.instance.dialogueResources.enterEchoLocator } }
            );
            echoLocator.Enable(_level.maxRays, _level.maxBounces);
        }

        private void RunPlanPhase()
        {
            echoLocator.Disable();

            // ninja talk
            EventManager.Publish(
                    EventNames.presentDialogue,
                    new Payload() { args = new object[] { ResourceManager.instance.dialogueResources.enterPlan } }
            );
            planController.StartPlanning();
        }

        private IEnumerator RunPerformPhase()
        {
            AudioManager.instance.StopMusic();

            _level.doorEntrance.Open();

            yield return _player.MoveToAsync(_level.doorEntrance.transform.position).ToCoroutine();
            // ninja talk
            EventManager.Publish(
                    EventNames.presentDialogue,
                    new Payload() { args = new object[] { ResourceManager.instance.dialogueResources.enterPerform } }
            );

            WrappedAudioClip combat = ResourceManager.instance.audioResources.backgroundAudios.Combat;
            _combatAudioToken = AudioManager.instance.PlaySFXLoop(combat.clip, combat.volume);

            planPerformer.PerformPlan(_player);
        }

        private IEnumerator RunEndPhase()
        {
            AudioManager.instance.StopSFXLoop(_combatAudioToken);
            _combatAudioToken = -1;

            bool isWin = _level.AreAllEnemiesDead();

            yield return new WaitForSeconds(2);

            // terrible code but works well
            Task<UIPanel> openPanelTask;
            openPanelTask = UIManager.instance.OpenUIAsync(AvailableUI.GameEndPanel).AsTask();

            while (openPanelTask.IsCompleted == false)
            {
                yield return null;
            }

            GameEndPanel endPanel = openPanelTask.Result as GameEndPanel;
            endPanel.SetEndGameState(isWin, _player.State == CharacterStates.DeadState, _level.AreAllEnemiesDead(), HasNextLevel());

            AudioManager.instance.StopMusic();
        }

        public async UniTask NavigateToMenu()
        {
            await levelLoader.UnloadCurrentLevel();
            GameManager.instance.SwitchScene(AvailableScene.Menu);
        }

        public void PlayLevel(Level level)
        {
            _level = level;
            SetState(GameState.Loading);
        }

        public bool HasNextLevel()
        {
            int levelCount = ResourceManager.instance.levelResources.levels.Length;
            if (_currentLevelIndex == -1)
            {
                Debug.Log("Test level does not have next level.");
                return false;
            }
            if (_currentLevelIndex + 1 >= levelCount)
            {
                Debug.Log($"Current level is the last level. Current: {_currentLevelIndex} Level count: {levelCount}");
                return false;
            }
            return true;
        }

        public async UniTask NextLevel()
        {
            if (HasNextLevel() == false) return;

            _level.doorExit.Open();
            await _player.MoveToAsync(_level.exitPoint.position);

            OnExitLevel();
            echoLocator.Disable(true);

            _currentLevelIndex += 1;
            await levelLoader.LoadLevel(LevelOption.Levels, _currentLevelIndex);
        }

        public void RetryCurrentLevel()
        {
            _playerDeadEventDisposable?.Dispose();
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }
            AudioManager.instance.StopSFXLoop(_combatAudioToken);
            _combatAudioToken = -1;
            _level.Init();
            echoLocator.Disable(true);
            SetState(GameState.Loading);
        }

        private void OnExitLevel()
        {
            _playerDeadEventDisposable?.Dispose();
            _level = null;
            _player = null;
        }
    }
}
