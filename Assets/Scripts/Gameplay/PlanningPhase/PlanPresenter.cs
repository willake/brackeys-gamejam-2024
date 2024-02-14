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
        public PlanRuntimeState planRuntimeState;
        public GameObject root;
        public GameObject[] movementDestinations;
        public LineRenderer[] movementPaths;
        public GameObject[] attackPositions;
        public LineRenderer[] attackDirections;

        private void Start()
        {
            planRuntimeState
                .onChangedObservable
                .ObserveOnMainThread()
                .Subscribe(_ => PresentPlan(
                    planRuntimeState.moveplans.ToArray(), planRuntimeState.actionPlans.ToArray()))
                .AddTo(this);

            foreach (var point in movementDestinations)
            {
                point.gameObject.SetActive(false);
            }
            foreach (var path in movementPaths)
            {
                path.gameObject.SetActive(false);
            }
            foreach (var point in attackPositions)
            {
                point.gameObject.SetActive(false);
            }
            foreach (var attackDirection in attackDirections)
            {
                attackDirection.gameObject.SetActive(false);
            }
        }

        public void SetVisisble(bool visible)
        {
            root.gameObject.SetActive(visible);
        }

        private void PresentPlan(MovePlanNode[] movePlans, ActionPlanNode[] actionPlans)
        {
            foreach (var point in movementDestinations)
            {
                point.gameObject.SetActive(false);
            }
            foreach (var path in movementPaths)
            {
                path.gameObject.SetActive(false);
            }
            foreach (var point in attackPositions)
            {
                point.gameObject.SetActive(false);
            }
            foreach (var attackDirection in attackDirections)
            {
                attackDirection.gameObject.SetActive(false);
            }

            for (int i = 0; i < movePlans.Length; i++)
            {
                movementDestinations[i].SetActive(true);
                movementDestinations[i].transform.position = movePlans[i].destination;

                var line = movementPaths[i];
                line.gameObject.SetActive(true);
                line.SetPositions(new Vector3[] { movePlans[i].start, movePlans[i].destination });
            }

            for (int i = 0; i < actionPlans.Length; i++)
            {
                attackPositions[i].SetActive(true);
                attackPositions[i].transform.position = actionPlans[i].attackPosition;

                attackDirections[i].gameObject.SetActive(true);
                var line = attackDirections[i];
                line.SetPositions(new Vector3[] {
                    actionPlans[i].attackPosition,
                    actionPlans[i].attackPosition + actionPlans[i].direction });
            }
        }
    }
}