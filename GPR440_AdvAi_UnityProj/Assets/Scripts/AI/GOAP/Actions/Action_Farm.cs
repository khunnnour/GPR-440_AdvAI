using System.Collections.Generic;

public class Action_Farm : GoapAction
{
    public Action_Farm() { }

    public Action_Farm(GoapAgent a) : base(a)
    {
        _target = _agent.HomeCity.farm;
        inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition>(); // no preconditions
        _effects = new HashSet<Effect> {Effect.MAKE_FOOD}; // only makes food
    }

    public override void Init(GoapAgent a)
    {
        _agent = a;
        _target = _agent.HomeCity.farm;
        inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition>(); // no preconditions
        _effects = new HashSet<Effect> {Effect.MAKE_FOOD}; // only makes food
    }

    public override bool PerformAction()
    {
        if (inRange)
        {
            _agent.AddResource(CityResource.FOOD); // pick up food
            return true;
        }
        return false;
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
