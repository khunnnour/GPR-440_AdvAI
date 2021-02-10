using System;
using AI_Agent;
using UnityEngine;
using UnityEngine.UI;

public class TriWhiskerBrain : MonoBehaviour
{
    public float driftSpeed = 5f;
    
    [Header("Whisker Settings")] public float fLength = 3f;
    public float sLength = 1f;
    public float angle = 20f;

    [Header("Avoidance Settings")]
    [Range(0f, 10f)] public float forwardAvoidCoeff = 2f;
    [Range(0f, 10f)] public float sideAvoidCoeff = 1.5f;
    public bool useHitNormals = true;

    private Movement _moveScript;
    private Vector3 _targetVelocity;
    private float _rayOffset;
    private int _collisions;
    private Text _collText;

    // Start is called before the first frame update
    void Start()
    {
        _collisions = 0;
        _collText = GetComponentInChildren<Text>();
        _collText.text = _collisions.ToString();

        _moveScript = GetComponent<Movement>();
        UpdateRay();
        _moveScript.SetTargetDirection(Vector3.up);

        // calculate offset so ray is outside of collider
        _rayOffset = GetComponent<CircleCollider2D>().radius * transform.localScale.x + 0.001f;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRay();
        _moveScript.SetTargetDirection(_targetVelocity);
    }

    // update and then cast the whisker ray
    private void UpdateRay()
    {
        RaycastHit2D[] hits = new RaycastHit2D[3];

        // get the base forward rotation
        float rot = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 forward = new Vector2(Mathf.Sin(rot), Mathf.Cos(rot));

        Vector2 or, dir;
        // -- handle forward whisker -- //
        // get origin (slightly outside of agent)
        Vector2 position = transform.position;
        or = position + forward * _rayOffset;
        // Cast ray (no need to calc direction, just use forward)
        Debug.DrawRay(or, forward * fLength, Color.green);
        hits[0] = Physics2D.Raycast(or, forward, fLength);

        // -- handle side whiskers -- //
        // left whisker -
        // get direction and origin
        dir = new Vector2(Mathf.Sin(rot - angle), Mathf.Cos(rot - angle));
        or = position + dir * _rayOffset;
        // cast the ray
        Debug.DrawRay(or, dir * sLength, Color.blue);
        hits[1] = Physics2D.Raycast(or, dir, sLength);

        // right whisker -
        // get direction and origin
        dir = new Vector2(Mathf.Sin(rot + angle), Mathf.Cos(rot + angle));
        or = position + dir * _rayOffset;
        // cast the ray
        Debug.DrawRay(or, dir * sLength, Color.blue);
        hits[2] = Physics2D.Raycast(or, dir, sLength);

        // handle avoidance
        Avoid(hits, forward);
    }

