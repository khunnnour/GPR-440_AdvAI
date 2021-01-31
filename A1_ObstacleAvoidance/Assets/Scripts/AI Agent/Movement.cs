using UnityEngine;
using Random = UnityEngine.Random;

namespace AI_Agent
{
    struct PhysData
    {
        public Vector3 velocity;
        public Vector3 acceleration;
        public float rotVelocity;
        public float rotAcceleration;
    };

    public class Movement : MonoBehaviour
    {
        public float maxAcc = 2f;
        public float maxSpeed = 2f;
        public float maxRotAcc = 0.5f;
        public float maxRotVel = 1.0f;
        public float slowRot = 30f;
        public float stopRot = 5f;

        private PhysData _physData;
        private Vector3 _targetDirection;
        private Rigidbody2D _rb;
        private float _maxAccSqr, _maxSpeedSqr;
        private float _a, _h, _k; // for slowing rot acc

        void Awake()
        {
            // determine random initial rotation
            float rngRot = Random.Range(0f, 360f);
            transform.rotation = Quaternion.Euler(0, 0, rngRot);

            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            _physData = new PhysData
            {
                velocity = Vector3.zero,
                acceleration = Vector3.zero,
                rotVelocity = 0f,
                rotAcceleration = 0f
            };

            _maxSpeedSqr = maxSpeed * maxSpeed;
            _maxAccSqr = maxAcc * maxAcc;

            _h = (stopRot + slowRot) * 0.5f;
            _k = maxRotAcc * -0.75f;
            _a = -_k / (_h - stopRot);
            //Debug.Log(_h + ", " + _k + ", " + _a);
        }

        void FixedUpdate()
        {
            UpdatePhysics();
            Move();
            Look();
        }

        private void UpdatePhysics()
        {
            /*
        // Debug.Log(_velocity.ToString("F2") + ", " + _targetVelocity.ToString("F2"));
        _velocity = Vector3.Lerp(_velocity, _targetDirection, Time.fixedDeltaTime * maxAcc);
        _velocity.z = 0f;

        // clamp speed
        if (_velocity.sqrMagnitude > maxSpeed * maxSpeed)
            _velocity = _velocity.normalized * maxSpeed;
        */

            // calculate new positional acceleration
            // accelerate in the direction of the vector between the endpoints of target and current velocity 
            _physData.acceleration = _targetDirection - _physData.velocity;

            // calculate new rotational acceleration
            // get target rotation
            //float tRot = Mathf.Atan2(-_targetDirection.x, _targetDirection.y) * Mathf.Rad2Deg;
            // get rotation difference
            //float rotDiff = transform.rotation.eulerAngles.z;
            //// adjust degrees to (-180,180]
            //if (rotDiff > 180f)
            //    rotDiff -= 360f;
            //rotDiff = Mathf.Atan2(-_targetDirection.x, _targetDirection.y) * Mathf.Rad2Deg - rotDiff;

            // get the base forward vector
            /*float rot = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            Vector3 forward = new Vector3(Mathf.Sin(rot), Mathf.Cos(rot), 0f);
            // dot with the target direction
            float rotDiff = Vector3.Dot(forward, _targetDirection.normalized);
            // convert [-1,1] result to [-180,180)
            rotDiff = 180f * (rotDiff - 1);
            if(forward.x>)
            
            // determine the rotational acceleration
            if (Mathf.Abs(rotDiff) <= stopRot)
            {
                // stop accelerating if close enough to the target
                _physData.rotAcceleration = 0f;
            }
            else
            {
                // else: calculate; Is a linear equation meant to slow when close to the correct rotation
                _physData.rotAcceleration = _a * Mathf.Abs(rotDiff - _h) + _k;
                
                // if negative change result of equation
                if (rotDiff < 0)
                    _physData.rotAcceleration *=-1f;
            }*/

            //_physData.rotAcceleration =(tRot - _rb.rotation);
            //Debug.Log(rotDiff.ToString("F2") + " => " + _physData.rotAcceleration.ToString("F2"));

            // cap accelerations
            if (_physData.acceleration.sqrMagnitude > _maxAccSqr)
            {
                _physData.acceleration.Normalize();
                _physData.acceleration *= maxAcc;
            }

            if (_physData.rotAcceleration > maxRotAcc)
            {
                _physData.rotAcceleration = maxRotAcc;
            }
            else if (_physData.rotAcceleration < -maxRotAcc)
            {
                _physData.rotAcceleration = -maxRotAcc;
            }

            // apply accelerations to velocities
            _physData.velocity += (_physData.acceleration * Time.fixedDeltaTime);
            _physData.rotVelocity += (_physData.rotAcceleration * Time.fixedDeltaTime);
            //Debug.Log(_physData.velocity + " | " + _physData.rotVelocity);

            // cap velocities
            if (_physData.velocity.sqrMagnitude > _maxSpeedSqr)
            {
                _physData.velocity.Normalize();
                _physData.velocity *= maxSpeed;
                _physData.acceleration = Vector3.zero;
            }

            if (_physData.rotVelocity > maxRotVel)
            {
                _physData.rotVelocity = maxRotVel;
                _physData.rotAcceleration = 0.0f;
            }
            else if (_physData.rotVelocity < -maxRotVel)
            {
                _physData.rotVelocity = -maxRotVel;
                _physData.rotAcceleration = 0.0f;
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
            //_rb.MoveRotation(_rb.rotation + _physData.rotVelocity * Time.fixedDeltaTime);
            float rot = Mathf.Atan2(-_physData.velocity.x, _physData.velocity.y) * Mathf.Rad2Deg;
            _rb.MoveRotation(rot);
        }

        public void SetTargetDirection(Vector3 t)
        {
            _targetDirection = t;
        }
    }
}