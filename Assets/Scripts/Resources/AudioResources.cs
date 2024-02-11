using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    [CreateAssetMenu(menuName = "MyGame/Resources/AudioResources")]
    public class AudioResources : ScriptableObject
    {
        [Header("UI Audio Assets")]
        public UIAudios uiAudios;
        public BackgroundAudios backgroundAudios;
        public GameplayAudios gameplayAudios;

        [Serializable]
        public class UIAudios
        {
            [FormerlySerializedAs("ButtonClick")] public WrappedAudioClip buttonClick;
            [FormerlySerializedAs("ButtonConfirm")] public WrappedAudioClip buttonConfirm;
        }

        [Serializable]
        public class BackgroundAudios
        {
        }

        [Serializable]
        public class GameplayAudios
        {
        }
    }
}