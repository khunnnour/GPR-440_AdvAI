using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UnitManager : MonoBehaviour
{
    public Text unitText;
    public Text obsCollText;
    public Text boidCollText;

    private int _numUnits, _numTowers;
    private int _numObstCollisions;
    private int _numBoidCollisions;
    private GameObject _unit,_tower1,_tower2;
    private Transform _unitHolder, _towerHolder;

    private void Start()
    {
        // get the unit holder
        _unitHolder = transform.GetChild(0);
        // get the tower holder
        _towerHolder = transform.GetChild(1);
        
        // only units should be under the unit manager
        _numUnits = _unitHolder.childCount;
        _numTowers = _towerHolder.childCount;
        // set collisions to 0
        _numObstCollisions = 0;
        _numBoidCollisions = 0;
        // update the ui
        UpdateUI();
        // get the unit prefab
        _unit = Resources.Load<GameObject>("Prefabs/TowerAgent");
        _tower1 = Resources.Load<GameObject>("Prefabs/Team1Tower");
        _tower2 = Resources.Load<GameObject>("Prefabs/Team2Tower");
    }

    private void UpdateUI()
    {
        boidCollText.text = (_numBoidCollisions * 0.5f).ToString("F0");
        obsCollText.text = _numObstCollisions.ToString();
        unitText.text = _numUnits.ToString();
    }

    public Transform[] GetNearbyUnits(Transform origin, float searchDist)
    {
        // get the searchDist^2 
        float distSqr = searchDist * searchDist;

        // create empty list for nearby units
        List<Transform> nearbyUnits = new List<Transform>();

        // cycle thru every child
        for (int i = 0; i < _numUnits; i++)
        {
            // get current child's transform
            Transform curr = _unitHolder.GetChild(i);
            // make sure not looking at the origin
            if (origin != curr)
            {
                // get vector between origin and current transform 
                Vector3 diff = origin.position - curr.position;
                // check if inside the search distance
                if (diff.sqrMagnitude <= distSqr)
                    nearbyUnits.Add(curr);
            }
        }

        return nearbyUnits.ToArray();
    }
    
    // returns a list of all towers
    public Tower[] GetTowers()
    {
        return _towerHolder.GetComponentsInChildren<Tower>();
    }

    // returns last tower in list of children
    public Tower GetLastTower()
    {
        return _towerHolder.GetChild(_towerHolder.childCount - 1).GetComponent<Tower>();
    }
    
    public void SetTargetPoint(Vector3 pos)
    {
        // cycle thru every child
        for (int i = 0; i < _numUnits; i++)
        {
            // get current child's transform
            Transform curr = _unitHolder.GetChild(i);
            curr.GetComponent<FlockSteeringBehavior>().SetTargetLocation(pos);
        }
    }

    public void SetPathing(bool b)
    {
        // cycle thru every child
        for (int i = 0; i < _numUnits; i++)
        {
            // get current child's transform
            Transform curr = _unitHolder.GetChild(i);
            curr.GetComponent<FlockSteeringBehavior>().SetShouldPath(b);
        }
    }

    public void SpawnNewUnit(Vector3 pos,int team)
    {
        Quaternion rngRot = Quaternion.LookRotation(new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f)));
        pos.z = 0;
        GameObject u=Instantiate(_unit, pos, rngRot, _unitHolder);
        u.GetComponent<TowerUnitBrain>().team = team;
        _numUnits++;
    }

    public void DeleteUnit(Vector3 pos)
    {
        pos.z = 0;
        // cycle thru every child
        for (int i = 0; i < _numUnits; i++)
        {
            // get current child's transform
            Transform curr = _unitHolder.GetChild(i);
            // check if click was within 0.5 units of boid 
            if ((curr.position - pos).sqrMagnitude < 0.25f)
            {
                Destroy(curr.gameObject);
                _numUnits--;
            }
        }
    }

    public void SpawnNewTower(Vector3 pos, int team)
    {
        Tower t = Instantiate(team == 0 ? _tower1 : _tower2, pos, Quaternion.identity, _towerHolder)
            .GetComponent<Tower>();
        t.Team = team;
        _numTowers++;
    }

    /// <summary>
    /// Called by an agent to increase the counter
    /// </summary>
    public void ReportCollision(bool hitBoid)
    {
        if (hitBoid)
            _numBoidCollisions++;
        else
            _numObstCollisions++;
        UpdateUI();
    }
}