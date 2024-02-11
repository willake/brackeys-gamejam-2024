using System.Collections;
using System.Collections.Generic;
using Game.RuntimeStates;
using UnityEngine;

namespace Game.Gameplay
{
    public class PlanningController : MonoBehaviour
    {
        public PlanRuntimeState planRuntimeState;

        private State _planningState;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                if (_planningState == State.Idle)
                {
                    AddMovePlan(worldPos);
                }
            }
        }


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

        public enum State
        {
            Idle,
            MoveDestinationSelected,
            AttackSelected
        }
    }
}