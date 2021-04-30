using System.Collections.Generic;

public class Action_Deliver_Ore : GoapAction
{
    public Action_Deliver_Ore()
    {
    }

    public Action_Deliver_Ore(GoapAgent a) : base(a)
    {
        _target = _agent.HomeCity.transform;
        inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition> {Precondition.HAS_ORE}; // no preconditions
        _effects = new HashSet<Effect> {Effect.DEPOSIT_ORE}; // only makes food
        _timeToComplete = 0.1f;
    }

    public override void Init(GoapAgent a)
    {
        _agent = a;
        _target = _agent.HomeCity.transform;
        inRange = _target == null; // sets in range to true if no target
        _preconditions = new HashSet<Precondition> {Precondition.HAS_ORE}; // no preconditions
        _effects = new HashSet<Effect> {Effect.DEPOSIT_ORE}; // only makes food
        _timeToComplete = 0.1f;
    }

    public override ActionStatus PerformAction()
    {
        if (inRange)
        {
            if (!_waiting)
            {
                // add to city (should be the only target given)
                _target.GetComponent<City>().DepositResource(CityResource.ORE, _agent.OreHeld);
                // remove food from agent
                _agent.LoseResource(CityResource.ORE, _agent.OreHeld);
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
        return worldState.Contains(Effect.MAKE_ORE); // has made 
    }
}
