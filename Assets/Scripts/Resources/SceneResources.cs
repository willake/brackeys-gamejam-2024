using UnityEngine;
using UnityEngine.Serialization;
using WillakeD.ScenePropertyDrawler;

namespace Game
{
    [CreateAssetMenu(menuName = "MyGame/Resources/SceneResources")]
    public class SceneResources : ScriptableObject
    {
        [FormerlySerializedAs("Menu")]
        [Header("Scenes")]
        [Scene]
        public string menu;
        [FormerlySerializedAs("Game")]
        [SerializeField]
        [Scene]
        public string game;
        [FormerlySerializedAs("EchoLocation")]
        [SerializeField]
        [Scene]
        public string echoLocation;
    }
}