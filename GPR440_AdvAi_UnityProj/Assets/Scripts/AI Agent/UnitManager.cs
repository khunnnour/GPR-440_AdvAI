using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitManager : MonoBehaviour
{
    public Text unitText;
    public Text collText;

    private int _numUnits;
    private int _numCollisions;

    private void Start()
    {
        // only units should be under the unit manager
        _numUnits = transform.childCount;
        unitText.text = _numUnits.ToString();
        // set collisions to 0
        _numCollisions = 0;
        collText.text = _numCollisions.ToString();
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
    
    /// <summary>
    /// Called by an agent to increase the counter
    /// </summary>
    public void ReportCollision()
    {
        _numCollisions++;
        collText.text = _numCollisions.ToString();
    }
}