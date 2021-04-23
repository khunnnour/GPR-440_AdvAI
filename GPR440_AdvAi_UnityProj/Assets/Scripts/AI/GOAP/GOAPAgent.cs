using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GoapAgent : MonoBehaviour
{
    protected GoapAction[] _availableActions;
    protected HashSet<Effect> _goals;
    protected List<GoapAction> _plan;
    protected int _hungerLvl;
    
    private GoapSteering _steering;
    private float _hungerTimer,_hungerInterval;

    public GoapAction[] AvailableActions => _availableActions;
    public HashSet<Effect> Goals => _goals;

    // Start is called before the first frame update
    void Start()
    {
        _steering = GetComponent<GoapSteering>();
        
        _hungerLvl = 0;
        _hungerInterval = GameManager.instance.GameDataObj.AllData.hungerSpeed;
        _hungerTimer = Time.time + _hungerInterval;
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
            }

            _hungerTimer = Time.time + _hungerInterval;
        }

        // check if it has a plan/has finished it's plan
        if (_plan.Count == 0)
        {
            // request new plan if it has nothing to do
            GoapPlanner.instance.RequestPlan(this, CalcGoalRelevancy());
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
}
