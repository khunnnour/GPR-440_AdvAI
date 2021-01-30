using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class Brain : MonoBehaviour
{
    public float avoidSeverity = 2f;
    [Header("Whisker Settings")] 
    public int numWhiskers = 1;
    public float whiskerLength = 3f;
    public bool sweep = false;
    public float sweepAngle = 0.349f;
    public float sweepSpeed = 0.5f;

    private Movement _moveScript;
    private Vector3 _targetVelocity,_prevTarget;
    private Ray2D _whisker;
    private float _rayOffset;
    private float[] _rotOffsets;
    private float _wedge, _halfWedge;
    private int _collisions;
    private Text _collText;
    
    // Start is called before the first frame update
    void Start()
    {
        _collisions = 0;
        _collText = GetComponentInChildren<Text>();
        _collText.text = _collisions.ToString();

        _wedge = sweepAngle / numWhiskers;
        _halfWedge = _wedge * 0.5f;
        _rotOffsets = new float[numWhiskers];
        for (int i = 0; i < numWhiskers; i++)
        {
            _rotOffsets[i] = i * _wedge - _halfWedge * (numWhiskers - 1);
        }
        
        _moveScript = GetComponent<Movement>();
        UpdateRay();
        _moveScript.SetTargetelocity(_targetVelocity);

        // calculate offset so ray is outside of collider
        _rayOffset = GetComponent<CircleCollider2D>().radius * transform.localScale.x + 0.001f;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRay();
        _moveScript.SetTargetelocity(_targetVelocity);
    }

    // update and then cast the whisker ray
    private void UpdateRay()
    {
        RaycastHit2D[] hits = new RaycastHit2D[numWhiskers]; 
        
        // get the base forward rotation
        float rot = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector3 forward = new Vector3(Mathf.Sin(rot), Mathf.Cos(rot), 0f);
        float newRot;
        // update all of the whiskers
        for (int i = 0; i < numWhiskers; i++)
        {
            
            // offset to current whiskers center rotation
            newRot = rot+_rotOffsets[i];
            
            // if sweeping then update the rotation
            if (sweep)
            {
                newRot += _halfWedge * Mathf.Sin(Time.time * sweepSpeed);
            }

            _whisker.direction = new Vector2(Mathf.Sin(newRot), Mathf.Cos(newRot));
            _whisker.origin = (Vector2) transform.position + _whisker.direction * _rayOffset;

            Debug.DrawRay(_whisker.origin, _whisker.direction * whiskerLength, Color.green);

            // cast the ray
            hits[i] = Physics2D.Raycast(_whisker.origin, _whisker.direction, whiskerLength);
            
            // handle avoidance
            Avoid(hits, forward);
        }
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

        // normal accumulator
        Vector3 normAccum = Vector3.zero;
        Vector3 diffAccum = Vector3.zero;
        Vector3 accum = Vector3.zero;
        int numHits = 0;
        // process all of the raycasthits
        for (int i = 0; i < numWhiskers; i++)
        {
            // if there's a hit 
            if (hits[i].collider)
            {
                // draw hit normal
                Debug.DrawRay(hits[i].point, hits[i].normal * avoidSeverity, Color.red);
                // add the normal to the accumulator
                //normAccum += (Vector3) hits[i].normal * avoidSeverity;

                // add the diff to the accumulator
                //diffAccum += (Vector3) hits[i].point - transform.position;

                // add this whisker's desired target to accumulator
                accum += (Vector3) hits[i].point - transform.position + (Vector3) hits[i].normal * avoidSeverity;

                numHits++;
            }
            else
            {
                // if no hit, then add forward as the desired target
                accum += forward*0.5f;
            }
        }

        // calculate velocity based on accumulated targets
        _targetVelocity = accum * (1f / numWhiskers);
        Debug.Log(_targetVelocity + " = " + accum + " / " + numWhiskers);
        
        // if there are no hits then accelerate straight forward to max speed if not already there
        if (numHits <= 1)
        {
            if (_targetVelocity.sqrMagnitude < _moveScript.maxSpeed * _moveScript.maxSpeed)
                _targetVelocity = Vector3.Lerp(_targetVelocity, _targetVelocity.normalized * _moveScript.maxSpeed,
                    _moveScript.acceleration);
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

        // log old target velocity
        _prevTarget = _targetVelocity;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        _collisions++;
        _collText.text = _collisions.ToString();
    }
}
