using UnityEngine;

public class Node
{
    private readonly FlowField _parent;
    private readonly Vector3 _position;
    private Vector3 _flowDir = Vector3.left;
    private int _weight = 0;
    private float _distance = 500f;

    // getters/setters
    public int Weight => _weight;
    public Vector3 Position => _position;

    public Vector3 FlowDir
    {
        get => _flowDir;
        set => _flowDir = value;
    }

    public float Distance
    {
        get => _distance;
        set => _distance = value;
    }

    public Node(FlowField f, Vector3 p)
    {
        _parent = f;
        _position = p;
    }

    public void UpdateWeight()
    {
        // weight starts at number of colliders in its cube (ignore agents)
        _weight = Physics2D.OverlapBoxAll(_position, _parent.HalfDims * 2f, 0f, ~LayerMask.GetMask("Agent")).Length;
    }
}
