using System.Collections;
using System.Collections.Generic;
using AI_Agent;
using UnityEngine;
using UnityEngine.UI;

public class FlockSteeringBehavior : MonoBehaviour
{
    public float checkRadius = 5f;

    [Header("Avoidance Settings")]
    [Range(0f, 10f)] public float forLength = 3f;
    [Range(0f, 10f)] public float forwardAvoidCoeff = 2f;
    [Range(0f, 10f)] public float sideLength = 1f;
    [Range(0f, 10f)] public float sideAvoidCoeff = 1.5f;
    [Range(0f, 4.3f)] public float angle = 0.5f;

    [Header("Steering Values")] public float forwardCoeff = 4f;
    public float alignmentCoeff = 4f;
    public float minimumSeparation = 2f;
    public float seekCoeff = 2f;

    [Header("Steering Weights")] 
    [Range(0f, 10f)] public float separationWeight = 0.2f;
    [Range(0f, 10f)] public float cohesionWeight = 0.1f;
    [Range(0f, 10f)]  public float alignmentWeight = 0.1f;
    [Range(0f, 10f)]  public float forwardWeight = 0.3f;
    [Range(0f, 10f)]public float avoidWeight = 0.3f;
    [Range(0f, 10f)]public float seekWeight = 0.3f;

    private UnitManager _unitManager;
    private Movement _moveScript;
    private Vector3 _targetLocation;
    private int _collisions;
    private bool _newDestination = false;
    
    private CohesionComponent _cohesion;
    private AlignmentComponent _alignment;
    private SeparationComponent _separation;
    private TriWhiskAvoidComponent _avoid;
    private SeekComponent _seek;

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
        _avoid = new TriWhiskAvoidComponent(t, forLength, forwardAvoidCoeff, sideLength, sideAvoidCoeff, angle);
        _seek = new SeekComponent(t, seekCoeff);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSteering();
        
        // check if you've reached a provided destination then stop seeking to it
        // checks if it is within 0.71 units
        if(_newDestination)
            if ((transform.position - _targetLocation).sqrMagnitude <= 0.5f)
                _newDestination = false;
    }

    private void UpdateSteering()
    {
        // get the nearby unit
        Transform[] nearby = _unitManager.GetNearbyUnits(transform, checkRadius);

        // debug out the calculated rays
        var tran = transform;
        Vector3 position = tran.position;
        
        // TODO: Try to move loop functionality here?
        // accumulator for new acceleration (initialize with forward vector)
        float rot = -tran.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector3 forw = new Vector3(Mathf.Sin(rot), Mathf.Cos(rot), 0f) * (forwardCoeff);
        Vector3 cohe = _cohesion.GetSteering(nearby);
        Vector3 algn = _alignment.GetSteering(nearby);
        Vector3 sepa = _separation.GetSteering(nearby);
        Vector3 wAvd = _avoid.GetSteering(nearby);
        Vector3 seek = _seek.GetSteering(_newDestination ? _targetLocation : position);

        Debug.DrawRay(position, forw, Color.green);
        Debug.DrawRay(position, cohe, Color.red);
        Debug.DrawRay(position, algn, Color.yellow);
        Debug.DrawRay(position, sepa, Color.blue);
        Debug.DrawRay(position, wAvd, Color.cyan);
        Debug.DrawRay(position, seek, Color.white);

        /*Vector3 newAcc = new Vector3(Mathf.Sin(rot), Mathf.Cos(rot), 0f) * (forwardCoeff * forwardWeight);

        // update the steering components and get their new accelerations
        newAcc += _cohesion.GetSteering(nearby) * cohesionWeight;
        newAcc += _alignment.GetSteering(nearby) * alignmentWeight;
        newAcc += _separation.GetSteering(nearby) * separationWeight;*/

        Vector3 newAcc = forw * forwardWeight + cohe * cohesionWeight + algn * alignmentWeight +
                         sepa * separationWeight + wAvd * avoidWeight + seek * avoidWeight;
        newAcc = newAcc.normalized * _moveScript.maxAcc;

        // debug out the final acceleration
        Debug.DrawRay(position, newAcc, Color.magenta);

        _moveScript.SetAcceleration(newAcc);
    }

    public void SetTargetLocation(Vector2 tL)
    {
        _targetLocation = tL;
        _newDestination = true;
    }
    
    private void OnCollisionEnter2D()
    {
        _unitManager.ReportCollision();
    }
}
