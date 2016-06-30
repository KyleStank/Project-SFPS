using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(FracturedChunk)), CanEditMultipleObjects]
public class FracturedChunkEditor : Editor
{
    SerializedProperty PropDontDeleteAfterBroken;
    SerializedProperty PropIsSupportChunk;

    FracturedChunk SelectedManuallyAddChunk;

    void OnEnable()
    {
        PropDontDeleteAfterBroken = serializedObject.FindProperty("DontDeleteAfterBroken");
        PropIsSupportChunk        = serializedObject.FindProperty("IsSupportChunk");
    }

    public void OnSceneGUI()
    {
        FracturedChunk fracturedChunk = target as FracturedChunk;

        if(fracturedChunk == null)
        {
            return;
        }

        FracturedObject fracturedObject = fracturedChunk.FracturedObjectSource;

        // Chunk connections

        bool bDrawLines = true;

        if(fracturedObject != null)
        {
            bDrawLines = fracturedObject.ShowChunkConnectionLines;
        }

        if(bDrawLines)
        {
            Color handlesColor = Handles.color;
            Handles.color = new Color32(155, 89, 182, 255);

            if(fracturedChunk.ListAdjacentChunks.Count > 0)
            {
                Handles.DotCap(0, fracturedChunk.transform.position, Quaternion.identity, HandleUtility.GetHandleSize(fracturedChunk.transform.position) * 0.03f);
            }

            foreach(FracturedChunk.AdjacencyInfo chunkAdjacency in fracturedChunk.ListAdjacentChunks)
            {
                if(chunkAdjacency.chunk)
                {
                    Handles.DotCap(0, chunkAdjacency.chunk.transform.position, Quaternion.identity, HandleUtility.GetHandleSize(chunkAdjacency.chunk.transform.position) * 0.03f);
                    Handles.DrawLine(fracturedChunk.transform.position, chunkAdjacency.chunk.transform.position);
                }
            }

            Handles.color = handlesColor;
        }
    }
	
