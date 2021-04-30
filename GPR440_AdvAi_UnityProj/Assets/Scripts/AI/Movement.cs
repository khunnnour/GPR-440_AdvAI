using UnityEngine;
using Random = UnityEngine.Random;

public class Movement : MonoBehaviour
{
    struct PhysData
    {
        public Vector3 velocity;
        public Vector3 acceleration;
    };

    public float maxAcc = 2f;
    public float maxSpeed = 2f;

    private PhysData _physData;
    private Vector3 _targetDirection,_targetLocation;
    private Rigidbody2D _rb;
    private float _maxAccSqr, _maxSpeedSqr, _invTimeToTarget;
    private bool _seekToLocation;

    public Vector3 Velocity => _physData.velocity;

    void Awake()
    {
        // determine random initial rotation
        float rngRot = Random.Range(-40f, 40f);
        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + rngRot);

        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _targetLocation = transform.position;
        _seekToLocation = false;
        
        _physData = new PhysData
        {
            velocity = Vector3.zero,
            acceleration = new Vector3(
                Mathf.Sin(transform.rotation.z * Mathf.Deg2Rad),
                Mathf.Cos(transform.rotation.z * Mathf.Deg2Rad), 0f)
        };

        _maxSpeedSqr = maxSpeed * maxSpeed;
        _maxAccSqr = maxAcc * maxAcc;

        _invTimeToTarget = 1f / 0.2f; // .2 seconds
    }

    void FixedUpdate()
    {
        UpdatePhysics();
        Move();
        Look();
    }

    private void UpdatePhysics()
    {
        // calculate new positional acceleration
        // accelerate in the direction of the vector between the endpoints of target and current velocity 
        if (_seekToLocation)
        {
            // get vector between current and target location
            _targetDirection = _targetLocation - transform.position;
            
            // determine target speed (arrive)
            float targetSpeed;
            if (_targetDirection.sqrMagnitude < 0.04f)
                targetSpeed = 0f;
            else if (_targetDirection.sqrMagnitude < _maxSpeedSqr)
                targetSpeed = _targetDirection.magnitude;
            else
                targetSpeed = maxSpeed;
            
            // calculate acceleration
            _physData.acceleration = _targetDirection.normalized * targetSpeed - _physData.velocity;
            _physData.acceleration *= _invTimeToTarget;
            
            Debug.DrawRay(transform.position, _physData.acceleration, Color.blue);
        }
        else
            _physData.acceleration = _targetDirection - _physData.velocity;

        // cap acceleration
        if (_physData.acceleration.sqrMagnitude > _maxAccSqr)
        {
            _physData.acceleration.Normalize();
            _physData.acceleration *= maxAcc;
        }

        // apply acceleration to velocity
        _physData.velocity += (_physData.acceleration * Time.fixedDeltaTime);

        // cap velocity
        if (_physData.velocity.sqrMagnitude > _maxSpeedSqr)
        {
            _physData.velocity.Normalize();
            _physData.velocity *= maxSpeed;
            _physData.acceleration = Vector3.zero;
        }
    }

    private void Move()
    {
        //transform.position += _velocity * Time.deltaTime;
        _rb.MovePosition(transform.position + _physData.velocity * Time.fixedDeltaTime);
    }

    private void Look()
    {
        //transform.rotation = Quaternion.Euler(0, 0, rot);
        float rot = Mathf.Atan2(-_physData.velocity.x, _physData.velocity.y) * Mathf.Rad2Deg;
        _rb.MoveRotation(rot);
    }

    public void SetTargetLocation(Vector3 t)
    {
        _seekToLocation = true;
        _targetLocation = t;
    }
    
    public void SetTargetDirection(Vector3 t)
    {
        _targetDirection = t;
    }

    public void SetAcceleration(Vector3 acc)
    {
        // calculate a target direction that would make
        // the new acceleration the given acceleration
        SetTargetDirection(acc + _physData.velocity);
    }
}