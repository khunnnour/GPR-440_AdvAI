using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowComponent : SteerComponent
{
    // reference to the flow field
    private readonly FlowField _flowField;

    public FlowComponent(Transform self, FlowField fF) : base(self)
    {
        _flowField = fF;
    }

    public override Vector3 GetSteering(Transform[] nearby)
    {
        // seek doesnt care about nearby, and if no target then no acceleration
        return Vector3.zero;
    }

    public Vector3 GetSteering()
    {
        // return flow direction of their node
        return _flowField.GetFlowDir(_self.position);
    }
}
