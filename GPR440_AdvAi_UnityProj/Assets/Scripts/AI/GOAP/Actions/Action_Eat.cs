using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Eat : GoapAction
{
    public Action_Eat()
    {
    }

    public Action_Eat(GoapAgent a) : base(a)
    {
        _target = _agent.transform;
        inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition> {Precondition.HAS_FOOD}; // no preconditions
        _effects = new HashSet<Effect> {Effect.DECREASES_HUNGER}; // only makes food
        _timeToComplete = 0.2f;
    }

    public override void Init(GoapAgent a)
    {
        _agent = a;
        _target = _agent.transform;
        inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition> {Precondition.HAS_FOOD}; // no preconditions
        _effects = new HashSet<Effect> {Effect.DECREASES_HUNGER}; // only makes food
        _timeToComplete = 0.2f;
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
                if (_agent.EatFood())
                    return ActionStatus.COMPLETE;
                
                return ActionStatus.FAILED;
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
