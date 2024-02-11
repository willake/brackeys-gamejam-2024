using UnityEngine;
using UnityEngine.Serialization;
using WillakeD.ScenePropertyDrawler;

namespace Game
{
    [CreateAssetMenu(menuName = "MyGame/Resources/LevelResources")]
    public class LevelResources : ScriptableObject
    {
        [Header("Levels")]
        [FormerlySerializedAs("Test")]
        [Scene]
        public string test;
        [FormerlySerializedAs("Level1")]
        [Scene]
        public string level1;
    }
}