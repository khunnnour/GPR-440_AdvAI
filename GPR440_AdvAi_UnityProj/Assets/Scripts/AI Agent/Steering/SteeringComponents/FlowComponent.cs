using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowComponent : SteerComponent
{
    // reference to the flow field
    private readonly Grid _grid;
    private readonly int _team;
    
    public FlowComponent(Transform self, Grid fF, int t) : base(self)
    {
        _grid = fF;
        _team = t;
    }

    public override Vector3 GetSteering(Transform[] nearby)
    {
        // seek doesnt care about nearby, and if no target then no acceleration
        return Vector3.zero;
    }

    public Vector3 GetSteering()
    {
        // return flow direction of their node
        return _grid.GetFlowDir(_self.position, _team);
    }
}
