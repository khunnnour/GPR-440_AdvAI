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

    private int _numUnits;
    private int _numObstCollisions;
    private int _numBoidCollisions;
    private GameObject _unit;

    private void Start()
    {
        // only units should be under the unit manager
        _numUnits = transform.childCount;
        // set collisions to 0
        _numObstCollisions = 0;
        _numBoidCollisions = 0;
        // update the ui
        UpdateUI();
        // get the unit prefab
        _unit = Resources.Load<GameObject>("Prefabs/BoidAgent");
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
            Transform curr = transform.GetChild(i);
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

    public void SetTargetPoint(Vector3 pos)
    {
        // cycle thru every child
        for (int i = 0; i < _numUnits; i++)
        {
            // get current child's transform
            Transform curr = transform.GetChild(i);
            curr.GetComponent<FlockSteeringBehavior>().SetTargetLocation(pos);
        }
    }

    public void SpawnNewUnit(Vector3 pos)
    {
        Quaternion rngRot = Quaternion.LookRotation(new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f)));
        pos.z = 0;
        Instantiate(_unit, pos, rngRot, transform);
        _numUnits++;
    }

    public void DeleteUnit(Vector3 pos)
    {
        pos.z = 0;
        Debug.Log("Tried deleting a unit at " + pos.ToString());
        // cycle thru every child
        for (int i = 0; i < _numUnits; i++)
        {
            // get current child's transform
            Transform curr = transform.GetChild(i);
            // check if click was within 0.7 units of boid 
            if ((curr.position - pos).sqrMagnitude < 0.5f)
            {
                Destroy(curr.gameObject);
                _numUnits--;
            }
        }
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

    private void UpdateUI()
    {
        boidCollText.text = (_numBoidCollisions * 0.5f).ToString("F0");
        obsCollText.text = _numObstCollisions.ToString();
        unitText.text = _numUnits.ToString();
    }
}