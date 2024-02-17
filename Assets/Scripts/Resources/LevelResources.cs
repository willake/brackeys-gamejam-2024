using UnityEngine;
using UnityEngine.Serialization;
using WillakeD.ScenePropertyDrawler;

namespace Game
{
    [CreateAssetMenu(menuName = "MyGame/Resources/LevelResources")]
    public class LevelResources : ScriptableObject
    {
        [Header("Test Scene")]
        [FormerlySerializedAs("Test")]
        [Scene]
        public string test;
        [Header("Levels")]
        [FormerlySerializedAs("Levels")]
        [Scene]
        public string[] levels;
    }
}