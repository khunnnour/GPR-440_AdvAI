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
    private float _babyTimer, _timeToMakeBaby;
    private int _numPpl;
    private int _foodAmt, _oreAmt, _weapAmt;

    // public properties
    public int FoodAmount => _foodAmt;
    public int OreAmount => _oreAmt;
    public int WeaponAmount => _weapAmt;
    
    // Start is called before the first frame update
    void Start()
    {
        // -- get starting data from GameManager -- //
        // get timer amounts
        _timeToMakeBaby = 5.0f;
        
        // get init amounts
        _foodAmt=GameManager.instance.GameDataObj.AllData.startFood;
        _oreAmt=GameManager.instance.GameDataObj.AllData.startOre;
        _weapAmt=GameManager.instance.GameDataObj.AllData.startWeapons;
        
        // -- initialize other stuff -- //
        _babyTimer = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (_foodAmt >= 3)
        {
            _babyTimer += Time.deltaTime;
            if (_babyTimer >= _timeToMakeBaby)
                MakePerson();
        }
    }

    // spawns a unit for the city
    private void MakePerson()
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

    public bool WithdrawResource(CityResource resource, int quantity)
    {
        switch (resource)
        {
            case CityResource.FOOD:
            {
                if (_foodAmt >= quantity)
                {
                    _foodAmt -= quantity;
                    return true;
                }
                else
                    return false;
            }
            case CityResource.ORE:
                if (_oreAmt >= quantity)
                {
                    _oreAmt -= quantity;
                    return true;
                }
                else
                    return false;
            case CityResource.WEAPON:
                if (_weapAmt >= quantity)
                {
                    _weapAmt -= quantity;
                    return true;
                }
                else
                    return false;
        }

        // invalid resource provided
        return false;
    }
}
