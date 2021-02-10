using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockSteeringBehavior : MonoBehaviour
{
    public float separationWeight=0.2f;
    public float cohesionWeight=0.1f;
    public float forwardWeight=0.3f;
    public float avoidWeight=0.4f;

    private Vector2 _targetLocation;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
