using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GAgent_Unemployed : GoapAgent
{
	private void Start()
	{
		InitBase();

		_goals = new List<Goal>
		{
			new Goal
			{
				goal = Effect.DECREASES_HUNGER,
				bias = 0f,
				relevance = 0f
			}
		};

		_availableActions = new List<GoapAction>
		{
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
			//Debug.Log(transform.gameObject.name + ": no plan; requesting one");
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
		//Debug.Log("Performing action: " + _plan[0]);
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
		return new HashSet<Effect>() { _goals[0].goal }; // only has the one goal
	}
}
