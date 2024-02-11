using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using WillakeD.CommonPatterns;

namespace Game
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        [Header("Resources")]
        [FormerlySerializedAs("AudioResources")] public AudioResources audioResources;
        [FormerlySerializedAs("SceneResources")] public SceneResources sceneResources;
        [FormerlySerializedAs("UIPanelResources")] public UIPanelResources uiPanelResources;
        [FormerlySerializedAs("GameplayResources")] public GameplayResources gameplayResources;
        [FormerlySerializedAs("LevelResources")] public LevelResources levelResources;
    }
}