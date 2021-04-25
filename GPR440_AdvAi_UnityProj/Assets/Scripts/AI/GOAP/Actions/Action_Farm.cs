using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Farm : GoapAction
{
    public Action_Farm(Transform t,GoapAgent a) : base(t,a)
    {
        _preconditions = new HashSet<Precondition>(); // no preconditions
        _effects = new HashSet<Effect> {Effect.MAKE_FOOD}; // only makes food
    }

    public override bool PerformAction()
    {
        if (_inRange)
        {
            _agent.AddResource(CityResource.FOOD); // pick up food
            return true;
        }
        return false;
    }

    public override bool SatisfiesPreconditions(GoapAgent agent)
    {
        return true; // no preconditions
    }
    public override bool SatisfiesPreconditions(WorldState worldState)
    {
        return true; // no preconditions
    }
    public override bool SatisfiesPreconditions(HashSet<Effect> worldState)
    {
        return true; // no preconditions
    }
}
