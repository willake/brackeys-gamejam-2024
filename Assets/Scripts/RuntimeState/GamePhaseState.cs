using System.Collections;
using System.Collections.Generic;
using Game.Gameplay;
using UnityEngine;

namespace Game.RuntimeStates
{
    [CreateAssetMenu(fileName = "GamePhaseState", menuName = "MyGame/RuntimeStates/GamePhaseState", order = 0)]
    public class GamePhaseState : RuntimeState<GamePhase>
    {
    }
}