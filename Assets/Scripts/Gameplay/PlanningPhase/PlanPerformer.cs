using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.RuntimeStates;
using UnityEngine;

namespace Game.Gameplay
{
    public class PlanPerformer : MonoBehaviour
    {
        public PlanRuntimeState planRuntimeState;

        public async void PerformPlan(Character character)
        {
            // extract the plan
            List<PerformerPlanNode> plans =
                ExtractPlans(planRuntimeState.moveplans.ToArray(), planRuntimeState.actionPlans.ToArray());

            foreach (PerformerPlanNode plan in plans)
            {
                if (plan.nodeType == PlanNodeType.Move)
                {
                    await character.MoveToAsync(plan.v1);
                }
                else
                {
                    await character.MoveToAsync(plan.v0);
                    await character.AttackAsync(plan.v1);
                }
            }
        }

        private List<PerformerPlanNode> ExtractPlans(MovePlanNode[] movePlans, ActionPlanNode[] actionPlans)
        {
            List<PerformerPlanNode> plans = new();
            int moveCount = movePlans.Length;

            for (int i = 0; i < moveCount; i++)
            {
                // bad for performance but no worries for a prototype
                actionPlans.Where(x => x.pathIdx == i).ToList().ForEach(action => plans.Add(
                    new PerformerPlanNode()
                    {
                        nodeType = PlanNodeType.Attack,
                        v0 = action.attackPosition,
                        v1 = action.direction
                    }
                ));

                plans.Add(new PerformerPlanNode()
                {
                    nodeType = PlanNodeType.Move,
                    v0 = movePlans[i].start,
                    v1 = movePlans[i].destination
                });
            }

            return plans;
        }
    }
}