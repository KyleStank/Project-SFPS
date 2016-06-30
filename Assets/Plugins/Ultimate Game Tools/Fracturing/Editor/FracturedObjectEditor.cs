using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(FracturedObject))]
public class FracturedObjectEditor : Editor
{
    SerializedProperty PropSourceObject;
    SerializedProperty PropGenerateIslands;
    SerializedProperty PropGenerateChunkConnectionInfo;
    SerializedProperty PropStartStatic;
    SerializedProperty PropChunkConnectionMinArea;
    SerializedProperty PropChunkConnectionStrength;
    SerializedProperty PropChunkHorizontalRadiusSupportStrength;
    SerializedProperty PropSupportChunksAreIndestructible;
    SerializedProperty PropChunkIslandConnectionMaxDistance;
    SerializedProperty PropTotalMass;
    SerializedProperty PropChunkPhysicMaterial;
    SerializedProperty PropMinColliderVolumeForBox;
    SerializedProperty PropCapPrecisionFix;
    SerializedProperty PropInvertCapNormals;

    SerializedProperty PropFracturePattern;
    SerializedProperty PropVoronoiVolumeOptimization;
    SerializedProperty PropVoronoiProximityOptimization;
    SerializedProperty PropVoronoiMultithreading;
    SerializedProperty PropVoronoiCellsXCount;
    SerializedProperty PropVoronoiCellsYCount;
    SerializedProperty PropVoronoiCellsZCount;
    SerializedProperty PropVoronoiCellsXSizeVariation;
    SerializedProperty PropVoronoiCellsYSizeVariation;
    SerializedProperty PropVoronoiCellsZSizeVariation;
    SerializedProperty PropNumChunks;
    SerializedProperty PropSplitsWorldSpace;
    SerializedProperty PropSplitRegularly;
    SerializedProperty PropSplitXProbability;
    SerializedProperty PropSplitYProbability;
    SerializedProperty PropSplitZProbability;   
    SerializedProperty PropSplitSizeVariation;
    SerializedProperty PropSplitXVariation;
    SerializedProperty PropSplitYVariation;
    SerializedProperty PropSplitZVariation;   
    SerializedProperty PropSplitMaterial;
    SerializedProperty PropSplitMappingTileU;
    SerializedProperty PropSplitMappingTileV;

    SerializedProperty PropEventDetachMinMass;
    SerializedProperty PropEventDetachMinVelocity;
    SerializedProperty PropEventDetachExitForce;
    SerializedProperty PropEventDetachUpwardsModifier;
    SerializedProperty PropEventDetachSound;
    SerializedProperty PropEventDetachPrefabsArray;
    SerializedProperty PropEventDetachCollisionCallMethod;
    SerializedProperty PropEventDetachCollisionCallGameObject;
    SerializedProperty PropEventDetachedMinLifeTime;
    SerializedProperty PropEventDetachedMaxLifeTime;
    SerializedProperty PropEventDetachedOffscreenLifeTime;
    SerializedProperty PropEventDetachedMinMass;
    SerializedProperty PropEventDetachedMinVelocity;
    SerializedProperty PropEventDetachedMaxSounds;
    SerializedProperty PropEventDetachedSoundArray;
    SerializedProperty PropEventDetachedMaxPrefabs;
    SerializedProperty PropEventDetachedPrefabsArray;
    SerializedProperty PropEventDetachedCollisionCallMethod;
    SerializedProperty PropEventDetachedCollisionCallGameObject;
    SerializedProperty PropEventExplosionSound;
    SerializedProperty PropEventExplosionPrefabInstanceCount;
    SerializedProperty PropEventExplosionPrefabsArray;
    SerializedProperty PropEventImpactSound;
    SerializedProperty PropEventImpactPrefabsArray;
    SerializedProperty PropEventDetachedAnyCallMethod;
    SerializedProperty PropEventDetachedAnyCallGameObject;

    SerializedProperty PropRandomSeed;
    SerializedProperty PropDecomposePreview;
    SerializedProperty PropAlwaysComputeColliders;
    SerializedProperty PropShowChunkConnectionLines;
    SerializedProperty PropShowChunkColoredState;
    SerializedProperty PropShowChunkColoredRandomly;
    SerializedProperty PropSaveMeshDataToAsset;
    SerializedProperty PropMeshAssetDataFile;
    SerializedProperty PropVerbose;

    SerializedProperty PropIntegrateWithConcaveCollider;
    SerializedProperty PropConcaveColliderAlgorithm;
    SerializedProperty PropConcaveColliderMaxHulls;
    SerializedProperty PropConcaveColliderMaxHullVertices;
    SerializedProperty PropConcaveColliderLegacySteps;

    bool               m_bProgressCancelled;

    [MenuItem("GameObject/Create Other/Ultimate Game Tools/Fractured Object")]
    static void CreateFracturedObject() 
    {
        GameObject fracturedObject = new GameObject();
        fracturedObject.name = "Fractured Object";
        fracturedObject.transform.position = Vector3.zero;
        fracturedObject.AddComponent<FracturedObject>();

        Selection.activeGameObject = fracturedObject;
    }

    void Progress(string strTitle, string strMessage, float fT)
    {
        if(EditorUtility.DisplayCancelableProgressBar(strTitle, strMessage, fT))
        {
            UltimateFracturing.Fracturer.CancelFracturing();
            m_bProgressCancelled = true;
        }
    }

