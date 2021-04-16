using UnityEngine;

public class SeekComponent : SteerComponent
{
    public SeekComponent(Transform self) : base(self)
    {
    }

    public override Vector3 GetSteering(Transform[] nearby)
    {
        // seek doesnt care about nearby, and if no target then no acceleration
        return Vector3.zero;
    }

    public Vector3 GetSteering(Vector3 tLoc)
    {
        // diff vector
        Vector3 diff = tLoc - _self.position;

        return diff.normalized;
    }
}