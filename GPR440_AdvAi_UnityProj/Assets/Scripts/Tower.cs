using System;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public float minDist=3f, maxDist=15f;
    
    [SerializeField]
    private float influence = 7f;
    
    private int _team;
    private float _m, _b;

    private void Start()
    {
        // calculates for mx+b that produces an upside down function that zeroes at x=minDist,maxDist
        _m = (2f * influence) / (maxDist - minDist);
        _b = -(influence * (minDist + maxDist)) / (maxDist - minDist);
    }

    public float Influence => influence;
    public int Team
    {
        get => _team;
        set => _team = value;
    }

    public float M => _m;
    public float B => _b;
}
