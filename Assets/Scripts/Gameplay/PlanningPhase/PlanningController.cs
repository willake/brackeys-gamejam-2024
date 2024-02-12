using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.RuntimeStates;
using Game.UI;
using UniRx;
using UnityEditorInternal;
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
        private Vector3 _lastMousePos;
        private Vector3 _lastMouseWorldPos;

        public void Init()
        {
            _planningState = PlanState.PlanMove;
            planRuntimeState.Value.Clear();

            planningPanel.planningActionList
                .onSelectActionObservable
                .Where(_ => _planningState == PlanState.PlanAction)
                .ObserveOnMainThread()
                .Subscribe(actionType => AddActionPlan(actionType, Vector2.zero))
                .AddTo(this);
        }

        private void Update()
        {
            if (canPlan && Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 mousePos = Input.mousePosition;
                Debug.Log($"mouse click at {mousePos}");
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
                HandleClick(mousePos, mouseWorldPos);
            }
        }

        private void HandleClick(Vector3 mousePos, Vector3 mouseWorldPos)
        {
            _lastMousePos = mousePos;
            _lastMouseWorldPos = mouseWorldPos;
            if (_planningState == PlanState.PlanMove)
            {
                AddMovePlan(mouseWorldPos);
            }
        }

        private void ChangeState(PlanState state)
        {
            // handle exit state

            _planningState = state;

            // handle enter state
            if (state == PlanState.PlanAction)
            {
                // get the move destination, open the action list above it
                // PlanNode last = planRuntimeState.Value.Last();
                planningPanel.OpenActionList(_lastMousePos).Forget();
            }

        }

        public void AddMovePlan(Vector2 destination)
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

        public void AddActionPlan(PlanActionType actionType, Vector2 destination)
        {
            if (actionType == PlanActionType.Attack)
            {
                planRuntimeState.Value.Add(
                    new PlanNode()
                    {
                        nodeType = PlanNodeType.Attack,
                        destination = destination
                    }
                );
                Debug.Log("Add an attack plan");
            }
            else
            {
                Debug.Log("Add an idle plan");
            }

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