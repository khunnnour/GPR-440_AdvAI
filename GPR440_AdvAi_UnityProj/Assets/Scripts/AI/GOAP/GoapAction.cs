using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Precondition
{
    LOCATED_AT,
    HAS_WEAPON,
    HAS_FOOD,
    HAS_ORE,
    IS_SUPERIOR
}

public enum Effect
{
    MAKE_ORE,
    MAKE_FOOD,
    MAKE_WEAPON,
    DEPOSIT_ORE,
    DEPOSIT_FOOD,
    DEPOSIT_WEAPON,
    MAKE_PERSON,
    INCREASES_HUNGER,
    DECREASES_HUNGER,
    EQUIP_WEAPON,
    JOINS_PATROL,
    ATTACKS_PATROL,
    SET_GOAL_BIAS
}

public abstract class GoapAction
{
    public bool inRange;
    
    protected HashSet<Precondition> _preconditions;
    protected HashSet<Effect> _effects;
    protected Transform _target;
    protected GoapAgent _agent;

    public HashSet<Precondition> Preconditions => _preconditions;
    public HashSet<Effect> Effects => _effects;

    public Transform Target => _target;

    protected GoapAction() { }

    protected GoapAction(GoapAgent a)
    {
        _agent = a;
    }

    public abstract void Init(GoapAgent a);
    
    public abstract bool PerformAction();

    // removes a condition (marking it complete) from preconditions hashset
    /*public void RemovePrecondition(Precondition condition)
    {
        if (_preconditions.Contains(condition))
            _preconditions.Remove(condition);
    }*/
	
    // check if the agent potentially performing the action satisfies the preconditions
    //public abstract bool SatisfiesPreconditions(GoapAgent agent);
    // check if the current world state satisfies the preconditions
    public abstract bool SatisfiesPreconditions(WorldState worldState);
    // check if the current world state satisfies the preconditions
    public abstract bool SatisfiesPreconditions(HashSet<Effect> worldState);
}
