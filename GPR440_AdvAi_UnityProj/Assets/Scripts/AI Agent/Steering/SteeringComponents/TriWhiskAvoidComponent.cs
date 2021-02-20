using UnityEngine;

// ReSharper disable once CheckNamespace
public class TriWhiskAvoidComponent : SteerComponent
{
    private readonly float _forLength;
    private readonly float _forwardAvoidCoeff;
    private readonly float _sideLength;
    private readonly float _sideAvoidCoeff;
    private readonly float _angle, _halfAng;
    private readonly float _rayOffset;
    private Vector3 _forward;

    public TriWhiskAvoidComponent(
        Transform self,
        float fL = 3f,
        float fAvoidCoeff = 2f,
        float sL = 1f,
        float sAvoidCoeff = 1.5f,
        float ang = 30f
    ) : base(self)
    {
        // assign the values
        _forLength = fL;
        _forwardAvoidCoeff = fAvoidCoeff;
        _sideLength = sL;
        _sideAvoidCoeff = sAvoidCoeff;
        _angle = ang;

        _halfAng = 0.5f * _angle;

        // calculate offset so ray is outside of collider
        _rayOffset = 0.5f * _self.localScale.x + 0.001f;
    }

    public override Vector3 GetSteering(Transform[] nearby)
    {
        // Cast the rays
        RaycastHit2D[] hits = UpdateRays();

        // -- process the raycasts -- //
        Vector3 desAcc = Vector3.zero;
        // get the base forward rotation
        float rot = -_self.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector3 forward = new Vector3(Mathf.Sin(rot), Mathf.Cos(rot), 0f);

        // otherwise do fancy decision making
        // get the left vector
        Vector2 left = new Vector2(-forward.y, forward.x);

        // hit on left
        if (hits[1] && !hits[2])
        {
            // turn right
            desAcc = left * -_sideAvoidCoeff;
        }
        // hit on right
        else if (!hits[1] && hits[2])
        {
            // turn left
            desAcc = left * _sideAvoidCoeff;
        }
        // hit on both sides
        else if (hits[1] && hits[2])
        {
            // turn around
            desAcc = -forward * _sideAvoidCoeff;
        }
        // only a hit forward
        else if (hits[0])
        {
            // turn left or right based on where the normal points
            desAcc = left * (Mathf.Sign(Vec2Cross(forward, hits[0].normal)) * _forwardAvoidCoeff);
        }
        // else no hits so do nothing
        // and the desired acceleration stays a 0 vector

        return desAcc.normalized;
    }

    private RaycastHit2D[] UpdateRays()
    {
        RaycastHit2D[] hits = new RaycastHit2D[3];

        // get the base forward rotation
        float rot = -_self.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector3 forward = new Vector3(Mathf.Sin(rot), Mathf.Cos(rot), 0f);

        Vector3 or, dir;
        // -- handle forward whisker -- //
        // get origin (slightly outside of agent)
        Vector3 position = _self.position;
        or = position + forward * _rayOffset;
        // Cast ray (no need to calc direction, just use forward)
        //Debug.DrawRay(or, forward * _forLength, Color.green);
        hits[0] = Physics2D.Raycast(or, forward, _forLength, ~LayerMask.GetMask("Agent"));

        // -- handle side whiskers -- //
        // left whisker -
        // get direction and origin
        dir = new Vector3(Mathf.Sin(rot - _halfAng), Mathf.Cos(rot - _halfAng));
        or = position + dir * _rayOffset;
        // cast the ray
        //Debug.DrawRay(or, dir * _sideLength, Color.blue);
        hits[1] = Physics2D.Raycast(or, dir, _sideLength,~LayerMask.GetMask("Agent"));

        // right whisker -
        // get direction and origin
        dir = new Vector2(Mathf.Sin(rot + _halfAng), Mathf.Cos(rot + _halfAng));
        or = position + dir * _rayOffset;
        // cast the ray
        //Debug.DrawRay(or, dir * _sideLength, Color.blue);
        hits[2] = Physics2D.Raycast(or, dir, _sideLength,~LayerMask.GetMask("Agent"));

        return hits;
    }

    private float Vec2Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }
}