    // if the ray hit something, then calculate the new target velocity
    private void Avoid(RaycastHit2D[] hits, Vector3 forward)
    {
        /*
        // attempt to avoid if there is a hit
        if (hit.collider)
            Avoid(hit);
        else
        {
            // if target velocity hasn't changed in a few frames
            //    ramp the velocity to the max magnitude
            if (_targetVelocity == _prevTarget&&_targetVelocity.sqrMagnitude < _moveScript.maxSpeed * _moveScript.maxSpeed)
            {
                _targetVelocity = Vector3.Lerp(_targetVelocity, _targetVelocity.normalized * _moveScript.maxSpeed,
                    _moveScript.acceleration);
            }
            // if the ray is short then slowly ramp the vector back up to straight forward at the right length
            // if ()
            //    _targetVelocity = _whisker.direction * _moveScript.maxSpeed;
        }
        */

        // -- process the raycasts -- //
        Vector2 accum = Vector2.zero;
        Vector2 position = transform.position;
        int numHits = 0;
        if (useHitNormals)
        {
            // collect the hit normals
            if (hits[0]) //if forward hit
            {
                accum += (-position + (hits[0].point + hits[0].normal * forwardAvoidCoeff));
                numHits++;
            }

            if (hits[1]) //if left hit
            {
                accum += (-position + (hits[1].point + hits[1].normal * sideAvoidCoeff));
                numHits++;
            }

            if (hits[2]) //if right hit
            {
                accum += (-position + (hits[2].point + hits[2].normal * sideAvoidCoeff));
                numHits++;
            }
            
            if (numHits > 0)
                _targetVelocity = accum.normalized * _moveScript.maxAcc;
        }
        else
        {
            // otherwise do fancy decision making
            // get the left vector
            Vector2 left = new Vector2(-forward.y, forward.x);

            // hit on left
            if (hits[1] && !hits[2])
            {
                // turn right
                _targetVelocity = left * -sideAvoidCoeff;
                numHits++;
            }
            // hit on right
            else if (!hits[1] && hits[2])
            {
                // turn left
                _targetVelocity = left * sideAvoidCoeff;
                numHits++;
            }
            // hit on both sides
            else if (hits[1] && hits[2])
            {
                // turn around
                _targetVelocity = -forward * sideAvoidCoeff;
                numHits++;
            }
            // only a hit forward
            else if (hits[0])
            {
                // turn left or right based on where the normal points
                _targetVelocity = left * (Mathf.Sign(Vec2Cross(forward, hits[0].normal)) * forwardAvoidCoeff);
                numHits++;
            }
            // else no hits so do nothing
            
            /*if (hits[0])
            {
                // if forward whisker hit something, and:
                // hit on left
                if (hits[1] && !hits[2])
                {
                    // turn right
                    _targetVelocity = left * -sideAvoidCoeff;
                }
                // hit on right
                else if (!hits[1] && hits[2])
                {
                    // turn left
                    _targetVelocity = left * sideAvoidCoeff;
                }
                else
                {
                    // no side hits then turn left or right based on where the normal points
                    _targetVelocity = left * (Mathf.Sign(Vec2Cross(forward, hits[0].normal)) * forwardAvoidCoeff);
                }
            }
            else
            {
                // if forward whisker hit nothing, but:
                // hit on left
                if (hits[1] && !hits[2])
                {
                    // turn right
                    _targetVelocity = left * -sideAvoidCoeff;
                }
                // hit on right
                else if (!hits[1] && hits[2])
                {
                    // turn left
                    _targetVelocity = left * sideAvoidCoeff;
                }
                // else no no hits at all so do nothing
            }*/
            
        }

        if (numHits <= 0) // if there are no hits
        {
            // and if velocity is under the max speed: slowly accelerate straight forward
            //if (_targetVelocity.sqrMagnitude < _moveScript.maxSpeed * _moveScript.maxSpeed)
            //    _targetVelocity = Vector3.Lerp(_targetVelocity, _targetVelocity.normalized * _moveScript.maxSpeed,
            //         _moveScript.maxAcc * Time.deltaTime);
            _targetVelocity = Vector3.Lerp(_targetVelocity, forward * _moveScript.maxAcc,
                driftSpeed * Time.deltaTime);
        }

        /*
        // normal accumulator
        //Vector3 normAccum = Vector3.zero;
        //Vector3 diffAccum = Vector3.zero;
        Vector3 accum = Vector3.zero;
        int numHits = 0;
        // process all of the raycasthits
        for (int i = 0; i < numWhiskers; i++)
        {
            // if there's a hit 
            if (hits[i].collider)
            {
                // draw hit normal
                Debug.DrawRay(hits[i].point, hits[i].normal, Color.red);
                // add the normal to the accumulator
                //normAccum += (Vector3) hits[i].normal * avoidSeverity;

                // add the diff to the accumulator
                //diffAccum += (Vector3) hits[i].point - transform.position;

                // add this whisker's desired target to accumulator
                //accum +=  - transform.position + (Vector3) (hits[i].point+hits[i].normal * avoidSeverity);

                accum += (-transform.position + (Vector3) (hits[i].point + hits[i].normal*avoidSeverity)).normalized;

                numHits++;
            }
            else
            {
                // if no hit, then add forward as the desired target
                //accum += forward*0.5f;
            }
        }

        // calculate velocity based on accumulated targets only if there is a hit
        if (numHits > 0)
        {
            _targetVelocity = accum * (1f / numHits);
            //Debug.Log(numHits + " | " + _targetVelocity + " = " + accum + " / " + numHits);
        }

        // if there are no hits
        if (numHits <= 0)
        {
            // and if velocity is under the max speed
            if (_targetVelocity.sqrMagnitude < _moveScript.maxSpeed * _moveScript.maxSpeed)
                _targetVelocity = Vector3.Lerp(_targetVelocity, _targetVelocity.normalized * _moveScript.maxSpeed,
                    _moveScript.maxAcc*Time.deltaTime);
        }
        /*else
        {
            // otherwise, calculate a new target velocity
            //float coeff = 1f / numHits;
            //_targetVelocity = coeff * (diffAccum + normAccum * avoidSeverity+);
            _targetVelocity = accum
            Debug.Log(_targetVelocity + " = " + coeff + " * (" + diffAccum + " + " + normAccum + " * " + avoidSeverity +
                      ")");
        }*/


        //_targetVelocity = diff + (Vector3) hit.normal * avoidSeverity;
        // draw the target velocity
        Debug.DrawRay(transform.position, _targetVelocity, Color.cyan);
    }

    private float Vec2Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        _collisions++;
        _collText.text = _collisions.ToString();
    }
}