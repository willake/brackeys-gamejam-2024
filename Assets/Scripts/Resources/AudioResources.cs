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
            [FormerlySerializedAs("Dialogue")] public WrappedAudioClip dialogue;
        }

        [Serializable]
        public class BackgroundAudios
        {
            [FormerlySerializedAs("MusicMainTheme")] public WrappedAudioClip musicMainTheme;
            [FormerlySerializedAs("MusicPerform")] public WrappedAudioClip musicPlan;
            [FormerlySerializedAs("Combat")] public WrappedAudioClip Combat;
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
            [FormerlySerializedAs("AcceptPlan1")] public WrappedAudioClip acceptPlan1;
            [FormerlySerializedAs("AcceptPlan2")] public WrappedAudioClip acceptPlan2;
            [FormerlySerializedAs("AcceptPlan3")] public WrappedAudioClip acceptPlan3;
            [FormerlySerializedAs("Door")] public WrappedAudioClip door;
            [FormerlySerializedAs("Attack")] public WrappedAudioClip attack;
            [FormerlySerializedAs("Dead")] public WrappedAudioClip dead;
            [FormerlySerializedAs("StingWin")] public WrappedAudioClip stingWin;
            [FormerlySerializedAs("StingLose")] public WrappedAudioClip stingLose;
            [FormerlySerializedAs("HitCupboard1")] public WrappedAudioClip hitCupboard1;
            [FormerlySerializedAs("HitCupboard2")] public WrappedAudioClip hitCupboard2;
            [FormerlySerializedAs("HitEnnemy1")] public WrappedAudioClip hitEnnemy1;
            [FormerlySerializedAs("HitEnnemy2")] public WrappedAudioClip hitEnnemy2;
            [FormerlySerializedAs("HitWall1")] public WrappedAudioClip hitWall1;
            [FormerlySerializedAs("HitWall2")] public WrappedAudioClip hitWall2;
            [FormerlySerializedAs("CastSpell")] public WrappedAudioClip castSpell;

        }
    }
}