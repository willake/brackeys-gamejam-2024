using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Codice.CM.WorkspaceServer.DataStore;
using Cysharp.Threading.Tasks;
using DG.Tweening.Core;
using Game.RuntimeStates;
using Game.UI;
using UniRx;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Game.Gameplay
{
    public class PlanController : MonoBehaviour
    {
        [Header("References")]
        public PlanRuntimeState planRuntimeState;
        public PlanningPanel planningPanel;
        public GameObject attackPositionIndicator;
        public LineRenderer attackDirectionIndicator;

        private IPlanningState _currentState;

        // move plan related
        private int _plannedMoves;
        private Vector2 _startPoint;

        // action plan related
        private int _plannedActions;
        private Vector2 _actionPosition;
        private int _actionPathIdx;
        private PlanActionType _actionType;
        private Vector3[] _actionDirectionPoses;


        [Header("Settings")]
        public bool canPlan = false;
        public int maxMoves = 3;
        public int maxActions = 3;

        public void Init(Vector3 startPoint)
        {
            _startPoint = startPoint;
            planRuntimeState.moveplans.Clear();
            planRuntimeState.actionPlans.Clear();

            attackPositionIndicator.gameObject.SetActive(false);
            attackDirectionIndicator.gameObject.SetActive(false);
            _actionDirectionPoses = new Vector3[2];

            _plannedMoves = 0;
            _plannedActions = 0;

            planningPanel.planningActionList.Setup(maxActions);
            planningPanel.SetActionListVisible(false);

            SetState(PlanningStates.PlanMoveState);
        }
        private void Update()
        {
            if (canPlan == false || _currentState == null) return;

            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
                // Debug.Log($"mouse click at {mousePos}, with world pos {mouseWorldPos}");
                HandleLeftClick(mousePos, mouseWorldPos);
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                HandleRightClick();
            }

            if (_currentState.canPlanAttackPosition)
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;
                UpdateAttackPositionIndicator(mouseWorldPos);
            }

            if (_currentState.canPlanAttackDirection)
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;
                UpdateAttackDirectionIndicator(mouseWorldPos);
            }
        }

        private void HandleLeftClick(Vector3 mousePos, Vector3 mouseWorldPos)
        {
            if (_currentState.canPlanMove)
            {
                AddMovePlan(mouseWorldPos);
            }
            else if (_currentState.canPlanAttackPosition)
            {
                SetState(PlanningStates.PlanAttackDirectionState);
            }
            else if (_currentState.canPlanAttackDirection)
            {
                AddActionPlan(PlanActionType.Attack, mouseWorldPos);
            }
        }

        private void HandleRightClick()
        {
            // TODO implement undo feature
        }

        private void UpdateAttackPositionIndicator(Vector2 mouseWorldPos)
        {
            int pathCount = planRuntimeState.moveplans.Count;

            Vector2 closestProjection = Vector2.zero;
            float closestDistance = 999;
            int closestPath = -1;

            for (int i = 0; i < pathCount; i++)
            {
                // calculate mouse pos projection on each line 
                MovePlanNode movePlan = planRuntimeState.moveplans[i];
                Vector2 AB = movePlan.destination - movePlan.start;
                Vector2 AP = mouseWorldPos - movePlan.start;
                float ABdotAP = Vector2.Dot(AB, AP);
                float length2 = AB.magnitude * AB.magnitude;
                Vector2 projection = movePlan.start + AB * ABdotAP / length2;

                float distA = (projection - movePlan.start).magnitude;
                float distB = (projection - movePlan.destination).magnitude;

                // projection is not on the segment
                if (distA + distB > AB.magnitude + 0.1)
                {
                    projection = distA < distB ? movePlan.start : movePlan.destination;
                }

                float distance = (projection - mouseWorldPos).magnitude;

                if (distance < closestDistance)
                {
                    closestProjection = projection;
                    closestDistance = distance;
                    closestPath = i;
                }
            }

            _actionPosition = closestProjection;
            _actionPathIdx = closestPath;
            attackPositionIndicator.transform.position = _actionPosition;
        }

        private void UpdateAttackDirectionIndicator(Vector3 mouseWorldPos)
        {
            mouseWorldPos.z = 0;
            Vector2 d = new Vector2(mouseWorldPos.x, mouseWorldPos.y) - _actionPosition;
            _actionDirectionPoses[0] = _actionPosition;
            _actionDirectionPoses[1] = _actionPosition + d.normalized;
            attackDirectionIndicator.SetPositions(_actionDirectionPoses);
        }

        private void SetState(IPlanningState state)
        {
            // handle exit state
            OnExitState(_currentState, state);

            _currentState = state;
            Debug.Log($"Enter State: {_currentState.name}");

            // handle enter state
            OnEnterState(state);
        }

        private void OnExitState(IPlanningState prev, IPlanningState next)
        {
            if (prev == PlanningStates.PlanMoveState &&
                next == PlanningStates.PlanAttackPositionState)
            {
                planningPanel.SetActionListVisible(true);
            }
            else if (prev == PlanningStates.PlanAttackDirectionState &&
                    next != PlanningStates.PlanAttackPositionState)
            {
                attackPositionIndicator.gameObject.SetActive(false);
                attackDirectionIndicator.gameObject.SetActive(false);
                planningPanel.SetActionListVisible(false);
            }

            if (prev == PlanningStates.PlanAttackDirectionState)
            {
                attackDirectionIndicator.gameObject.SetActive(false);
            }
        }

        private void OnEnterState(IPlanningState state)
        {
            if (state == PlanningStates.PlanAttackPositionState)
            {
                attackPositionIndicator.gameObject.SetActive(true);
                planningPanel.HighlightAction(_plannedActions);
            }
            else if (state == PlanningStates.PlanAttackDirectionState)
            {
                // show direction selecting
                attackDirectionIndicator.gameObject.SetActive(true);
            }
        }

        private void AddMovePlan(Vector2 destination)
        {
            planRuntimeState.moveplans.Add(
                new MovePlanNode()
                {
                    start = _plannedMoves > 0 ? planRuntimeState.moveplans.Last().destination : _startPoint,
                    destination = destination
                }
            );
            Debug.Log("Add a move plan");

            _plannedMoves += 1;

            if (_plannedMoves >= maxMoves) SetState(PlanningStates.PlanAttackPositionState);
        }

        private void AddActionPlan(PlanActionType actionType, Vector2 destination)
        {
            Vector2 direction = (destination - _actionPosition).normalized;
            planRuntimeState.actionPlans.Add(
                new ActionPlanNode()
                {
                    pathIdx = _actionPathIdx,
                    attackPosition = _actionPosition,
                    direction = direction
                }
            );
            Debug.Log("Add an attack plan");

            _plannedActions += 1;
            if (_plannedActions >= maxActions) SetState(PlanningStates.IdleState);
            else SetState(PlanningStates.PlanAttackPositionState);
        }
    }

    public enum PlanActionType
    {
        Attack,
        Idle
    }
}