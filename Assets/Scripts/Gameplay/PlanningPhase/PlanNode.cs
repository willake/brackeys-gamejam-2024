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
    public struct PlanNode
    {
        public PlanNodeType nodeType;
        public Vector3 destination;
    }
}