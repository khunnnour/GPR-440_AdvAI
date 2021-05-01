using System.Collections.Generic;
using UnityEngine;

public class Action_Farm : GoapAction
{
    public Action_Farm() { }

    public Action_Farm(GoapAgent a) : base(a)
    {
        _target = _agent.HomeCity.farm;
        inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition>(); // no preconditions
        _effects = new HashSet<Effect> {Effect.MAKE_FOOD}; // only makes food
        _timeToComplete = 0.5f;
		_cost = 1.5f;
	}

	public override void Init(GoapAgent a)
    {
        _agent = a;
        _target = _agent.HomeCity.farm;
        inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition>(); // no preconditions
        _effects = new HashSet<Effect> {Effect.MAKE_FOOD}; // only makes food
        _timeToComplete = 0.5f;
		_cost = 1.5f;
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
                _agent.AddResource(CityResource.FOOD, 1); // pick up food
                return ActionStatus.COMPLETE;
            }

            return ActionStatus.WAITING;
        }
        return ActionStatus.FAILED;
    }

    //public override bool SatisfiesPreconditions(GoapAgent agent)
    //{
    //    return true; // no preconditions
    //}
    public override bool SatisfiesPreconditions(WorldState worldState)
    {
        return true; // no preconditions
    }
    public override bool SatisfiesPreconditions(HashSet<Effect> worldState)
    {
        return true; // no preconditions
    }
}
