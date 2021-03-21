using UnityEngine;

public class Node
{
    private readonly Grid _parent;
    private readonly Vector3 _position;
    private Vector3 _team1FlowDir = Vector3.left;
    private Vector3 _team2FlowDir = Vector3.left;
    private int _weight;
    private float _distance = 500f;
    private float _netInfluence,_totInfluence;

    // getters/setters
    public int Weight => _weight;
    public Vector3 Position => _position;

    public Vector3 Team1FlowDir
    {
        get => _team1FlowDir;
        set => _team1FlowDir = value;
    }
    public Vector3 Team2FlowDir
    {
        get => _team2FlowDir;
        set => _team2FlowDir = value;
    }

    public float Distance
    {
        get => _distance;
        set => _distance = value;
    }

    public float NetInfluence => _netInfluence;
    public float TotInfluence => _totInfluence;

    // rest
    public Node(Grid f, Vector3 p)
    {
        _parent = f;
        _position = p;
    }

    public void UpdateWeight()
    {
        // weight starts at number of terrain colliders in (most of) its cube
        Collider2D[] colls = Physics2D.OverlapBoxAll(
            _position,
            _parent.HalfDims * 1.95f,
            0f, LayerMask.GetMask("Terrain"));

        foreach (Collider2D coll in colls)
        {
            if (coll.CompareTag("Impassible"))
                _weight += 32;
            else if (coll.CompareTag("Difficult"))
                _weight += 1;
        }
    }

    public void AddInfluence(float inf)
    {
        _netInfluence += inf;
        _totInfluence += Mathf.Abs(inf);
    }
    
    public void SetInfluence(float nInf,float tInf)
    {
        _netInfluence = nInf;
        _totInfluence = tInf;
    }
}
