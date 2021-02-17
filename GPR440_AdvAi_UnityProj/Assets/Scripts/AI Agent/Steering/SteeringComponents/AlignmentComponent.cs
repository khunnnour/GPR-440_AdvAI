﻿using UnityEngine;

public class AlignmentComponent : SteerComponent
{
    public AlignmentComponent(Transform self) : base(self)
    {
    }

    public override Vector3 GetSteering(Transform[] nearby)
    {
        // accumulator for rotation
        float rot = -_self.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector3 avgHeading = new Vector3(Mathf.Sin(rot), Mathf.Cos(rot), 0f);

        // accumulate rotations
        foreach (Transform transform in nearby)
        {
            rot = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            avgHeading += new Vector3(Mathf.Sin(rot), Mathf.Cos(rot), 0f);
        }

        // return the desired heading
        return avgHeading.normalized;
    }
}