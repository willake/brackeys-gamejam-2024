using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game
{
    [CreateAssetMenu(menuName = "MyGame/Resources/GameplayResources")]
    public class GameplayResources : ScriptableObject
    {
        public GameObject enemyPrefab;
    }
}