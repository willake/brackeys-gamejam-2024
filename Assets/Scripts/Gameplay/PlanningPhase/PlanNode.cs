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

    // might have different attack style
    public enum PlanActionType
    {
        Attack,
        Idle
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

    [System.Serializable]
    public struct PerformerPlanNode
    {
        public PlanNodeType nodeType;
        public Vector2 v0;
        public Vector2 v1;
    }
}