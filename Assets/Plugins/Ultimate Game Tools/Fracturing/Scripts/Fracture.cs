using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace UltimateFracturing
{
    public static partial class Fracturer
    {
        private class FracturingStats
        {
            public FracturingStats()
            {
                nChunkCount       = 0;
                nTotalChunks      = 0;
                nSplitCount       = 0;

                bCancelFracturing = false;
            }

            public int  nChunkCount;
            public int  nTotalChunks;
            public int  nSplitCount;

            public bool bCancelFracturing;
        }

        private class VoronoiCell
        {
            public class Face
            {
                public Face(Plane plane, Matrix4x4 mtxPlane, int nAdjacentCell)
                {
                    this.plane         = plane;
                    this.mtxPlane      = mtxPlane;
                    this.nAdjacentCell = nAdjacentCell;
                }

                public Plane     plane;
                public Matrix4x4 mtxPlane;
                public int       nAdjacentCell;
            }

            public VoronoiCell(int nIndex, int x, int y, int z)
            {
                this.nIndex = nIndex;

                this.x = x;
                this.y = y;
                this.z = z;

                v3Center      = Vector3.zero;
                listCellFaces = new List<Face>();
            }

            public int        nIndex;
            public int        x;
            public int        y;
            public int        z;
            public Vector3    v3Center;
            public Vector3    v3Min;
            public Vector3    v3Max;
            public List<Face> listCellFaces;
        }

        private class VoronoiPointDistance
        {
            public VoronoiPointDistance(int nIndex, float fDistanceSqr)
            {
                this.nIndex       = nIndex;
                this.fDistanceSqr = fDistanceSqr;
            }

            public class IncreasingDistanceComparer : IComparer<VoronoiPointDistance>
            {
                public int Compare(VoronoiPointDistance a, VoronoiPointDistance b)
                {
                    return a.fDistanceSqr - b.fDistanceSqr < 0.0f ? -1 : 1;
                }
            }

            public int   nIndex;
            public float fDistanceSqr;
        }

        public struct VoronoiCellKey
        {
            public int x;
            public int y;
            public int z;

            public VoronoiCellKey(int x, int y, int z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public class EqualityComparer : IEqualityComparer<VoronoiCellKey>
            {
                public bool Equals(VoronoiCellKey x, VoronoiCellKey y)
                {
                    if(x.x == y.x && x.y == y.y && x.z == y.z)
                    {
                        return true;
                    }

                    return false;
                }

                public int GetHashCode(VoronoiCellKey x)
                {
                    return x.x.GetHashCode() + x.y.GetHashCode() + x.z.GetHashCode();
                }
            }
        }

        private class VoronoiThreadData
        {
            public VoronoiThreadData()
            {
                listVoronoiCells    = new List<VoronoiCell>();
                listMeshDatasChunks = new List<MeshData>();
            }

            public List<VoronoiCell> listVoronoiCells;
            public MeshData          meshDataCube;
            public List<MeshData>    listMeshDatasObject;
            public List<MeshData>    listMeshDatasChunks;
            public SpaceTreeNode     spaceTree;
            public FracturedObject   fracturedComponent;
            public int               nCurrentCell;
            public int               nTotalCells;
            public int               nCellsProcessed;
            public ProgressDelegate  progress;
        }

        private static FracturingStats   s_FracturingStats   = new FracturingStats();
        private static VoronoiThreadData s_VoronoiThreadData = new VoronoiThreadData();

        public delegate void ProgressDelegate(string strTitle, string message, float fT);

        public static void CancelFracturing()
        {
            lock(s_FracturingStats)
            {
                s_FracturingStats.bCancelFracturing = true;
            }
        }

        public static bool IsFracturingCancelled()
        {
            bool bCancelled = false;

            lock(s_FracturingStats)
            {
                bCancelled = s_FracturingStats.bCancelFracturing;
            }

            return bCancelled;
        }

        public static bool FractureToChunks(FracturedObject fracturedComponent, bool bPositionOnSourceAndHideOriginal, out List<GameObject> listGameObjectsOut, ProgressDelegate progress = null)
        {
            listGameObjectsOut = new List<GameObject>();
            bool bFracturingOK = false;

            GameObject gameObjectIn = fracturedComponent.SourceObject;
            MeshFilter meshfIn      = null;
            Mesh       newMesh      = null;

            if(gameObjectIn)
            {
                meshfIn = fracturedComponent.SourceObject.GetComponent<MeshFilter>();

                if(meshfIn)
                {
                    newMesh = CopyMesh(meshfIn);
                    newMesh.name = string.Format("mesh_{0}{1}", fracturedComponent.SourceObject.gameObject.name, fracturedComponent.SourceObject.gameObject.GetInstanceID().ToString());

                    //Debug.Log("In: " + gameObjectIn.name + " (Layer " + LayerMask.LayerToName(gameObjectIn.layer) + " "  + gameObjectIn.layer + ")" + ": " + meshfIn.sharedMesh.subMeshCount + " submesh(es), " + ": " + (meshfIn.sharedMesh.triangles.Length / 3) + " triangles, " + meshfIn.sharedMesh.vertexCount + " vertices, " + (meshfIn.sharedMesh.normals != null ? meshfIn.sharedMesh.normals.Length : 0) + " normals, " + (meshfIn.sharedMesh.tangents != null ? meshfIn.sharedMesh.tangents.Length : 0) + " tangents, " + (meshfIn.sharedMesh.colors != null ? meshfIn.sharedMesh.colors.Length : 0) + " colors, " + (meshfIn.sharedMesh.colors32 != null ? meshfIn.sharedMesh.colors32.Length : 0) + " colors32, " + (meshfIn.sharedMesh.uv != null ? meshfIn.sharedMesh.uv.Length : 0) + " uv1, " + (meshfIn.sharedMesh.uv2 != null ? meshfIn.sharedMesh.uv2.Length : 0) + " uv2");
                }
            }

            if(fracturedComponent.FracturePattern == FracturedObject.EFracturePattern.BSP)
            {
                bFracturingOK = FractureToChunksBSP(fracturedComponent, bPositionOnSourceAndHideOriginal, out listGameObjectsOut, progress);
            }
            else if(fracturedComponent.FracturePattern == FracturedObject.EFracturePattern.Voronoi)
            {
                bFracturingOK = FractureToChunksVoronoi(fracturedComponent, bPositionOnSourceAndHideOriginal, out listGameObjectsOut, progress);
            }

            if(fracturedComponent.SingleMeshObject != null)
            {
                GameObject.DestroyImmediate(fracturedComponent.SingleMeshObject);
            }

            if(bFracturingOK && Fracturer.IsFracturingCancelled() == false)
            {
                fracturedComponent.OnCreateFracturedObject();
                fracturedComponent.SingleMeshObject = GameObject.Instantiate(fracturedComponent.SourceObject) as GameObject;
                fracturedComponent.SingleMeshObject.name = "@" + fracturedComponent.name + " (single mesh)";
                fracturedComponent.SingleMeshObject.transform.localPosition = fracturedComponent.transform.position;
                fracturedComponent.SingleMeshObject.transform.localRotation = fracturedComponent.transform.rotation;
                fracturedComponent.SingleMeshObject.transform.localScale    = fracturedComponent.SourceObject.transform.localScale;
                fracturedComponent.SingleMeshObject.transform.parent        = fracturedComponent.transform;

                Component[] aComponents = fracturedComponent.SingleMeshObject.GetComponents<Component>();

                for(int i = 0; i < aComponents.Length; i++)
                {
                    Component c = aComponents[i];

                    if(c.GetType() != typeof(Transform) && c.GetType() != typeof(MeshRenderer) && c.GetType() != typeof(MeshFilter))
                    {
                        Component.DestroyImmediate(c);
                    }
                }

                MeshFilter singleMeshFilter = fracturedComponent.SingleMeshObject.GetComponent<MeshFilter>();
                singleMeshFilter.sharedMesh = newMesh;

#if UNITY_3_5
                fracturedComponent.SingleMeshObject.SetActiveRecursively(true);
#else
                fracturedComponent.SingleMeshObject.SetActive(true);
#endif

/*
#if UNITY_EDITOR && !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
                UnityEditor.Undo.RegisterCreatedObjectUndo(fracturedComponent.SingleMeshObject, "Created " + fracturedComponent.SingleMeshObject.name);

                foreach(GameObject chunk in listGameObjectsOut)
                {
                    UnityEditor.Undo.RegisterCreatedObjectUndo(chunk, "Created " + chunk.name);
                }
#endif
*/
            }

            return bFracturingOK;
        }

        private static Mesh CopyMesh(MeshFilter meshfIn)
        {
            Mesh newMesh = new Mesh();

            Vector3[] av3Vertices = meshfIn.sharedMesh.vertices;
            Vector3[] av3Normals  = meshfIn.sharedMesh.normals;
            Vector4[] av4Tangents = meshfIn.sharedMesh.tangents;
            Vector2[] av2Mapping1 = meshfIn.sharedMesh.uv;
            Vector2[] av2Mapping2 = meshfIn.sharedMesh.uv2;
            Color[]   acolColors  = meshfIn.sharedMesh.colors;
            Color32[] aColors32   = meshfIn.sharedMesh.colors32;

            if(av3Vertices != null)
            {
                if(av3Vertices.Length > 0)
                {
                    Vector3[] av3NewVertices = new Vector3[av3Vertices.Length];
                    av3Vertices.CopyTo(av3NewVertices, 0);
                    newMesh.vertices = av3NewVertices;
                }
            }

            if(av3Normals != null)
            {
                if(av3Normals.Length > 0)
                {
                    Vector3[] av3NewNormals = new Vector3[av3Normals.Length];
                    av3Normals.CopyTo(av3NewNormals, 0);
                    newMesh.normals = av3NewNormals;
                }
            }

            if(av4Tangents != null)
            {
                if(av4Tangents.Length > 0)
                {
                    Vector4[] av4NewTangents = new Vector4[av4Tangents.Length];
                    av4Tangents.CopyTo(av4NewTangents, 0);
                    newMesh.tangents = av4NewTangents;
                }
            }

            if(av2Mapping1 != null)
            {
                if(av2Mapping1.Length > 0)
                {
                    Vector2[] av2NewMapping1 = new Vector2[av2Mapping1.Length];
                    av2Mapping1.CopyTo(av2NewMapping1, 0);
                    newMesh.uv = av2NewMapping1;

                }
            }

            if(av2Mapping2 != null)
            {
                if(av2Mapping2.Length > 0)
                {
                    Vector2[] av2NewMapping2 = new Vector2[av2Mapping2.Length];
                    av2Mapping2.CopyTo(av2NewMapping2, 0);
                    newMesh.uv2 = av2NewMapping2;
                }
            }

            if(acolColors != null)
            {
                if(acolColors.Length > 0)
                {
                    Color[] acolNewColors = new Color[acolColors.Length];
                    acolColors.CopyTo(acolNewColors, 0);
                    newMesh.colors = acolNewColors;
                }
            }

            if(aColors32 != null)
            {
                if(aColors32.Length > 0)
                {
                    Color32[] aNewColors32 = new Color32[aColors32.Length];
                    aColors32.CopyTo(aNewColors32, 0);
                    newMesh.colors32 = aNewColors32;
                }
            }

            newMesh.subMeshCount = meshfIn.sharedMesh.subMeshCount;

            for(int nSubMesh = 0; nSubMesh < meshfIn.sharedMesh.subMeshCount; nSubMesh++)
            {
                int[] anTriangles = meshfIn.sharedMesh.GetTriangles(nSubMesh);
                int[] anNewTriangles = new int[anTriangles.Length];
                anTriangles.CopyTo(anNewTriangles, 0);

                newMesh.SetTriangles(anNewTriangles, nSubMesh);
            }

            return newMesh;
        }

        private static bool FractureToChunksBSP(FracturedObject fracturedComponent, bool bPositionOnSourceAndHideOriginal, out List<GameObject> listGameObjectsOut, ProgressDelegate progress = null)
        {
            listGameObjectsOut = new List<GameObject>();

            MeshFilter meshfIn = fracturedComponent.SourceObject.GetComponent<MeshFilter>();

            if(meshfIn == null)
            {
                return false;
            }

            s_FracturingStats = new FracturingStats();
            s_FracturingStats.nTotalChunks = fracturedComponent.GenerateNumChunks;

            if(progress != null)
            {
                progress("Fracturing", "Initializing...", 0.0f);
            }

            foreach(FracturedChunk chunk in fracturedComponent.ListFracturedChunks)
            {
                if(chunk != null)
                {
                    UnityEngine.Object.DestroyImmediate(chunk.gameObject);
                }
            }

            fracturedComponent.ListFracturedChunks.Clear();
            fracturedComponent.DecomposeRadius = (meshfIn.sharedMesh.bounds.max - meshfIn.sharedMesh.bounds.min).magnitude;
            Random.seed = fracturedComponent.RandomSeed;

            // Check if the input object already has been split, to get its split closing submesh

            FracturedChunk fracturedChunk = fracturedComponent.gameObject.GetComponent<FracturedChunk>();

            int nSplitCloseSubMesh = fracturedChunk != null ? fracturedChunk.SplitSubMeshIndex : -1;

            if(nSplitCloseSubMesh == -1 && fracturedComponent.SourceObject.GetComponent<Renderer>())
            {
                // Check if its material is the same as the split material

                if(fracturedComponent.SourceObject.GetComponent<Renderer>().sharedMaterial == fracturedComponent.SplitMaterial)
                {
                    nSplitCloseSubMesh = 0;
                }
            }

            SplitOptions splitOptions      = SplitOptions.Default;
            splitOptions.bVerticesAreLocal = false;

            Vector3    v3OriginalPos = fracturedComponent.transform.position;
            Quaternion qOriginalRot  = fracturedComponent.transform.rotation;

            // We'll do the splitting in (0, 0, 0) in order to improve precision
            Vector3 v3OriginalSourcePos = fracturedComponent.SourceObject.transform.position;
            fracturedComponent.SourceObject.transform.position = Vector3.zero;

            fracturedComponent.transform.position = fracturedComponent.SourceObject.transform.position;
            fracturedComponent.transform.rotation = fracturedComponent.SourceObject.transform.rotation;

            Material[] aMaterials = fracturedComponent.SourceObject.GetComponent<Renderer>() ? fracturedComponent.SourceObject.GetComponent<Renderer>().sharedMaterials : null;
            MeshData meshDataIn = new MeshData(meshfIn.transform, meshfIn.sharedMesh, aMaterials, fracturedComponent.SourceObject.transform.localToWorldMatrix, true, nSplitCloseSubMesh, true);

            // Fracture the object

            Queue<MeshData> queueMeshDatas  = new Queue<MeshData>();  // For depth order traversal, we are targeting a number of generated chunks
            Queue<int>      queueLevels     = new Queue<int>();       // Here we store the depth level of each meshData

            if(fracturedComponent.GenerateIslands)
            {
                CombinedMesh combinedMesh = fracturedComponent.SourceObject.GetComponent<CombinedMesh>();

                if(combinedMesh != null)
                {
                    // Does the mesh come from an combined mesh? -> separate objects and detect mesh islands

                    combinedMesh.TransformObjInfoMeshVectorsToLocal(fracturedComponent.SourceObject.transform.transform);

                    List<MeshData> listMeshDatasCombined = new List<MeshData>();

                    for(int nObject = 0; nObject < combinedMesh.GetObjectCount(); nObject++)
                    {
                        CombinedMesh.ObjectInfo objectMeshInfo = combinedMesh.GetObjectInfo(nObject);

                        MeshData compositeObjectMesh = new MeshData(meshfIn.transform, objectMeshInfo.mesh, objectMeshInfo.aMaterials, fracturedComponent.transform.localToWorldMatrix * objectMeshInfo.mtxLocal, true, -1, true);

                        List<MeshData> listIslands = ComputeMeshDataIslands(compositeObjectMesh, false, fracturedComponent, progress);

                        foreach(MeshData island in listIslands)
                        {
                            queueMeshDatas.Enqueue(island);
                            queueLevels.Enqueue(0);

                            listMeshDatasCombined.Add(island);
                        }
                    }

                    if(fracturedComponent.GenerateChunkConnectionInfo)
                    {
                        for(int i = 0; i < listMeshDatasCombined.Count; i++)
                        {
                            if(progress != null)
                            {
                                progress("Fracturing", "Processing combined object chunks connectivity...", i / (float)listMeshDatasCombined.Count);
                                if(Fracturer.IsFracturingCancelled()) return false;
                            }

                            for(int j = 0; j < listMeshDatasCombined.Count; j++)
                            {
                                if(i != j)
                                {
                                    ComputeIslandsMeshDataConnectivity(fracturedComponent, false, listMeshDatasCombined[i], listMeshDatasCombined[j]);
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<MeshData> listIslands = ComputeMeshDataIslands(meshDataIn, false, fracturedComponent, progress);

                    foreach(MeshData island in listIslands)
                    {
                        queueMeshDatas.Enqueue(island);
                        queueLevels.Enqueue(0);
                    }
                }
            }
            else
            {
                queueMeshDatas.Enqueue(meshDataIn);
                queueLevels.Enqueue(0);
            }

            s_FracturingStats.nChunkCount = 1;

            bool bEqualSize = fracturedComponent.GenerateIslands; // Generate chunks equally sized no matter the size of the islands detected

            while(queueMeshDatas.Count < s_FracturingStats.nTotalChunks)
            {
                if(IsFracturingCancelled())
                {
                    break;
                }

                MeshData meshDataCurrent = queueMeshDatas.Dequeue();
                int      nDepthCurrent   = queueLevels.Dequeue();

                if(progress != null)
                {
                    progress("Fracturing", string.Format("Computing chunk {0}/{1} (Depth {2})", s_FracturingStats.nChunkCount + 1, s_FracturingStats.nTotalChunks, bEqualSize ? ": size ordered traversal" : nDepthCurrent.ToString()), Mathf.Clamp01((float)s_FracturingStats.nChunkCount / (float)s_FracturingStats.nTotalChunks));
                }

                List<MeshData> listMeshDataPos;
                List<MeshData> listMeshDataNeg;

                int nSplitAxis = -1;

                Matrix4x4 planeMtx = GetRandomPlaneSplitMatrix(meshDataCurrent, fracturedComponent, out nSplitAxis);

                if(SplitMeshUsingPlane(meshDataCurrent, fracturedComponent, splitOptions, planeMtx.MultiplyVector(Vector3.up), planeMtx.MultiplyVector(Vector3.right), planeMtx.MultiplyPoint3x4(Vector3.zero), out listMeshDataPos, out listMeshDataNeg, progress) == true)
                {
                    s_FracturingStats.nSplitCount++;

                    foreach(MeshData meshDataPos in listMeshDataPos) queueMeshDatas.Enqueue(meshDataPos); queueLevels.Enqueue(nDepthCurrent + 1);
                    foreach(MeshData meshDataNeg in listMeshDataNeg) queueMeshDatas.Enqueue(meshDataNeg); queueLevels.Enqueue(nDepthCurrent + 1);
                }

                if(bEqualSize)
                {
                    // If we want to have equally sized objects mainly because of small islands preprocessing detection, then order the results by size for the next iterations

                    List<MeshData> listSizeOrderedMeshDatas = new List<MeshData>();

                    while(queueMeshDatas.Count > 0)
                    {
                        listSizeOrderedMeshDatas.Add(queueMeshDatas.Dequeue());
                    }

                    listSizeOrderedMeshDatas.Sort(new MeshData.DecreasingSizeComparer(nSplitAxis));

                    foreach(MeshData meshDataOrdered in listSizeOrderedMeshDatas)
                    {
                        queueMeshDatas.Enqueue(meshDataOrdered);
                    }
                }

                s_FracturingStats.nChunkCount = queueMeshDatas.Count;
            }

            MeshData[] aMeshDataOut = queueMeshDatas.ToArray();

            // Set the mesh properties and add objects to list

            if(IsFracturingCancelled() == false)
            {
                for(int nMeshCount = 0; nMeshCount < aMeshDataOut.Length; nMeshCount++)
                {
                    // Create new game object

                    GameObject newGameObject = CreateNewSplitGameObject(fracturedComponent.SourceObject, fracturedComponent, fracturedComponent.SourceObject.name + (nMeshCount + 1), true, aMeshDataOut[nMeshCount]);
                    newGameObject.AddComponent<Rigidbody>();
                    newGameObject.GetComponent<Rigidbody>().isKinematic = true;
                    listGameObjectsOut.Add(newGameObject);
                }

                if(fracturedComponent.GenerateChunkConnectionInfo)
                {
                    ComputeChunkConnections(fracturedComponent, listGameObjectsOut, new List<MeshData>(aMeshDataOut), progress);
                }

                fracturedComponent.ComputeChunksRelativeVolume();
                fracturedComponent.ComputeChunksMass(fracturedComponent.TotalMass);
                fracturedComponent.ComputeSupportPlaneIntersections();
            }

            // Compute the colliders if necessary

            if(fracturedComponent.AlwaysComputeColliders)
            {
                ComputeChunkColliders(fracturedComponent, progress);
            }

            bool bCancelled = IsFracturingCancelled();

            // Reposition and hide original?

            fracturedComponent.SourceObject.transform.position = v3OriginalSourcePos;

            if(bPositionOnSourceAndHideOriginal)
            {
                fracturedComponent.gameObject.transform.position = fracturedComponent.SourceObject.transform.position;
                fracturedComponent.gameObject.transform.rotation = fracturedComponent.SourceObject.transform.rotation;
                
#if UNITY_3_5
                fracturedComponent.SourceObject.SetActiveRecursively(false);
#else
                fracturedComponent.SourceObject.SetActive(false);
#endif
            }
            else
            {
                fracturedComponent.transform.position = v3OriginalPos;
                fracturedComponent.transform.rotation = qOriginalRot;
            }

            return bCancelled == false;
        }

        private static bool FractureToChunksVoronoi(FracturedObject fracturedComponent, bool bPositionOnSourceAndHideOriginal, out List<GameObject> listGameObjectsOut, ProgressDelegate progress = null)
        {
            listGameObjectsOut = new List<GameObject>();

            MeshFilter meshfIn = fracturedComponent.SourceObject.GetComponent<MeshFilter>();

            if(meshfIn == null)
            {
                return false;
            }

            int nTotalCellsX = Mathf.Max(1, fracturedComponent.VoronoiCellsXCount);
            int nTotalCellsY = Mathf.Max(1, fracturedComponent.VoronoiCellsYCount);
            int nTotalCellsZ = Mathf.Max(1, fracturedComponent.VoronoiCellsZCount);

            s_FracturingStats = new FracturingStats();
            int nTotalCells = nTotalCellsX * nTotalCellsY * nTotalCellsZ;

            s_VoronoiThreadData = new VoronoiThreadData();

            if(progress != null)
            {
                progress("Fracturing", "Initializing...", 0.0f);
            }

            foreach(FracturedChunk chunk in fracturedComponent.ListFracturedChunks)
            {
                if(chunk != null)
                {
                    UnityEngine.Object.DestroyImmediate(chunk.gameObject);
                }
            }

            fracturedComponent.ListFracturedChunks.Clear();
            fracturedComponent.DecomposeRadius = (meshfIn.sharedMesh.bounds.max - meshfIn.sharedMesh.bounds.min).magnitude;
            Random.seed = fracturedComponent.RandomSeed;

            // Check if the input object already has been split, to get its split closing submesh

            FracturedChunk fracturedChunk = fracturedComponent.gameObject.GetComponent<FracturedChunk>();

            int nSplitCloseSubMesh = fracturedChunk != null ? fracturedChunk.SplitSubMeshIndex : -1;

            if(nSplitCloseSubMesh == -1 && fracturedComponent.SourceObject.GetComponent<Renderer>())
            {
                // Check if its material is the same as the split material

                if(fracturedComponent.SourceObject.GetComponent<Renderer>().sharedMaterial == fracturedComponent.SplitMaterial)
                {
                    nSplitCloseSubMesh = 0;
                }
            }

            // Mesh data will be in local coordinates

            Material[] aMaterials = fracturedComponent.SourceObject.GetComponent<Renderer>() ? fracturedComponent.SourceObject.GetComponent<Renderer>().sharedMaterials : null;

            MeshData meshDataIn = new MeshData(meshfIn.transform, meshfIn.sharedMesh, aMaterials, fracturedComponent.SourceObject.transform.localToWorldMatrix, false, nSplitCloseSubMesh, true);

            // Precompute space volumes with mesh datas for each volume to speed up cell generation

            SpaceTreeNode spaceTree = fracturedComponent.VoronoiVolumeOptimization ? SpaceTreeNode.BuildSpaceTree(meshDataIn, 8, fracturedComponent, progress) : null;

            // Create the cells and planes

            List<VoronoiCell> listVoronoiCells = new List<VoronoiCell>();
            Dictionary<VoronoiCellKey, VoronoiCell> dicPos2Cell = new Dictionary<VoronoiCellKey,VoronoiCell>();
            VoronoiCellKey cellKey = new VoronoiCellKey();

            float fMinX = meshDataIn.v3Min.x;
            float fMinY = meshDataIn.v3Min.y;
            float fMinZ = meshDataIn.v3Min.z;
            float fMaxX = meshDataIn.v3Max.x;
            float fMaxY = meshDataIn.v3Max.y;
            float fMaxZ = meshDataIn.v3Max.z;

            float fSizeX = (fMaxX - fMinX) / nTotalCellsX;
            float fSizeY = (fMaxY - fMinY) / nTotalCellsY;
            float fSizeZ = (fMaxZ - fMinZ) / nTotalCellsZ;

            // We assume cells in this radius will affect the cell in the center

            int nCellInfluenceRadius = fracturedComponent.VoronoiProximityOptimization ? 2 : 20;

            for(int nCellX = 0; nCellX < nTotalCellsX; nCellX++)
            {
                for(int nCellY = 0; nCellY < nTotalCellsY; nCellY++)
                {
                    for(int nCellZ = 0; nCellZ < nTotalCellsZ; nCellZ++)
                    {
                        float fCenterX = fMinX + (fSizeX * 0.5f) + (fSizeX * nCellX);
                        float fCenterY = fMinY + (fSizeY * 0.5f) + (fSizeY * nCellY);
                        float fCenterZ = fMinZ + (fSizeZ * 0.5f) + (fSizeZ * nCellZ);

                        fCenterX += UnityEngine.Random.Range(-1.0f, 1.0f) * fSizeX * 0.5f * 0.99f * fracturedComponent.VoronoiCellsXSizeVariation;
                        fCenterY += UnityEngine.Random.Range(-1.0f, 1.0f) * fSizeY * 0.5f * 0.99f * fracturedComponent.VoronoiCellsYSizeVariation;
                        fCenterZ += UnityEngine.Random.Range(-1.0f, 1.0f) * fSizeZ * 0.5f * 0.99f * fracturedComponent.VoronoiCellsZSizeVariation;

                        VoronoiCell newCell = new VoronoiCell(listVoronoiCells.Count, nCellX, nCellY, nCellZ);
                        newCell.v3Center = new Vector3(fCenterX, fCenterY, fCenterZ);

                        // We'll compute the v3Min and v3Max later, but this are rough values.

                        float fRoughInfluenceRadius = nCellInfluenceRadius + (nCellInfluenceRadius * 0.5f) + 0.01f;

                        newCell.v3Min = newCell.v3Center - new Vector3(fSizeX * fRoughInfluenceRadius, fSizeY * fRoughInfluenceRadius, fSizeZ * fRoughInfluenceRadius);
                        newCell.v3Max = newCell.v3Center + new Vector3(fSizeX * fRoughInfluenceRadius, fSizeY * fRoughInfluenceRadius, fSizeZ * fRoughInfluenceRadius);

                        listVoronoiCells.Add(newCell);
                        dicPos2Cell.Add(new VoronoiCellKey(nCellX, nCellY, nCellZ), newCell);
                    }
                }
            }

            List<VoronoiPointDistance> listPointDistances = new List<VoronoiPointDistance>();

            for(int nCell = 0; nCell < listVoronoiCells.Count; nCell++)
            {
                if(progress != null)
                {
                    progress("Fracturing", string.Format("Creating cell planes... (cell {0}/{1}) ", nCell + 1, listVoronoiCells.Count), (float)nCell / (float)listVoronoiCells.Count);
                }

                if(IsFracturingCancelled())
                {
                    break;
                }

                Vector3 v3CellCenter = listVoronoiCells[nCell].v3Center;

                listPointDistances.Clear();
                listPointDistances.Capacity = Mathf.RoundToInt(Mathf.Pow(nCellInfluenceRadius * 2 + 1, 3)) + 1;

                // Build separation planes

                for(int x = -nCellInfluenceRadius; x <= nCellInfluenceRadius; x++)
                {
                    for(int y = -nCellInfluenceRadius; y <= nCellInfluenceRadius; y++)
                    {
                        for(int z = -nCellInfluenceRadius; z <= nCellInfluenceRadius; z++)
                        {
                            if(x == 0 && y == 0 && z == 0)
                            {
                                continue;
                            }

                            int nCellX = listVoronoiCells[nCell].x + x;
                            int nCellY = listVoronoiCells[nCell].y + y;
                            int nCellZ = listVoronoiCells[nCell].z + z;

                            if(nCellX < 0 || nCellX >= nTotalCellsX) continue;
                            if(nCellY < 0 || nCellY >= nTotalCellsY) continue;
                            if(nCellZ < 0 || nCellZ >= nTotalCellsZ) continue;

                            cellKey.x = nCellX;
                            cellKey.y = nCellY;
                            cellKey.z = nCellZ;

                            VoronoiCell otherCell = dicPos2Cell[cellKey];
                            listPointDistances.Add(new VoronoiPointDistance(otherCell.nIndex, (v3CellCenter - otherCell.v3Center).sqrMagnitude));
                        }
                    }
                }

                listPointDistances.Sort(new VoronoiPointDistance.IncreasingDistanceComparer());

                foreach(VoronoiPointDistance pointDistance in listPointDistances)
                {
                    int nCellSeparation = pointDistance.nIndex;

                    Vector3 v3NewPlaneNormal = (v3CellCenter - listVoronoiCells[nCellSeparation].v3Center).normalized;
                    Vector3 v3NewPlanePoint  = (v3CellCenter + listVoronoiCells[nCellSeparation].v3Center) * 0.5f;

                    Plane newCellPlane = new Plane(v3NewPlaneNormal, v3NewPlanePoint);

                    Vector3 v3Forward = Vector3.forward;

                    float fMagX = Mathf.Abs(v3NewPlaneNormal.x);
                    float fMagY = Mathf.Abs(v3NewPlaneNormal.y);
                    float fMagZ = Mathf.Abs(v3NewPlaneNormal.z);

                    if(fMagX <= fMagY && fMagX <= fMagZ) v3Forward = Vector3.Cross(v3NewPlaneNormal, Vector3.right);
                    if(fMagY <= fMagX && fMagY <= fMagZ) v3Forward = Vector3.Cross(v3NewPlaneNormal, Vector3.up);
                    if(fMagZ <= fMagX && fMagZ <= fMagY) v3Forward = Vector3.Cross(v3NewPlaneNormal, Vector3.forward);

                    Quaternion qPlaneRot = Quaternion.LookRotation(v3Forward, v3NewPlaneNormal);
                    Matrix4x4  mtxPlane  = Matrix4x4.TRS(-v3NewPlaneNormal * newCellPlane.distance, qPlaneRot, Vector3.one);

                    listVoronoiCells[nCell].listCellFaces.Add(new VoronoiCell.Face(newCellPlane, mtxPlane, nCellSeparation));
                }
            }

            // Fracture a dummy cube and see which planes affect each cell

            MeshData meshDataCube = MeshData.CreateBoxMeshData(Vector3.zero, Quaternion.identity, Vector3.one, meshDataIn.v3Min, meshDataIn.v3Max);

            Thread[] threads = fracturedComponent.VoronoiMultithreading ? new Thread[UnityEngine.SystemInfo.processorCount] : new Thread[1];

            s_VoronoiThreadData.listVoronoiCells    = listVoronoiCells;
            s_VoronoiThreadData.meshDataCube        = meshDataCube;
            s_VoronoiThreadData.listMeshDatasObject = null;
            s_VoronoiThreadData.spaceTree           = spaceTree;
            s_VoronoiThreadData.fracturedComponent  = fracturedComponent;
            s_VoronoiThreadData.nCurrentCell        = 0;
            s_VoronoiThreadData.nTotalCells         = nTotalCells;
            s_VoronoiThreadData.nCellsProcessed     = 0;
            s_VoronoiThreadData.progress            = progress;

            for(int nThread = 0; nThread < threads.Length; nThread++) 
            {
                threads[nThread] = new Thread(new ThreadStart(ThreadVoronoiComputePlaneDependencies));
                threads[nThread].Start();
            }

            int nLastCell = -1;

//          float fStartTime = Time.realtimeSinceStartup;

            while(true)
            {
                if(IsFracturingCancelled())
                {
                    break;
                }

                int nCurrentCell = 0;

                lock(typeof(VoronoiThreadData))
                {
                    nCurrentCell = s_VoronoiThreadData.nCurrentCell;
                }

                if(nCurrentCell != nLastCell)
                {
                    if(s_VoronoiThreadData.progress != null)
                    {
                        s_VoronoiThreadData.progress("Fracturing", string.Format("Finding cell plane dependencies... (cell {0}/{1}) ", nCurrentCell + 1, nTotalCells), (float)nCurrentCell / (float)nTotalCells);
                    }

                    nLastCell = nCurrentCell;
                }

                lock(typeof(VoronoiThreadData))
                {
                    if(s_VoronoiThreadData.nCellsProcessed == nTotalCells)
                    {
                        break;
                    }
                }

                Thread.Sleep(0);
            }

//          float fEndTime = Time.realtimeSinceStartup;
//          Debug.Log("Multithread time (" + threads.Length + " threads) = " + (fEndTime - fStartTime) + " seconds");

            // Fracture the object

            List<MeshData> listMeshDataOut = new List<MeshData>();
            List<MeshData> listMeshDatasIn = new List<MeshData>();

            if(IsFracturingCancelled() == false)
            {
                if(fracturedComponent.GenerateIslands)
                {
                    CombinedMesh combinedMesh = fracturedComponent.SourceObject.GetComponent<CombinedMesh>();

                    if(combinedMesh != null)
                    {
                        // Does the mesh come from an combined mesh? -> separate objects and detect mesh islands

                        combinedMesh.TransformObjInfoMeshVectorsToLocal(fracturedComponent.transform);

                        for(int nObject = 0; nObject < combinedMesh.GetObjectCount(); nObject++)
                        {
                            CombinedMesh.ObjectInfo objectMeshInfo = combinedMesh.GetObjectInfo(nObject);

                            MeshData compositeObjectMesh = new MeshData(meshfIn.transform, objectMeshInfo.mesh, objectMeshInfo.aMaterials, fracturedComponent.transform.localToWorldMatrix * objectMeshInfo.mtxLocal, true, -1, true);

                            List<MeshData> listIslands = ComputeMeshDataIslands(compositeObjectMesh, true, fracturedComponent, progress);

                            foreach(MeshData island in listIslands)
                            {
                                listMeshDatasIn.Add(island);
                            }
                        }

                        if(fracturedComponent.GenerateChunkConnectionInfo)
                        {
                            for(int i = 0; i < listMeshDatasIn.Count; i++)
                            {
                                if(progress != null)
                                {
                                    progress("Fracturing", "Processing combined object chunks connectivity...", i / (float)listMeshDatasIn.Count);
                                    if(Fracturer.IsFracturingCancelled()) return false;
                                }

                                for(int j = 0; j < listMeshDatasIn.Count; j++)
                                {
                                    if(i != j)
                                    {
                                        ComputeIslandsMeshDataConnectivity(fracturedComponent, true, listMeshDatasIn[i], listMeshDatasIn[j]);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        listMeshDatasIn = ComputeMeshDataIslands(meshDataIn, true, fracturedComponent, progress);
                    }
                }
                else
                {
                    listMeshDatasIn.Add(meshDataIn);
                }

                if(listVoronoiCells.Count == 1)
                {
                    listMeshDataOut.AddRange(listMeshDatasIn);
                }
                else
                {
                    s_VoronoiThreadData.listMeshDatasObject = listMeshDatasIn;
                    s_VoronoiThreadData.fracturedComponent  = fracturedComponent;
                    s_VoronoiThreadData.nCurrentCell        = 0;
                    s_VoronoiThreadData.nCellsProcessed     = 0;

                    for(int nThread = 0; nThread < threads.Length; nThread++) 
                    {
                        threads[nThread] = new Thread(new ThreadStart(ThreadVoronoiComputeCells));
                        threads[nThread].Start();
                    }

                    nLastCell = -1;

//                  fStartTime = Time.realtimeSinceStartup;

                    while(true)
                    {
                        if(IsFracturingCancelled())
                        {
                            break;
                        }

                        int nCurrentCell = 0;

                        lock(typeof(VoronoiThreadData))
                        {
                            nCurrentCell = s_VoronoiThreadData.nCurrentCell;
                        }

                        //if(nCurrentCell != nLastCell)
                        {
                            if(s_VoronoiThreadData.progress != null)
                            {
                                s_VoronoiThreadData.progress("Fracturing", string.Format("Computing cell {0}/{1}", nCurrentCell, nTotalCells), Mathf.Clamp01((float)nCurrentCell / (float)nTotalCells));
                            }

                            nLastCell = nCurrentCell;
                        }

                        lock(typeof(VoronoiThreadData))
                        {
                            if(s_VoronoiThreadData.nCellsProcessed == nTotalCells)
                            {
                                break;
                            }
                        }

                        Thread.Sleep(0);
                    }

//                  fEndTime = Time.realtimeSinceStartup;

//                  Debug.Log("Multithread time cells (" + threads.Length + " threads) = " + (fEndTime - fStartTime) + " seconds");

                    listMeshDataOut.AddRange(s_VoronoiThreadData.listMeshDatasChunks);
                }
            }

            // Set the mesh properties and add objects to list

            if(IsFracturingCancelled() == false)
            {
                if(fracturedComponent.Verbose)
                {
                    Debug.Log(string.Format("Computed {0} slices for {1} chunks (Average: {2}).", s_FracturingStats.nSplitCount, listMeshDataOut.Count, (float)s_FracturingStats.nSplitCount / (float)listMeshDataOut.Count));
                }

                if(listMeshDataOut.Count > 0)
                {
                    for(int nMeshCount = 0; nMeshCount < listMeshDataOut.Count; nMeshCount++)
                    {
                        // Create new game object and have into account we need to transform from old local space to new chunk local space

                        listMeshDataOut[nMeshCount].v3Position = Vector3.Scale(listMeshDataOut[nMeshCount].v3Position, meshfIn.transform.localScale);

                        for(int v = 0; v < listMeshDataOut[nMeshCount].aVertexData.Length; v++)
                        {
                            Vector3 v3Vertex = listMeshDataOut[nMeshCount].aVertexData[v].v3Vertex;
                            v3Vertex  = Vector3.Scale(v3Vertex, meshfIn.transform.localScale);
                            v3Vertex -= listMeshDataOut[nMeshCount].v3Position;
                            listMeshDataOut[nMeshCount].aVertexData[v].v3Vertex = v3Vertex;
                        }

                        listMeshDataOut[nMeshCount].v3Position = fracturedComponent.transform.TransformPoint(listMeshDataOut[nMeshCount].v3Position);
                        listMeshDataOut[nMeshCount].qRotation  = fracturedComponent.transform.rotation;
                        listMeshDataOut[nMeshCount].v3Scale    = fracturedComponent.transform.localScale;
                        GameObject newGameObject = CreateNewSplitGameObject(fracturedComponent.SourceObject, fracturedComponent, fracturedComponent.SourceObject.name + (nMeshCount + 1), false, listMeshDataOut[nMeshCount]);
                        newGameObject.AddComponent<Rigidbody>();
                        newGameObject.GetComponent<Rigidbody>().isKinematic = true;
                        listGameObjectsOut.Add(newGameObject);

                        // Now that the gameobject has vertices in local coordinates, transform vertices from meshdatas to world space for connection computation later

                        for(int v = 0; v < listMeshDataOut[nMeshCount].aVertexData.Length; v++)
                        {
                            Vector3 v3Vertex = listMeshDataOut[nMeshCount].aVertexData[v].v3Vertex;
                            v3Vertex = newGameObject.transform.TransformPoint(v3Vertex);
                            listMeshDataOut[nMeshCount].aVertexData[v].v3Vertex = v3Vertex;
                        }
                    }
                }

                if(fracturedComponent.GenerateChunkConnectionInfo)
                {
                    ComputeChunkConnections(fracturedComponent, listGameObjectsOut, listMeshDataOut, progress);
                }

                fracturedComponent.ComputeChunksRelativeVolume();
                fracturedComponent.ComputeChunksMass(fracturedComponent.TotalMass);
                fracturedComponent.ComputeSupportPlaneIntersections();
            }

            // Compute the colliders if necessary

            if(fracturedComponent.AlwaysComputeColliders && IsFracturingCancelled() == false)
            {
                ComputeChunkColliders(fracturedComponent, progress);
            }

            bool bCancelled = IsFracturingCancelled();

            // Reposition and hide original?

            if(bPositionOnSourceAndHideOriginal)
            {
                fracturedComponent.gameObject.transform.position = fracturedComponent.SourceObject.transform.position;
                fracturedComponent.gameObject.transform.rotation = fracturedComponent.SourceObject.transform.rotation;
#if UNITY_3_5
                fracturedComponent.SourceObject.SetActiveRecursively(false);
#else
                fracturedComponent.SourceObject.SetActive(false);
#endif

            }

            return bCancelled == false;
        }

        public static void ThreadVoronoiComputePlaneDependencies()
        {
            SplitOptions splitOptionsCube = new SplitOptions();

            splitOptionsCube.bForceNoIslandGeneration     = true;
            splitOptionsCube.bForceNoChunkConnectionInfo  = true;            
            splitOptionsCube.bForceNoIslandConnectionInfo = true;
            splitOptionsCube.bForceNoCap                  = false;
            splitOptionsCube.bForceCapVertexSoup          = true;
            splitOptionsCube.bVerticesAreLocal            = true;

            while(true)
            {
                int nCell;

                lock(typeof(VoronoiThreadData))
                {
                    if(s_VoronoiThreadData.nCurrentCell >= s_VoronoiThreadData.nTotalCells)
                    {
                        break;
                    }

                    nCell = s_VoronoiThreadData.nCurrentCell;
                    s_VoronoiThreadData.nCurrentCell++;
                }

                if(IsFracturingCancelled())
                {
                    break;
                }

                if(s_VoronoiThreadData.spaceTree != null)
                {
                    List<MeshData> volumeMeshDatas = SpaceTreeNode.GetSmallestPossibleMeshData(s_VoronoiThreadData.spaceTree, s_VoronoiThreadData.listVoronoiCells[nCell].v3Min, s_VoronoiThreadData.listVoronoiCells[nCell].v3Max);

                    if(volumeMeshDatas.Count == 0)
                    {
                        s_VoronoiThreadData.listVoronoiCells[nCell].listCellFaces.Clear();

                        lock(typeof(VoronoiThreadData))
                        {
                            s_VoronoiThreadData.nCellsProcessed++;
                        }

                        continue;
                    }
                }

                List<MeshData> listMeshDataPos;
                List<MeshData> listMeshDataNeg;
                List<MeshData> listMeshDataCell = new List<MeshData>();

                listMeshDataCell.Add(s_VoronoiThreadData.meshDataCube.GetDeepCopy());

                for(int nFace = 0; nFace < s_VoronoiThreadData.listVoronoiCells[nCell].listCellFaces.Count; nFace++)
                {
                    VoronoiCell.Face cellFace = s_VoronoiThreadData.listVoronoiCells[nCell].listCellFaces[nFace];

                    List<MeshData> listMeshDataIteration = new List<MeshData>();

                    bool bCutSomething = false;

                    foreach(MeshData meshDataCell in listMeshDataCell)
                    {
                        if(SplitMeshUsingPlane(meshDataCell, s_VoronoiThreadData.fracturedComponent, splitOptionsCube, cellFace.mtxPlane.MultiplyVector(Vector3.up), cellFace.mtxPlane.MultiplyVector(Vector3.right), cellFace.mtxPlane.MultiplyPoint3x4(Vector3.zero), out listMeshDataPos, out listMeshDataNeg, s_VoronoiThreadData.progress) == true)
                        {
                            listMeshDataIteration.AddRange(listMeshDataPos);

                            if(listMeshDataNeg.Count > 0)
                            {
                                bCutSomething = true;
                            }
                        }
                    }

                    listMeshDataCell = listMeshDataIteration;

                    if(bCutSomething == false)
                    {
                        // Didn't cut anything, remove this face
                        s_VoronoiThreadData.listVoronoiCells[nCell].listCellFaces.RemoveAt(nFace);
                        nFace--;
                    }

                    if(listMeshDataCell.Count == 0)
                    {
                        // Empty cell
                        s_VoronoiThreadData.listVoronoiCells[nCell].listCellFaces.Clear();
                        break;
                    }
                }

                // Recompute cell bounding box min/max with all the cells that affect this one

                Vector3 v3Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 v3Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                foreach(MeshData meshDataCell in listMeshDataCell)
                {
                    if(meshDataCell.v3Min.x < v3Min.x) v3Min.x = meshDataCell.v3Min.x;
                    if(meshDataCell.v3Min.y < v3Min.y) v3Min.y = meshDataCell.v3Min.y;
                    if(meshDataCell.v3Min.z < v3Min.z) v3Min.z = meshDataCell.v3Min.z;

                    if(meshDataCell.v3Max.x > v3Max.x) v3Max.x = meshDataCell.v3Max.x;
                    if(meshDataCell.v3Max.y > v3Max.y) v3Max.y = meshDataCell.v3Max.y;
                    if(meshDataCell.v3Max.z > v3Max.z) v3Max.z = meshDataCell.v3Max.z;
                }

                s_VoronoiThreadData.listVoronoiCells[nCell].v3Min = v3Min;
                s_VoronoiThreadData.listVoronoiCells[nCell].v3Max = v3Max;

                lock(typeof(VoronoiThreadData))
                {
                    s_VoronoiThreadData.nCellsProcessed++;
                }
            }
        }

        public static void ThreadVoronoiComputeCells()
        {
            SplitOptions splitOptionsMesh = new SplitOptions();

            splitOptionsMesh.bForceNoProgressInfo     = true;
            splitOptionsMesh.bVerticesAreLocal        = true;
            splitOptionsMesh.bIgnoreNegativeSide      = true;
            splitOptionsMesh.bForceNoIslandGeneration = true;

            while(true)
            {
                int nCell;

                lock(typeof(VoronoiThreadData))
                {
                    if(s_VoronoiThreadData.nCurrentCell >= s_VoronoiThreadData.nTotalCells)
                    {
                        break;
                    }

                    nCell = s_VoronoiThreadData.nCurrentCell;
                    s_VoronoiThreadData.nCurrentCell++;
                }

                if(IsFracturingCancelled())
                {
                    break;
                }

                List<MeshData> listMeshDataPos;
                List<MeshData> listMeshDataNeg;
                List<MeshData> listMeshDataCell = new List<MeshData>();

                foreach(MeshData meshData in s_VoronoiThreadData.listMeshDatasObject)
                {
                    listMeshDataCell.Add(meshData);
                }

                if(s_VoronoiThreadData.spaceTree != null)
                {
                    listMeshDataCell = SpaceTreeNode.GetSmallestPossibleMeshData(s_VoronoiThreadData.spaceTree, s_VoronoiThreadData.listVoronoiCells[nCell].v3Min, s_VoronoiThreadData.listVoronoiCells[nCell].v3Max);

                    if(listMeshDataCell.Count == 0)
                    {
                        lock(typeof(VoronoiThreadData))
                        {
                            s_VoronoiThreadData.nCellsProcessed++;
                        }

                        continue;
                    }
                }

                for(int nFace = 0; nFace < s_VoronoiThreadData.listVoronoiCells[nCell].listCellFaces.Count; nFace++)
                {
                    VoronoiCell.Face cellFace = s_VoronoiThreadData.listVoronoiCells[nCell].listCellFaces[nFace];

                    List<MeshData> listMeshDataIteration = new List<MeshData>();

                    splitOptionsMesh.nForceMeshConnectivityHash = CreateMeshConnectivityVoronoiHash(nCell, s_VoronoiThreadData.listVoronoiCells[nCell].listCellFaces[nFace].nAdjacentCell);

                    foreach(MeshData meshDataCell in listMeshDataCell)
                    {
                        if(SplitMeshUsingPlane(meshDataCell, s_VoronoiThreadData.fracturedComponent, splitOptionsMesh, cellFace.mtxPlane.MultiplyVector(Vector3.up), cellFace.mtxPlane.MultiplyVector(Vector3.right), cellFace.mtxPlane.MultiplyPoint3x4(Vector3.zero), out listMeshDataPos, out listMeshDataNeg, s_VoronoiThreadData.progress) == true)
                        {
                            lock(s_FracturingStats)
                            {
                                s_FracturingStats.nSplitCount++;
                            }

                            listMeshDataIteration.AddRange(listMeshDataPos);
                        }
                    }

                    listMeshDataCell = listMeshDataIteration;

                    if(listMeshDataCell.Count == 0)
                    {
                        break;
                    }
                }

                lock(typeof(VoronoiThreadData))
                {
                    if(s_VoronoiThreadData.fracturedComponent.GenerateIslands)
                    {
                        // Should be only 0 or 1 elements in listMeshDataCell (we forced no island generation), but to be consistent with our implementation
                        foreach(MeshData meshData in listMeshDataCell)
                        {
                            s_VoronoiThreadData.listMeshDatasChunks.AddRange(ComputeMeshDataIslands(meshData, splitOptionsMesh.bVerticesAreLocal, s_VoronoiThreadData.fracturedComponent, null));
                       }
                    }
                    else
                    {
                        s_VoronoiThreadData.listMeshDatasChunks.AddRange(listMeshDataCell);
                    }
                }

                lock(typeof(VoronoiThreadData))
                {
                    s_VoronoiThreadData.nCellsProcessed++;
                }
            }
        }

        public static List<MeshData> ComputeMeshDataIslands(MeshData meshDataIn, bool bVerticesAreLocal, FracturedObject fracturedComponent, ProgressDelegate progress)
        {
            MeshFaceConnectivity faceConnectivity = new MeshFaceConnectivity();
            MeshDataConnectivity meshConnectivity = new MeshDataConnectivity();

            List<int>[]      aListIndices   = new List<int>[meshDataIn.aaIndices.Length];
            List<VertexData> listVertexData = new List<VertexData>();

            for(int nSubMesh = 0; nSubMesh < meshDataIn.nSubMeshCount; nSubMesh++)
            {
                aListIndices[nSubMesh] = new List<int>();

                for(int i = 0; i < meshDataIn.aaIndices[nSubMesh].Length / 3; i++)
                {
                    int nIndex1 = meshDataIn.aaIndices[nSubMesh][i * 3 + 0];
                    int nIndex2 = meshDataIn.aaIndices[nSubMesh][i * 3 + 1];
                    int nIndex3 = meshDataIn.aaIndices[nSubMesh][i * 3 + 2];

                    int nHashV1 = meshDataIn.aVertexData[nIndex1].nVertexHash;
                    int nHashV2 = meshDataIn.aVertexData[nIndex2].nVertexHash;
                    int nHashV3 = meshDataIn.aVertexData[nIndex3].nVertexHash;

                    Vector3 v1 = meshDataIn.aVertexData[nIndex1].v3Vertex;
                    Vector3 v2 = meshDataIn.aVertexData[nIndex2].v3Vertex;
                    Vector3 v3 = meshDataIn.aVertexData[nIndex3].v3Vertex;

                    aListIndices[nSubMesh].Add(nIndex1);
                    aListIndices[nSubMesh].Add(nIndex2);
                    aListIndices[nSubMesh].Add(nIndex3);

                    if(fracturedComponent.GenerateChunkConnectionInfo)
                    {
                        meshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, i);
                    }

                    faceConnectivity.AddEdge(nSubMesh, v1, v2, nHashV1, nHashV2, nIndex1, nIndex2);
                    faceConnectivity.AddEdge(nSubMesh, v2, v3, nHashV2, nHashV3, nIndex2, nIndex3);
                    faceConnectivity.AddEdge(nSubMesh, v3, v1, nHashV3, nHashV1, nIndex3, nIndex1);                
                }
            }

            listVertexData.AddRange(meshDataIn.aVertexData);

            meshDataIn.meshDataConnectivity = meshConnectivity;

            List<MeshData> listIslands = MeshData.PostProcessConnectivity(meshDataIn, faceConnectivity, meshConnectivity, aListIndices, listVertexData, meshDataIn.nSplitCloseSubMesh, meshDataIn.nCurrentVertexHash, false);

            if(fracturedComponent.GenerateChunkConnectionInfo)
            {
                for(int i = 0; i < listIslands.Count; i++)
                {
                    if(progress != null)
                    {
                        progress("Fracturing", "Processing initial island connectivity...", i / (float)listIslands.Count);
                        if(Fracturer.IsFracturingCancelled()) return new List<MeshData>();
                    }

                    for(int j = 0; j < listIslands.Count; j++)
                    {
                        if(i != j)
                        {
                            ComputeIslandsMeshDataConnectivity(fracturedComponent, bVerticesAreLocal, listIslands[i], listIslands[j]);
                        }
                    }
                }
            }

            return listIslands;
        }

        public static void ComputeChunkColliders(FracturedObject fracturedComponent, ProgressDelegate progress)
        {
            int nChunk = 0;
            int nTotalFaces = 0;

            s_FracturingStats = new FracturingStats();

            foreach(FracturedChunk fracturedChunk in fracturedComponent.ListFracturedChunks)
            {
                if(IsFracturingCancelled())
                {
                    break;
                }

                if(progress != null)
                {
                    progress("Computing colliders", string.Format("Collider {0}/{1}", nChunk + 1, fracturedComponent.ListFracturedChunks.Count), (float)nChunk / (float)fracturedComponent.ListFracturedChunks.Count);
                }

                if(fracturedChunk == null)
                {
                    continue;
                }

                if(fracturedChunk.GetComponent<Collider>() != null)
                {
                    MeshCollider meshCollider = fracturedChunk.GetComponent<MeshCollider>();

                    if(meshCollider)
                    {
                        meshCollider.sharedMesh = null;
                    }

                    if(fracturedChunk.GetComponent<Collider>())
                    {
                        Object.DestroyImmediate(fracturedChunk.GetComponent<Collider>());
                    }
                }

                if(fracturedChunk.GetComponent<Rigidbody>() != null)
                {
                    Object.DestroyImmediate(fracturedChunk.GetComponent<Rigidbody>());
                }

                while(fracturedChunk.transform.childCount > 0)
                {
                    // Destroy concave collider hulls
                    Object.DestroyImmediate(fracturedChunk.transform.GetChild(0).gameObject);
                }

                fracturedChunk.HasConcaveCollider = false;
                bool bColliderCreated = false;

                RemoveEmptySubmeshes(fracturedChunk);

                if(fracturedChunk.Volume > fracturedComponent.MinColliderVolumeForBox)
                {                     
                    if(fracturedComponent.IntegrateWithConcaveCollider)
                    {
                        int nTriangles = UltimateFracturing.ConcaveColliderInterface.ComputeHull(fracturedChunk.gameObject, fracturedComponent);

                        if(nTriangles > 0)
                        {
                            fracturedChunk.HasConcaveCollider = true;
                            bColliderCreated = true;

                            nTotalFaces += nTriangles;
                        }
                    }
                    else
                    {
                        MeshFilter meshFilter = fracturedChunk.GetComponent<MeshFilter>();

                        if(meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.vertexCount >= 3)
                        {
                            fracturedChunk.HasConcaveCollider = false;

                            MeshCollider newCollider = fracturedChunk.gameObject.AddComponent<MeshCollider>();

                            newCollider.convex = true;
                            bColliderCreated   = true;

                            if(newCollider.sharedMesh)
                            {
                                nTotalFaces += newCollider.sharedMesh.triangles.Length / 3;
                            }
                        }
                    }
                }

                if(bColliderCreated == false)
                {
                    fracturedChunk.gameObject.AddComponent<BoxCollider>();
                    nTotalFaces += 12;
                }

                if(fracturedChunk.GetComponent<Collider>())
                {
                    fracturedChunk.GetComponent<Collider>().material = fracturedComponent.ChunkPhysicMaterial;
                }

                fracturedChunk.gameObject.AddComponent<Rigidbody>();

                fracturedChunk.GetComponent<Rigidbody>().isKinematic = true;

                nChunk++;
            }

            if(IsFracturingCancelled() == false)
            {
                fracturedComponent.ComputeChunksMass(fracturedComponent.TotalMass);
            }

            if(fracturedComponent.Verbose && fracturedComponent.ListFracturedChunks.Count > 0)
            {
                Debug.Log("Total collider triangles: " + nTotalFaces + ". Average = " + (nTotalFaces / fracturedComponent.ListFracturedChunks.Count));
            }
        }

        public static void DeleteChunkColliders(FracturedObject fracturedComponent)
        {
            foreach(FracturedChunk fracturedChunk in fracturedComponent.ListFracturedChunks)
            {
                while(fracturedChunk.transform.childCount > 0)
                {
                    // Destroy concave collider hulls
                    Object.DestroyImmediate(fracturedChunk.transform.GetChild(0).gameObject);
                }

                if(fracturedChunk.GetComponent<Collider>() != null)
                {
                    Object.DestroyImmediate(fracturedChunk.GetComponent<Collider>());
                }

                if(fracturedChunk.GetComponent<Rigidbody>() != null)
                {
                    Object.DestroyImmediate(fracturedChunk.GetComponent<Rigidbody>());
                }
            }
        }

        private static Matrix4x4 GetRandomPlaneSplitMatrix(MeshData meshDataIn, FracturedObject fracturedComponent, out int nSplitAxisPerformed)
        {
            Vector3    v3RandomPosition = Vector3.zero;
            Quaternion qPlane           = Quaternion.identity;

            Vector3 v3Min = meshDataIn.v3Min - meshDataIn.v3Position;
            Vector3 v3Max = meshDataIn.v3Max - meshDataIn.v3Position;

            if(fracturedComponent.SplitsWorldSpace == false)
            {
                // Compute min/max values in local space :(

                v3Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                v3Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                Matrix4x4 mtxTransformVertices = Matrix4x4.TRS(meshDataIn.v3Position, meshDataIn.qRotation, meshDataIn.v3Scale).inverse;

                for(int i = 0; i < meshDataIn.aVertexData.Length; i++)
                {
                    Vector3 v3Local = mtxTransformVertices.MultiplyPoint3x4(meshDataIn.aVertexData[i].v3Vertex);
                    if(v3Local.x < v3Min.x) v3Min.x = v3Local.x;
                    if(v3Local.y < v3Min.y) v3Min.y = v3Local.y;
                    if(v3Local.z < v3Min.z) v3Min.z = v3Local.z;
                    if(v3Local.x > v3Max.x) v3Max.x = v3Local.x;
                    if(v3Local.y > v3Max.y) v3Max.y = v3Local.y;
                    if(v3Local.z > v3Max.z) v3Max.z = v3Local.z;
                }
            }

            int nSplitAxis = -1;

            if(fracturedComponent.SplitRegularly)
            {
                float fSizeX = v3Max.x - v3Min.x;
                float fSizeY = v3Max.y - v3Min.y;
                float fSizeZ = v3Max.z - v3Min.z;

                if(fSizeX >= fSizeY && fSizeX >= fSizeZ)
                {
                    nSplitAxis = 0;
                }
                else if(fSizeY >= fSizeX && fSizeY >= fSizeZ)
                {
                    nSplitAxis = 1;
                }
                else
                {
                    nSplitAxis = 2;
                }
            }
            else
            {
                for(int i = 0; i < 3; i++)
                {
                    float fRandomAxis = Random.value;
                    bool  bSkipX      = Mathf.Approximately(fracturedComponent.SplitXProbability, 0.0f);
                    bool  bSkipY      = Mathf.Approximately(fracturedComponent.SplitYProbability, 0.0f);
                    bool  bSkipZ      = Mathf.Approximately(fracturedComponent.SplitZProbability, 0.0f);

                    if(bSkipX == false)
                    {
                        if(fRandomAxis <= fracturedComponent.SplitXProbability || (bSkipY && bSkipZ))
                        {
                            nSplitAxis = 0;
                        }
                    }

                    if(bSkipY == false && nSplitAxis == -1)
                    {
                        float fSplitInYProbabilityAccum = fracturedComponent.SplitXProbability + fracturedComponent.SplitYProbability;

                        if(fRandomAxis <= fSplitInYProbabilityAccum || bSkipZ == true)
                        {
                            nSplitAxis = 1;
                        }
                    }

                    if(nSplitAxis == -1)
                    {
                        nSplitAxis = 2;
                    }
                }
            }

            nSplitAxisPerformed = nSplitAxis;

            float fRadius   = 0.0f;
            float fAngleMax = 45.0f;

            if(nSplitAxis == 0)
            {
                fRadius = (v3Max.x - v3Min.x) * 0.5f;
                qPlane = Quaternion.LookRotation(-Vector3.up, Vector3.right) * Quaternion.Euler(new Vector3(0.0f, Random.Range(-fAngleMax, fAngleMax) * fracturedComponent.SplitXVariation, Random.Range(-fAngleMax, fAngleMax) * fracturedComponent.SplitXVariation));
            }
            else if(nSplitAxis == 1)
            {
                fRadius = (v3Max.y - v3Min.y) * 0.5f;
                qPlane  = Quaternion.Euler(new Vector3(Random.Range(-fAngleMax, fAngleMax) * fracturedComponent.SplitYVariation, 0.0f, Random.Range(-fAngleMax, fAngleMax) * fracturedComponent.SplitYVariation));
            }
            else if(nSplitAxis == 2)
            {
                fRadius = (v3Max.z - v3Min.z) * 0.5f;
                qPlane = Quaternion.LookRotation(-Vector3.up, Vector3.forward) * Quaternion.Euler(new Vector3(Random.Range(-fAngleMax, fAngleMax) * fracturedComponent.SplitZVariation, Random.Range(-fAngleMax, fAngleMax) * fracturedComponent.SplitZVariation, 0.0f));
            }

            fRadius = fRadius * fracturedComponent.SplitSizeVariation * 0.8f;

            v3RandomPosition = new Vector3(Random.Range(-1.0f, 1.0f) * fRadius, Random.Range(-1.0f, 1.0f) * fRadius, Random.Range(-1.0f, 1.0f) * fRadius);

            if(fracturedComponent.SplitsWorldSpace == false)
            {
                return Matrix4x4.TRS(v3RandomPosition + meshDataIn.v3Position, fracturedComponent.SourceObject.transform.rotation * qPlane, Vector3.one);
            }

            return Matrix4x4.TRS(v3RandomPosition + meshDataIn.v3Position, qPlane, Vector3.one);
        }

        private static GameObject CreateNewSplitGameObject(GameObject gameObjectIn, FracturedObject fracturedComponent, string strName, bool bTransformVerticesBackToLocal, MeshData meshData)
        {
            GameObject     newGameObject  = new GameObject(strName);
            MeshFilter     meshFilter     = newGameObject.AddComponent<MeshFilter>();
            FracturedChunk fracturedChunk = newGameObject.AddComponent<FracturedChunk>();

            fracturedChunk.transform.parent = fracturedComponent.transform;

            if(fracturedComponent.SourceObject)
            {
                newGameObject.layer = fracturedComponent.SourceObject.layer;
            }
            else
            {
                newGameObject.layer = fracturedComponent.gameObject.layer;
            }

            fracturedComponent.ListFracturedChunks.Add(fracturedChunk);

            meshData.FillMeshFilter(meshFilter, bTransformVerticesBackToLocal);
            fracturedChunk.SplitSubMeshIndex = meshData.nSplitCloseSubMesh;
            fracturedChunk.OnCreateFromFracturedObject(fracturedComponent, meshData.nSplitCloseSubMesh);

            newGameObject.AddComponent<MeshRenderer>();

            newGameObject.GetComponent<Renderer>().shadowCastingMode = gameObjectIn.GetComponent<Renderer>().shadowCastingMode;
            newGameObject.GetComponent<Renderer>().receiveShadows    = gameObjectIn.GetComponent<Renderer>().receiveShadows;
            newGameObject.GetComponent<Renderer>().enabled           = false;

            Material[] aMaterials = new Material[meshData.nSubMeshCount];
            meshData.aMaterials.CopyTo(aMaterials, 0);

            if(meshData.aMaterials.Length < meshData.nSubMeshCount)
            {
                // Add split material
                aMaterials[meshData.nSubMeshCount - 1] = fracturedComponent.SplitMaterial;
            }

            newGameObject.GetComponent<Renderer>().sharedMaterials = aMaterials;

            newGameObject.GetComponent<Renderer>().lightmapIndex       = gameObjectIn.GetComponent<Renderer>().lightmapIndex;
            newGameObject.GetComponent<Renderer>().lightmapScaleOffset = gameObjectIn.GetComponent<Renderer>().lightmapScaleOffset;
            newGameObject.GetComponent<Renderer>().useLightProbes      = gameObjectIn.GetComponent<Renderer>().useLightProbes;

//          Debug.Log("Out: " + newGameObject.name + ": " + meshFilter.sharedMesh.subMeshCount + " submesh(es), " + ": " + (meshFilter.sharedMesh.triangles.Length / 3) + " triangles, " + meshFilter.sharedMesh.vertexCount + " vertices, " + (meshFilter.sharedMesh.normals != null ? meshFilter.sharedMesh.normals.Length : 0) + " normals, " + (meshFilter.sharedMesh.tangents != null ? meshFilter.sharedMesh.tangents.Length : 0) + " tangents, " + (meshFilter.sharedMesh.colors != null ? meshFilter.sharedMesh.colors.Length : 0) + " colors, " + (meshFilter.sharedMesh.colors32 != null ? meshFilter.sharedMesh.colors32.Length : 0) + " colors32, " + (meshFilter.sharedMesh.uv != null ? meshFilter.sharedMesh.uv.Length : 0) + " uv1, " + (meshFilter.sharedMesh.uv2 != null ? meshFilter.sharedMesh.uv2.Length : 0) + " uv2");

            return newGameObject;
        }

        private static int CreateMeshConnectivityVoronoiHash(int nCell1, int nCell2)
        {
            int nMax = Mathf.Max(nCell1, nCell2) + 256;
            int nMin = Mathf.Min(nCell1, nCell2) + 256;

            return (nMax << 16) | nMin;
        }

        private static void ComputeChunkConnections(FracturedObject fracturedObject, List<GameObject> listGameObjects, List<MeshData> listMeshDatas, ProgressDelegate progress = null)
        {
            for(int i = 0; i < listGameObjects.Count; i++)
            {
                if(progress != null)
                {
                    progress("Fracturing", "Computing connections...", i / (float)listGameObjects.Count);
                }

                if(IsFracturingCancelled())
                {
                    return;
                }

                FracturedChunk chunkA = listGameObjects[i].GetComponent<FracturedChunk>();
                List<FracturedChunk.AdjacencyInfo> listAdjacentChunks = new List<FracturedChunk.AdjacencyInfo>();

                for(int j = 0; j < listGameObjects.Count; j++)
                {
                    if(i == j) continue;

                    FracturedChunk chunkB = listGameObjects[j].GetComponent<FracturedChunk>();

                    float fSharedArea = 0.0f;
                    bool  bConnected  = listMeshDatas[i].GetSharedFacesArea(fracturedObject, listMeshDatas[j], out fSharedArea);
                    bool  bShared     = fSharedArea >= fracturedObject.ChunkConnectionMinArea;

                    if(Mathf.Approximately(fracturedObject.ChunkConnectionMinArea, 0.0f) && bConnected)
                    {
                        bShared = true;
                    }

                    if(bShared && bConnected)
                    {
                        listAdjacentChunks.Add(new FracturedChunk.AdjacencyInfo(chunkB, fSharedArea));
                    }
                }

                chunkA.ListAdjacentChunks = listAdjacentChunks;
            }
        }

        private static void RemoveEmptySubmeshes(FracturedChunk fracturedChunk)
        {
          MeshFilter meshFilter = fracturedChunk.GetComponent<MeshFilter>();

          if (meshFilter == null)
          {
            return;
          }

          Mesh mesh = meshFilter.sharedMesh;

          if (mesh.subMeshCount < 2)
          {
            return;
          }

          MeshRenderer meshRenderer = fracturedChunk.GetComponent<MeshRenderer>();

          List<Material> listMaterials = new List<Material>();
          List<int[]> listSubmeshes = new List<int[]>();

          int nSplitIndexSubtract = 0;

          for (int i = 0; i < mesh.subMeshCount; i++)
          {
            int[] indices = mesh.GetIndices(i);

            if (indices != null && indices.Length > 0)
            {
              listMaterials.Add(meshRenderer.sharedMaterials[i]);
              listSubmeshes.Add(indices);
            }
            else
            {
              if (i < fracturedChunk.SplitSubMeshIndex)
              {
                nSplitIndexSubtract--;
              }
            }
          }

          mesh.subMeshCount = listSubmeshes.Count;

          for (int i = 0; i < listSubmeshes.Count; i++)
          {
            mesh.SetTriangles(listSubmeshes[i], i);
            meshRenderer.sharedMaterials = listMaterials.ToArray();
          }

          fracturedChunk.SplitSubMeshIndex -= nSplitIndexSubtract;
          meshFilter.sharedMesh = mesh;
        }
    }
}