    void OnEnable()
    {
        PropSourceObject                            = serializedObject.FindProperty("SourceObject");
        PropGenerateIslands                         = serializedObject.FindProperty("GenerateIslands");
        PropGenerateChunkConnectionInfo             = serializedObject.FindProperty("GenerateChunkConnectionInfo");
        PropStartStatic                             = serializedObject.FindProperty("StartStatic");
        PropChunkConnectionMinArea                  = serializedObject.FindProperty("ChunkConnectionMinArea");
        PropChunkConnectionStrength                 = serializedObject.FindProperty("ChunkConnectionStrength");
        PropChunkHorizontalRadiusSupportStrength    = serializedObject.FindProperty("ChunkHorizontalRadiusSupportStrength");
        PropSupportChunksAreIndestructible          = serializedObject.FindProperty("SupportChunksAreIndestructible");
        PropChunkIslandConnectionMaxDistance        = serializedObject.FindProperty("ChunkIslandConnectionMaxDistance");
        PropTotalMass                               = serializedObject.FindProperty("TotalMass");
        PropChunkPhysicMaterial                     = serializedObject.FindProperty("ChunkPhysicMaterial");
        PropMinColliderVolumeForBox                 = serializedObject.FindProperty("MinColliderVolumeForBox");
        PropCapPrecisionFix                         = serializedObject.FindProperty("CapPrecisionFix");
        PropInvertCapNormals                        = serializedObject.FindProperty("InvertCapNormals");

        PropFracturePattern                         = serializedObject.FindProperty("FracturePattern");
        PropVoronoiVolumeOptimization               = serializedObject.FindProperty("VoronoiVolumeOptimization");
        PropVoronoiProximityOptimization            = serializedObject.FindProperty("VoronoiProximityOptimization");
        PropVoronoiMultithreading                   = serializedObject.FindProperty("VoronoiMultithreading");
        PropVoronoiCellsXCount                      = serializedObject.FindProperty("VoronoiCellsXCount");
        PropVoronoiCellsYCount                      = serializedObject.FindProperty("VoronoiCellsYCount");
        PropVoronoiCellsZCount                      = serializedObject.FindProperty("VoronoiCellsZCount");
        PropVoronoiCellsXSizeVariation              = serializedObject.FindProperty("VoronoiCellsXSizeVariation");
        PropVoronoiCellsYSizeVariation              = serializedObject.FindProperty("VoronoiCellsYSizeVariation");
        PropVoronoiCellsZSizeVariation              = serializedObject.FindProperty("VoronoiCellsZSizeVariation");
        PropNumChunks                               = serializedObject.FindProperty("GenerateNumChunks");
        PropSplitsWorldSpace                        = serializedObject.FindProperty("SplitsWorldSpace");
        PropSplitRegularly                          = serializedObject.FindProperty("SplitRegularly");
        PropSplitXProbability                       = serializedObject.FindProperty("SplitXProbability");
        PropSplitYProbability                       = serializedObject.FindProperty("SplitYProbability");
        PropSplitZProbability                       = serializedObject.FindProperty("SplitZProbability");
        PropSplitSizeVariation                      = serializedObject.FindProperty("SplitSizeVariation");
        PropSplitXVariation                         = serializedObject.FindProperty("SplitXVariation");
        PropSplitYVariation                         = serializedObject.FindProperty("SplitYVariation");       
        PropSplitZVariation                         = serializedObject.FindProperty("SplitZVariation");
        PropSplitMaterial                           = serializedObject.FindProperty("SplitMaterial");
        PropSplitMappingTileU                       = serializedObject.FindProperty("SplitMappingTileU");
        PropSplitMappingTileV                       = serializedObject.FindProperty("SplitMappingTileV");

        PropEventDetachMinMass                      = serializedObject.FindProperty("EventDetachMinMass");
        PropEventDetachMinVelocity                  = serializedObject.FindProperty("EventDetachMinVelocity");
        PropEventDetachExitForce                    = serializedObject.FindProperty("EventDetachExitForce");
        PropEventDetachUpwardsModifier              = serializedObject.FindProperty("EventDetachUpwardsModifier");
        PropEventDetachSound                        = serializedObject.FindProperty("EventDetachSound");
        PropEventDetachPrefabsArray                 = serializedObject.FindProperty("EventDetachPrefabsArray");
        PropEventDetachCollisionCallMethod          = serializedObject.FindProperty("EventDetachCollisionCallMethod");
        PropEventDetachCollisionCallGameObject      = serializedObject.FindProperty("EventDetachCollisionCallGameObject");
        PropEventDetachedMinLifeTime                = serializedObject.FindProperty("EventDetachedMinLifeTime");
        PropEventDetachedMaxLifeTime                = serializedObject.FindProperty("EventDetachedMaxLifeTime");
        PropEventDetachedOffscreenLifeTime          = serializedObject.FindProperty("EventDetachedOffscreenLifeTime");
        PropEventDetachedMinMass                    = serializedObject.FindProperty("EventDetachedMinMass");
        PropEventDetachedMinVelocity                = serializedObject.FindProperty("EventDetachedMinVelocity");
        PropEventDetachedMaxSounds                  = serializedObject.FindProperty("EventDetachedMaxSounds");
        PropEventDetachedSoundArray                 = serializedObject.FindProperty("EventDetachedSoundArray");
        PropEventDetachedMaxPrefabs                 = serializedObject.FindProperty("EventDetachedMaxPrefabs");
        PropEventDetachedPrefabsArray               = serializedObject.FindProperty("EventDetachedPrefabsArray");
        PropEventDetachedCollisionCallMethod        = serializedObject.FindProperty("EventDetachedCollisionCallMethod");
        PropEventDetachedCollisionCallGameObject    = serializedObject.FindProperty("EventDetachedCollisionCallGameObject");
        PropEventExplosionSound                     = serializedObject.FindProperty("EventExplosionSound");
        PropEventExplosionPrefabInstanceCount       = serializedObject.FindProperty("EventExplosionPrefabsInstanceCount");
        PropEventExplosionPrefabsArray              = serializedObject.FindProperty("EventExplosionPrefabsArray");
        PropEventImpactSound                        = serializedObject.FindProperty("EventImpactSound");
        PropEventImpactPrefabsArray                 = serializedObject.FindProperty("EventImpactPrefabsArray");
        PropEventDetachedAnyCallMethod              = serializedObject.FindProperty("EventDetachedAnyCallMethod");
        PropEventDetachedAnyCallGameObject          = serializedObject.FindProperty("EventDetachedAnyCallGameObject");

        PropRandomSeed                              = serializedObject.FindProperty("RandomSeed");
        PropDecomposePreview                        = serializedObject.FindProperty("DecomposePreview");
        PropShowChunkConnectionLines                = serializedObject.FindProperty("ShowChunkConnectionLines");
        PropShowChunkColoredState                   = serializedObject.FindProperty("ShowChunkColoredState");
        PropShowChunkColoredRandomly                = serializedObject.FindProperty("ShowChunkColoredRandomly");
        PropSaveMeshDataToAsset                     = serializedObject.FindProperty("SaveMeshDataToAsset");
        PropMeshAssetDataFile                       = serializedObject.FindProperty("MeshAssetDataFile");
        PropAlwaysComputeColliders                  = serializedObject.FindProperty("AlwaysComputeColliders");
        PropVerbose                                 = serializedObject.FindProperty("Verbose");

        PropIntegrateWithConcaveCollider            = serializedObject.FindProperty("IntegrateWithConcaveCollider");
        PropConcaveColliderAlgorithm                = serializedObject.FindProperty("ConcaveColliderAlgorithm");
        PropConcaveColliderMaxHulls                 = serializedObject.FindProperty("ConcaveColliderMaxHulls");
        PropConcaveColliderMaxHullVertices          = serializedObject.FindProperty("ConcaveColliderMaxHullVertices");
        PropConcaveColliderLegacySteps              = serializedObject.FindProperty("ConcaveColliderLegacySteps");
	}

