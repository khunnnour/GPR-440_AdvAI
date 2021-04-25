﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GoapAgent : MonoBehaviour
{
    protected GoapSteering _steering;
    protected GoapAction[] _availableActions;
    protected HashSet<Effect> _goals;
    protected List<GoapAction> _plan;
    protected int _hungerLvl;
    protected int _foodHeld, _oreHeld, _weaponHeld;
     protected bool _weaponEquiped,_seeking;
   
    private float _hungerTimer,_hungerInterval;

    public GoapAction[] AvailableActions => _availableActions;
    public HashSet<Effect> Goals => _goals;
    public bool HasWeapon => _weaponEquiped;

    public int FoodHeld => _foodHeld;
    public int OreHeld => _oreHeld;

    // Start is called before the first frame update
    void Start()
    {
        _steering = GetComponent<GoapSteering>();
        
        _seeking = false;
        _weaponEquiped = false;
        
        _hungerLvl = 0;
        _hungerInterval = GameManager.instance.GameDataObj.AllData.hungerSpeed;
        _hungerTimer = Time.time + _hungerInterval;

        _foodHeld = 0;
        _oreHeld = 0;
        _weaponHeld = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // handle hunger
        if (Time.time >= _hungerTimer)
        {
            _hungerLvl++; //increment hunger

            // check if dead
            if (_hungerLvl >= 3)
            {
                // TODO: Add dying
                Debug.Log("Agent died");
            }

            _hungerTimer = Time.time + _hungerInterval;
        }

        // check if it has a plan/has finished it's plan
        if (_plan.Count == 0)
        {
            // request new plan if it has nothing to do
            GoapPlanner.Instance.RequestPlan(this, CalcGoalRelevancy());
        }
        else
        {
            // if there is a plan then perform the action
            PerformCurrentAction();
        }
    }

    // cycle thru preconditions and handle them
    protected abstract void PerformCurrentAction();

    // calculate goal relevancy
    // add the bias should there be one
    // return the goal w highest relevancy
    protected abstract HashSet<Effect> CalcGoalRelevancy();

    public void SetPlan(List<GoapAction> p)
    {
        _plan = p;
    }

    public void AddResource(CityResource resource)
    {
        switch (resource)
        {
            case CityResource.ORE:
                _oreHeld++;
                break;
            case CityResource.FOOD:
                _foodHeld++;
                break;
            case CityResource.WEAPON:
                _weaponHeld++;
                break;
        }
    }
    
    public void LoseResource(CityResource resource)
    {
        switch (resource)
        {
            case CityResource.ORE:
                _oreHeld--;
                break;
            case CityResource.FOOD:
                _foodHeld--;
                break;
            case CityResource.WEAPON:
                _weaponHeld--;
                break;
        }
    }
}