    public override void OnInspectorGUI()
    {
        bool bCheckDetachNonSupportedChunks = false;
        bool bMarkNonSupportedChunks        = false;

        Color colorGUI = GUI.color;

        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.

        serializedObject.Update();

        FracturedChunk fracturedChunk = serializedObject.targetObject as FracturedChunk;

        PropDontDeleteAfterBroken.boolValue = EditorGUILayout.Toggle(new GUIContent("Don't Delete If Broken", "Will prevent this chunk to be deleted (due to lifetime parameters configured on the FracturedObject panel) after being broken off from the object."), PropDontDeleteAfterBroken.boolValue);
        EditorGUI.BeginChangeCheck();
        PropIsSupportChunk.boolValue = EditorGUILayout.Toggle(new GUIContent("Is Support Chunk", "Chunks that are tagged as support can't be destroyed and will hold the object together. A chunk needs to be connected to a support chunk (directly or through other chunks) or otherwise it will fall. This prevents chunks from staying static in the air and enables realistic collapsing behavior."), PropIsSupportChunk.boolValue);
        if(EditorGUI.EndChangeCheck())
        {
            if(fracturedChunk.FracturedObjectSource)
            {
                if(Application.isPlaying)
                {
                    bCheckDetachNonSupportedChunks = true;
                }

                bMarkNonSupportedChunks = true;
            }
        }

        // Apply changes to the serializedProperty.

        serializedObject.ApplyModifiedProperties();

        if(bCheckDetachNonSupportedChunks)
        {
            fracturedChunk.FracturedObjectSource.CheckDetachNonSupportedChunks();
        }

        if(bMarkNonSupportedChunks)
        {
            fracturedChunk.FracturedObjectSource.MarkNonSupportedChunks();
        }

        if(bCheckDetachNonSupportedChunks || bMarkNonSupportedChunks)
        {
            SceneView.RepaintAll();
        }

        bCheckDetachNonSupportedChunks = false;
        bMarkNonSupportedChunks        = false;

        EditorGUILayout.Space();

        if(serializedObject.targetObjects.Length == 1)
        {
            EditorGUILayout.LabelField(new GUIContent("Chunk Attached To Object: " + (fracturedChunk.IsDetachedChunk ? "No" : "Yes"), "Is the chunk attached to the object or has it been detached?"));
            EditorGUILayout.LabelField(new GUIContent("Chunk Connected To Support: " + (fracturedChunk.IsNonSupportedChunk ? "No" : "Yes"), "Is the chunk directly or indirectly connected to a support chunk?"));
            EditorGUILayout.LabelField(new GUIContent("Chunk Volume: " + string.Format("{0} ({1:0.##}% of total)", fracturedChunk.Volume, fracturedChunk.RelativeVolume * 100.0f), "The chunk volume and % of volume with respect to the object"));
            EditorGUILayout.LabelField(new GUIContent("Concave Collider Generated: " + (fracturedChunk.HasConcaveCollider ? "Yes" : "No"), "Does this chunk have one or more colliders generated by the Concave Collider utility?"));
        }
        else
        {
            GUI.color = new Color(0.5f, 1.0f, 0.0f, 1.0f);
            EditorGUILayout.LabelField("Multiselection mode");
            GUI.color = colorGUI;
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        Vector4 v4GUIColor = GUI.color;

        EditorGUILayout.BeginHorizontal();
        SelectedManuallyAddChunk = EditorGUILayout.ObjectField(new GUIContent("Manually Add Connection"), SelectedManuallyAddChunk, typeof(FracturedChunk), true) as FracturedChunk;
        if(GUILayout.Button(new GUIContent("Connect", "Connects this chunk to the selected one."), GUILayout.Width(80)))
        {
            if(SelectedManuallyAddChunk)
            {
                if(serializedObject.targetObjects.Length == 1)
                {
                    if(fracturedChunk.FracturedObjectSource != SelectedManuallyAddChunk.FracturedObjectSource)
                    {
                        EditorUtility.DisplayDialog("Chunks not of the same object", "The two chunks belong to different fractured objects.", "OK");
                    }
                    else if(fracturedChunk.IsConnectedTo(SelectedManuallyAddChunk))
                    {
                        EditorUtility.DisplayDialog("Chunks already connected", "The two chunks are already connected.", "OK");
                    }
                    else
                    {
                        fracturedChunk.ConnectTo(SelectedManuallyAddChunk, Mathf.Min(Mathf.Sqrt(fracturedChunk.Volume), Mathf.Sqrt(SelectedManuallyAddChunk.Volume)));
                        bCheckDetachNonSupportedChunks = true;
                        bMarkNonSupportedChunks        = true;
                    }
                }
                else
                {
                    // Multi-edit

                    foreach(Object objectMultiselection in serializedObject.targetObjects)
                    {
                        FracturedChunk chunkMultiSelected = objectMultiselection as FracturedChunk;

                        chunkMultiSelected.ConnectTo(SelectedManuallyAddChunk, Mathf.Min(Mathf.Sqrt(chunkMultiSelected.Volume), Mathf.Sqrt(SelectedManuallyAddChunk.Volume)));
                        bCheckDetachNonSupportedChunks = true;
                        bMarkNonSupportedChunks        = true;
                    }
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if(serializedObject.targetObjects.Length == 1)
        {
            GUILayout.Label("Connections to other chunks:");
        }
        else
        {
            GUILayout.Label("Common connections to other chunks:");
        }

        FracturedChunk chunkToDisconnectFrom = null;

        // Compute common connections in multi-edit mode

        List<FracturedChunk> listConnectedChunks = new List<FracturedChunk>();
        List<float>          listAreas           = new List<float>();

        for(int nConnectedChunk = 0; nConnectedChunk < fracturedChunk.ListAdjacentChunks.Count; nConnectedChunk++)
        {
            FracturedChunk.AdjacencyInfo info = fracturedChunk.ListAdjacentChunks[nConnectedChunk];

            if(info.chunk)
            {
                listConnectedChunks.Add(info.chunk);
                listAreas.Add(info.fArea);
            }
        }

        if(serializedObject.targetObjects.Length > 1)
        {
            // Multi-edit, only finish with those that are shared among all selected chunks

            foreach(Object objectMultiselection in serializedObject.targetObjects)
            {
                FracturedChunk chunkMultiSelected = objectMultiselection as FracturedChunk;

                List<FracturedChunk> listCommonChunks = new List<FracturedChunk>();

                foreach(FracturedChunk.AdjacencyInfo info in chunkMultiSelected.ListAdjacentChunks)
                {
                    if(listConnectedChunks.Contains(info.chunk))
                    {
                        listCommonChunks.Add(info.chunk);
                    }
                }

                listConnectedChunks = listCommonChunks;
            }
        }


        for(int nConnectedChunk = 0; nConnectedChunk < listConnectedChunks.Count; nConnectedChunk++)
        {
            FracturedChunk chunk = listConnectedChunks[nConnectedChunk];
            float          fArea = listAreas[nConnectedChunk];

            if(chunk)
            {
                EditorGUILayout.BeginHorizontal();
                if(chunk.IsSupportChunk)
                {
                    GUI.color = new Color(0.6f, 0.6f, 1.0f, 1.0f);
                }
                GUILayout.Label(string.Format("{0} (Surface {1})", chunk.name, serializedObject.targetObjects.Length == 1 ? fArea.ToString("F3") : ""));
                GUI.color = v4GUIColor;
                GUILayout.FlexibleSpace();
                if(GUILayout.Button(new GUIContent("Disconnect", "Disconnects the currently selected chunk from this one"), GUILayout.Width(80)))
                {
                    if(fracturedChunk.FracturedObjectSource != null)
                    {
                        chunkToDisconnectFrom = chunk;
                    }
                }
                if(GUILayout.Button(new GUIContent("Select", "Changes the active selection to this chunk"), GUILayout.Width(80)))
                {
                    if(fracturedChunk.FracturedObjectSource != null)
                    {
                        UnityEditor.Selection.activeGameObject = chunk.gameObject;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        if(chunkToDisconnectFrom != null)
        {
            foreach(Object objectMultiselection in serializedObject.targetObjects)
            {
                FracturedChunk chunkMultiSelected = objectMultiselection as FracturedChunk;
                chunkMultiSelected.DisconnectFrom(chunkToDisconnectFrom);
            }

            bCheckDetachNonSupportedChunks = true;
            bMarkNonSupportedChunks        = true;
        }

        if(bCheckDetachNonSupportedChunks)
        {
            fracturedChunk.FracturedObjectSource.CheckDetachNonSupportedChunks();
        }

        if(bMarkNonSupportedChunks)
        {
            fracturedChunk.FracturedObjectSource.MarkNonSupportedChunks();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        int nButtonWidth = 200;

        GUI.enabled = serializedObject.targetObjects.Length == 1;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(" ");
        if(GUILayout.Button(new GUIContent("Go To Fractured Object", "Selects the FracturedObject this chunk comes from"), GUILayout.Width(nButtonWidth)))
        {
            if(fracturedChunk.FracturedObjectSource != null)
            {
                UnityEditor.Selection.activeGameObject = fracturedChunk.FracturedObjectSource.gameObject;
            }
        }
        GUILayout.Label(" ");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(" ");
        if(GUILayout.Button(new GUIContent("Show Non-Connected Chunks", "Enables the MeshRenderer of all the chunks that are not directly connected to this chunk."), GUILayout.Width(nButtonWidth)))
        {
            if(fracturedChunk.FracturedObjectSource != null)
            {
                foreach(FracturedChunk chunk in fracturedChunk.FracturedObjectSource.ListFracturedChunks)
                {
                    if(chunk)
                    {
                        if(fracturedChunk.IsConnectedTo(chunk) == false)
                        {
                            chunk.GetComponent<Renderer>().enabled = true;
                        }
                    }
                }
            }
        }
        GUILayout.Label(" ");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(" ");
        if(GUILayout.Button(new GUIContent("Hide Non-Connected Chunks", "Disables the MeshRenderer of all the chunks that are not directly connected to this chunk."), GUILayout.Width(nButtonWidth)))
        {
            if(fracturedChunk.FracturedObjectSource != null)
            {
                foreach(FracturedChunk chunk in fracturedChunk.FracturedObjectSource.ListFracturedChunks)
                {
                    if(chunk)
                    {
                        if(fracturedChunk.IsConnectedTo(chunk) == false && fracturedChunk != chunk)
                        {
                            chunk.GetComponent<Renderer>().enabled = false;
                        }
                    }
                }
            }
        }
        GUILayout.Label(" ");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(" ");
        if(GUILayout.Button(new GUIContent("Show All Other Chunks", "Enables the MeshRenderer of all the other chunks in the object."), GUILayout.Width(nButtonWidth)))
        {
            if(fracturedChunk.FracturedObjectSource != null)
            {
                foreach(FracturedChunk chunk in fracturedChunk.FracturedObjectSource.ListFracturedChunks)
                {
                    if(chunk && fracturedChunk != chunk)
                    {
                        chunk.GetComponent<Renderer>().enabled = true;
                    }
                }
            }
        }
        GUILayout.Label(" ");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(" ");
        if(GUILayout.Button(new GUIContent("Hide All Other Chunks", "Disables the MeshRenderer of all the other chunks in the object."), GUILayout.Width(nButtonWidth)))
        {
            if(fracturedChunk.FracturedObjectSource != null)
            {
                foreach(FracturedChunk chunk in fracturedChunk.FracturedObjectSource.ListFracturedChunks)
                {
                    if(chunk && fracturedChunk != chunk)
                    {
                        chunk.GetComponent<Renderer>().enabled = false;
                    }
                }
            }
        }
        GUILayout.Label(" ");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUI.enabled = true;

        if(bCheckDetachNonSupportedChunks || bMarkNonSupportedChunks)
        {
            SceneView.RepaintAll();
        }
    }
}
