using UnityEngine;

public class SeekComponent : SteerComponent
{
    private float _maxSpeed, _maxSpeedSqr;
    
    public SeekComponent(float mS,Transform self) : base(self)
    {
        _maxSpeed = mS;
        _maxSpeedSqr = _maxSpeed * _maxSpeed;
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
        Vector3 diffNorm = diff.normalized;

        if (diff.sqrMagnitude < _maxSpeedSqr)
            return diffNorm * (_maxSpeed * -0.5f);
        
        return diffNorm;
    }
}