using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Gameplay
{
    public class Level : MonoBehaviour
    {
        [Header("References")]
        public Light2D globalLight;
        public Character[] enemies;
        public Door doorEntrance;
        public Door doorExit;

        [Header("Settings")]
        public Transform spawnPoint;
        public Transform doorFront;
        public Transform echolocatorPoint;
        public int maxMoves = 3;
        public int maxActions = 3;
        public int maxRays = 3;
        public int maxBounces = 5;

        private void Start()
        {
            MainGameScene gameScene = GameManager.instance.gameScene as MainGameScene;

            globalLight.intensity = 0;
            gameScene.PlayLevel(this);
        }

        public bool AreAllEnemiesDead()
        {
            foreach (var enemy in enemies)
            {
                if (enemy.State != CharacterStates.DeadState) return false;
            }

            return true;
        }
    }
}