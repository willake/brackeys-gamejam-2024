using UnityEngine;

namespace Game.RuntimeStates
{
    [CreateAssetMenu(fileName = "BoolState", menuName = "MyGame/RuntimeStates/BoolState", order = 0)]
    public class BoolState : RuntimeState<bool>
    {
    }
}