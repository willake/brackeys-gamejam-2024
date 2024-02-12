using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.RuntimeStates;
using Game.UI;
using UnityEngine;

namespace Game.Gameplay
{
    public class PlanningController : MonoBehaviour
    {
        [Header("References")]
        public PlanRuntimeState planRuntimeState;
        public PlanningPanel planningPanel;

        private PlanState _planningState;
        private int _plannedSteps = 0;

        [Header("Settings")]
        public bool canPlan = false;
        public int maxSteps = 3;

        public void Init()
        {
            _planningState = PlanState.PlanMove;
            planRuntimeState.Value.Clear();
        }

        private void Update()
        {
            if (canPlan && Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                if (_planningState == PlanState.PlanMove)
                {
                    AddMovePlan(worldPos);
                }
            }
        }

        private void ChangeState(PlanState state)
        {
            // handle exit state

            // handle enter state
            if (state == PlanState.PlanAction)
            {
                // get the move destination, open the action list above it
                PlanNode last = planRuntimeState.Value.Last();
                planningPanel.OpenActionList(last.destination).Forget();
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
            Debug.Log("Add a move plan");
            ChangeState(PlanState.PlanAction);
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
            Debug.Log("Add an action plan");
            ChangeState(PlanState.PlanMove);

            _plannedSteps += 1;
            if (_plannedSteps >= maxSteps)
            {
                ChangeState(PlanState.End);
            }
            else
            {
                ChangeState(PlanState.PlanMove);
            }
        }

        public enum PlanState
        {
            PlanMove,
            PlanAction,
            End
        }
    }

    public enum PlanActionType
    {
        Attack,
        Idle
    }
}