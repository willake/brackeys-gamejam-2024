using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Gameplay
{
    public class Level : MonoBehaviour
    {
        [Header("References")]
        public Transform startPoint;
        public Character[] enemies;

        private void Start()
        {
            MainGameScene gameScene = GameManager.instance.gameScene as MainGameScene;
            gameScene.StartLevel(this).Forget();
        }
    }
}