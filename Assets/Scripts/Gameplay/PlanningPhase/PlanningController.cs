using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Codice.CM.WorkspaceServer.DataStore;
using Cysharp.Threading.Tasks;
using Game.RuntimeStates;
using Game.UI;
using UniRx;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Gameplay
{
    public class PlanningController : MonoBehaviour
    {
        [Header("References")]
        public PlanRuntimeState planRuntimeState;
        public PlanningPanel planningPanel;
        public GameObject[] destinations;
        public LineRenderer[] paths;
        public LineRenderer[] attackDirections;
        public LineRenderer direction;
        public Vector3[] directionPoses;

        private Vector3 _lastMousePos;
        private Vector3 _lastMouseWorldPos;
        private PlanActionType _selectedActionType = PlanActionType.Idle;
        private Vector3 _actionPosition;
        private IPlanningState _currentState;
        private int _planningIdx = 0;
        private int _plannedSteps = 0;

        private UnityEvent _onMouseClick = new UnityEvent();
        private CancellationTokenSource _cts = new CancellationTokenSource();

        [Header("Settings")]
        public bool canPlan = false;
        public int maxSteps = 3;
        public void Init()
        {
            planRuntimeState.Value.Clear();

            planningPanel.planningActionList
                .onSelectActionObservable
                .Where(_ => _currentState == PlanningStates.PlanAttackState)
                .ObserveOnMainThread()
                .Subscribe(HandleActionSelect)
                .AddTo(this);

            planRuntimeState
                .onChangedObservable
                .ObserveOnMainThread()
                .Subscribe(_ => PresentPlan(planRuntimeState.Value.ToArray()))
                .AddTo(this);

            foreach (var destination in destinations)
            {
                destination.gameObject.SetActive(false);
            }
            foreach (var path in paths)
            {
                path.gameObject.SetActive(false);
            }
            foreach (var attackDirection in attackDirections)
            {
                attackDirection.gameObject.SetActive(false);
            }

            direction.gameObject.SetActive(false);
            directionPoses = new Vector3[2];

            _planningIdx = -1;

            RequestNextPlanNode();
        }
        private void Update()
        {
            if (canPlan == false || _currentState == null) return;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
                Debug.Log($"mouse click at {mousePos}, with world pos {mouseWorldPos}");
                HandleLeftClick(mousePos, mouseWorldPos);
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                HandleRightClick();
            }

            if (_currentState.canPlanAttackDirection)
            {
                _actionPosition.z = 0;
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;
                Vector3 d = mouseWorldPos - _actionPosition;
                directionPoses[0] = _actionPosition;
                directionPoses[1] = _actionPosition + d.normalized;
                direction.SetPositions(directionPoses);
            }
        }

        private void HandleLeftClick(Vector3 mousePos, Vector3 mouseWorldPos)
        {
            if (_currentState.canPlanMove)
            {
                _lastMousePos = mousePos;
                _lastMouseWorldPos = mouseWorldPos;
                AddMovePlan(mouseWorldPos);
            }
            else if (_currentState.canPlanAttackDirection)
            {
                AddActionPlan(PlanActionType.Attack, mouseWorldPos);
            }
        }

        private void HandleRightClick()
        {
            RequestPrevPlanNode();
        }

        private void HandleActionSelect(PlanActionType actionType)
        {
            _selectedActionType = actionType;

            if (actionType == PlanActionType.Idle)
            {
                AddActionPlan(PlanActionType.Idle, Vector2.zero);
            }
            else
            {
                SetState(PlanningStates.PlanAttackDirectionState);
            }
        }

        private void SetState(IPlanningState state)
        {
            // handle exit state
            if (_currentState == PlanningStates.PlanAttackState)
            {
                planningPanel.CloseActionList().Forget();
            }
            else if (_currentState == PlanningStates.PlanAttackDirectionState)
            {
                direction.gameObject.SetActive(false);
            }

            _currentState = state;

            // handle enter state
            if (state == PlanningStates.PlanAttackState)
            {
                // get the move destination, open the action list above it
                // PlanNode last = planRuntimeState.Value.Last();
                planningPanel.OpenActionList(_lastMousePos).Forget();
            }
            else if (state == PlanningStates.PlanAttackDirectionState)
            {
                // show direction selecting
                direction.gameObject.SetActive(true);
            }
        }

        private void RequestPrevPlanNode()
        {
            if (_planningIdx <= 0) return;

            planRuntimeState.Value.RemoveAt(_planningIdx);
            _planningIdx -= 1;

            if (_planningIdx % 2 == 0) SetState(PlanningStates.PlanMoveState);
            else SetState(PlanningStates.PlanAttackState);
        }

        private void RequestNextPlanNode()
        {
            if (_planningIdx >= maxSteps * 2 - 1)
            {
                SetState(PlanningStates.IdleState);
                return;
            }

            _planningIdx += 1;

            if (_planningIdx % 2 == 0) SetState(PlanningStates.PlanMoveState);
            else SetState(PlanningStates.PlanAttackState);
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
            _actionPosition = destination;
            Debug.Log("Add a move plan");
            RequestNextPlanNode();
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

            RequestNextPlanNode();
        }

        public void PresentPlan(PlanNode[] planNodes)
        {
            int movePlanIdx = 0;
            int attackPlanIdx = 0;
            Vector2 lastDestination = Vector2.zero;
            for (int i = 0; i < planNodes.Length; i++)
            {
                PlanNodeType nodeType = planNodes[i].nodeType;
                Vector2 destination = planNodes[i].destination;

                if (nodeType == PlanNodeType.Move)
                {
                    destinations[movePlanIdx].SetActive(true);
                    destinations[movePlanIdx].transform.position = destination;

                    if (movePlanIdx > 0)
                    {
                        var line = paths[movePlanIdx - 1];
                        line.gameObject.SetActive(true);
                        line.SetPositions(new Vector3[] { lastDestination, destination });
                    }

                    lastDestination = destination;
                    movePlanIdx += 1;
                }
                else if (nodeType == PlanNodeType.Attack)
                {
                    attackDirections[attackPlanIdx].gameObject.SetActive(true);
                    var line = attackDirections[attackPlanIdx];
                    Vector2 d = destination - lastDestination;
                    line.SetPositions(new Vector3[] { lastDestination, lastDestination + d.normalized });
                    attackPlanIdx += 1;
                }
            }
        }
    }

    public enum PlanActionType
    {
        Attack,
        Idle
    }
}