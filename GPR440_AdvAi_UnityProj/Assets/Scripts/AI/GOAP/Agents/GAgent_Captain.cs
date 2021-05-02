using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GAgent_Captain : GoapAgent
{
	public Text captainText;

	private UnitManager _unitManager;

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
			},
			new Goal
			{
				goal = Effect.DRAFT_AGENT,
				bias=0f,
				relevance=0
			},
			new Goal
			{
				goal = Effect.EMPLOY_AGENT,
				bias=0f,
				relevance=0
			},
			new Goal
			{
				goal = Effect.SET_GOAL_BIAS,
				bias=0f,
				relevance=0
			}
		};

		_availableActions = new List<GoapAction>
		{
			new Action_Retrieve_Food(this),
			new Action_Eat(this)
		};

		_unitManager = GameObject.FindGameObjectWithTag("UnitManager").GetComponent<UnitManager>();
	}

	void Update()
	{
		UpdateHunger();

		// check if it has a plan/has finished it's plan
		if (_plan.Count == 0)
		{
			// request new plan if it has nothing to do
			GoapPlanner.Instance.RequestPlan(this, CalcGoalRelevancy());
			Debug.Log(transform.gameObject.name + ": no plan; requesting one");
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
		HashSet<Effect> relevantGoals = new HashSet<Effect>();

		// iterate over goal list
		float high = 0f;
		int highestIn = 0;
		for (int i = 0; i < _goals.Count; i++)
		{
			// use appropriate relevancy function
			switch (_goals[i].goal)
			{
				case Effect.DECREASES_HUNGER:
					_goals[i].relevance = 0.25f * _hungerLvl * _hungerLvl; // based on hungerlevel: closer to death, higher the relevance
					break;
				case Effect.DRAFT_AGENT:
					//_goals[i].relevance = 0.313f * _homeCity.NumEnemies - 0.188f * _unitManager.NumSoldiers + 0.25f; // draft based on enemies and soldiers
					_goals[i].relevance = 0; // hard-coded for now
					break;
				case Effect.EMPLOY_AGENT:
					_goals[i].relevance = -0.250f * _homeCity.FoodAmount + 0.125f * _unitManager.NumUnits + 0.5f; // if more food, don't need as many laborers
					// may need ot account for ore at some point
					break;
				case Effect.SET_GOAL_BIAS:
					_goals[i].relevance = -0.225f * _unitManager.NumUnemployed + 0.9f; // more unemployed = need to assign them
					break;
			}

			// if relevance is over 1, add to goals
			if (_goals[i].relevance > high)
			{
				highestIn = i;
				high = _goals[i].relevance;
			}
		}

		// update captain ui
		captainText.text =
			"DECREASE_HUNGER: " + _goals[0].relevance.ToString("F3") + " (" + _hungerLvl +
			")\nDRAFT_AGENT: " + _goals[1].relevance.ToString("F3") +
			"\nEMPLOY_AGENT: " + _goals[2].relevance.ToString("F3") +
			"\nSET_GOAL_BIAS: " + _goals[3].relevance.ToString("F3");


		relevantGoals.Add(_goals[highestIn].goal);
		return relevantGoals;
	}
}