    public void OnSceneGUI()
    {
        FracturedObject fracturedComponent = target as FracturedObject;

        if(fracturedComponent == null)
        {
            return;
        }

        // Chunk connections

        if(fracturedComponent.ShowChunkConnectionLines)
        {
            Color handlesColor = Handles.color;
            Handles.color = new Color32(255, 0, 0, 255); //new Color32(155, 89, 182, 255);

            foreach(FracturedChunk chunkA in fracturedComponent.ListFracturedChunks)
            {
                if(chunkA)
                {
                    if(chunkA.ListAdjacentChunks.Count > 0)
                    {
                        Handles.DotCap(0, chunkA.transform.position, Quaternion.identity, HandleUtility.GetHandleSize(chunkA.transform.position) * 0.05f);
                    }

                    foreach(FracturedChunk.AdjacencyInfo chunkAdjacency in chunkA.ListAdjacentChunks)
                    {
                        if(chunkAdjacency.chunk)
                        {
                            Handles.DrawLine(chunkA.transform.position, chunkAdjacency.chunk.transform.position);
                        }
                    }
                }
            }

            Handles.color = handlesColor;
        }

        // Support planes

        bool bPlanesChanged = false;

        if(fracturedComponent.ListSupportPlanes != null)
        {
            foreach(UltimateFracturing.SupportPlane supportPlane in fracturedComponent.ListSupportPlanes)
            {
                if(supportPlane.GUIShowInScene == false)
                {
                    continue;
                }

                Vector3    v3WorldPosition = fracturedComponent.transform.TransformPoint(supportPlane.v3PlanePosition);
                Quaternion qWorldRotation  = fracturedComponent.transform.rotation * supportPlane.qPlaneRotation;
                Vector3    v3WorldScale    = Vector3.Scale(supportPlane.v3PlaneScale, fracturedComponent.transform.localScale);

                Handles.Label(v3WorldPosition, supportPlane.GUIName);

                // Normalize qWorldRotation

                float fSum = 0;                
                for(int i = 0; i < 4; ++i) fSum += qWorldRotation[i] * qWorldRotation[i];
                float fMagnitudeInverse = 1.0f / Mathf.Sqrt(fSum);
                for(int i = 0; i < 4; ++i) qWorldRotation[i] *= fMagnitudeInverse;

                Vector3    v3PlanePosition = supportPlane.v3PlanePosition;
                Quaternion qPlaneRotation  = supportPlane.qPlaneRotation;
                Vector3    v3PlaneScale    = supportPlane.v3PlaneScale;

                // Use tools

                switch(Tools.current)
                {
                    case Tool.Move:

                        v3PlanePosition = fracturedComponent.transform.InverseTransformPoint(Handles.PositionHandle(v3WorldPosition, qWorldRotation));
                        break;

                    case Tool.Rotate:

                        qPlaneRotation = Quaternion.Inverse(fracturedComponent.transform.rotation) * Handles.RotationHandle(qWorldRotation, v3WorldPosition);
                        break;

                    case Tool.Scale:

                        v3PlaneScale = Vector3.Scale(Handles.ScaleHandle(v3WorldScale, v3WorldPosition, qWorldRotation, HandleUtility.GetHandleSize(v3WorldPosition)), new Vector3(1.0f / fracturedComponent.transform.localScale.x, 1.0f / fracturedComponent.transform.localScale.y, 1.0f / fracturedComponent.transform.localScale.z));
                        break;
                }

                if(GUI.changed)
                {
                    EditorUtility.SetDirty(fracturedComponent);
                    bPlanesChanged = true;

                    switch(Tools.current)
                    {
                        case Tool.Move:
						supportPlane.v3PlanePosition = v3PlanePosition;
						break;

                        case Tool.Rotate:
						supportPlane.qPlaneRotation = qPlaneRotation;
						break;

                        case Tool.Scale:
						supportPlane.v3PlaneScale = v3PlaneScale;
						break;
                    }
                }
            }
        }

        if(bPlanesChanged)
        {
            fracturedComponent.ComputeSupportPlaneIntersections();
            fracturedComponent.MarkNonSupportedChunks();
        }
    }
	
