using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameData))]
public class SubtitleHolderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Load From File"))
        {
            ((GameData)target).LoadData();
        }
        if (GUILayout.Button("Save To File"))
        {
            ((GameData)target).SaveChanges();
        }
    }
}