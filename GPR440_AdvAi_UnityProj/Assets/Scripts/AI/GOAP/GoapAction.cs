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
    private HashSet<Precondition> _preconditions;
    private HashSet<Effect> _effects;

    public HashSet<Precondition> Preconditions => _preconditions;
    public HashSet<Effect> Effects => _effects;

    private Transform _target;

    protected GoapAction(Transform t)
    {
        _target = t;
    }

    // removes a condition (marking it complete) from preconditions hashset
    public void RemovePrecondition(Precondition condition)
    {
        if (_preconditions.Contains(condition))
            _preconditions.Remove(condition);
    }
}
