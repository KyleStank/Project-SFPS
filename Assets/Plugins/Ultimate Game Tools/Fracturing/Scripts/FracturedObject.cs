using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UltimateFracturing;

[ExecuteInEditMode]
public class FracturedObject : MonoBehaviour
{
    public enum EFracturePattern
    {
        BSP,
        Voronoi
    }

    [Serializable]
    public class PrefabInfo
    {
        public PrefabInfo()
        {
            MinLifeTime = Mathf.Infinity;
            MaxLifeTime = Mathf.Infinity;
            GameObject  = null;
        }

        public float      MinLifeTime = Mathf.Infinity;
        public float      MaxLifeTime = Mathf.Infinity;
        public GameObject GameObject  = null;
    }

    public bool             GUIExpandMain                        = true;
    public GameObject       SourceObject;
    public bool             GenerateIslands                      = true;
    public bool             GenerateChunkConnectionInfo          = true;
    public bool             StartStatic                          = false;
    public float            ChunkConnectionMinArea               = 0.0f;
    public float            ChunkConnectionStrength              = 0.8f;
    public float            ChunkHorizontalRadiusSupportStrength = Mathf.Infinity;
    public bool             SupportChunksAreIndestructible       = true;
    public float            ChunkIslandConnectionMaxDistance     = 0.02f;
    public float            TotalMass                            = 10.0f;
    public PhysicMaterial   ChunkPhysicMaterial                  = null;
    public float            MinColliderVolumeForBox              = 0.00001f;
    public float            CapPrecisionFix                      = 0.0f;
    public bool             InvertCapNormals                     = false;

    public bool             GUIExpandSplits                      = true;
    public EFracturePattern FracturePattern                      = EFracturePattern.BSP;
    public bool             VoronoiVolumeOptimization            = true;
    public bool             VoronoiProximityOptimization         = true;
    public bool             VoronoiMultithreading                = true;
    public int              VoronoiCellsXCount                   = 3;
    public int              VoronoiCellsYCount                   = 3;
    public int              VoronoiCellsZCount                   = 3;
    public float            VoronoiCellsXSizeVariation           = 0.5f;
    public float            VoronoiCellsYSizeVariation           = 0.5f;
    public float            VoronoiCellsZSizeVariation           = 0.5f;
    public int              GenerateNumChunks                    = 20;
    public bool             SplitsWorldSpace                     = true;
    public bool             SplitRegularly                       = false;
    public float            SplitXProbability                    = 0.3333f;
    public float            SplitYProbability                    = 0.3333f;
    public float            SplitZProbability                    = 0.3333f;
    public float            SplitSizeVariation                   = 0.0f;
    public float            SplitXVariation                      = 0.6f;
    public float            SplitYVariation                      = 0.6f;
    public float            SplitZVariation                      = 0.6f;
    public Material         SplitMaterial;
    public float            SplitMappingTileU                    = 1.0f;
    public float            SplitMappingTileV                    = 1.0f;

    public bool             GUIExpandEvents                      = false;
    public float            EventDetachMinMass                   = 1.0f;
    public float            EventDetachMinVelocity               = 1.0f;
    public float            EventDetachExitForce                 = 0.0f;
    public float            EventDetachUpwardsModifier           = 0.0f;
    public AudioClip        EventDetachSound                     = null;
    public PrefabInfo[]     EventDetachPrefabsArray              = null;
    public string           EventDetachCollisionCallMethod       = "";
    public GameObject       EventDetachCollisionCallGameObject   = null;
    public float            EventDetachedMinLifeTime             = Mathf.Infinity;
    public float            EventDetachedMaxLifeTime             = Mathf.Infinity;
    public float            EventDetachedOffscreenLifeTime       = Mathf.Infinity;
    public float            EventDetachedMinMass                 = 1.0f;
    public float            EventDetachedMinVelocity             = 1.0f;
    public int              EventDetachedMaxSounds               = 5;
    public AudioClip[]      EventDetachedSoundArray;
    public int              EventDetachedMaxPrefabs              = 5;
    public PrefabInfo[]     EventDetachedPrefabsArray;
    public string           EventDetachedCollisionCallMethod     = "";
    public GameObject       EventDetachedCollisionCallGameObject = null;
    public AudioClip        EventExplosionSound                  = null;
    public int              EventExplosionPrefabsInstanceCount   = 10;
    public PrefabInfo[]     EventExplosionPrefabsArray;
    public AudioClip        EventImpactSound                     = null;
    public PrefabInfo[]     EventImpactPrefabsArray;
    public string           EventDetachedAnyCallMethod           = "";
    public GameObject       EventDetachedAnyCallGameObject       = null;

