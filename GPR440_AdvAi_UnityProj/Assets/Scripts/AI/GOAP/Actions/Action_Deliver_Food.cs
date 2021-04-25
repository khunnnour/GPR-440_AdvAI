using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Deliver_Food: GoapAction
{
    public Action_Deliver_Food(Transform t,GoapAgent a) : base(a)
    {
        _target = _agent.HomeCity.transform;
        _inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition> {Precondition.HAS_FOOD}; // no preconditions
        _effects = new HashSet<Effect> {Effect.DEPOSIT_FOOD}; // only makes food
    }

    public override bool PerformAction()
    {
        if (_inRange)
        {
            _agent.LoseResource(CityResource.FOOD); // remove food from agent
            _target.GetComponent<City>().DepositResource(CityResource.FOOD, 1); // add to city (should be the only target given)
            return true;
        }
        return false;
    }

    public override bool SatisfiesPreconditions(GoapAgent agent)
    {
        return agent.FoodHeld > 0; // only condition is that agent holds food
    }
    public override bool SatisfiesPreconditions(WorldState worldState)
    {
        return true; // no world-based preconditions
    }
    public override bool SatisfiesPreconditions(HashSet<Effect> worldState)
    {
        return true; // no world-based preconditions
    }
}
