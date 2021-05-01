using System;
using System.Collections.Generic;
using UnityEngine;

public class GAgent_Farmer : GoapAgent
{
    private void Start()
    {
        InitBase();

        _goals = new List<Goal>
        {
            new Goal
            {
                goal = Effect.DEPOSIT_FOOD,
                bias = 0f,
                relevance = 0f
            },
            new Goal
            {
                goal = Effect.DECREASES_HUNGER,
                bias = 0f,
                relevance = 0f
            }
        };

        _availableActions = new List<GoapAction>
        {
            new Action_Farm(this),
            new Action_Deliver_Food(this),
            new Action_Retrieve_Food(this),
            new Action_Eat(this)
        };
    }

    void Update()
    {
        UpdateHunger();
        
        // check if it has a plan/has finished it's plan
        if (_plan.Count == 0)
        {
            // request new plan if it has nothing to do
            GoapPlanner.Instance.RequestPlan(this, CalcGoalRelevancy());
            Debug.Log(transform.gameObject.name+": no plan; requesting one");
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
                    _plan[0].inRange = true;
                }
            }
        }
    }

    protected override void PerformCurrentAction()
    {
        Debug.Log("Performing action: " + _plan[0]);
        // check if action is in range
        if (_plan[0].inRange)
        {
            ActionStatus result = _plan[0].PerformAction();
            // if it is, then perform the action
            if (result == ActionStatus.COMPLETE)
            {
                // successful
                _plan.RemoveAt(0); // remove that action from plan
            }
            else if (result == ActionStatus.FAILED)
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
        HashSet<Effect> relevantGoals = new HashSet<Effect>();
        
        // iterate over goal list and find most relevant goal
        float high = 0f;
        int highestIn = 0;
        for (int i=0;i<_goals.Count;i++)
        {
            // use appropriate relevancy function
            switch (_goals[i].goal)
            {
                case Effect.DECREASES_HUNGER:
                    _goals[i].relevance = 0.25f * _hungerLvl * _hungerLvl; // based on hungerlevel: closer to death, higher the relevance
                    break;
                case Effect.DEPOSIT_FOOD:
                    _goals[i].relevance = -2.75f * _homeCity.FoodAmount + 0.5f * _homeCity.NumPpl + 2.25f;
                    break;
            }

            if (_goals[i].relevance > high)
            {
                highestIn = i;
                high = _goals[i].relevance;
            }
        }

        relevantGoals.Add(_goals[highestIn].goal);
        return relevantGoals;
    }
}
