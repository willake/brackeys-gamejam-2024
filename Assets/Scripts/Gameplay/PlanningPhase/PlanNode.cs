using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay
{
    // might be good to use struct, but nevermind it's a proptotype
    public enum PlanNodeType
    {
        Move,
        Attack
    }
    [System.Serializable]
    public struct MovePlanNode
    {
        public Vector2 start;
        public Vector2 destination;
    }

    [System.Serializable]
    public struct ActionPlanNode
    {
        public int pathIdx;
        public Vector2 attackPosition;
        public Vector2 direction;
    }
}