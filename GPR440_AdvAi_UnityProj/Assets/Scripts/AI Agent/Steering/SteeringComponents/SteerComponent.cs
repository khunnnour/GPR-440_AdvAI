using UnityEngine;

public abstract class SteerComponent
{
    protected Transform _self;

    protected SteerComponent(Transform self)
    {
        _self = self;
    }
    
    public abstract Vector3 GetSteering(Transform[] nearby);
}