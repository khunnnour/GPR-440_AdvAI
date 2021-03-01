using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowField : MonoBehaviour
{
    [Tooltip("The number of nodes on each side of the map")]
    public Vector3Int dimensions;

    [Tooltip("Space between individual nodes")]
    public float spacing = 2f;

    private Vector3 _target;
    private Vector3 _centerOffset,_halfDims;
    private Node[] _map;
    private int _numNodes;
    private float _mapToWorld;

    public Vector3 HalfDims => _halfDims;
    
    // Start is called before the first frame update
    void Start()
    {
        // calculate inverse of spacing to convert world coord to map coord
        _mapToWorld = 1f / spacing;

        // calculate offset to center the map
        _centerOffset = (Vector3) (dimensions) * (0.5f * spacing);

        // calculate the half dims
        _halfDims = Vector3.one * spacing * 0.5f;

        // init the map
        _numNodes = dimensions.x * dimensions.y * dimensions.z;
        _map = new Node[_numNodes];
        // populate the map
        for (int i = 0; i < _numNodes; i++)
        {
            _map[i] = new Node(this, (Vector3) (IndexToMap(i)) * spacing - _centerOffset);
            _map[i].UpdateWeight();
        }
    }

    private void CalculateFlowField()
    {
        Node curr = _map[MapToIndex(_target)];
        curr.Distance = 0f;
        
    }

    public void SetTarget(Vector3 t)
    {
        // -- turn into a map coord --
        // un-offset it
        t += _centerOffset;
        // account for spacing
        t *= _mapToWorld;

        // floor values to get map coord
        t.x = Mathf.Floor(t.x);
        t.y = Mathf.Floor(t.y);
        t.z = Mathf.Floor(t.z);
        
        // check if new map coord; if a new target map coord then calculate a new flow field
        if (_target != t)
        {
            _target = t;
            CalculateFlowField();
        }
    }

    // helpers --
    int MapToIndex(Vector3 coord)
    {
        return (int)(coord.x + coord.z * dimensions.x + coord.y * dimensions.x * dimensions.z);
    }

    Vector3 IndexToMap(int index)
    {
        Vector3 coord = new Vector3();

        coord.y = Mathf.FloorToInt(index / (dimensions.x * dimensions.z));
        index = index % (dimensions.x * dimensions.z);
        coord.z = Mathf.FloorToInt(index / dimensions.x);
        coord.x = index % dimensions.x;

        return coord;
    }
}
