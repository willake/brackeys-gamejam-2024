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
            [FormerlySerializedAs("FootstepsWood1")] public WrappedAudioClip footstepsWood1;
            [FormerlySerializedAs("FootstepsWood2")] public WrappedAudioClip footstepsWood2;
            [FormerlySerializedAs("FootstepsWood3")] public WrappedAudioClip footstepsWood3;
            [FormerlySerializedAs("ClickPhath1")] public WrappedAudioClip clickPath1;
            [FormerlySerializedAs("ClickPhath2")] public WrappedAudioClip clickPath2;
            [FormerlySerializedAs("ClickPhath3")] public WrappedAudioClip clickPath3;
            [FormerlySerializedAs("Attack")] public WrappedAudioClip attack;
            [FormerlySerializedAs("Dead")] public WrappedAudioClip dead;
        }
    }
}