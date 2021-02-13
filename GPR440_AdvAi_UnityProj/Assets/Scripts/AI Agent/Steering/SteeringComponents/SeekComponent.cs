using UnityEngine;

public class SeekComponent : SteerComponent
{
    private readonly float _intensity;

    public SeekComponent(Transform self, float coeff) : base(self)
    {
        _intensity = coeff;
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

        return diff * _intensity;
    }
}