using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitManager : MonoBehaviour
{
	private GameObject _unit;

	private List<GAgent_Unemployed> unemployeds;
	private List<GAgent_Laborer> laborers;

	public int NumUnits => transform.childCount;
	public int NumUnemployed => unemployeds.Count;

	private void Start()
	{
		_unit = Resources.Load<GameObject>("Prefabs/TowerAgent");
		FindUnemployed();
	}

	public Transform[] GetNearbyUnits(Transform origin, float searchDist)
	{
		// get the searchDist^2 
		float distSqr = searchDist * searchDist;

		// create empty list for nearby units
		List<Transform> nearbyUnits = new List<Transform>();

		// cycle thru every child
		for (int i = 0; i < NumUnits; i++)
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

	public void SpawnNewUnit()
	{
		Instantiate(_unit, transform);
	}

	private void FindUnemployed()
	{
		unemployeds = new List<GAgent_Unemployed>();
		for (int i = 0; i < NumUnits; i++)
			if (transform.GetChild(i).CompareTag("Unemployed"))
				unemployeds.Add(transform.GetChild(i).GetComponent<GAgent_Unemployed>());
	}

	public GAgent_Unemployed GetUnemployed()
	{
		// update list
		FindUnemployed();

		// return first one or nothing
		return unemployeds.Count > 0 ? unemployeds[0] : null;
	}

	public void DeleteUnit(Vector3 pos)
	{
		pos.z = 0;
		// cycle thru every child
		for (int i = 0; i < NumUnits; i++)
		{
			// get current child's transform
			Transform curr = transform.GetChild(i);
			// check if click was within 0.3 units of boid 
			if ((curr.position - pos).sqrMagnitude < 0.1f)
			{
				Destroy(curr.gameObject);
			}
		}
	}
}
