using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float acceleration = 2f;
    public float maxSpeed = 2f;

    private float _speed;
    private Vector3 _targetVelocity;
    private Vector3 _velocity;
    private Rigidbody2D _rb;

    void Awake()
    {
        // determine random initial rotation
        float rngRot = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0, 0, rngRot);

        _rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        UpdatePhysics();
        Move();
        Look();
    }

    private void UpdatePhysics()
    {
        // Debug.Log(_velocity.ToString("F2") + ", " + _targetVelocity.ToString("F2"));
        _velocity = Vector3.Lerp(_velocity, _targetVelocity, Time.fixedDeltaTime * acceleration);
        _velocity.z = 0f;

        // clamp speed
        if (_velocity.sqrMagnitude > maxSpeed * maxSpeed)
            _velocity = _velocity.normalized * maxSpeed;
    }

    private void Move()
    {
        //transform.position += _velocity * Time.deltaTime;
        _rb.MovePosition(transform.position + _velocity * Time.fixedDeltaTime);
    }

    private void Look()
    {
        float rot = Mathf.Atan2(-_velocity.x, _velocity.y) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0, 0, rot);
        _rb.MoveRotation(rot);
    }

    public void SetTargetelocity(Vector3 t)
    {
        _targetVelocity = t;
    }
}
