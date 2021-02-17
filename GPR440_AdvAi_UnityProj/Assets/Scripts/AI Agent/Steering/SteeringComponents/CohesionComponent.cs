using UnityEngine;

public class CohesionComponent : SteerComponent
{
    public CohesionComponent(Transform self) : base(self)
    {
    }

    public override Vector3 GetSteering(Transform[] nearby)
    {
        // accumulator for position
        Vector3 avgPos = Vector3.zero;

        // accumulate positions
        foreach (Transform transform in nearby)
        {
            avgPos += transform.position;
        }

        // average them out
        if (nearby.Length > 0)
            avgPos *= (1f / nearby.Length);
        else
            avgPos = _self.position;

        // return the desired acceleration
        return (avgPos - _self.position).normalized;
    }
}
