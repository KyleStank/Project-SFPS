using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(ExplosionSource))]
public class ExplosionSourceEditor : Editor
{
    SerializedProperty PropInfluenceRadius;
    SerializedProperty PropForce;
    SerializedProperty PropCheckStructureIntegrity;
    SerializedProperty PropMoveManually;
    SerializedProperty PropPosStart;
    SerializedProperty PropPosEnd;
    SerializedProperty PropMoveDuration;

    [MenuItem("GameObject/Create Other/Ultimate Game Tools/Explosion Source")]
    static void CreateExplosionSource() 
    {
        GameObject explosionSource = new GameObject();
        explosionSource.name = "Explosion Source";
        explosionSource.transform.position = Vector3.zero;
        explosionSource.AddComponent<ExplosionSource>();

        Selection.activeGameObject = explosionSource;
    }

    void OnEnable()
    {
        PropInfluenceRadius         = serializedObject.FindProperty("InfluenceRadius");
        PropForce                   = serializedObject.FindProperty("Force");
        PropCheckStructureIntegrity = serializedObject.FindProperty("CheckStructureIntegrity");
        PropMoveManually            = serializedObject.FindProperty("MoveManually");
        PropPosStart                = serializedObject.FindProperty("PosStart");
        PropPosEnd                  = serializedObject.FindProperty("PosEnd");
        PropMoveDuration            = serializedObject.FindProperty("MoveDuration");
    }

    public void OnSceneGUI()
    {
        ExplosionSource explosionSource = target as ExplosionSource;

        if(explosionSource == null)
        {
            return;
        }

        Color handlesColor = Handles.color;
        Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);

        Handles.DrawWireDisc(explosionSource.transform.position, explosionSource.transform.right,   explosionSource.InfluenceRadius);
        Handles.DrawWireDisc(explosionSource.transform.position, explosionSource.transform.up,      explosionSource.InfluenceRadius);
        Handles.DrawWireDisc(explosionSource.transform.position, explosionSource.transform.forward, explosionSource.InfluenceRadius);

        Handles.color = handlesColor;
    }
	
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(PropInfluenceRadius,         new GUIContent("Influence Radius",          "The radius of influence. Attached chunks inside this radius will be exploded."));
        EditorGUILayout.PropertyField(PropForce,                   new GUIContent("Force",                     "The explosion force."));
        EditorGUILayout.PropertyField(PropCheckStructureIntegrity, new GUIContent("Check Structure Integrity", "Checks for structure integrity each frame. Usually leave this unchecked unless you have support chunks."));
        EditorGUILayout.PropertyField(PropMoveManually,            new GUIContent("Move Manually",             "Will you move the source through scripting?"));
        GUI.enabled = PropMoveManually.boolValue == false;
        EditorGUILayout.PropertyField(PropPosStart,                new GUIContent("Pos Start",                 "Start position when moving automatically."));
        EditorGUILayout.PropertyField(PropPosEnd,                  new GUIContent("Pos End",                   "End position when moving automatically."));
        EditorGUILayout.PropertyField(PropMoveDuration,            new GUIContent("Move Duration",             "Time in second it will take to go from start to end, if moving automatically."));
        GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }
}
