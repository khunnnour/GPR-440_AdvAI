using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapAgent : MonoBehaviour
{
    private Queue<GoapAction> _plan;
    private GoapSteering _steering;

    // Start is called before the first frame update
    void Start()
    {
        _steering = GetComponent<GoapSteering>();
    }

    // Update is called once per frame
    void Update()
    {
        // check if it has a plan
        if (_plan.Count == 0)
        {
            GoapPlanner.instance.RequestPlan(this,CalcGoalRelevancy());
        }
    }

    // calculate goal relevancy
    private Effect CalcGoalRelevancy()
    {
        // logic for determining relevancy of a goal
        // add the bias should there be one

        // return the goal w highest relevancy
        return Effect.MAKE_FOOD;
    }
}
