using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable All

public enum CityResource
{
    FOOD,
    ORE,
    WEAPON
}

public class City : MonoBehaviour
{
    // private variables 
    private float _sightRange;
    private int _numPpl;
    private int _foodAmt, _oreAmt, _weapAmt;

    // public properties
    public int FoodAmount => _foodAmt;
    public int OreAmount => _oreAmt;
    public int WeaponAmount => _weapAmt;
    
    // Start is called before the first frame update
    void Start()
    {
        // get starting data from GameManager
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // increment appropriate resource by provided quantity
    public void DepositResource(CityResource resource, int quantity)
    {
        switch (resource)
        {
            case CityResource.FOOD:
                _foodAmt += quantity;
                return;
            case CityResource.ORE:
                _oreAmt += quantity;
                return;
            case CityResource.WEAPON:
                _weapAmt += quantity;
                return;
        }
    }
}
