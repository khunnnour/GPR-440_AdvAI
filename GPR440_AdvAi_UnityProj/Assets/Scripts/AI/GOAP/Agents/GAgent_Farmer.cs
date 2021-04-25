using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GAgent_Farmer : GoapAgent
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!_seeking) // if not currently seeking, perform the action
            PerformCurrentAction();
        else // otherwise check if you reached target
        {
            // check if in range yet
            if ((_plan[0].Target.position - transform.position).sqrMagnitude <= 1f)
            {
                _seeking = false;
                _plan[0]._inRange = true;
            }
        }
    }

    protected override void PerformCurrentAction()
    {
        // check if action is in range
        if (_plan[0]._inRange)
        {
            // if it is, then perform the action
            _plan[0].PerformAction();
        }
        else
        {
            // if not, then set seek target
            _steering.SetTargetLocation(_plan[0].Target.position);
            _seeking = true;
        }
    }

    protected override HashSet<Effect> CalcGoalRelevancy()
    {
        throw new System.NotImplementedException();
    }
}
