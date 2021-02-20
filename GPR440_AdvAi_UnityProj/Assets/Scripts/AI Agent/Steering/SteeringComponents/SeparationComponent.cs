using UnityEngine;

public class SeparationComponent : SteerComponent
{
    private readonly float _minSeparation;
    private readonly float _minSeparationSqr;

    public SeparationComponent(Transform self, float minSep) : base(self)
    {
        _minSeparation = minSep;
        _minSeparationSqr = minSep * minSep;
    }

    public override Vector3 GetSteering(Transform[] nearby)
    {
        // accumulator for distances
        Vector3 diffAccum = Vector3.zero;

        // accumulate rotations
        foreach (Transform transform in nearby)
        {
            // Get vector between neighbor and self
            Vector3 diff = _self.position - transform.position;
            // If it's within the minimum distance then add to the accum
            if (diff.sqrMagnitude < _minSeparationSqr)
            {
                // weights so that diff's that are shorter are weighted higher than further ones
                diff = diff.normalized * (_minSeparation - diff.magnitude);

                diffAccum += diff;
            }
        }

        // return the desired direction
        return diffAccum.normalized;
    }
}