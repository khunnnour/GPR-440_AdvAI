using UnityEngine;

public class Node
{
    private readonly Vector3 _position;
    private Vector3 _flowDir = Vector3.zero;
    private int _weight = 0;
    private FlowField _parent;
    private float _distance;
    
    private Vector3[] checkDirs = new[]
    {
        Vector3.up, Vector3.down,
        Vector3.left, Vector3.right
    };
    
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
        // weight starts at number of colliders in its cube
        _weight = Physics.OverlapBox(_position, _parent.HalfDims).Length;
        // add weight for every broken sight-line towards target
        // cycle through every direction that is in direction of 
        //foreach (Vector3 dir in checkDirs)
        //{
        //    // if a raycast hits something in direction of next node then increase the weight
        //    if (Physics2D.Raycast(_position, dir, _boxHalfDims.x * 2f))
        //        _weight++;
        //}
    }
}
