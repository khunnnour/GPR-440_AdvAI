using System;
using System.Collections.Generic;
using UnityEngine;

public class GAgent_Farmer : GoapAgent
{
    private void Start()
    {
        InitBase();

        _goals = new HashSet<Effect> {Effect.DEPOSIT_FOOD};

        _availableActions = new List<GoapAction>
        {
            new Action_Farm(_homeCity.farm, this), 
            new Action_Deliver_Food(_homeCity.transform, this)
        };
    }

    void Update()
    {
        // check if it has a plan/has finished it's plan
        if (_plan.Count == 0)
        {
            // request new plan if it has nothing to do
            GoapPlanner.Instance.RequestPlan(this, CalcGoalRelevancy());
        }
        else
        {
            if (!_seeking) // if not currently seeking, perform the action
                PerformCurrentAction();
            else // otherwise check if you reached target
            {
                // check if in range yet
                if ((_plan[0].Target.position - transform.position).sqrMagnitude <= 2.25f)
                {
                    _seeking = false;
                    _plan[0]._inRange = true;
                }
            }
        }
    }

    protected override void PerformCurrentAction()
    {
        // check if action is in range
        if (_plan[0]._inRange)
        {
            // if it is, then perform the action
            if (_plan[0].PerformAction())
            {
                // successful
                _plan.RemoveAt(0); // remove that action from plan
            }
            else
                Debug.LogWarning("Could not perform action " + _plan[0]);
        }
        else
        {
            // if not, then set seek target
            _steering.SetTargetLocation(_plan[0].Target.position);
            _seeking = true;
        }
    }

    protected override HashSet<Effect> CalcGoalRelevancy()
    {
        return _goals;
    }
}