    public bool             GUIExpandSupportPlanes               = false;

    public int              RandomSeed                           = 0;
    public float            DecomposePreview                     = 0.0f;
    public bool             AlwaysComputeColliders               = true;
    public bool             ShowChunkConnectionLines             = false;
    public bool             ShowChunkColoredState                = true;
    public bool             ShowChunkColoredRandomly             = false;
    public bool             SaveMeshDataToAsset                  = false;
    public string           MeshAssetDataFile                    = "";
    public bool             Verbose                              = false;

    public enum ECCAlgorithm
    {
        Fast,
        Normal,
        Legacy
    }

    public bool             IntegrateWithConcaveCollider         = false;
    public ECCAlgorithm     ConcaveColliderAlgorithm             = ECCAlgorithm.Fast;
    public int              ConcaveColliderMaxHulls              = 1;
    public int              ConcaveColliderMaxHullVertices       = 64;
    public int              ConcaveColliderLegacySteps           = 1;

    [HideInInspector] public List<UltimateFracturing.SupportPlane> ListSupportPlanes   = new List<UltimateFracturing.SupportPlane>();
    [HideInInspector] public List<FracturedChunk>                  ListFracturedChunks = new List<FracturedChunk>();

    [HideInInspector] public GameObject SingleMeshObject            = null;  // Optimization. When no chunks have been deattached, we draw the structure using this object, otherwise the chunks individually.
    [HideInInspector] public bool       IsUsingSingleMeshObjectDraw = false;
    [HideInInspector] public float      DecomposeRadius             = 1.0f;

    public static Color  GizmoColorSupport    = new Color(0.0f, 0.0f, 0.2f, 0.7f);
    public static Color  GizmoColorNonSupport = new Color(0.3f, 0.0f, 0.0f, 0.7f);

    private bool         m_bCheckDetachNonSupportedChunkds;
    private bool         m_bExploded;
    private bool         m_bDetached;
    private float[]      m_afFreeChunkSoundTimers;
    private float[]      m_afFreeChunkPrefabTimers;

    public Material GizmosMaterial
    {
        get
        {
            if(m_GizmosMaterial == null)
            {
                 m_GizmosMaterial = new Material(Shader.Find("Unlit/FracturingColored"));
            }

            return m_GizmosMaterial;
        }

        set
        {
            m_GizmosMaterial = value;
        }
    }

    private Material m_GizmosMaterial = null;

    void Awake()
    {

    }

    void Start()
    {
        m_bCheckDetachNonSupportedChunkds = false;
        m_bExploded = false;
        m_bDetached = false;

        if(Application.isPlaying)
        {
            SetSingleMeshVisibility(true);

            if(StartStatic == false)
            {
                CheckDetachNonSupportedChunks(true);
            }

            m_afFreeChunkSoundTimers  = new float[Mathf.Max(0, EventDetachedMaxSounds)];
            m_afFreeChunkPrefabTimers = new float[Mathf.Max(0, EventDetachedMaxPrefabs)];
        }
    }

    void Update()
    {
        if(Application.isPlaying)
        {
            if(m_bCheckDetachNonSupportedChunkds)
            {
                CheckDetachNonSupportedChunksInternal();
                m_bCheckDetachNonSupportedChunkds = false;
            }

            if(m_afFreeChunkSoundTimers.Length != EventDetachedMaxSounds)
            {
                Array.Resize<float>(ref m_afFreeChunkSoundTimers, EventDetachedMaxSounds);
            }

            if(m_afFreeChunkPrefabTimers.Length != EventDetachedMaxPrefabs)
            {
                Array.Resize<float>(ref m_afFreeChunkPrefabTimers, EventDetachedMaxPrefabs);
            }

            for(int nSound = 0; nSound < m_afFreeChunkSoundTimers.Length; nSound++)
            {
                if(m_afFreeChunkSoundTimers[nSound] >= 0.0f)
                {
                    m_afFreeChunkSoundTimers[nSound] -= Time.deltaTime;
                }
            }

            for(int nPrefab = 0; nPrefab < m_afFreeChunkPrefabTimers.Length; nPrefab++)
            {
                if(m_afFreeChunkPrefabTimers[nPrefab] >= 0.0f)
                {
                    m_afFreeChunkPrefabTimers[nPrefab] -= Time.deltaTime;
                }
            }
        }
    }

