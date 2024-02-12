using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.WorkspaceServer.DataStore;
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
        public GameObject[] destinations;
        public LineRenderer[] paths;

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

            planRuntimeState.Value
                .ObserveCountChanged()
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
        }

        private void Update()
        {
            if (canPlan && Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
                Debug.Log($"mouse click at {mousePos}, with world pos {mouseWorldPos}");
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

        public void PresentPlan(PlanNode[] planNodes)
        {
            Debug.Log("Update plan");
            int movePlanIdx = 0;
            int attackPlanIdx = 0;
            Vector2 lastDestination = Vector2.zero;
            for (int i = 0; i < planNodes.Length; i++)
            {
                PlanNodeType nodeType = planNodes[i].nodeType;
                Vector3 destination = planNodes[i].destination;

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