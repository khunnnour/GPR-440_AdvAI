using UnityEngine;

// simple steering with only obstacle avoidance and seeking
public class GoapSteering : MonoBehaviour
{
    [Header("Avoidance Settings")] 
    
    [Range(0f, 10f)] public float forLength = 3f;
    [Range(0f, 10f)] public float forwardAvoidCoeff = 2f;
    [Range(0f, 10f)] public float sideLength = 1f;
    [Range(0f, 10f)] public float sideAvoidCoeff = 1.5f;
    [Range(0f, 4.3f)] public float angle = 0.5f;

    [Range(0f, 75f)] public float avoidWeight = 10f;
    [Range(0f, 75f)] public float seekWeight = 3f;

    private Movement _moveScript;
    private Vector3 _targetLocation;

    private TriWhiskAvoidComponent _avoid;
    private SeekComponent _seek;

    // Start is called before the first frame update
    void Start()
    {
        _moveScript = GetComponent<Movement>();
        _moveScript.SetTargetDirection(Vector3.up);

        Transform t = transform;
        _avoid = new TriWhiskAvoidComponent(t, forLength, forwardAvoidCoeff, sideLength, sideAvoidCoeff, angle);
        _seek = new SeekComponent(t);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSteering();
    }

    private void UpdateSteering()
    {
        var tran = transform;
        Vector3 position = tran.position;

        Vector3 wAvd = _avoid.GetSteering();
        Vector3 seek = _seek.GetSteering(_targetLocation);

        Debug.DrawRay(position, wAvd, Color.cyan);
        Debug.DrawRay(position, seek, Color.white);

        Vector3 newAcc = wAvd * avoidWeight + seek * seekWeight;
        newAcc = newAcc.normalized * _moveScript.maxAcc;

        // debug out the final acceleration
        Debug.DrawRay(position, newAcc, Color.magenta);

        _moveScript.SetAcceleration(newAcc);
    }

    public void SetTargetLocation(Vector2 tL)
    {
        _targetLocation = tL;
    }
}