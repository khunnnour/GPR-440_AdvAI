using System.Collections;
using System.Collections.Generic;
using AI_Agent;
using UnityEngine;
using UnityEngine.UI;

public class FlockSteeringBehavior : MonoBehaviour
{
    public float checkRadius = 5f;

    [Header("Steering Values")]
    public float forwardCoeff = 4f;
    public float alignmentCoeff = 4f;
    public float minimumSeparation = 2f;

    [Header("Steering Weights")] public float separationWeight = 0.2f;
    public float cohesionWeight = 0.1f;
    public float alignmentWeight = 0.1f;
    public float forwardWeight = 0.3f;
    public float avoidWeight = 0.3f;

    private UnitManager _unitManager;
    private Movement _moveScript;
    private Vector2 _targetLocation;
    private int _collisions;

    private CohesionComponent _cohesion;
    private AlignmentComponent _alignment;
    private SeparationComponent _separation;

    // Start is called before the first frame update
    void Start()
    {
        _unitManager = transform.GetComponentInParent<UnitManager>();

        _moveScript = GetComponent<Movement>();
        _moveScript.SetTargetDirection(Vector3.up);

        Transform t = transform;
        _cohesion = new CohesionComponent(t);
        _alignment = new AlignmentComponent(t, alignmentCoeff);
        _separation = new SeparationComponent(t, minimumSeparation);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSteering();
    }

    private void UpdateSteering()
    {
        // get the nearby units
        Transform[] nearby = _unitManager.GetNearbyUnits(transform, checkRadius);

        // accumulator for new acceleration (initialize with forward vector)
        float rot = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector3 forw = new Vector3(Mathf.Sin(rot), Mathf.Cos(rot), 0f) * (forwardCoeff);
        Vector3 cohe = _cohesion.GetSteering(nearby);
        Vector3 algn = _alignment.GetSteering(nearby);
        Vector3 sepa = _separation.GetSteering(nearby);

        Debug.DrawRay(transform.position, forw, Color.green);
        Debug.DrawRay(transform.position, cohe, Color.red);
        Debug.DrawRay(transform.position, algn, Color.yellow);
        Debug.DrawRay(transform.position, sepa, Color.blue);

        /*Vector3 newAcc = new Vector3(Mathf.Sin(rot), Mathf.Cos(rot), 0f) * (forwardCoeff * forwardWeight);

        // update the steering components and get their new accelerations
        newAcc += _cohesion.GetSteering(nearby) * cohesionWeight;
        newAcc += _alignment.GetSteering(nearby) * alignmentWeight;
        newAcc += _separation.GetSteering(nearby) * separationWeight;*/

        Vector3 newAcc = forw * forwardWeight + cohe * cohesionWeight + algn * alignmentWeight +
                         sepa * separationWeight;
        newAcc = newAcc.normalized * _moveScript.maxAcc;
        
        Debug.DrawRay(transform.position, newAcc, Color.magenta);

        _moveScript.SetAcceleration(newAcc);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        _unitManager.ReportCollision();
    }
}