    void OnRenderObject()
    {

#if UNITY_EDITOR

        if(ListSupportPlanes == null)
        {
            return;
        }

        bool bSelected = false;

        foreach(GameObject selection in UnityEditor.Selection.gameObjects)
        {
            if(selection == gameObject)
            {
                bSelected = true;
                break;
            }
        }

        if(bSelected == false)
        {
            return;
        }

        // Draw support plane on the editor window.
        // We use this approach instead of Handles.xxx because it integrates better (depth test with other objects, our own shader etc.).

        if(UnityEditor.SceneView.lastActiveSceneView)
        {
            if(Application.isPlaying == false && Camera.current == UnityEditor.SceneView.lastActiveSceneView.camera)
            {
                foreach(UltimateFracturing.SupportPlane supportPlane in ListSupportPlanes)
                {
                    if (supportPlane.GUIShowInScene && supportPlane.planeMesh != null)
                    {
                        GizmosMaterial.SetColor("_Color", GizmoColorSupport);
                        GizmosMaterial.SetPass(0);
                        Graphics.DrawMeshNow(supportPlane.planeMesh, transform.localToWorldMatrix * supportPlane.GetLocalMatrix());
                    }
                }
            }
        }
#endif
    }

    public void OnCreateFracturedObject()
    {
        m_bCheckDetachNonSupportedChunkds = false;
        m_bExploded = false;
        m_bDetached = false;

        IsUsingSingleMeshObjectDraw = true;

        if(Application.isPlaying)
        {
            if(StartStatic == false)
            {
                CheckDetachNonSupportedChunks(true);
            }

            m_afFreeChunkSoundTimers  = new float[Mathf.Max(0, EventDetachedMaxSounds)];
            m_afFreeChunkPrefabTimers = new float[Mathf.Max(0, EventDetachedMaxPrefabs)];
        }
    }

