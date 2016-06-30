using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(CombinedMesh))]
public class CombinedMeshEditor : Editor
{
    SerializedProperty PropSaveMeshAsset;
    SerializedProperty PropKeepPosition;
    SerializedProperty PropPivotMode;
    SerializedProperty PropMeshObjects;    
    SerializedProperty PropRootNode;

    [MenuItem("GameObject/Create Other/Ultimate Game Tools/Combined Mesh")]
    static void CreateFracturedObject() 
    {
        GameObject combinedMeshObject = new GameObject();
        combinedMeshObject.name = "Combined Mesh";
        combinedMeshObject.transform.position = Vector3.zero;
        combinedMeshObject.AddComponent<CombinedMesh>();

        Selection.activeGameObject = combinedMeshObject;
    }

    void Progress(string strMessage, float fT)
    {
        CombinedMesh combinedMesh = serializedObject.targetObject as CombinedMesh;

        Repaint();

        if(EditorUtility.DisplayCancelableProgressBar("Combining", strMessage, fT))
        {
            combinedMesh.CancelCombining();
        }
    }

    void OnEnable()
    {
        PropSaveMeshAsset = serializedObject.FindProperty("SaveMeshAsset");
        PropKeepPosition  = serializedObject.FindProperty("KeepPosition");
        PropPivotMode     = serializedObject.FindProperty("PivotMode");
        PropMeshObjects   = serializedObject.FindProperty("MeshObjects");        
        PropRootNode      = serializedObject.FindProperty("RootNode");
    }

    public override void OnInspectorGUI()
    {
        int nButtonWidth = 200;

        serializedObject.Update();

        CombinedMesh combinedMesh = serializedObject.targetObject as CombinedMesh;

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        int nNumObjects = combinedMesh.MeshObjects != null ? combinedMesh.MeshObjects.Length : 0;

        EditorGUILayout.PropertyField(PropSaveMeshAsset, new GUIContent("Enable Prefab Usage", "If activated, will save the mesh to an asset file on disc. Use this if you want to use the generated mesh in prefabs, otherwise prefabs won't reference the mesh correctly."));
        EditorGUILayout.PropertyField(PropKeepPosition,  new GUIContent("Keep Position", "If keep position is activated, the gameobject will keep its current position. Otherwise it will reposition itself to match the objects being combined."));
        EditorGUILayout.PropertyField(PropPivotMode,     new GUIContent("Place Pivot Mode", "Where to place the pivot."));
        EditorGUILayout.PropertyField(PropMeshObjects,   new GUIContent("Source Mesh Objects List (" + nNumObjects + " elements)", "The list of objects whose meshes to combine."), true);
        EditorGUILayout.PropertyField(PropRootNode,      new GUIContent("Root node", "Specify an object to set it and its whole hierarchy to the list of objects to combine."));

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(" ");

        serializedObject.ApplyModifiedProperties();

        if(GUILayout.Button(new GUIContent("Build List From Root Node", "Will build the Source Mesh Objects List using the given object and all the hierarchy below it."), GUILayout.Width(nButtonWidth)))
        {
            if(PropRootNode.objectReferenceValue)
            {
                List<MeshFilter> listMeshFilters = new List<MeshFilter>();
                BuildMeshFilterListRecursive(PropRootNode.objectReferenceValue as GameObject, listMeshFilters);
                combinedMesh.MeshObjects = listMeshFilters.ToArray();
            }
        }

        GUILayout.Label(" ");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(" ");

        if(GUILayout.Button(new GUIContent("Combine", "Starts the combine process."), GUILayout.Width(nButtonWidth)))
        {
            try
            {
                combinedMesh.Combine(Progress);
            }
            catch(System.Exception e)
            {
                Debug.LogError("Exception Type: " + e.GetType().ToString() + ". Message: " + e.Message.ToString() + ". Stack Trace: " + e.StackTrace.ToString());
            }

            EditorUtility.ClearProgressBar();
        }

        GUILayout.Label(" ");
        EditorGUILayout.EndHorizontal();
    }

    void BuildMeshFilterListRecursive(GameObject node, List<MeshFilter> listMeshFilters)
    {
        MeshFilter meshFilter = node.GetComponent<MeshFilter>();

        if(meshFilter && node.GetComponent<Renderer>())
        {
            listMeshFilters.Add(meshFilter);
        }

        for(int nChild = 0; nChild < node.transform.childCount; nChild++)
        {
            BuildMeshFilterListRecursive(node.transform.GetChild(nChild).gameObject, listMeshFilters);
        }
    }
}
