using System.Collections;
using System.Collections.Generic;
using Game.Gameplay;
using UnityEngine;

namespace Game.RuntimeStates
{
    [CreateAssetMenu(fileName = "GameRuntimeState", menuName = "MyGame/RuntimeStates/GameRuntimeState", order = 0)]
    public class GameRuntimeState : RuntimeState<GameState>
    {
    }
}