    public void SetSingleMeshVisibility(bool bEnabled)
    {
        if(SingleMeshObject == null)
        {
            return;
        }

        if(IsUsingSingleMeshObjectDraw == bEnabled)
        {
            return;
        }

        SingleMeshObject.GetComponent<Renderer>().enabled = bEnabled;

        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                chunk.GetComponent<Renderer>().enabled = !bEnabled;
            }
        }

        IsUsingSingleMeshObjectDraw = bEnabled;
    }

    public bool ResetChunks()
    {
        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("ResetChunks() call invalid: some chunks have already been deleted. Ignoring...");
#endif
                return false;
            }
        }

        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            chunk.ResetChunk(this);
        }

        if(StartStatic == false)
        {
            CheckDetachNonSupportedChunks(true);
        }

        return true;
    }

    public List<FracturedChunk> GetDestructibleChunksInRadius(Vector3 v3Position, float fRadius, bool bAlsoIncludeFreeChunks)
    {
        List<FracturedChunk> listResult = new List<FracturedChunk>();

        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                bool bValid = bAlsoIncludeFreeChunks ? true : chunk.IsDetachedChunk == false;

                if(bValid && chunk.IsDestructibleChunk())
                {
                    if(Vector3.Distance(chunk.transform.position, v3Position) < fRadius)
                    {
                        listResult.Add(chunk);
                    }
                }
            }
        }

        return listResult;
    }

    public void Explode(Vector3 v3ExplosionPosition, float fExplosionForce)
    {
        // Explodes all chunks

        if(m_bExploded == true)
        {
            return;
        }

        // Play sound

        if(EventExplosionSound)
        {
            AudioSource.PlayClipAtPoint(EventExplosionSound, v3ExplosionPosition);
        }

        // Instance prefabs on random positions

        if(EventExplosionPrefabsArray.Length > 0 && EventExplosionPrefabsInstanceCount > 0 && ListFracturedChunks.Count > 0)
        {
            for(int i = 0; i < EventExplosionPrefabsInstanceCount; i++)
            {
                FracturedObject.PrefabInfo prefab = EventExplosionPrefabsArray[UnityEngine.Random.Range(0, EventExplosionPrefabsArray.Length)];

                if(prefab != null)
                {
                    FracturedChunk chunkRandom = null;

                    while(chunkRandom == null)
                    {
                        chunkRandom = ListFracturedChunks[UnityEngine.Random.Range(0, ListFracturedChunks.Count)];
                    }

                    GameObject newGameObject = Instantiate(prefab.GameObject, chunkRandom.transform.position, prefab.GameObject.transform.rotation) as GameObject;

                    if(Mathf.Approximately(prefab.MinLifeTime, 0.0f) == false || Mathf.Approximately(prefab.MaxLifeTime, 0.0f) == false)
                    {
                        DieTimer timer = newGameObject.AddComponent<DieTimer>();
                        timer.SecondsToDie = UnityEngine.Random.Range(prefab.MinLifeTime, prefab.MaxLifeTime);
                    }
                }
            }
        }

        // Explode chunks

        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                chunk.ListAdjacentChunks.Clear();

                if(chunk.IsDestructibleChunk() && chunk.IsDetachedChunk == false)
                {
                    chunk.DetachFromObject(false);
                    chunk.GetComponent<Rigidbody>().AddExplosionForce(fExplosionForce, v3ExplosionPosition, 0.0f, 0.0f);
                }
            }
        }

        m_bExploded = true;
    }

    public void Explode(Vector3 v3ExplosionPosition, float fExplosionForce, float fRadius, bool bPlayExplosionSound, bool bInstanceExplosionPrefabs, bool bAlsoExplodeFree, bool bCheckStructureIntegrityAfter)
    {
        // Explodes only chunks in a given radius

        List<FracturedChunk> listChunksAffected = new List<FracturedChunk>();

        // Play sound

        if(EventExplosionSound && bPlayExplosionSound)
        {
            AudioSource.PlayClipAtPoint(EventExplosionSound, v3ExplosionPosition);
        }

        // Explode chunks in range

        foreach(FracturedChunk chunk in GetDestructibleChunksInRadius(v3ExplosionPosition, fRadius, bAlsoExplodeFree))
        {
            if(chunk)
            {
                chunk.DetachFromObject(false);
                chunk.GetComponent<Rigidbody>().AddExplosionForce(fExplosionForce, v3ExplosionPosition, 0.0f, 0.0f);
                listChunksAffected.Add(chunk);
            }
        }

        // Instance prefabs on random positions

        if(bInstanceExplosionPrefabs && EventExplosionPrefabsArray.Length > 0 && EventExplosionPrefabsInstanceCount > 0 && listChunksAffected.Count > 0)
        {
            for(int i = 0; i < Mathf.Max(EventExplosionPrefabsInstanceCount, listChunksAffected.Count); i++)
            {
                FracturedObject.PrefabInfo prefab = EventExplosionPrefabsArray[UnityEngine.Random.Range(0, EventExplosionPrefabsArray.Length)];

                if(prefab != null)
                {
                    GameObject newGameObject = Instantiate(prefab.GameObject, listChunksAffected[UnityEngine.Random.Range(0, listChunksAffected.Count)].transform.position, prefab.GameObject.transform.rotation) as GameObject;

                    if(Mathf.Approximately(prefab.MinLifeTime, 0.0f) == false || Mathf.Approximately(prefab.MaxLifeTime, 0.0f) == false)
                    {
                        DieTimer timer = newGameObject.AddComponent<DieTimer>();
                        timer.SecondsToDie = UnityEngine.Random.Range(prefab.MinLifeTime, prefab.MaxLifeTime);
                    }
                }
            }
        }

        if(bCheckStructureIntegrityAfter)
        {
            CheckDetachNonSupportedChunks();
        }
    }

    public void DeleteChunks()
    {
        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                UnityEngine.Object.DestroyImmediate(chunk.gameObject);
            }
        }

        ListFracturedChunks.Clear();

        if(SingleMeshObject)
        {
            UnityEngine.Object.DestroyImmediate(SingleMeshObject);
        }
    }

    public void CollapseChunks()
    {
        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                chunk.ListAdjacentChunks.Clear();

                if(chunk.IsDestructibleChunk() && chunk.IsDetachedChunk == false)
                {
                    chunk.DetachFromObject(false);
                }
            }
        }
    }

    public void ExplodeChunks(float fExplosionForce, Vector3 v3ExplosionPosition, float fUpwardsModifier)
    {
        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                chunk.ListAdjacentChunks.Clear();

                if(chunk.IsDestructibleChunk() && chunk.IsDetachedChunk == false)
                {
                    chunk.DetachFromObject(false);
                    chunk.GetComponent<Rigidbody>().AddExplosionForce(fExplosionForce, v3ExplosionPosition, 0.0f, fUpwardsModifier);
                }
            }
        }
    }

    public void ComputeChunksRelativeVolume()
    {
        float fTotalVolume = 0.0f;

        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();

                if(meshFilter)
                {
                    fTotalVolume += meshFilter.sharedMesh.bounds.size.x * meshFilter.sharedMesh.bounds.size.y * meshFilter.sharedMesh.bounds.size.z;
                }
            }
        }

        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();

                if(meshFilter)
                {
                    float fChunkVolume = meshFilter.sharedMesh.bounds.size.x * meshFilter.sharedMesh.bounds.size.y * meshFilter.sharedMesh.bounds.size.z;
                    chunk.RelativeVolume = fChunkVolume / fTotalVolume;
                    chunk.Volume         = fChunkVolume;
                }
            }
        }
    }

    public void ComputeChunksMass(float fTotalMass)
    {
        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                if(chunk.GetComponent<Rigidbody>())
                {
                    float fMass = fTotalMass * chunk.RelativeVolume;
                    if(fMass < 0.001f) fMass = 0.001f;
                    chunk.GetComponent<Rigidbody>().mass = fMass;
                }
            }
        }
    }

    public bool HasDetachedChunks()
    {
        return m_bDetached;
    }

    public void NotifyChunkDetach(FracturedChunk chunk)
    {
        m_bDetached = true;
        SetSingleMeshVisibility(false);

        if(EventDetachedAnyCallMethod.Length > 0 && EventDetachedAnyCallGameObject != null)
        {
            EventDetachedAnyCallGameObject.SendMessage(EventDetachedAnyCallMethod);
        }
    }

    public void NotifyDetachChunkCollision(FracturedChunk.CollisionInfo collisionInfo)
    {
        // This one will be called internally from FracturedChunk from within a collision

        if(EventDetachCollisionCallGameObject != null && EventDetachCollisionCallMethod.Length > 0)
        {
            EventDetachCollisionCallGameObject.SendMessage(EventDetachCollisionCallMethod, collisionInfo);
        }

        if(collisionInfo.bCancelCollisionEvent == false)
        {
            NotifyDetachChunkCollision(collisionInfo.collisionInfo.contacts[0].point, collisionInfo.bIsMain);
        }
    }

    public void NotifyDetachChunkCollision(Vector3 v3Position, bool bIsMain)
    {
        if(EventDetachSound != null && bIsMain)
        {
            AudioSource.PlayClipAtPoint(EventDetachSound, v3Position);
        }

        if(EventDetachPrefabsArray != null && bIsMain)
        {
            if(EventDetachPrefabsArray.Length > 0)
            {
                PrefabInfo prefab = EventDetachPrefabsArray[UnityEngine.Random.Range(0, EventDetachPrefabsArray.Length)];
                GameObject newGameObject = Instantiate(prefab.GameObject, v3Position, prefab.GameObject.transform.rotation) as GameObject;

                if(Mathf.Approximately(prefab.MinLifeTime, 0.0f) == false || Mathf.Approximately(prefab.MaxLifeTime, 0.0f) == false)
                {
                    DieTimer timer = newGameObject.AddComponent<DieTimer>();
                    timer.SecondsToDie = UnityEngine.Random.Range(prefab.MinLifeTime, prefab.MaxLifeTime);
                }
            }
        }
    }

    public void NotifyFreeChunkCollision(FracturedChunk.CollisionInfo collisionInfo)
    {
        if(EventDetachedCollisionCallGameObject != null && EventDetachedCollisionCallMethod.Length > 0)
        {
            EventDetachedCollisionCallGameObject.SendMessage(EventDetachedCollisionCallMethod, collisionInfo);
        }

        if(collisionInfo.bCancelCollisionEvent == false)
        {
            if(EventDetachedSoundArray.Length > 0)
            {
                int nFreeSound = -1;

                for(int nSound = 0; nSound < m_afFreeChunkSoundTimers.Length; nSound++)
                {
                    if(m_afFreeChunkSoundTimers[nSound] <= 0.0f)
                    {
                        nFreeSound = nSound;
                        break;
                    }
                }

                if(nFreeSound != -1)
                {
                    AudioClip clip = EventDetachedSoundArray[UnityEngine.Random.Range(0, EventDetachedSoundArray.Length)];

                    if(clip != null)
                    {
                        AudioSource.PlayClipAtPoint(clip, collisionInfo.collisionInfo.contacts[0].point);
                    }

                    m_afFreeChunkSoundTimers[nFreeSound] = clip.length;
                }
            }

            if(EventDetachedPrefabsArray.Length > 0)
            {
                FracturedObject.PrefabInfo prefab = EventDetachedPrefabsArray[UnityEngine.Random.Range(0, EventDetachedPrefabsArray.Length)];

                if(prefab != null)
                {
                    int nFreePrefab = -1;

                    for(int nPrefab = 0; nPrefab < m_afFreeChunkPrefabTimers.Length; nPrefab++)
                    {
                        if(m_afFreeChunkPrefabTimers[nPrefab] <= 0.0f)
                        {
                            nFreePrefab = nPrefab;
                            break;
                        }
                    }

                    if(nFreePrefab != -1)
                    {
                        GameObject newGameObject = Instantiate(prefab.GameObject, collisionInfo.collisionInfo.contacts[0].point, prefab.GameObject.transform.rotation) as GameObject;

                        if(Mathf.Approximately(prefab.MinLifeTime, 0.0f) == false || Mathf.Approximately(prefab.MaxLifeTime, 0.0f) == false)
                        {
                            DieTimer timer = newGameObject.AddComponent<DieTimer>();
                            timer.SecondsToDie = UnityEngine.Random.Range(prefab.MinLifeTime, prefab.MaxLifeTime);

                            m_afFreeChunkPrefabTimers[nFreePrefab] = timer.SecondsToDie;
                        }
                        else
                        {
                            m_afFreeChunkPrefabTimers[nFreePrefab] = float.MaxValue;
                        }
                    }
                }
            }
        }
    }

    public void NotifyImpact(Vector3 v3Position)
    {
        if(EventImpactSound != null)
        {
            AudioSource.PlayClipAtPoint(EventImpactSound, v3Position);
        }

        if(EventImpactPrefabsArray != null)
        {
            if(EventImpactPrefabsArray.Length > 0)
            {
                PrefabInfo prefab = EventImpactPrefabsArray[UnityEngine.Random.Range(0, EventImpactPrefabsArray.Length)];
                GameObject newGameObject = Instantiate(prefab.GameObject, v3Position, prefab.GameObject.transform.rotation) as GameObject;

                if(Mathf.Approximately(prefab.MinLifeTime, 0.0f) == false || Mathf.Approximately(prefab.MaxLifeTime, 0.0f) == false)
                {
                    DieTimer timer = newGameObject.AddComponent<DieTimer>();
                    timer.SecondsToDie = UnityEngine.Random.Range(prefab.MinLifeTime, prefab.MaxLifeTime);
                }
            }
        }
    }

    public void ResetAllChunkVisitedFlags()
    {
        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                chunk.Visited = false;
            }
        }
    }

    public void CheckDetachNonSupportedChunks(bool bForceImmediate = false)
    {
        if(bForceImmediate)
        {
            CheckDetachNonSupportedChunksInternal();
        }
        else
        {
            m_bCheckDetachNonSupportedChunkds = true;
        }
    }

    public void MarkNonSupportedChunks()
    {
        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                chunk.IsNonSupportedChunk = false;
            }
        }

        CheckDetachNonSupportedChunksInternal(true);
    }

    private void CheckDetachNonSupportedChunksInternal(bool bOnlyMarkThem = false)
    {
        if(GenerateChunkConnectionInfo == false)
        {
            return;
        }

        List<FracturedChunk> listNodesToProcess = new List<FracturedChunk>();

        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                if(chunk.IsDetachedChunk == false && chunk.IsSupportChunk == false)
                {
                    listNodesToProcess.Add(chunk);
                }
            }
        }

        ResetAllChunkVisitedFlags();

        while(listNodesToProcess.Count > 0)
        {
            // Be sure not to process a free chunk (can be as a result of a previous iteration)

            while(listNodesToProcess.Count > 0)
            {
                if(listNodesToProcess[0].IsDetachedChunk)
                {
                    listNodesToProcess[0].IsNonSupportedChunk = true;
                    listNodesToProcess.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            if(listNodesToProcess.Count == 0)
            {
                break;
            }

            // Get interconnected group

            List<FracturedChunk> listChunksVisited = new List<FracturedChunk>();
            List<FracturedChunk> listChunksSupport = new List<FracturedChunk>();

            bool bAreSupportedGroup = AreSupportedChunksRecursive(listNodesToProcess[0], listChunksVisited, listChunksSupport);

            foreach(FracturedChunk chunkVisited in listChunksVisited)
            {
                listNodesToProcess.Remove(chunkVisited);
            }
         
            if(bAreSupportedGroup == false)
            {
                foreach(FracturedChunk chunkVisited in listChunksVisited)
                {
                    if(bOnlyMarkThem == false)
                    {
                        // If not connected to any support node, then detach
                        chunkVisited.DetachFromObject(false);
                    }
                    else
                    {
                        chunkVisited.IsNonSupportedChunk = true;
                    }
                }                
            }
            else if(ChunkHorizontalRadiusSupportStrength < 10e15)
            {
                // Connected to support nodes, but let's check if the horizontal distance is less than the max. permitted distance
                foreach(FracturedChunk chunkVisited in listChunksVisited)
                {
                    if(chunkVisited.IsSupportChunk == false)
                    {
                        float fMinRadius = float.MaxValue;

                        foreach(FracturedChunk chunkSupport in listChunksSupport)
                        {
                            if(chunkVisited != chunkSupport)
                            {
                                Vector3 v3Horizontal = chunkVisited.transform.position - chunkSupport.transform.position;
                                v3Horizontal = new Vector3(v3Horizontal.x, 0.0f, v3Horizontal.z);

                                if(v3Horizontal.magnitude < fMinRadius)
                                {
                                    fMinRadius = v3Horizontal.magnitude;
                                }
                            }
                        }

                        if(fMinRadius > ChunkHorizontalRadiusSupportStrength)
                        {
                            if(bOnlyMarkThem == false)
                            {
                                // Detach because it's too far horizontally from the nearest support chunk
                                chunkVisited.DetachFromObject(false);
                            }
                            else
                            {
                                chunkVisited.IsNonSupportedChunk = true;
                            }
                        }
                    }
                }                
            }
        }
    }

    private static bool AreSupportedChunksRecursive(FracturedChunk chunk, List<FracturedChunk> listChunksVisited, List<FracturedChunk> listChunksSupport)
    {
        if(chunk.Visited)
        {
            return false;
        }

        chunk.Visited = true;
        listChunksVisited.Add(chunk);

        if(chunk.IsSupportChunk)
        {
            listChunksSupport.Add(chunk);
        }

        bool bAnyConnectedIsSupport = false;

        foreach(FracturedChunk.AdjacencyInfo adjacencyInfo in chunk.ListAdjacentChunks)
        {
            if(adjacencyInfo.chunk)
            {
                if(adjacencyInfo.chunk.FracturedObjectSource)
                {
                    if(adjacencyInfo.fArea >= adjacencyInfo.chunk.FracturedObjectSource.ChunkConnectionMinArea)
                    {
                        if(AreSupportedChunksRecursive(adjacencyInfo.chunk, listChunksVisited, listChunksSupport))
                        {
                            bAnyConnectedIsSupport = true;
                        }
                    }
                }
            }
        }

        return chunk.IsSupportChunk || bAnyConnectedIsSupport;
    }

    public void AddSupportPlane()
    {
        if(ListSupportPlanes == null)
        {
            ListSupportPlanes = new List<UltimateFracturing.SupportPlane>();
        }

        ListSupportPlanes.Add(new UltimateFracturing.SupportPlane(this));
    }

    public void ComputeSupportPlaneIntersections()
    {
        foreach(FracturedChunk chunk in ListFracturedChunks)
        {
            if(chunk)
            {
                chunk.IsSupportChunk = false;

                foreach(UltimateFracturing.SupportPlane supportPlane in ListSupportPlanes)
                {
                    if(supportPlane.IntersectsWith(chunk.gameObject, true))
                    {
                        chunk.IsSupportChunk = true;
                    }
                }
            }
        }
    }
}