    public override void OnInspectorGUI()
    {
        int nIndentationJump = 2;
        Vector4 v4GUIColor = GUI.contentColor;

        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.

        serializedObject.Update();

        FracturedObject fracturedComponent = serializedObject.targetObject as FracturedObject;

        // Show the custom GUI controls

        bool bDecomposePreviewUpdate = false;
        bool bReassignSplitMaterial  = false;
        bool bComputeFracturing      = false;
        bool bComputeColliders       = false;
        bool bDeleteColliders        = false;
        bool bChunksDeleted          = false;
        bool bMarkNonSupportedChunks = false;
        bool bRecomputePlanes        = false;

        EditorGUILayout.Space();
        fracturedComponent.GUIExpandMain = EditorGUILayout.Foldout(fracturedComponent.GUIExpandMain, new GUIContent("Main", "Main fracturing parameters"));

        if(fracturedComponent.GUIExpandMain)
        {
            GUI.contentColor = PropSourceObject.objectReferenceValue == null ? Color.red : GUI.contentColor;
            PropSourceObject.objectReferenceValue               = EditorGUILayout.ObjectField(new GUIContent("Source Object",             "The object whose mesh will be used as input for the fracturing. It won't be deleted, just used as input."), PropSourceObject.objectReferenceValue, typeof(GameObject), true);
            GUI.contentColor = v4GUIColor;
            PropGenerateIslands.boolValue                       = EditorGUILayout.Toggle     (new GUIContent("Island Generation",         "Detect isolated meshes when splitting and make them separated chunks (f.e. if you split a U shape horizontally, it will give 3 objects with island generation (the bottom and the two tips separated), instead of 2 (the bottom on one hand and the two tips as the same object on the other)."), PropGenerateIslands.boolValue);
            PropGenerateChunkConnectionInfo.boolValue           = EditorGUILayout.Toggle     (new GUIContent("Chunk Interconnection",     "Will generate a connection graph between chunks to enable structural behavior."), PropGenerateChunkConnectionInfo.boolValue);
            GUI.enabled = PropGenerateChunkConnectionInfo.boolValue;
            PropStartStatic.boolValue                           = EditorGUILayout.Toggle     (new GUIContent("Start Static",              "If Chunk Interconnection is checked and no support planes or support chunks have been defined, an object would collapse on start. Check this if you want the object to stay static until first contact."), PropStartStatic.boolValue);
            PropChunkConnectionMinArea.floatValue               = EditorGUILayout.FloatField (new GUIContent("Interconnection Min Area",  "Minimum area between 2 connected chunks to consider them connected. Setting it to zero won't consider all chunks connected, only those that share at least a common face area no matter how small."), PropChunkConnectionMinArea.floatValue);
            PropChunkConnectionStrength.floatValue              = EditorGUILayout.Slider     (new GUIContent("Interconnection Strength",  "When a chunk attached to the object is hit and detached, this controls how many connected chunks will detach too. 0.0 will make the whole object collapse, 1.0 won't detach any connected chunks."), PropChunkConnectionStrength.floatValue, 0.0f, 1.0f);
            EditorGUI.BeginChangeCheck();
            PropChunkHorizontalRadiusSupportStrength.floatValue = EditorGUILayout.FloatField (new GUIContent("Support Hor. Strength",     "Controls the maximum horizontal distance a chunk must be from a support chunk to stay attached to the object. If its distance is greater than this value, it will fall."), PropChunkHorizontalRadiusSupportStrength.floatValue);
            if(EditorGUI.EndChangeCheck())
            {
                bMarkNonSupportedChunks = true;
            }
            PropSupportChunksAreIndestructible.boolValue        = EditorGUILayout.Toggle     (new GUIContent("Support Is Indestructible", "If it is enabled, support chunks can not be destroyed/removed from the object."), PropSupportChunksAreIndestructible.boolValue);
            GUI.enabled = PropGenerateChunkConnectionInfo.boolValue && PropGenerateIslands.boolValue;
            PropChunkIslandConnectionMaxDistance.floatValue     = EditorGUILayout.FloatField (new GUIContent("Island Max Connect Dist.",  "When feeding a source object, and Island Generation is active, it may detect multiple closed meshes inside and some of them may be connected to others. This controls how far a face from one island can be from another island to consider the two islands connected."), PropChunkIslandConnectionMaxDistance.floatValue);
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();
            PropTotalMass.floatValue                            = EditorGUILayout.FloatField (new GUIContent("Total Mass",                "The total mass of the object. Each chunk mass will be computed depending on its size and this value."), PropTotalMass.floatValue);
            if(EditorGUI.EndChangeCheck())
            {
                fracturedComponent.ComputeChunksMass(PropTotalMass.floatValue);
            }
            PropChunkPhysicMaterial.objectReferenceValue        = EditorGUILayout.ObjectField(new GUIContent("Chunk Physic Material",     "The physic material assigned to each chunk."), PropChunkPhysicMaterial.objectReferenceValue, typeof(PhysicMaterial), true);
            PropMinColliderVolumeForBox.floatValue              = EditorGUILayout.FloatField (new GUIContent("Min Collider Volume",       "Chunks with a volume less than this value will have a box collider instead of a mesh collider to speed up collisions."), PropMinColliderVolumeForBox.floatValue);
            PropCapPrecisionFix.floatValue                      = EditorGUILayout.FloatField (new GUIContent("Cap Precision Fix (Adv.)",  "Change this value from 0 only if you experience weird triangles added to the mesh or unexpected crashes! This usually happens on meshes with very thin faces due to floating point errors. A good range is usually 0.001 to 0.02, larger values will introduce small visible variations on some splits. If there's only 2 or 3 problematic faces, then an alternative would be using another random seed."), PropCapPrecisionFix.floatValue);
            PropInvertCapNormals.boolValue                      = EditorGUILayout.Toggle     (new GUIContent("Reverse Cap Normals",       "Check this if for some reason the interior faces have reversed lighting."), PropInvertCapNormals.boolValue);
        }

        EditorGUILayout.Space();
        fracturedComponent.GUIExpandSplits = EditorGUILayout.Foldout(fracturedComponent.GUIExpandSplits, new GUIContent("Fracturing & Interior Material", "These parameters control the way the slicing will be performed."));

        if(fracturedComponent.GUIExpandSplits)
        {
            bool  bProbabilityXChanged = false;
            bool  bProbabilityYChanged = false;
            bool  bProbabilityZChanged = false;
            float fProbabilityXBefore  = PropSplitXProbability.floatValue;
            float fProbabilityYBefore  = PropSplitYProbability.floatValue;
            float fProbabilityZBefore  = PropSplitZProbability.floatValue;

            EditorGUILayout.PropertyField(PropFracturePattern, new GUIContent("Fracture Method", "The fracture algorithm. Voronoi generates cellular, more natural looking chunks while BSP slices objects progressively using planes until the target number of chunks is reached. BSP is faster to compute and provides different control."));

            if(PropFracturePattern.enumNames[PropFracturePattern.enumValueIndex] == FracturedObject.EFracturePattern.Voronoi.ToString())
            {
                EditorGUILayout.PropertyField(PropVoronoiVolumeOptimization, new GUIContent("Use Volume Optimization", "Will compute fracturing faster when using large meshes or with huge empty space."));
                EditorGUILayout.PropertyField(PropVoronoiProximityOptimization, new GUIContent("Use Proximity Optimiza.", "Disable this if you find intersecting chunks."));
                EditorGUILayout.PropertyField(PropVoronoiMultithreading, new GUIContent("Use Multithreading", "Will use multithreading for some Voronoi computation steps. It isn't fully tested/optimized but it should work. Only disable if you experience any problems."));
                EditorGUILayout.PropertyField(PropVoronoiCellsXCount, new GUIContent("Cells In Local X", "Voronoi will generate X*Y*Z cells. This is number of cells to generate in the X dimension."));
                EditorGUILayout.PropertyField(PropVoronoiCellsYCount, new GUIContent("Cells In Local Y", "Voronoi will generate X*Y*Z cells. This is number of cells to generate in the Y dimension."));
                EditorGUILayout.PropertyField(PropVoronoiCellsZCount, new GUIContent("Cells In Local Z", "Voronoi will generate X*Y*Z cells. This is number of cells to generate in the Z dimension."));
                PropVoronoiCellsXSizeVariation.floatValue = EditorGUILayout.Slider(new GUIContent("X Cells Variation", "Greater values will increase difference in size and positioning of cells in the X dimension"), PropVoronoiCellsXSizeVariation.floatValue, 0.0f, 1.0f);
                PropVoronoiCellsYSizeVariation.floatValue = EditorGUILayout.Slider(new GUIContent("Y Cells Variation", "Greater values will increase difference in size and positioning of cells in the Y dimension"), PropVoronoiCellsYSizeVariation.floatValue, 0.0f, 1.0f);
                PropVoronoiCellsZSizeVariation.floatValue = EditorGUILayout.Slider(new GUIContent("Z Cells Variation", "Greater values will increase difference in size and positioning of cells in the Z dimension"), PropVoronoiCellsZSizeVariation.floatValue, 0.0f, 1.0f);
            }

            if(PropFracturePattern.enumNames[PropFracturePattern.enumValueIndex] == FracturedObject.EFracturePattern.BSP.ToString())
            {
                PropNumChunks.intValue         = EditorGUILayout.IntField(new GUIContent("Number Of Chunks",     "The number of chunks to fracture the mesh into."), PropNumChunks.intValue);
                PropSplitsWorldSpace.boolValue = EditorGUILayout.Toggle  (new GUIContent("Slice In World Space", "Controls if the slicing will be performed in local object space or in world space. Note that the original object orientation is considered, not the fractured object."), PropSplitsWorldSpace.boolValue);
                PropSplitRegularly.boolValue   = EditorGUILayout.Toggle  (new GUIContent("Slice Regularly",      "If set, slices will always be performed to minimize the chunk size in all its axes, otherwise they will be performed randomly with the probabilities given."), PropSplitRegularly.boolValue);

                GUI.enabled = PropSplitRegularly.boolValue == false;
                EditorGUI.BeginChangeCheck();
                PropSplitXProbability.floatValue  = EditorGUILayout.Slider(new GUIContent("Slice X Probability",  "Probability (0-1) that a slice is performed in X"), PropSplitXProbability.floatValue, 0.0f, 1.0f);
                if(EditorGUI.EndChangeCheck()) bProbabilityXChanged = true;
                EditorGUI.BeginChangeCheck();
                PropSplitYProbability.floatValue  = EditorGUILayout.Slider(new GUIContent("Slice Y Probability",  "Probability (0-1) that a slice is performed in Y"), PropSplitYProbability.floatValue, 0.0f, 1.0f);
                if(EditorGUI.EndChangeCheck()) bProbabilityYChanged = true;
                EditorGUI.BeginChangeCheck();
                PropSplitZProbability.floatValue  = EditorGUILayout.Slider(new GUIContent("Slice Z Probability",  "Probability (0-1) that a slice is performed in Z"), PropSplitZProbability.floatValue, 0.0f, 1.0f);
                if(EditorGUI.EndChangeCheck()) bProbabilityZChanged = true;
                GUI.enabled = true;

                PropSplitSizeVariation.floatValue = EditorGUILayout.Slider(new GUIContent("Slice Size Variation", "0.0 will give chunks more equally sized. Increasing values will give chunks varying more in size."), PropSplitSizeVariation.floatValue, 0.0f, 1.0f);
                PropSplitXVariation.floatValue    = EditorGUILayout.Slider(new GUIContent("Slice X Variation",    "Angular variation for the slices in the X plane."), PropSplitXVariation.floatValue, 0.0f, 1.0f);
                PropSplitYVariation.floatValue    = EditorGUILayout.Slider(new GUIContent("Slice Y Variation",    "Angular variation for the slices in the Y plane."), PropSplitYVariation.floatValue, 0.0f, 1.0f);
                PropSplitZVariation.floatValue    = EditorGUILayout.Slider(new GUIContent("Slice Z Variation",    "Angular variation for the slices in the Z plane."), PropSplitZVariation.floatValue, 0.0f, 1.0f);
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            PropSplitMaterial.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Interior Material", "Material applied to the interior faces of the chunks."), PropSplitMaterial.objectReferenceValue, typeof(Material), true);
            if(EditorGUI.EndChangeCheck())
            {
                bReassignSplitMaterial = true;
            }

            PropSplitMappingTileU.floatValue = EditorGUILayout.FloatField (new GUIContent("Interior Mapping U Tile", "U Tiling of the interior faces mapping."), PropSplitMappingTileU.floatValue);
            PropSplitMappingTileV.floatValue = EditorGUILayout.FloatField (new GUIContent("Interior Mapping V Tile", "V Tiling of the interior faces mapping."), PropSplitMappingTileV.floatValue);

            float fNewSplitXProbability = PropSplitXProbability.floatValue;
            float fNewSplitYProbability = PropSplitYProbability.floatValue;
            float fNewSplitZProbability = PropSplitZProbability.floatValue;

            if(bProbabilityXChanged) ChangeProbability(PropSplitXProbability.floatValue, fProbabilityXBefore, ref fNewSplitYProbability, ref fNewSplitZProbability);
            if(bProbabilityYChanged) ChangeProbability(PropSplitYProbability.floatValue, fProbabilityYBefore, ref fNewSplitXProbability, ref fNewSplitZProbability);
            if(bProbabilityZChanged) ChangeProbability(PropSplitZProbability.floatValue, fProbabilityZBefore, ref fNewSplitXProbability, ref fNewSplitYProbability);

            PropSplitXProbability.floatValue = fNewSplitXProbability;
            PropSplitYProbability.floatValue = fNewSplitYProbability;
            PropSplitZProbability.floatValue = fNewSplitZProbability;
        }

        EditorGUILayout.Space();
        fracturedComponent.GUIExpandEvents = EditorGUILayout.Foldout(fracturedComponent.GUIExpandEvents, new GUIContent("Events", "These parameters control the behavior of the object on some events."));

        if(fracturedComponent.GUIExpandEvents)
        {
            EditorGUILayout.LabelField("Chunk Detach From Object Due To Physics Collision:");
            EditorGUI.indentLevel += nIndentationJump;
            EditorGUILayout.PropertyField(PropEventDetachMinMass,                       new GUIContent("Min Impact Mass",        "The minimum mass an object needs to have to detach a chunk from this object on impact."));
            EditorGUILayout.PropertyField(PropEventDetachMinVelocity,                   new GUIContent("Min Impact Velocity",    "The minimum velocity an object needs to impact with to detach a chunk from this object."));
            EditorGUILayout.PropertyField(PropEventDetachExitForce,                     new GUIContent("Exit Force",             "If a chunk is detached due to an impact, it will have this value applied to it."));
            EditorGUILayout.PropertyField(PropEventDetachUpwardsModifier,               new GUIContent("Upwards Modifier",       "Adds an upwards explosion effect to the chunks that have exit force. A value of 0.0 won't add any effect, while 2.0 means it will apply the force from a distance of 2 below the chunk."));
            EditorGUILayout.PropertyField(PropEventDetachSound,                         new GUIContent("Detach Sound",           "Will play this sound on the collision point when a chunk is detached due to an impact."));
            EditorGUILayout.PropertyField(PropEventDetachPrefabsArray,                  new GUIContent("Instance Prefab List (1 Randomly Spawned On Detach)", "A list of prefabs. When a chunk is detached due to an impact, a random prefab will be picked from this list and instanced on the collision point. Use this for particles/explosions."), true);
            EditorGUILayout.PropertyField(PropEventDetachCollisionCallMethod,           new GUIContent("Call Method Name",       "The method name that will be called on an impact-triggered detach chunk event."));
            EditorGUILayout.PropertyField(PropEventDetachCollisionCallGameObject,       new GUIContent("Call GameObject",        "The GameObject whose method will be called."));
            EditorGUI.indentLevel -= nIndentationJump;
            EditorGUILayout.LabelField("Free (Detached) Chunks:");
            EditorGUI.indentLevel += nIndentationJump;
            EditorGUILayout.PropertyField(PropEventDetachedMinLifeTime,                 new GUIContent("Min Chunk LifeTime",     "The minimum lifetime of a free chunk. When the life of a chunk expires it will be deleted."));
            EditorGUILayout.PropertyField(PropEventDetachedMaxLifeTime,                 new GUIContent("Max Chunk LifeTime",     "The maximum lifetime of a free chunk. When the life of a chunk expires it will be deleted."));
            EditorGUILayout.PropertyField(PropEventDetachedOffscreenLifeTime,           new GUIContent("Offscreen LifeTime",     "If a free chunk is outside the visible screen for more than this seconds, it will be deleted."));
            EditorGUILayout.PropertyField(PropEventDetachedMinMass,                     new GUIContent("Min Impact Mass",        "The minimum mass a free chunk need to impact with in order to trigger a collision event."));
            EditorGUILayout.PropertyField(PropEventDetachedMinVelocity,                 new GUIContent("Min Impact Velocity",    "The minimum velocity a free chunk need to impact with in order to trigger a collision event."));
            EditorGUILayout.PropertyField(PropEventDetachedMaxSounds,                   new GUIContent("Max Simult. Sounds",     "The maximum collision sounds that will be played at the same time."));
            EditorGUILayout.PropertyField(PropEventDetachedSoundArray,                  new GUIContent("Collision Sound List (1 Randomly Played On Collision)", "A list of sounds. On a free chunk collision a random sound will be picked from this list and played on the collision point."), true);
            EditorGUILayout.PropertyField(PropEventDetachedMaxPrefabs,                  new GUIContent("Max Simult. Prefabs",    "The maximum number of collision prefabs present at the same time."));
            EditorGUILayout.PropertyField(PropEventDetachedPrefabsArray,                new GUIContent("Collision Prefab List (1 Randomly Spawned On Collision)", "A list of prefabs. On a free chunk collision a random prefab will be picked from this list and instanced on the collision point. Use this for particles/explosions."), true);
            EditorGUILayout.PropertyField(PropEventDetachedCollisionCallMethod,         new GUIContent("Call Method Name",       "The method name that will be called on a free chunk collision."));
            EditorGUILayout.PropertyField(PropEventDetachedCollisionCallGameObject,     new GUIContent("Call GameObject",        "The GameObject whose method will be called."));
            EditorGUI.indentLevel -= nIndentationJump;
            EditorGUILayout.LabelField("When Explode() Is Called Through Scripting (Explosions):");
            EditorGUI.indentLevel += nIndentationJump;
            EditorGUILayout.PropertyField(PropEventExplosionSound,                      new GUIContent("Explosion Sound",        "The sound that will be played when Explode() is called on this object."));
            EditorGUILayout.PropertyField(PropEventExplosionPrefabInstanceCount,        new GUIContent("Random Prefabs",         "The number of prefabs to instance on random positions of the object."));
            EditorGUILayout.PropertyField(PropEventExplosionPrefabsArray,               new GUIContent("Instance Prefab List (Spawned Randomly Around)", "A list of prefabs. When Explode() is called a random number of them will be instanced on random positions of the object. Use this for particles/explosions."), true);
            EditorGUI.indentLevel -= nIndentationJump;
            EditorGUILayout.LabelField("When Impact() Is Called Through Scripting (f.e. Missiles):");
            EditorGUI.indentLevel += nIndentationJump;
            EditorGUILayout.PropertyField(PropEventImpactSound,                         new GUIContent("Impact Sound",           "The sound that will be played when Impact() is called on this object."));
            EditorGUILayout.PropertyField(PropEventImpactPrefabsArray,                  new GUIContent("Impact Prefab List (1 Randomly Spawned On Impact)", "A list of prefabs. When Impact() is called a random prefab will be instanced on the impact point. Use this for particles/explosions."), true);
            EditorGUI.indentLevel -= nIndentationJump;
            EditorGUILayout.LabelField("On Every Chunk Detach Event (Collision, Impact, Explosion, User scripted detach...):");
            EditorGUI.indentLevel += nIndentationJump;
            EditorGUILayout.PropertyField(PropEventDetachedAnyCallMethod,               new GUIContent("Call Method Name",       "The method name that will be called every time a chunk is detached for whatever reason."));
            EditorGUILayout.PropertyField(PropEventDetachedAnyCallGameObject,           new GUIContent("Call GameObject",        "The GameObject whose method will be called."));
            EditorGUI.indentLevel -= nIndentationJump;
        }

        EditorGUILayout.Space();
        fracturedComponent.GUIExpandSupportPlanes = EditorGUILayout.Foldout(fracturedComponent.GUIExpandSupportPlanes, new GUIContent("Support Planes", "Support planes control which chunks are tagged as support. Chunks that act as support can't be destroyed and will hold the object together. A chunk needs to be connected to a support chunk (directly or through other chunks) or otherwise it will fall. This prevents chunks from staying static in the air and enables realistic collapsing behavior."));

        if(fracturedComponent.GUIExpandSupportPlanes)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(" ");

            if(GUILayout.Button(new GUIContent("Add Support Plane", "Adds a new support plane to this object. Support planes control which chunks are tagged as support. Chunks that act as support can't be destroyed and will hold the object together. A chunk needs to be connected to a support chunk (directly or through other chunks) or otherwise it will fall. This prevents chunks from staying static in the air and enables realistic collapsing behavior."), GUILayout.Width(200)))
            {
                fracturedComponent.AddSupportPlane();
                fracturedComponent.ComputeSupportPlaneIntersections();
                fracturedComponent.MarkNonSupportedChunks();
            }

            GUILayout.Label(" ");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            int  nDeletePlane     = -1;
            int  nPlaneIndex      = 0;

            EditorGUI.indentLevel += nIndentationJump;

            foreach(UltimateFracturing.SupportPlane supportPlane in fracturedComponent.ListSupportPlanes)
            {
                supportPlane.GUIExpanded = EditorGUILayout.Foldout(supportPlane.GUIExpanded, new GUIContent(supportPlane.GUIName, "Support plane parameters."));

                if(supportPlane.GUIExpanded)
                {
                    EditorGUI.indentLevel += nIndentationJump;

                    EditorGUILayout.Space();
                    EditorGUI.BeginChangeCheck();

                    supportPlane.GUIShowInScene  = EditorGUILayout.Toggle        (new GUIContent("Show In Scene",  "Controls if the plane is drawn or not in the scene view."), supportPlane.GUIShowInScene);
                    supportPlane.GUIName         = EditorGUILayout.TextField     (new GUIContent("Plane Name",     "The name that will be shown on the scene view."), supportPlane.GUIName);
                    supportPlane.v3PlanePosition = EditorGUILayout.Vector3Field                 ("Local Position", supportPlane.v3PlanePosition);
                    supportPlane.qPlaneRotation  = Quaternion.Euler(EditorGUILayout.Vector3Field("Local Rotation", supportPlane.qPlaneRotation.eulerAngles));
                    supportPlane.v3PlaneScale    = EditorGUILayout.Vector3Field                 ("Local Scale",    supportPlane.v3PlaneScale);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(" ");
                    if(GUILayout.Button(new GUIContent("Delete", "Deletes this support plane."), GUILayout.Width(100)))
                    {
                        nDeletePlane = nPlaneIndex;
                        bRecomputePlanes = true;
                    }
                    GUILayout.Label(" ");
                    EditorGUILayout.EndHorizontal();

                    if(EditorGUI.EndChangeCheck())
                    {
                        bRecomputePlanes = true;
                    }

                    EditorGUI.indentLevel -= nIndentationJump;
                }

                nPlaneIndex++;
            }

            EditorGUI.indentLevel -= nIndentationJump;

            if(nDeletePlane != -1)
            {
                fracturedComponent.ListSupportPlanes.RemoveAt(nDeletePlane);
            }
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        PropRandomSeed.intValue = EditorGUILayout.IntField(new GUIContent("Random Seed", "Each seed will create a differently fractured object. Change the seed if you are not happy with the results."), PropRandomSeed.intValue);
        if(GUILayout.Button(new GUIContent("New Seed"), GUILayout.Width(100)))
        {
            PropRandomSeed.intValue = Mathf.RoundToInt(Random.value * 1000000.0f);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        PropDecomposePreview.floatValue = EditorGUILayout.Slider(new GUIContent("Preview Chunks", "Use this slider to preview the chunks that were generated."), PropDecomposePreview.floatValue, 0.0f, fracturedComponent.DecomposeRadius);
        if(EditorGUI.EndChangeCheck())
        {
            bDecomposePreviewUpdate = true;
        }

        PropAlwaysComputeColliders.boolValue   = EditorGUILayout.Toggle(new GUIContent("Always Gen. Colliders",  "Will also generate colliders each time the chunks are computed."), PropAlwaysComputeColliders.boolValue);
        PropShowChunkConnectionLines.boolValue = EditorGUILayout.Toggle(new GUIContent("Show Chunk Connections", "Will draw lines on the scene view to show how chunks are connected between each other."), PropShowChunkConnectionLines.boolValue);
        PropShowChunkColoredState.boolValue    = EditorGUILayout.Toggle(new GUIContent("Color Chunk State",      "Will color chunks in the scene view to show which ones are support chunks and which ones aren't connected to any support chunks directly or indirectly and will fall down when checking for structure integrity."), PropShowChunkColoredState.boolValue);
        PropShowChunkColoredRandomly.boolValue = EditorGUILayout.Toggle(new GUIContent("Color Chunks",           "Will color chunks randomly in the scene window to see them better."), PropShowChunkColoredRandomly.boolValue);

        EditorGUI.BeginChangeCheck();
        PropSaveMeshDataToAsset.boolValue      = EditorGUILayout.Toggle(new GUIContent("Enable Prefab Usage",    "Will save the chunk and collider meshes to an asset file on disk when they are computed. Use this if you want to add this object to a prefab, otherwise the meshes and colliders won't be instanced properly."), PropSaveMeshDataToAsset.boolValue);
        if(EditorGUI.EndChangeCheck())
        {
            if(PropSaveMeshDataToAsset.boolValue)
            {
                if(System.IO.File.Exists(fracturedComponent.MeshAssetDataFile) == false)
                {
                    PropMeshAssetDataFile.stringValue = UnityEditor.EditorUtility.SaveFilePanelInProject("Save mesh asset", "mesh_" + fracturedComponent.name + this.GetInstanceID().ToString() + ".asset", "asset", "Please enter a file name to save the mesh asset to");
                }

                if(PropMeshAssetDataFile.stringValue.Length == 0)
                {
                    PropSaveMeshDataToAsset.boolValue = false;
                }
            }
            else
            {
                PropMeshAssetDataFile.stringValue = "";
            }
        }

        PropVerbose.boolValue = EditorGUILayout.Toggle(new GUIContent("Output Console Info", "Outputs messages and warnings to the console window."), PropVerbose.boolValue);

        // Fracture?

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = PropSourceObject.objectReferenceValue != null;
        bComputeFracturing = GUILayout.Button(new GUIContent("Compute Chunks", "Computes the fractured chunks"));
        GUI.enabled = true;

        bool bHasChunks = fracturedComponent.ListFracturedChunks.Count > 0;

        GUI.enabled = bHasChunks;
        if(GUILayout.Button(new GUIContent("Delete Chunks", "Deletes the fractured chunks")))
        {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
            Undo.RegisterSceneUndo("Delete Chunks");
#endif
            fracturedComponent.DeleteChunks();
            bChunksDeleted = true;
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        PropIntegrateWithConcaveCollider.boolValue = EditorGUILayout.Toggle(new GUIContent("Use Concave Collider", "Use the external Concave Collider utility to have more control over how mesh colliders are generated."), PropIntegrateWithConcaveCollider.boolValue);
        if(EditorGUI.EndChangeCheck())
        {
            if(PropIntegrateWithConcaveCollider.boolValue)
            {
                if(System.IO.File.Exists("Assets/Plugins/ConvexDecompositionDll.dll") == false && System.IO.File.Exists("Assets/Plugins/x86_64/ConvexDecompositionDll.dll") == false)
                {
                    if(EditorUtility.DisplayDialog("Concave Collider not found", "Concave Collider is a utility for Unity that allows the automatic generation of compound colliders for dynamic objects.\n\nThe Ultimate Fracturing tool can use it to avoid generating hulls bigger than 255 triangles. It will also allow to specify the maximum number of vertices to generate for each collider to optimize collision calculations.\n\nNote: The Concave Collider requires a PRO license of Unity3D.", "Show Asset", "Cancel"))
                    {
                        Application.OpenURL("https://www.assetstore.unity3d.com/#/content/4596");
                    }

                    PropIntegrateWithConcaveCollider.boolValue = false;
                }
            }
        }

        GUI.enabled = PropIntegrateWithConcaveCollider.boolValue;
        EditorGUILayout.PropertyField(PropConcaveColliderAlgorithm, new GUIContent("Algorithm", "The convex decomposition algorithm. Fast should be the best for 95% of the cases, use the other ones if fast does not work."));

        // Uncomment for advanced control. The self-assignment is to avoid a compiler warning (warning CS0414: field assigned but value never used).
        PropConcaveColliderMaxHulls.intValue = PropConcaveColliderMaxHulls.intValue;
        // PropConcaveColliderMaxHulls.intValue = EditorGUILayout.IntSlider(new GUIContent("Max Colliders Per Chunk", "Limits the maximum collider hulls a chunk can have."), PropConcaveColliderMaxHulls.intValue, 1, 10);

        if(PropConcaveColliderAlgorithm.enumNames[PropConcaveColliderAlgorithm.enumValueIndex] == FracturedObject.ECCAlgorithm.Fast.ToString() || PropConcaveColliderAlgorithm.enumNames[PropConcaveColliderAlgorithm.enumValueIndex] == FracturedObject.ECCAlgorithm.Normal.ToString())
        {
            PropConcaveColliderMaxHullVertices.intValue = EditorGUILayout.IntSlider(new GUIContent("Max Collider Vertices", "Limits the maximum vertices a collider hull can have."), PropConcaveColliderMaxHullVertices.intValue, 4, 1024);
        }
        if(PropConcaveColliderAlgorithm.enumNames[PropConcaveColliderAlgorithm.enumValueIndex] == FracturedObject.ECCAlgorithm.Legacy.ToString())
        {
            // Uncomment for advanced control. The self-assignment is to avoid a compiler warning (warning CS0414: field assigned but value never used).
            PropConcaveColliderLegacySteps.intValue = PropConcaveColliderLegacySteps.intValue;
            // EditorGUILayout.IntSlider(PropConcaveColliderLegacySteps, 1, 6, new GUIContent("Legacy Steps", "How many iterations to compute. More steps = more hulls and more computing time"));
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUI.enabled = fracturedComponent.transform.childCount > 0;

        if(GUILayout.Button(new GUIContent("Compute Colliders", "Computes the chunk colliders.")))
        {
            if(PropSaveMeshDataToAsset.boolValue == true)
            {
                EditorUtility.DisplayDialog("Chunks must be recomputed, sorry!", "If the parameter \"Enable Prefab Usage\" is checked, the colliders can only be recomputed through the \"Compute Chunks\" button with the \"Always Gen Colliders\" parameter enabled.", "OK");
            }
            else
            {
                bComputeColliders = true;
            }
        }

        if(GUILayout.Button(new GUIContent("Delete Colliders", "Deletes the chunk colliders")))
        {
            bDeleteColliders = true;
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        // Apply changes to the serializedProperty

        serializedObject.ApplyModifiedProperties();

        // Perform actions

        bool bProgressBarCreated = false;
        m_bProgressCancelled = false;

        if(bComputeFracturing)
        {
            bProgressBarCreated = true;

            GameObject goSource = PropSourceObject.objectReferenceValue as GameObject;

            if(goSource.GetComponent<MeshFilter>() == null)
            {
                EditorUtility.DisplayDialog("Error", "Source object has no mesh assigned", "OK");
            }
            else
            {
                bool bPositionOnSourceAndHideOriginal = false;

#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
                Undo.RegisterSceneUndo("Compute Chunks");
#endif
				
#if UNITY_3_5
				bool bIsActive = goSource.active;
#else
                bool bIsActive = goSource.activeSelf;
#endif

				bool bIsAsset = AssetDatabase.Contains(goSource);

                if(bIsActive && bHasChunks == false && bIsAsset == false)
                {
                    if(EditorUtility.DisplayDialog("Hide old and position new?", "Do you want to hide the original object and place the new fractured object in its position?", "Yes", "No"))
                    {
                        bPositionOnSourceAndHideOriginal = true;
                    }
                }

                List<GameObject> listGameObjects;

                bool bError = false;

                float fStartTime = Time.realtimeSinceStartup;

                try
                {
                    UltimateFracturing.Fracturer.FractureToChunks(fracturedComponent, bPositionOnSourceAndHideOriginal, out listGameObjects, Progress);
                }
                catch(System.Exception e)
                {
                    Debug.LogError(string.Format("Exception computing chunks ({0}):\n{1}", e.Message, e.StackTrace));
                    bError = true;
                }

                float fEndTime = Time.realtimeSinceStartup;

                EditorUtility.ClearProgressBar();

                if(bError == false && m_bProgressCancelled == false)
                {
                    if(fracturedComponent.Verbose)
                    {
                        Debug.Log("Compute time = " + (fEndTime - fStartTime) + "seconds");
                    }
                }

                if(m_bProgressCancelled)
                {
                    fracturedComponent.DeleteChunks();
                }
            }
        }

        if(bComputeColliders && m_bProgressCancelled == false)
        {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
            Undo.RegisterSceneUndo("Compute Colliders");
#endif
            bProgressBarCreated = true;
            UltimateFracturing.Fracturer.ComputeChunkColliders(fracturedComponent, Progress);
        }

        if(bDeleteColliders)
        {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
            Undo.RegisterSceneUndo("Delete Colliders");
#endif
            UltimateFracturing.Fracturer.DeleteChunkColliders(fracturedComponent);
        }

        if((bDecomposePreviewUpdate || bComputeFracturing) && Application.isPlaying == false)
        {
            if(fracturedComponent.HasDetachedChunks() == false)
            {
                if(Mathf.Equals(PropDecomposePreview.floatValue, 0.0f))
                {
                    fracturedComponent.SetSingleMeshVisibility(true);
                }
                else
                {
                    fracturedComponent.SetSingleMeshVisibility(false);
                }
            }
        }

        foreach(FracturedChunk fracturedChunk in fracturedComponent.ListFracturedChunks)
        {
            if(fracturedChunk != null)
            {
                if(bDecomposePreviewUpdate || bComputeFracturing)
                {
                    fracturedChunk.PreviewDecompositionValue = PropDecomposePreview.floatValue;
                    fracturedChunk.UpdatePreviewDecompositionPosition();
                }

                if(bReassignSplitMaterial)
                {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
					Undo.RegisterUndo(fracturedChunk.renderer, "Assign Inside Material");
#else
					Undo.RecordObject(fracturedChunk.GetComponent<Renderer>(), "Assign Inside Material");
#endif

                    if(fracturedChunk.SplitSubMeshIndex != -1)
                    {
                        Material[] aMaterials = new Material[fracturedChunk.GetComponent<Renderer>().sharedMaterials.Length];

                        for(int nMaterial = 0; nMaterial < aMaterials.Length; nMaterial++)
                        {
                            aMaterials[nMaterial] = nMaterial == fracturedChunk.SplitSubMeshIndex ? PropSplitMaterial.objectReferenceValue as Material : fracturedChunk.GetComponent<Renderer>().sharedMaterials[nMaterial];
                        }

                        fracturedChunk.GetComponent<Renderer>().sharedMaterials = aMaterials;
                    }
                }
            }
        }

        if(bRecomputePlanes)
        {
            SceneView.RepaintAll();
            fracturedComponent.ComputeSupportPlaneIntersections();
        }

        if(bMarkNonSupportedChunks || bComputeFracturing || bRecomputePlanes)
        {
            fracturedComponent.MarkNonSupportedChunks();
        }

        if(fracturedComponent.SaveMeshDataToAsset && (bComputeFracturing || bComputeColliders) && (m_bProgressCancelled == false))
        {
            bProgressBarCreated = true;

            if(fracturedComponent.MeshAssetDataFile.Length > 0)
            {
                bool bFirstAdded = false;

                AssetDatabase.DeleteAsset(fracturedComponent.MeshAssetDataFile);

                // Save chunks

                for(int nChunk = 0; nChunk < fracturedComponent.ListFracturedChunks.Count && m_bProgressCancelled == false; nChunk++)
                {
                    FracturedChunk chunk = fracturedComponent.ListFracturedChunks[nChunk];

                    Progress("Saving mesh assets to disk", string.Format("Chunk {0}/{1}", nChunk + 1, fracturedComponent.ListFracturedChunks.Count), (float)nChunk / (float)fracturedComponent.ListFracturedChunks.Count);

                    MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();

                    // Save mesh

                    if(bFirstAdded == false && meshFilter != null)
                    {
                        UnityEditor.AssetDatabase.CreateAsset(meshFilter.sharedMesh, fracturedComponent.MeshAssetDataFile);
                        bFirstAdded = true;
                    }
                    else if(meshFilter != null)
                    {
                        UnityEditor.AssetDatabase.AddObjectToAsset(meshFilter.sharedMesh, fracturedComponent.MeshAssetDataFile);
		 				UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(meshFilter.sharedMesh));
                    }

                    // Save collider mesh if it was generated by the concave collider

                    if(chunk.HasConcaveCollider)
                    {
                        MeshCollider meshCollider = chunk.GetComponent<MeshCollider>();

                        if(bFirstAdded == false && meshCollider != null)
                        {
                            UnityEditor.AssetDatabase.CreateAsset(meshCollider.sharedMesh, fracturedComponent.MeshAssetDataFile);
                            bFirstAdded = true;
                        }
                        else if(meshCollider != null)
                        {
                            UnityEditor.AssetDatabase.AddObjectToAsset(meshCollider.sharedMesh, fracturedComponent.MeshAssetDataFile);
		 				    UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(meshCollider.sharedMesh));
                        }
                    }
                }

                // Save planes

                for(int nPlane = 0; nPlane < fracturedComponent.ListSupportPlanes.Count && m_bProgressCancelled == false; nPlane++)
                {
                    UltimateFracturing.SupportPlane supportPlane = fracturedComponent.ListSupportPlanes[nPlane];

                    Progress("Saving support plane mesh assets to disk", string.Format("Plane {0}/{1}", nPlane + 1, fracturedComponent.ListSupportPlanes.Count), (float)nPlane / (float)fracturedComponent.ListSupportPlanes.Count);

                    // Save mesh

                    if(bFirstAdded == false && supportPlane.planeMesh != null)
                    {
                        UnityEditor.AssetDatabase.CreateAsset(supportPlane.planeMesh, fracturedComponent.MeshAssetDataFile);
                        bFirstAdded = true;
                    }
                    else if(supportPlane.planeMesh != null)
                    {
                        UnityEditor.AssetDatabase.AddObjectToAsset(supportPlane.planeMesh, fracturedComponent.MeshAssetDataFile);
		 				UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(supportPlane.planeMesh));
                    }
                }

				// Save single draw object
				
				MeshFilter meshFilterSingle = null;
				
				if(fracturedComponent.SingleMeshObject)
				{
					meshFilterSingle = fracturedComponent.SingleMeshObject.GetComponent<MeshFilter>();
				}
				
				if(meshFilterSingle)
				{
					Progress("Saving single draw object mesh asset to disk", "Mesh", 1.0f);
					
					if(bFirstAdded == false && meshFilterSingle.sharedMesh != null)
					{
						UnityEditor.AssetDatabase.CreateAsset(meshFilterSingle.sharedMesh, fracturedComponent.MeshAssetDataFile);
						bFirstAdded = true;
					}
					else if(meshFilterSingle.sharedMesh != null)
					{
						UnityEditor.AssetDatabase.AddObjectToAsset(meshFilterSingle.sharedMesh, fracturedComponent.MeshAssetDataFile);
						UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(meshFilterSingle.sharedMesh));
					}
				}

                UnityEditor.AssetDatabase.Refresh();
            }
        }

        if(bProgressBarCreated)
        {
            EditorUtility.ClearProgressBar();
        }

        if(bComputeColliders || bComputeFracturing || bDeleteColliders || bChunksDeleted)
        {
            Resources.UnloadUnusedAssets();
        }
    }

    void ChangeProbability(float fNewValue, float fOldValue, ref float fOtherValue1Ref, ref float fOtherValue2Ref)
    {
        float fChange = fNewValue - fOldValue;

        if(Mathf.Approximately(fOtherValue1Ref, 0.0f) && Mathf.Approximately(fOtherValue2Ref, 0.0f) == false)
        {
            fOtherValue1Ref = 0.0f;
            fOtherValue2Ref = 1.0f - fNewValue;
        }
        else if(Mathf.Approximately(fOtherValue2Ref, 0.0f) && Mathf.Approximately(fOtherValue1Ref, 0.0f) == false)
        {
            fOtherValue1Ref = 1.0f - fNewValue;
            fOtherValue2Ref = 0.0f;
        }
        else
        {
            fOtherValue1Ref = Mathf.Clamp01(fOtherValue1Ref - (fChange * 0.5f));
            fOtherValue2Ref = 1.0f - (fNewValue + fOtherValue1Ref);
        }
    }
}
