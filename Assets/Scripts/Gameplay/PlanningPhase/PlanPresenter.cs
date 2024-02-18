using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.RuntimeStates;
using UniRx;
using UnityEngine;

namespace Game.Gameplay
{
    public class PlanPresenter : MonoBehaviour
    {
        [Header("References")]
        public GameRuntimeState gameRuntimeState;
        public PlanRuntimeState planRuntimeState;
        public GameObject root;
        public GameObject moveDestinationPrefab;
        public GameObject movePathPrefab;
        public GameObject attackPositionPrefab;
        public GameObject attackDirectionPrefab;

        private List<GameObject> _moveDestinations = new();
        private List<LineRenderer> _movePaths = new();
        private List<GameObject> _attackPositions = new();
        private List<LineRenderer> _attackDirections = new();

        private int _maxMoves = 0;
        private int _maxActions = 0;

        private void Start()
        {
            planRuntimeState
                .onChangedObservable
                .ObserveOnMainThread()
                .Subscribe(_ => PresentPlan(
                    planRuntimeState.moveplans.ToArray(), planRuntimeState.actionPlans.ToArray()))
                .AddTo(this);

            gameRuntimeState
                .OnValueChanged
                .ObserveOnMainThread()
                .Subscribe(state => SetVisisble(state == GameState.Plan));
        }

        public void Init(int maxMoves, int maxActions)
        {
            Clear();
            for (int i = 0; i < maxMoves; i++)
            {
                GameObject destinationObj = Instantiate(moveDestinationPrefab, root.transform);
                destinationObj.name = $"Destination {i}";
                destinationObj.SetActive(false);
                _moveDestinations.Add(destinationObj);
                GameObject pathObj = Instantiate(movePathPrefab, root.transform);
                pathObj.name = $"Path {i}";
                pathObj.SetActive(false);
                LineRenderer pathRenderer = pathObj.GetComponent<LineRenderer>();
                _movePaths.Add(pathRenderer);
            }

            for (int i = 0; i < maxMoves; i++)
            {
                GameObject attackPositionObj = Instantiate(attackPositionPrefab, root.transform);
                attackPositionObj.name = $"Attack Position {i}";
                attackPositionObj.SetActive(false);
                _attackPositions.Add(attackPositionObj);
                GameObject directionObj = Instantiate(attackDirectionPrefab, root.transform);
                directionObj.name = $"Attack Direction {i}";
                directionObj.SetActive(false);
                LineRenderer pathRenderer = directionObj.GetComponent<LineRenderer>();
                _attackDirections.Add(pathRenderer);
            }

            _maxMoves = maxMoves;
            _maxActions = maxActions;
        }

        private void Clear()
        {
            for (int i = 0; i < _moveDestinations.Count; i++)
            {
                Destroy(_moveDestinations[i]);
            }

            for (int i = 0; i < _movePaths.Count; i++)
            {
                Destroy(_movePaths[i].gameObject);
            }

            for (int i = 0; i < _attackPositions.Count; i++)
            {
                Destroy(_attackPositions[i]);
            }

            for (int i = 0; i < _attackDirections.Count; i++)
            {
                Destroy(_attackDirections[i].gameObject);
            }

            _moveDestinations.Clear();
            _movePaths.Clear();
            _attackPositions.Clear();
            _attackDirections.Clear();
        }

        public void SetVisisble(bool visible)
        {
            if (root)
            {
                root.SetActive(visible);
            }
        }

        private void PresentPlan(MovePlanNode[] movePlans, ActionPlanNode[] actionPlans)
        {
            foreach (var point in _moveDestinations)
            {
                point.gameObject.SetActive(false);
            }
            foreach (var path in _movePaths)
            {
                path.gameObject.SetActive(false);
            }
            foreach (var point in _attackPositions)
            {
                point.gameObject.SetActive(false);
            }
            foreach (var attackDirection in _attackDirections)
            {
                attackDirection.gameObject.SetActive(false);
            }

            for (int i = 0; i < movePlans.Length; i++)
            {
                _moveDestinations[i].SetActive(true);
                _moveDestinations[i].transform.position = movePlans[i].destination;

                var line = _movePaths[i];
                line.gameObject.SetActive(true);
                line.SetPositions(new Vector3[] { movePlans[i].start, movePlans[i].destination });
            }

            for (int i = 0; i < actionPlans.Length; i++)
            {
                _attackPositions[i].SetActive(true);
                _attackPositions[i].transform.position = actionPlans[i].attackPosition;

                _attackDirections[i].gameObject.SetActive(true);
                var line = _attackDirections[i];
                line.SetPositions(new Vector3[] {
                    actionPlans[i].attackPosition,
                    actionPlans[i].attackPosition + actionPlans[i].direction });
            }
        }
    }
}