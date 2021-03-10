using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    public Vector3[] points;
    public float speed;

    private Grid _grid;
    private int _pIndex;
    private Vector3 _lastPos;

    // Start is called before the first frame update
    void Start()
    {
        _pIndex = 0;
        _lastPos = transform.position;
        _grid = GameObject.FindGameObjectWithTag("FlowField").GetComponent<Grid>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveToNextPoint();
        CheckMoveDist();
    }

    private void MoveToNextPoint()
    {
        // move towards next point
        transform.position = Vector3.MoveTowards(transform.position, points[_pIndex], Time.deltaTime * speed);

        // check if reached the point; increment to next point
        if ((transform.position - points[_pIndex]).sqrMagnitude < 0.1f)
            _pIndex = (_pIndex + 1) % points.Length;
    }

    private void CheckMoveDist()
    {
        // if moved more than 0.5 units then report movement
        Vector3 position = transform.position;
        if ((position - _lastPos).sqrMagnitude > 0.25f)
        {
            _lastPos = position;
            _grid.UpdateFlowRegion(position);
        }
    }
}
