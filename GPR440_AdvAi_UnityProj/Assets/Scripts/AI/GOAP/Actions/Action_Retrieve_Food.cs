﻿using System.Collections.Generic;
using UnityEngine;

public class Action_Retrieve_Food : GoapAction
{
    public Action_Retrieve_Food()
    {
    }

    public Action_Retrieve_Food(GoapAgent a) : base(a)
    {
        _target = _agent.HomeCity.transform;
        inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition> {}; // no preconditions
        _effects = new HashSet<Effect> {Effect.MAKE_FOOD}; // only makes food
        _timeToComplete = 0.2f;
		_cost = 1.0f;
	}

	public override void Init(GoapAgent a)
    {
        _agent = a;
        _target = _agent.HomeCity.transform;
        inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition> {}; // no preconditions
        _effects = new HashSet<Effect> {Effect.MAKE_FOOD}; // only makes food
        _timeToComplete = 0.2f;
		_cost = 1.0f;
	}

	public override ActionStatus PerformAction()
    {
        if (!_activated)
        {
            _activated = true;
            _waitTimer = Time.time + _timeToComplete;
        }
        if (inRange)
        {
            if (Time.time > _waitTimer) // check if wait is over
                _waiting = false;

            if (!_waiting) // if not waiting then perform action
            {
                // withdraw one food from city
                _target.GetComponent<City>().WithdrawResource(CityResource.FOOD, 1);
                _agent.AddResource(CityResource.FOOD, 1); // add 1 food to agent
                return ActionStatus.COMPLETE;
            }

            return ActionStatus.WAITING;
        }

        return ActionStatus.FAILED;
    }

    //public override bool SatisfiesPreconditions(GoapAgent agent)
    //{
    //    return agent.FoodHeld > 0; // only condition is that agent holds food
    //}
    public override bool SatisfiesPreconditions(WorldState worldState)
    {
        return true; // no world-based preconditions
    }

    public override bool SatisfiesPreconditions(HashSet<Effect> worldState)
    {
        return worldState.Contains(Effect.MAKE_FOOD); // has made 
    }
}
