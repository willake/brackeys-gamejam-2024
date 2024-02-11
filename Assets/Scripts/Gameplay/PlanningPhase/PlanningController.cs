using System.Collections;
using System.Collections.Generic;
using Game.RuntimeStates;
using UnityEngine;

namespace Game.Gameplay
{
    public class PlanningController : MonoBehaviour
    {
        public PlanRuntimeState planRuntimeState;


        public void AddMovePlan(Vector3 destination)
        {
            planRuntimeState.Value.Add(
                new PlanNode()
                {
                    nodeType = PlanNodeType.Move,
                    destination = destination
                }
            );
        }

        public void AddAttackPlan(Vector3 destination)
        {
            planRuntimeState.Value.Add(
                new PlanNode()
                {
                    nodeType = PlanNodeType.Attack,
                    destination = destination
                }
            );
        }
    }
}