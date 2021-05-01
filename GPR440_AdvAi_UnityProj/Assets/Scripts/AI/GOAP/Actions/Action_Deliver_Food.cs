using System.Collections.Generic;
using UnityEngine;

public class Action_Deliver_Food : GoapAction
{
    public Action_Deliver_Food()
    {
    }

    public Action_Deliver_Food(GoapAgent a) : base(a)
    {
        _target = _agent.HomeCity.transform;
        inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition> {Precondition.HAS_FOOD}; // no preconditions
        _effects = new HashSet<Effect> {Effect.DEPOSIT_FOOD}; // only makes food
        _timeToComplete = 0.1f;
		_cost = 1.0f;
    }

    public override void Init(GoapAgent a)
    {
        _agent = a;
        _target = _agent.HomeCity.transform;
        inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition> {Precondition.HAS_FOOD}; // no preconditions
        _effects = new HashSet<Effect> {Effect.DEPOSIT_FOOD}; // only makes food
        _timeToComplete = 0.1f;
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
                // add to city (should be the only target given)
                _target.GetComponent<City>().DepositResource(CityResource.FOOD, _agent.FoodHeld);
                _agent.LoseResource(CityResource.FOOD, _agent.FoodHeld); // remove food from agent
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
