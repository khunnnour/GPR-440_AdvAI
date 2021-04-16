using UnityEngine;

public class TowerUnitSteering : MonoBehaviour
{
    public enum PathMode
    {
        SEEK,
        FLOW,
        STOP
    }
    
    [Header("Avoidance Settings")] [Range(0f, 10f)]
    public float forLength = 3f;

    [Range(0f, 10f)] public float forwardAvoidCoeff = 2f;
    [Range(0f, 10f)] public float sideLength = 1f;
    [Range(0f, 10f)] public float sideAvoidCoeff = 1.5f;
    [Range(0f, 4.3f)] public float angle = 0.5f;

    [Header("Steering Values")] public float unitCheckRadius = 5f;
    public float minimumSeparation = 2f;

    [Header("Steering Weights")] [Range(0f, 75f)]
    public float separationWeight = 4f;

    [Range(0f, 75f)] public float avoidWeight = 10f;
    [Range(0f, 75f)] public float pathWeight = 3f;

    private UnitManager _unitManager;
    private Movement _moveScript;
    private Vector3 _targetLocation;
    private int _collisions;
    [SerializeField]
    private PathMode _pathMode;

    private SeparationComponent _separation;
    private TriWhiskAvoidComponent _avoid;
    private FlowComponent _flowPath;
    private SeekComponent _seekPath;

    // Start is called before the first frame update
    void Start()
    {
        _unitManager = transform.GetComponentInParent<UnitManager>();

        _moveScript = GetComponent<Movement>();
        _moveScript.SetTargetDirection(Vector3.up);

        Transform t = transform;
        _separation = new SeparationComponent(t, minimumSeparation);
        _avoid = new TriWhiskAvoidComponent(t, forLength, forwardAvoidCoeff, sideLength, sideAvoidCoeff, angle);
        _flowPath = new FlowComponent(t, GameObject.FindWithTag("FlowField").GetComponent<Grid>(),
            GetComponent<TowerUnitBrain>().team);
        _seekPath = new SeekComponent(t);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSteering();
    }

    private void UpdateSteering()
    {
        // get the nearby unit
        Transform[] nearby = _unitManager.GetNearbyUnits(transform, unitCheckRadius);

        // debug out the calculated rays
        var tran = transform;
        Vector3 position = tran.position;
        
        Vector3 sepa = _separation.GetSteering(nearby);
        Vector3 wAvd = _avoid.GetSteering(nearby);
        // get path vector based on path mode
        Vector3 path;
        switch (_pathMode)
        {
            case PathMode.FLOW:
                path = _flowPath.GetSteering();
                break;
            case PathMode.SEEK:
                path = _seekPath.GetSteering(_targetLocation);
                break;
            case PathMode.STOP: // if stopping: use the negative of current vel
            default:
                path = -_moveScript.Velocity*0.9f;
                break;
        }

        Debug.DrawRay(position, sepa, Color.blue);
        Debug.DrawRay(position, wAvd, Color.red);
        Debug.DrawRay(position, path, Color.white);

        Vector3 newAcc = sepa * separationWeight + wAvd * avoidWeight + path * pathWeight;
        newAcc = newAcc.normalized * _moveScript.maxAcc;

        // debug out the final acceleration
        Debug.DrawRay(position, newAcc, Color.magenta);

        _moveScript.SetAcceleration(newAcc);
    }

    public void SetPathMode(PathMode pM)
    {
        _pathMode = pM;
    }

    public void SetTargetLocation(Vector2 tL)
    {
        _targetLocation = tL;
    }

    public void SetShouldPath(bool b)
    {
        // if trying to stop pathing then set mode to stop
        if (!b)
            _pathMode = PathMode.STOP;
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        _unitManager.ReportCollision(other.collider.CompareTag("Boid"));
    }
}
