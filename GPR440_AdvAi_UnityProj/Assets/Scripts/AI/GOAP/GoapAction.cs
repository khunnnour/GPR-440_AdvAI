using System.Collections.Generic;
using UnityEngine;

public enum Precondition
{
    HAS_FOOD,
    HAS_ORE,
    HAS_WEAPON,
	WEAPON_EQUIPED
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
    ATTACKS_ENEMY,
    SET_GOAL_BIAS,
	DRAFT_AGENT,
	EMPLOY_AGENT
}

public enum ActionStatus
{
    FAILED,
    WAITING,
    COMPLETE
}

public abstract class GoapAction
{
    public bool inRange;
    
    protected HashSet<Precondition> _preconditions;
    protected HashSet<Effect> _effects;
    protected Transform _target;
    protected GoapAgent _agent;
    protected bool _waiting=true,_activated; // bool if waiting to finish performing an action
    protected float _timeToComplete,_waitTimer; // time to wait when performing the action; timer for waiting
	protected float _cost=1.0f;

    public HashSet<Precondition> Preconditions => _preconditions;
    public HashSet<Effect> Effects => _effects;
	public float Cost => _cost;
    public Transform Target => _target;

    protected GoapAction() { }

    protected GoapAction(GoapAgent a)
    {
        _agent = a;
    }

    public abstract void Init(GoapAgent a);
    
    public abstract ActionStatus PerformAction();

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
