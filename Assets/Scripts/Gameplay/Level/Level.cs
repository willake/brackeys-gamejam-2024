using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Gameplay
{
    public class Level : MonoBehaviour
    {
        [Header("References")]
        public Light2D globalLight;
        public EnemySpawnPoint[] enemySpawnPoints;
        // public Character[] enemies;
        public Door doorEntrance;
        public Door doorExit;

        [Header("Settings")]
        public Transform spawnPoint;
        public Transform doorFront;
        public Transform echolocatorPoint;
        public Transform exitPoint;
        [Tooltip("Do not set enemy count over the enemy spawn point count")]
        public int enemyCount = 3;
        public int maxMoves = 3;
        public int maxActions = 3;
        public int maxRays = 3;
        public int maxBounces = 5;

        private List<AICharacter> enemies = new();

        private void Start()
        {
            MainGameScene gameScene = GameManager.instance.gameScene as MainGameScene;

            Init();

            gameScene.PlayLevel(this);
        }

        public void Init()
        {
            globalLight.intensity = 0;

            ClearEnemies();

            EnemySpawnPoint[] shuffledSpawnPoints = new EnemySpawnPoint[enemySpawnPoints.Length];
            enemySpawnPoints.CopyTo(shuffledSpawnPoints, 0);

            System.Random random = new();
            random.Shuffle(shuffledSpawnPoints);

            GameObject enemyPrefab = ResourceManager.instance.gameplayResources.enemyPrefab;

            for (int i = 0; i < enemyCount; i++)
            {
                var enemyObj = Instantiate(enemyPrefab, transform);
                enemyObj.name = $"Enemy {i}";
                AICharacter enemy = enemyObj.GetComponent<AICharacter>();

                EnemySpawnPoint spawnPoint = shuffledSpawnPoints[i];
                enemyObj.transform.position = spawnPoint.transform.position;
                enemy.Init(spawnPoint.facingDirectionInDegrees, spawnPoint.sightRangeInDegree, spawnPoint.sightDistance);
                enemies.Add(enemy);
            }

            doorEntrance.Close();
            doorExit.Close();
        }

        private void ClearEnemies()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                Destroy(enemies[i].gameObject);
            }

            enemies.Clear();
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