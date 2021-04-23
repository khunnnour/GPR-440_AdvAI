using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllLines", menuName = "ScriptableObjects/GameData")]
public class GameData : ScriptableObject
{
    [Serializable]
    public struct Data
    {
        public Vector3 city1Loc;
        public Vector3 city2Loc;
        public int startFood;
        public int startOre;
        public int startWeapons;
        public int startPeople;
        public float maxRatio;
        public float percOffset;
        public float perRngRange;
        public float citySeeRange;
        public float unitSeeRange;
        public float unitMoveSpeed;
        public float babyMakeTime;
        public float hungerSpeed;
    }

    public TextAsset dialogue;

    [SerializeField] private Data _data;

    public Data AllData => _data;

    public void LoadData()
    {
        if (dialogue)
        {
            string output = dialogue.text;
            _data = JsonUtility.FromJson<Data>(output);
        }
        else
            Debug.LogWarning("No dialogue file provided");
    }

    public void SaveChanges()
    {
        if (dialogue)
        {
            string output = JsonUtility.ToJson(_data, true);
            File.WriteAllText("Assets/Resources/Data/gameData.json", output);
        }
        else
            Debug.LogWarning("No dialogue file provided");
    }
}
