using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using WillakeD.ScenePropertyDrawler;
using System;
using UnityEngine.Events;

namespace Game.Gameplay
{
    public class LevelLoader : MonoBehaviour
    {
        private string _currentLevel = string.Empty;

        public OnLoadLevelEvent onLoadLevel = new OnLoadLevelEvent();
        public IObservable<string> OnLoadLevelObservable
        {
            get => onLoadLevel.AsObservable();
        }

        public OnLoadLevelEvent onLevelLoaded = new OnLoadLevelEvent();
        public IObservable<string> OnLevelLoadedObservable
        {
            get => onLevelLoaded.AsObservable();
        }

        public async UniTask LoadLevel(LevelOption level, int index)
        {
            string levelName = GetLevelName(level, index);

            onLoadLevel.Invoke(levelName);

            await LoadLevel(levelName);

            onLevelLoaded.Invoke(levelName);
        }

        private string GetLevelName(LevelOption option, int index)
        {
            switch (option)
            {
                case LevelOption.Test:
                default:
                    return ResourceManager.instance.levelResources.test.GetSceneNameByPath();
                case LevelOption.Levels:
                    return ResourceManager.instance.levelResources.levels[index].GetSceneNameByPath();
            }
        }

        private async UniTask LoadLevel(string levelName)
        {
            AsyncOperation loadSceneOperation =
                SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);

            await loadSceneOperation;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(levelName));

            loadSceneOperation.allowSceneActivation = false;

            if (_currentLevel != string.Empty)
            {
                AsyncOperation unloadSceneOperation =
                    SceneManager.UnloadSceneAsync(_currentLevel);

                await unloadSceneOperation;
            }

            loadSceneOperation.allowSceneActivation = true;

            _currentLevel = levelName;
        }

        public async UniTask UnloadCurrentLevel()
        {
            if (_currentLevel != string.Empty || _currentLevel != null)
            {
                AsyncOperation unloadSceneOperation =
                    SceneManager.UnloadSceneAsync(_currentLevel);

                await unloadSceneOperation;
            }

            _currentLevel = string.Empty;
        }

        // private void OnDestroy()
        // {
        //     UnloadCurrentLevel().Forget();
        // }

        public class OnLoadLevelEvent : UnityEvent<string> { }
    }
}