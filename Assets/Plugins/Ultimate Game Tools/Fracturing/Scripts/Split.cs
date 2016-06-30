using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UltimateFracturing
{
    public static partial class Fracturer
    {
        public class SplitOptions
        {
            public static SplitOptions Default = new SplitOptions();

            public SplitOptions()
            {
                bForceNoProgressInfo         = false;
                bForceNoIslandGeneration     = false;
                bForceNoChunkConnectionInfo  = false;
                bForceNoIslandConnectionInfo = false;
                bForceNoCap                  = false;
                bForceCapVertexSoup          = false;
                bIgnoreNegativeSide          = false;
                bVerticesAreLocal            = false;
                nForceMeshConnectivityHash   = -1;
            }

            public bool bForceNoProgressInfo;
            public bool bForceNoIslandGeneration;
            public bool bForceNoChunkConnectionInfo;
            public bool bForceNoIslandConnectionInfo;
            public bool bForceNoCap;
            public bool bForceCapVertexSoup;
            public bool bIgnoreNegativeSide;
            public bool bVerticesAreLocal;
            public int  nForceMeshConnectivityHash;
        }

        public static bool SplitMeshUsingPlane(GameObject gameObjectIn, FracturedObject fracturedComponent, SplitOptions splitOptions, Transform transformPlaneSplit, out List<GameObject> listGameObjectsPosOut, out List<GameObject> listGameObjectsNegOut, ProgressDelegate progress = null)
        {
            listGameObjectsPosOut = new List<GameObject>();
            listGameObjectsNegOut = new List<GameObject>();

            MeshFilter meshfIn = gameObjectIn.GetComponent<MeshFilter>();

            if(meshfIn == null)
            {
                return false;
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

//            Debug.Log("In: " + gameObjectIn.name + ": " + meshfIn.sharedMesh.subMeshCount + " submeshes, " + ": " + (meshfIn.sharedMesh.triangles.Length / 3) + " triangles, " + meshfIn.sharedMesh.vertexCount + " vertices, " + (meshfIn.sharedMesh.uv != null ? meshfIn.sharedMesh.uv.Length : 0) + " uv1, " + (meshfIn.sharedMesh.uv2 != null ? meshfIn.sharedMesh.uv2.Length : 0) + " uv2");

            // Check if the input object already has been split, to get its split closing submesh

            FracturedChunk fracturedChunk = gameObjectIn.GetComponent<FracturedChunk>();

            int nSplitCloseSubMesh = fracturedChunk != null ? fracturedChunk.SplitSubMeshIndex : -1;

            if(nSplitCloseSubMesh == -1 && gameObjectIn.GetComponent<Renderer>())
            {
                // Check if its material is the same as the split material

                if(gameObjectIn.GetComponent<Renderer>().sharedMaterial == fracturedComponent.SplitMaterial)
                {
                    nSplitCloseSubMesh = 0;
                }
            }

            List<MeshData> listMeshDatasPos;
            List<MeshData> listMeshDatasNeg;

            Material[] aMaterials = fracturedComponent.gameObject.GetComponent<Renderer>() ? fracturedComponent.gameObject.GetComponent<Renderer>().sharedMaterials : null;

            MeshData meshDataIn = new MeshData(meshfIn.transform, meshfIn.sharedMesh, aMaterials, meshfIn.transform.localToWorldMatrix, !splitOptions.bVerticesAreLocal, nSplitCloseSubMesh, true);

            if(SplitMeshUsingPlane(meshDataIn, fracturedComponent, splitOptions, transformPlaneSplit.up, transformPlaneSplit.right, transformPlaneSplit.position, out listMeshDatasPos, out listMeshDatasNeg, progress) == false)
            {
                return false;
            }

            // Set the mesh properties and add objects to list

            if(listMeshDatasPos.Count > 0)
            {
                for(int nMeshCount = 0; nMeshCount < listMeshDatasPos.Count; nMeshCount++)
                {
                    GameObject goPos = CreateNewSplitGameObject(gameObjectIn, fracturedComponent, gameObjectIn.name + "0" + (listMeshDatasPos.Count > 1 ? ("(" + nMeshCount + ")") : ""), !splitOptions.bVerticesAreLocal, listMeshDatasPos[nMeshCount]);
                    listGameObjectsPosOut.Add(goPos);
                }
            }

            if(listMeshDatasNeg.Count > 0)
            {
                for(int nMeshCount = 0; nMeshCount < listMeshDatasNeg.Count; nMeshCount++)
                {
                    GameObject goNeg = CreateNewSplitGameObject(gameObjectIn, fracturedComponent, gameObjectIn.name + "1" + (listMeshDatasNeg.Count > 1 ? ("(" + nMeshCount + ")") : ""), !splitOptions.bVerticesAreLocal, listMeshDatasNeg[nMeshCount]);
                    listGameObjectsNegOut.Add(goNeg);
                }
            }

            return true;
        }

        private static bool SplitMeshUsingPlane(MeshData meshDataIn, FracturedObject fracturedComponent, SplitOptions splitOptions, Vector3 v3PlaneNormal, Vector3 v3PlaneRight, Vector3 v3PlanePoint, out List<MeshData> listMeshDatasPosOut, out List<MeshData> listMeshDatasNegOut, ProgressDelegate progress = null)
        {
            Plane planeSplit = new Plane(v3PlaneNormal, v3PlanePoint);

            listMeshDatasPosOut = new List<MeshData>();
            listMeshDatasNegOut = new List<MeshData>();

            // Check if the input object already has been split, to get its split closing submesh

            bool bNeedsNewSplitSubMesh = meshDataIn.nSplitCloseSubMesh == -1;
            int  nSplitCloseSubMesh    = meshDataIn.nSplitCloseSubMesh;

            // Here we are going to store our output vertex/index data

            int nCurrentVertexHash = meshDataIn.nCurrentVertexHash; // We will use this to identify vertices with same coordinates but different vertex data. They will share the same vertex hash

            List<VertexData>     listVertexDataPos   = new List<VertexData>();
            List<VertexData>     listVertexDataNeg   = new List<VertexData>();
            List<int>[]          alistIndicesPos     = new List<int>[meshDataIn.nSubMeshCount + (meshDataIn.nSplitCloseSubMesh == -1 ? 1 : 0)];
            List<int>[]          alistIndicesNeg     = new List<int>[meshDataIn.nSubMeshCount + (meshDataIn.nSplitCloseSubMesh == -1 ? 1 : 0)];
            MeshFaceConnectivity faceConnectivityPos = new MeshFaceConnectivity();
            MeshFaceConnectivity faceConnectivityNeg = new MeshFaceConnectivity();
            MeshDataConnectivity meshConnectivityPos = new MeshDataConnectivity();
            MeshDataConnectivity meshConnectivityNeg = new MeshDataConnectivity();

            listVertexDataPos.Capacity = meshDataIn.aVertexData.Length / 2;
            listVertexDataNeg.Capacity = meshDataIn.aVertexData.Length / 2;

            if(bNeedsNewSplitSubMesh)
            {
                // Make room for the split closing submesh

                nSplitCloseSubMesh = meshDataIn.nSubMeshCount;

                alistIndicesPos[nSplitCloseSubMesh] = new List<int>();
                alistIndicesNeg[nSplitCloseSubMesh] = new List<int>();
            }

            // Our vertices that form the clipped cap

            Dictionary<EdgeKeyByHash, int>     dicClipVerticesHash = new Dictionary<EdgeKeyByHash, int>    (new EdgeKeyByHash.EqualityComparer());
            Dictionary<EdgeKeyByHash, CapEdge> dicCapEdges         = new Dictionary<EdgeKeyByHash, CapEdge>(new EdgeKeyByHash.EqualityComparer());

            // A hash table with our clipped edges, to reuse clipped vertices

            Dictionary<EdgeKeyByIndex, ClippedEdge> dicClippedEdgesPos = new Dictionary<EdgeKeyByIndex, ClippedEdge>(new EdgeKeyByIndex.EqualityComparer());
            Dictionary<EdgeKeyByIndex, ClippedEdge> dicClippedEdgesNeg = new Dictionary<EdgeKeyByIndex, ClippedEdge>(new EdgeKeyByIndex.EqualityComparer());

            int nClippedCacheHits   = 0;
            int nClippedCacheMisses = 0;

            // A hash table with the remapped indices, to reuse non-clipped vertices

            Dictionary<int, int> dicRemappedIndicesPos = new Dictionary<int, int>();
            Dictionary<int, int> dicRemappedIndicesNeg = new Dictionary<int, int>();

            for(int nSubMesh = 0; nSubMesh < meshDataIn.nSubMeshCount; nSubMesh++)
            {
                // Index list

                alistIndicesPos[nSubMesh] = new List<int>();
                alistIndicesNeg[nSubMesh] = new List<int>();
                List<int> listIndicesPos = alistIndicesPos[nSubMesh];
                List<int> listIndicesNeg = alistIndicesNeg[nSubMesh];

                alistIndicesPos[nSubMesh].Capacity = meshDataIn.aaIndices[nSubMesh].Length / 2;
                alistIndicesNeg[nSubMesh].Capacity = meshDataIn.aaIndices[nSubMesh].Length / 2;

                // A reference to the output arrays/lists (it will be switching between positive/negative side along the algorithm)

                List<VertexData>     plistVertexData    = listVertexDataPos;
                List<int>            plistObjectIndices = listIndicesPos;
                MeshFaceConnectivity pFaceConnectivity  = faceConnectivityPos;
                MeshDataConnectivity pMeshConnectivity  = meshConnectivityPos;

                Dictionary<EdgeKeyByIndex, ClippedEdge> pdicClippedEdges    = dicClippedEdgesPos;
                Dictionary<int, int>                    pdicRemappedIndices = dicRemappedIndicesPos;

                // Iterate through all submesh faces:

                for(int i = 0; i < meshDataIn.aaIndices[nSubMesh].Length / 3; i++)
                {
                    plistVertexData     = listVertexDataPos;
                    plistObjectIndices  = listIndicesPos;
                    pFaceConnectivity   = faceConnectivityPos;
                    pMeshConnectivity   = meshConnectivityPos;
                    pdicClippedEdges    = dicClippedEdgesPos;
                    pdicRemappedIndices = dicRemappedIndicesPos;

                    int nIndex1 = meshDataIn.aaIndices[nSubMesh][i * 3 + 0];
                    int nIndex2 = meshDataIn.aaIndices[nSubMesh][i * 3 + 1];
                    int nIndex3 = meshDataIn.aaIndices[nSubMesh][i * 3 + 2];

                    int nHashV1 = meshDataIn.aVertexData[nIndex1].nVertexHash;
                    int nHashV2 = meshDataIn.aVertexData[nIndex2].nVertexHash;
                    int nHashV3 = meshDataIn.aVertexData[nIndex3].nVertexHash;

                    Vector3 v1 = meshDataIn.aVertexData[nIndex1].v3Vertex;
                    Vector3 v2 = meshDataIn.aVertexData[nIndex2].v3Vertex;
                    Vector3 v3 = meshDataIn.aVertexData[nIndex3].v3Vertex;

                    // Classify vertices depending on the side of the plane they lay on, then clip if necessary.

                    float fSide1 = v1.x * planeSplit.normal.x + v1.y * planeSplit.normal.y + v1.z * planeSplit.normal.z + planeSplit.distance;
                    float fSide2 = v2.x * planeSplit.normal.x + v2.y * planeSplit.normal.y + v2.z * planeSplit.normal.z + planeSplit.distance;
                    float fSide3 = v3.x * planeSplit.normal.x + v3.y * planeSplit.normal.y + v3.z * planeSplit.normal.z + planeSplit.distance;

                    bool  bForceSameSide  = false;
                    int   nAlmostInPlane  = 0;
                    bool  bAlmostInPlane1 = false;
                    bool  bAlmostInPlane2 = false;
                    bool  bAlmostInPlane3 = false;
                    float fFurthest       = 0.0f;

                    if(Mathf.Abs(fSide1) < UltimateFracturing.Parameters.EPSILONDISTANCEPLANE) { bAlmostInPlane1 = true; nAlmostInPlane++; }
                    if(Mathf.Abs(fSide2) < UltimateFracturing.Parameters.EPSILONDISTANCEPLANE) { bAlmostInPlane2 = true; nAlmostInPlane++; }
                    if(Mathf.Abs(fSide3) < UltimateFracturing.Parameters.EPSILONDISTANCEPLANE) { bAlmostInPlane3 = true; nAlmostInPlane++; }

                    if(Mathf.Abs(fSide1) > Mathf.Abs(fFurthest)) fFurthest = fSide1;
                    if(Mathf.Abs(fSide2) > Mathf.Abs(fFurthest)) fFurthest = fSide2;
                    if(Mathf.Abs(fSide3) > Mathf.Abs(fFurthest)) fFurthest = fSide3;

                    if(nAlmostInPlane == 1)
                    {
                        // Look if the other two vertices are on the same side. If so, we'll skip the clipping too.
                        if(bAlmostInPlane1 && (fSide2 * fSide3 > 0.0f)) bForceSameSide = true;
                        if(bAlmostInPlane2 && (fSide1 * fSide3 > 0.0f)) bForceSameSide = true;
                        if(bAlmostInPlane3 && (fSide1 * fSide2 > 0.0f)) bForceSameSide = true;
                    }
                    else if(nAlmostInPlane > 1)
                    {
                        bForceSameSide = true;

                        if(nAlmostInPlane == 3)
                        {
                            // Coplanar
                            continue;
                        }
                    }

                    if((fSide1 * fSide2 > 0.0f && fSide2 * fSide3 > 0.0f) || bForceSameSide)
                    {
                        // All on the same side, no clipping needed

                        if(fFurthest < 0.0f)
                        {
                            plistVertexData     = listVertexDataNeg;
                            plistObjectIndices  = listIndicesNeg;
                            pFaceConnectivity   = faceConnectivityNeg;
                            pMeshConnectivity   = meshConnectivityNeg;
                            pdicClippedEdges    = dicClippedEdgesNeg;
                            pdicRemappedIndices = dicRemappedIndicesNeg;
                        }

                        int nNewIndex1 = -1;
                        int nNewIndex2 = -1;
                        int nNewIndex3 = -1;

                        // Find vertices in remapped indices list and add vertex data if not present

                        if(pdicRemappedIndices.ContainsKey(nIndex1))
                        {
                            nNewIndex1 = pdicRemappedIndices[nIndex1];
                        }
                        if(nNewIndex1 == -1)
                        {
                            nNewIndex1 = plistVertexData.Count;
                            plistVertexData.Add(meshDataIn.aVertexData[nIndex1].Copy());
                            pdicRemappedIndices[nIndex1] = nNewIndex1;
                        }

                        if(pdicRemappedIndices.ContainsKey(nIndex2))
                        {
                            nNewIndex2 = pdicRemappedIndices[nIndex2];
                        }
                        if(nNewIndex2 == -1)
                        {
                            nNewIndex2 = plistVertexData.Count;
                            plistVertexData.Add(meshDataIn.aVertexData[nIndex2].Copy());
                            pdicRemappedIndices[nIndex2] = nNewIndex2;
                        }

                        if(pdicRemappedIndices.ContainsKey(nIndex3))
                        {
                            nNewIndex3 = pdicRemappedIndices[nIndex3];
                        }
                        if(nNewIndex3 == -1)
                        {
                            nNewIndex3 = plistVertexData.Count;
                            plistVertexData.Add(meshDataIn.aVertexData[nIndex3].Copy());
                            pdicRemappedIndices[nIndex3] = nNewIndex3;
                        }

                        // Add triangle indices

                        if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                        {
                            pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                        }

                        plistObjectIndices.Add(nNewIndex1);
                        plistObjectIndices.Add(nNewIndex2);
                        plistObjectIndices.Add(nNewIndex3);

                        if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                        {
                            pFaceConnectivity.AddEdge(nSubMesh, v1, v2, nHashV1, nHashV2, nNewIndex1, nNewIndex2);
                            pFaceConnectivity.AddEdge(nSubMesh, v2, v3, nHashV2, nHashV3, nNewIndex2, nNewIndex3);
                            pFaceConnectivity.AddEdge(nSubMesh, v3, v1, nHashV3, nHashV1, nNewIndex3, nNewIndex1);
                        }

                        // Add cap edges only if an edge is lying on the plane

                        if(nAlmostInPlane == 2)
                        {
                            if(fFurthest > 0.0f)
                            {
                                if(bAlmostInPlane1 && bAlmostInPlane2 && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV1, nHashV2, v1, v2);
                                if(bAlmostInPlane2 && bAlmostInPlane3 && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV2, nHashV3, v2, v3);
                                if(bAlmostInPlane3 && bAlmostInPlane1 && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV3, nHashV1, v3, v1);
                            }
                            else
                            {
                                if(bAlmostInPlane1 && bAlmostInPlane2 && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV2, nHashV1, v2, v1);
                                if(bAlmostInPlane2 && bAlmostInPlane3 && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV3, nHashV2, v3, v2);
                                if(bAlmostInPlane3 && bAlmostInPlane1 && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV1, nHashV3, v1, v3);
                            }
                        }
                    }
                    else if(nAlmostInPlane == 1)
                    {
                        // Special treatment clipping for one vertex laying on the clipping plane and the other 2 on different sides

                        int  nNewIndex1 = -1;
                        int  nNewIndex2 = -1;
                        int  nNewIndex3 = -1;
                        int  nNewIndex4 = -1;
                        int  nHashV4    = -1;
                        bool bEdge      = false;

                        EdgeKeyByIndex clippedEdgeKey;

                        if(bAlmostInPlane1)
                        {
                            // v1 almost on the clipping plane

                            if(fSide2 < 0.0f)
                            {
                                plistVertexData     = listVertexDataNeg;
                                plistObjectIndices  = listIndicesNeg;
                                pFaceConnectivity   = faceConnectivityNeg;
                                pMeshConnectivity   = meshConnectivityNeg;
                                pdicClippedEdges    = dicClippedEdgesNeg;
                                pdicRemappedIndices = dicRemappedIndicesNeg;
                            }

                            clippedEdgeKey = new EdgeKeyByIndex(nIndex2, nIndex3);

                            if(pdicClippedEdges.ContainsKey(clippedEdgeKey))
                            {
                                nClippedCacheHits++;
                                bEdge = true;
                                nNewIndex2 = pdicClippedEdges[clippedEdgeKey].GetFirstIndex(nIndex2);
                                nNewIndex4 = pdicClippedEdges[clippedEdgeKey].nClippedIndex;
                            }
                            else
                            {
                                nClippedCacheMisses++;
                                if(pdicRemappedIndices.ContainsKey(nIndex2)) nNewIndex2 = pdicRemappedIndices[nIndex2];
                            }

                            // Clip if not present in clipped edge list

                            EdgeKeyByHash clippedEdgeKeyHash = new EdgeKeyByHash(nHashV2, nHashV3);

                            if(dicClipVerticesHash.ContainsKey(clippedEdgeKeyHash))
                            {
                                nHashV4 = dicClipVerticesHash[clippedEdgeKeyHash];
                            }
                            else
                            {
                                nHashV4 = nCurrentVertexHash++;
                                dicClipVerticesHash.Add(clippedEdgeKeyHash, nHashV4);
                            }

                            VertexData vd4 = new VertexData(nHashV4);

                            if(bEdge == false)
                            {
                                if(VertexData.ClipAgainstPlane(meshDataIn.aVertexData, nIndex2, nIndex3, v2, v3, planeSplit, ref vd4) == false)
                                {
                                    return false;
                                }
                            }

                            // Add geometry of one side

                            // Add vertex data for all data not present in remapped list

                            if(nNewIndex1 == -1)
                            {
                                if(pdicRemappedIndices.ContainsKey(nIndex1))
                                {
                                    nNewIndex1 = pdicRemappedIndices[nIndex1];
                                }
                            }
                            if(nNewIndex1 == -1)
                            {
                                nNewIndex1 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex1].Copy());
                                pdicRemappedIndices[nIndex1] = nNewIndex1;
                            }

                            if(nNewIndex2 == -1)
                            {
                                nNewIndex2 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex2].Copy());
                                pdicRemappedIndices[nIndex2] = nNewIndex2;
                            }

                            if(nNewIndex4 == -1)
                            {
                                nNewIndex4 = plistVertexData.Count;
                                plistVertexData.Add(vd4);
                            }

                            if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                            {
                                pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                            }

                            plistObjectIndices.Add(nNewIndex1);
                            plistObjectIndices.Add(nNewIndex2);
                            plistObjectIndices.Add(nNewIndex4);

                            Vector3 v4 = plistVertexData[nNewIndex4].v3Vertex;

                            if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                            {
                                pFaceConnectivity.AddEdge(nSubMesh, v1, v2, nHashV1, nHashV2, nNewIndex1, nNewIndex2);
                                pFaceConnectivity.AddEdge(nSubMesh, v2, v4, nHashV2, nHashV4, nNewIndex2, nNewIndex4);
                                pFaceConnectivity.AddEdge(nSubMesh, v4, v1, nHashV4, nHashV1, nNewIndex4, nNewIndex1);
                            }

                            // Update cap edges and cache

                            if(plistVertexData == listVertexDataPos && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV4, nHashV1, plistVertexData[nNewIndex4].v3Vertex, plistVertexData[nNewIndex1].v3Vertex);

                            if(bEdge == false) pdicClippedEdges.Add(clippedEdgeKey, new ClippedEdge(nIndex2, nIndex3, nNewIndex2, nNewIndex3, nNewIndex4));

                            // Add geometry of other side

                            if(fSide3 < 0.0f)
                            {
                                plistVertexData     = listVertexDataNeg;
                                plistObjectIndices  = listIndicesNeg;
                                pFaceConnectivity   = faceConnectivityNeg;
                                pMeshConnectivity   = meshConnectivityNeg;
                                pdicClippedEdges    = dicClippedEdgesNeg;
                                pdicRemappedIndices = dicRemappedIndicesNeg;
                            }
                            else
                            {
                                plistVertexData     = listVertexDataPos;
                                plistObjectIndices  = listIndicesPos;
                                pFaceConnectivity   = faceConnectivityPos;
                                pMeshConnectivity   = meshConnectivityPos;
                                pdicClippedEdges    = dicClippedEdgesPos;
                                pdicRemappedIndices = dicRemappedIndicesPos;
                            }

                            nNewIndex1 = -1;
                            nNewIndex2 = -1;
                            nNewIndex3 = -1;
                            nNewIndex4 = -1;
                            bEdge      = false;

                            // Find edges in cache

                            if(pdicClippedEdges.ContainsKey(clippedEdgeKey))
                            {
                                nClippedCacheHits++;
                                bEdge      = true;
                                nNewIndex3 = pdicClippedEdges[clippedEdgeKey].GetSecondIndex(nIndex3);
                                nNewIndex4 = pdicClippedEdges[clippedEdgeKey].nClippedIndex;
                            }
                            else
                            {
                                nClippedCacheMisses++;
                                if(pdicRemappedIndices.ContainsKey(nIndex3)) nNewIndex3 = pdicRemappedIndices[nIndex3];
                            }

                            // Add vertex data for all data not present in remapped list

                            if(nNewIndex1 == -1)
                            {
                                if(pdicRemappedIndices.ContainsKey(nIndex1))
                                {
                                    nNewIndex1 = pdicRemappedIndices[nIndex1];
                                }
                            }
                            if(nNewIndex1 == -1)
                            {
                                nNewIndex1 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex1].Copy());
                                pdicRemappedIndices[nIndex1] = nNewIndex1;
                            }

                            if(nNewIndex3 == -1)
                            {
                                nNewIndex3 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex3].Copy());
                                pdicRemappedIndices[nIndex3] = nNewIndex3;
                            }

                            if(nNewIndex4 == -1)
                            {
                                nNewIndex4 = plistVertexData.Count;
                                plistVertexData.Add(vd4);
                            }

                            if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                            {
                                pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                            }

                            plistObjectIndices.Add(nNewIndex1);
                            plistObjectIndices.Add(nNewIndex4);
                            plistObjectIndices.Add(nNewIndex3);

                            if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                            {
                                pFaceConnectivity.AddEdge(nSubMesh, v1, v4, nHashV1, nHashV4, nNewIndex1, nNewIndex4);
                                pFaceConnectivity.AddEdge(nSubMesh, v4, v3, nHashV4, nHashV3, nNewIndex4, nNewIndex3);
                                pFaceConnectivity.AddEdge(nSubMesh, v3, v1, nHashV3, nHashV1, nNewIndex3, nNewIndex1);
                            }

                            // Update cap edges and cache

                            if(plistVertexData == listVertexDataPos && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV1, nHashV4, plistVertexData[nNewIndex1].v3Vertex, plistVertexData[nNewIndex4].v3Vertex);

                            if(bEdge == false) pdicClippedEdges.Add(clippedEdgeKey, new ClippedEdge(nIndex2, nIndex3, nNewIndex2, nNewIndex3, nNewIndex4));
                        }
                        else if(bAlmostInPlane2)
                        {
                            // v2 almost on the clipping plane

                            if(fSide3 < 0.0f)
                            {
                                plistVertexData     = listVertexDataNeg;
                                plistObjectIndices  = listIndicesNeg;
                                pFaceConnectivity   = faceConnectivityNeg;
                                pMeshConnectivity   = meshConnectivityNeg;
                                pdicClippedEdges    = dicClippedEdgesNeg;
                                pdicRemappedIndices = dicRemappedIndicesNeg;
                            }

                            clippedEdgeKey = new EdgeKeyByIndex(nIndex3, nIndex1);

                            if(pdicClippedEdges.ContainsKey(clippedEdgeKey))
                            {
                                nClippedCacheHits++;
                                bEdge      = true;
                                nNewIndex3 = pdicClippedEdges[clippedEdgeKey].GetFirstIndex(nIndex3);
                                nNewIndex4 = pdicClippedEdges[clippedEdgeKey].nClippedIndex;
                            }
                            else
                            {
                                nClippedCacheMisses++;
                                if(pdicRemappedIndices.ContainsKey(nIndex3)) nNewIndex3 = pdicRemappedIndices[nIndex3];
                            }

                            // Clip if not present in clipped edge list

                            EdgeKeyByHash clippedEdgeKeyHash = new EdgeKeyByHash(nHashV3, nHashV1);

                            if(dicClipVerticesHash.ContainsKey(clippedEdgeKeyHash))
                            {
                                nHashV4 = dicClipVerticesHash[clippedEdgeKeyHash];
                            }
                            else
                            {
                                nHashV4 = nCurrentVertexHash++;
                                dicClipVerticesHash.Add(clippedEdgeKeyHash, nHashV4);
                            }

                            VertexData vd4 = new VertexData(nHashV4);

                            if(bEdge == false)
                            {
                                if(VertexData.ClipAgainstPlane(meshDataIn.aVertexData, nIndex3, nIndex1, v3, v1, planeSplit, ref vd4) == false)
                                {
                                    return false;
                                }
                            }

                            // Add geometry of one side

                            // Add vertex data for all data not present in remapped list

                            if(nNewIndex2 == -1)
                            {
                                if(pdicRemappedIndices.ContainsKey(nIndex2))
                                {
                                    nNewIndex2 = pdicRemappedIndices[nIndex2];
                                }
                            }
                            if(nNewIndex2 == -1)
                            {
                                nNewIndex2 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex2].Copy());
                                pdicRemappedIndices[nIndex2] = nNewIndex2;
                            }

                            if(nNewIndex3 == -1)
                            {
                                nNewIndex3 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex3].Copy());
                                pdicRemappedIndices[nIndex3] = nNewIndex3;
                            }

                            if(nNewIndex4 == -1)
                            {
                                nNewIndex4 = plistVertexData.Count;
                                plistVertexData.Add(vd4);
                            }

                            if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                            {
                                pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                            }

                            plistObjectIndices.Add(nNewIndex2);
                            plistObjectIndices.Add(nNewIndex3);
                            plistObjectIndices.Add(nNewIndex4);

                            Vector3 v4 = plistVertexData[nNewIndex4].v3Vertex;

                            if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                            {
                                pFaceConnectivity.AddEdge(nSubMesh, v2, v3, nHashV2, nHashV3, nNewIndex2, nNewIndex3);
                                pFaceConnectivity.AddEdge(nSubMesh, v3, v4, nHashV3, nHashV4, nNewIndex3, nNewIndex4);
                                pFaceConnectivity.AddEdge(nSubMesh, v4, v2, nHashV4, nHashV2, nNewIndex4, nNewIndex2);
                            }

                            // Update cap edges and cache

                            if(plistVertexData == listVertexDataPos && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV4, nHashV2, plistVertexData[nNewIndex4].v3Vertex, plistVertexData[nNewIndex2].v3Vertex);

                            if(bEdge == false) pdicClippedEdges.Add(clippedEdgeKey, new ClippedEdge(nIndex3, nIndex1, nNewIndex3, nNewIndex1, nNewIndex4));

                            // Add geometry of other side

                            if(fSide1 < 0.0f)
                            {
                                plistVertexData     = listVertexDataNeg;
                                plistObjectIndices  = listIndicesNeg;
                                pFaceConnectivity   = faceConnectivityNeg;
                                pMeshConnectivity   = meshConnectivityNeg;
                                pdicClippedEdges    = dicClippedEdgesNeg;
                                pdicRemappedIndices = dicRemappedIndicesNeg;
                            }
                            else
                            {
                                plistVertexData     = listVertexDataPos;
                                plistObjectIndices  = listIndicesPos;
                                pFaceConnectivity   = faceConnectivityPos;
                                pMeshConnectivity   = meshConnectivityPos;
                                pdicClippedEdges    = dicClippedEdgesPos;
                                pdicRemappedIndices = dicRemappedIndicesPos;
                            }

                            nNewIndex1 = -1;
                            nNewIndex2 = -1;
                            nNewIndex4 = -1;
                            bEdge      = false;

                            // Find edges in cache

                            if(pdicClippedEdges.ContainsKey(clippedEdgeKey))
                            {
                                nClippedCacheHits++;
                                bEdge      = true;
                                nNewIndex1 = pdicClippedEdges[clippedEdgeKey].GetSecondIndex(nIndex1);
                                nNewIndex4 = pdicClippedEdges[clippedEdgeKey].nClippedIndex;
                            }
                            else
                            {
                                nClippedCacheMisses++;
                                if(pdicRemappedIndices.ContainsKey(nIndex1)) nNewIndex1 = pdicRemappedIndices[nIndex1];
                            }

                            // Add vertex data for all data not present in remapped list

                            if(nNewIndex1 == -1)
                            {
                                nNewIndex1 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex1].Copy());
                                pdicRemappedIndices[nIndex1] = nNewIndex1;
                            }

                            if(nNewIndex2 == -1)
                            {
                                if(pdicRemappedIndices.ContainsKey(nIndex2))
                                {
                                    nNewIndex2 = pdicRemappedIndices[nIndex2];
                                }
                            }
                            if(nNewIndex2 == -1)
                            {
                                nNewIndex2 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex2].Copy());
                                pdicRemappedIndices[nIndex2] = nNewIndex2;
                            }

                            if(nNewIndex4 == -1)
                            {
                                nNewIndex4 = plistVertexData.Count;
                                plistVertexData.Add(vd4);
                            }

                            if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                            {
                                pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                            }

                            plistObjectIndices.Add(nNewIndex2);
                            plistObjectIndices.Add(nNewIndex4);
                            plistObjectIndices.Add(nNewIndex1);

                            if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                            {
                                pFaceConnectivity.AddEdge(nSubMesh, v2, v4, nHashV2, nHashV4, nNewIndex2, nNewIndex4);
                                pFaceConnectivity.AddEdge(nSubMesh, v4, v1, nHashV4, nHashV1, nNewIndex4, nNewIndex1);
                                pFaceConnectivity.AddEdge(nSubMesh, v1, v2, nHashV1, nHashV2, nNewIndex1, nNewIndex2);
                            }

                            // Update cap edges and cache

                            if(plistVertexData == listVertexDataPos && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV2, nHashV4, plistVertexData[nNewIndex2].v3Vertex, plistVertexData[nNewIndex4].v3Vertex);

                            if(bEdge == false) pdicClippedEdges.Add(clippedEdgeKey, new ClippedEdge(nIndex3, nIndex1, nNewIndex3, nNewIndex1, nNewIndex4));
                        }
                        else if(bAlmostInPlane3)
                        {
                            // v3 almost on the clipping plane

                            if(fSide1 < 0.0f)
                            {
                                plistVertexData     = listVertexDataNeg;
                                plistObjectIndices  = listIndicesNeg;
                                pFaceConnectivity   = faceConnectivityNeg;
                                pMeshConnectivity   = meshConnectivityNeg;
                                pdicClippedEdges    = dicClippedEdgesNeg;
                                pdicRemappedIndices = dicRemappedIndicesNeg;
                            }

                            clippedEdgeKey = new EdgeKeyByIndex(nIndex1, nIndex2);

                            if(pdicClippedEdges.ContainsKey(clippedEdgeKey))
                            {
                                nClippedCacheHits++;
                                bEdge      = true;
                                nNewIndex1 = pdicClippedEdges[clippedEdgeKey].GetFirstIndex(nIndex1);
                                nNewIndex4 = pdicClippedEdges[clippedEdgeKey].nClippedIndex;
                            }
                            else
                            {
                                nClippedCacheMisses++;
                                if(pdicRemappedIndices.ContainsKey(nIndex1)) nNewIndex1 = pdicRemappedIndices[nIndex1];
                            }

                            // Clip if not present in clipped edge list

                            EdgeKeyByHash clippedEdgeKeyHash = new EdgeKeyByHash(nHashV1, nHashV2);

                            if(dicClipVerticesHash.ContainsKey(clippedEdgeKeyHash))
                            {
                                nHashV4 = dicClipVerticesHash[clippedEdgeKeyHash];
                            }
                            else
                            {
                                nHashV4 = nCurrentVertexHash++;
                                dicClipVerticesHash.Add(clippedEdgeKeyHash, nHashV4);
                            }

                            VertexData vd4 = new VertexData(nHashV4);

                            if(bEdge == false)
                            {
                                if(VertexData.ClipAgainstPlane(meshDataIn.aVertexData, nIndex1, nIndex2, v1, v2, planeSplit, ref vd4) == false)
                                {
                                    return false;
                                }
                            }

                            // Add geometry of one side

                            // Add vertex data for all data not present in remapped list

                            if(nNewIndex1 == -1)
                            {
                                nNewIndex1 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex1].Copy());
                                pdicRemappedIndices[nIndex1] = nNewIndex1;
                            }

                            if(nNewIndex3 == -1)
                            {
                                if(pdicRemappedIndices.ContainsKey(nIndex3))
                                {
                                    nNewIndex3 = pdicRemappedIndices[nIndex3];
                                }
                            }
                            if(nNewIndex3 == -1)
                            {
                                nNewIndex3 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex3].Copy());
                                pdicRemappedIndices[nIndex3] = nNewIndex3;
                            }

                            if(nNewIndex4 == -1)
                            {
                                nNewIndex4 = plistVertexData.Count;
                                plistVertexData.Add(vd4);
                            }

                            if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                            {
                                pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                            }

                            plistObjectIndices.Add(nNewIndex1);
                            plistObjectIndices.Add(nNewIndex4);
                            plistObjectIndices.Add(nNewIndex3);

                            Vector3 v4 = plistVertexData[nNewIndex4].v3Vertex;

                            if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                            {
                                pFaceConnectivity.AddEdge(nSubMesh, v1, v4, nHashV1, nHashV4, nNewIndex1, nNewIndex4);
                                pFaceConnectivity.AddEdge(nSubMesh, v4, v3, nHashV4, nHashV3, nNewIndex4, nNewIndex3);
                                pFaceConnectivity.AddEdge(nSubMesh, v3, v1, nHashV3, nHashV1, nNewIndex3, nNewIndex1);
                            }

                            // Update cap edges and cache

                            if(plistVertexData == listVertexDataPos && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV4, nHashV3, plistVertexData[nNewIndex4].v3Vertex, plistVertexData[nNewIndex3].v3Vertex);

                            if(bEdge == false) pdicClippedEdges.Add(clippedEdgeKey, new ClippedEdge(nIndex1, nIndex2, nNewIndex1, nNewIndex2, nNewIndex4));

                            // Add geometry of other side

                            if(fSide2 < 0.0f)
                            {
                                plistVertexData     = listVertexDataNeg;
                                plistObjectIndices  = listIndicesNeg;
                                pFaceConnectivity   = faceConnectivityNeg;
                                pMeshConnectivity   = meshConnectivityNeg;
                                pdicClippedEdges    = dicClippedEdgesNeg;
                                pdicRemappedIndices = dicRemappedIndicesNeg;
                            }
                            else
                            {
                                plistVertexData     = listVertexDataPos;
                                plistObjectIndices  = listIndicesPos;
                                pFaceConnectivity   = faceConnectivityPos;
                                pMeshConnectivity   = meshConnectivityPos;
                                pdicClippedEdges    = dicClippedEdgesPos;
                                pdicRemappedIndices = dicRemappedIndicesPos;
                            }

                            nNewIndex2 = -1;
                            nNewIndex3 = -1;
                            nNewIndex4 = -1;
                            bEdge      = false;

                            // Find edges in cache

                            if(pdicClippedEdges.ContainsKey(clippedEdgeKey))
                            {
                                nClippedCacheHits++;
                                bEdge      = true;
                                nNewIndex2 = pdicClippedEdges[clippedEdgeKey].GetSecondIndex(nIndex2);
                                nNewIndex4 = pdicClippedEdges[clippedEdgeKey].nClippedIndex;
                            }
                            else
                            {
                                nClippedCacheMisses++;
                                if(pdicRemappedIndices.ContainsKey(nIndex2)) nNewIndex2 = pdicRemappedIndices[nIndex2];
                            }

                            // Add vertex data for all data not present in remapped list

                            if(nNewIndex2 == -1)
                            {
                                nNewIndex2 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex2].Copy());
                                pdicRemappedIndices[nIndex2] = nNewIndex2;
                            }

                            if(nNewIndex3 == -1)
                            {
                                if(pdicRemappedIndices.ContainsKey(nIndex3))
                                {
                                    nNewIndex3 = pdicRemappedIndices[nIndex3];
                                }
                            }
                            if(nNewIndex3 == -1)
                            {
                                nNewIndex3 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex3].Copy());
                                pdicRemappedIndices[nIndex3] = nNewIndex3;
                            }

                            if(nNewIndex4 == -1)
                            {
                                nNewIndex4 = plistVertexData.Count;
                                plistVertexData.Add(vd4);
                            }

                            if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                            {
                                pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                            }

                            plistObjectIndices.Add(nNewIndex2);
                            plistObjectIndices.Add(nNewIndex3);
                            plistObjectIndices.Add(nNewIndex4);

                            if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                            {
                                pFaceConnectivity.AddEdge(nSubMesh, v2, v3, nHashV2, nHashV3, nNewIndex2, nNewIndex3);
                                pFaceConnectivity.AddEdge(nSubMesh, v3, v4, nHashV3, nHashV4, nNewIndex3, nNewIndex4);
                                pFaceConnectivity.AddEdge(nSubMesh, v4, v2, nHashV4, nHashV2, nNewIndex4, nNewIndex2);
                            }

                            // Update cap edges and cache

                            if(plistVertexData == listVertexDataPos && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV3, nHashV4, plistVertexData[nNewIndex3].v3Vertex, plistVertexData[nNewIndex4].v3Vertex);

                            if(bEdge == false) pdicClippedEdges.Add(clippedEdgeKey, new ClippedEdge(nIndex1, nIndex2, nNewIndex1, nNewIndex2, nNewIndex4));
                        }
                    }
                    else
                    {
                        if(fSide1 * fSide2 < 0.0f)
                        {
                            // v1 and v2 on different sides

                            if(fSide2 * fSide3 < 0.0f)
                            {
                                // ... and v3 on same side as v1

                                if(fSide1 < 0.0f)
                                {
                                    plistVertexData     = listVertexDataNeg;
                                    plistObjectIndices  = listIndicesNeg;
                                    pFaceConnectivity   = faceConnectivityNeg;
                                    pMeshConnectivity   = meshConnectivityNeg;
                                    pdicClippedEdges    = dicClippedEdgesNeg;
                                    pdicRemappedIndices = dicRemappedIndicesNeg;
                                }

                                int  nNewIndex1 = -1;
                                int  nNewIndex2 = -1;
                                int  nNewIndex3 = -1;
                                int  nNewIndex4 = -1;
                                int  nNewIndex5 = -1;
                                int  nHashV4    = -1;
                                int  nHashV5    = -1;
                                bool bEdgeKey1  = false;
                                bool bEdgeKey2  = false;

                                EdgeKeyByIndex edgeKey1 = new EdgeKeyByIndex(nIndex1, nIndex2);
                                EdgeKeyByIndex edgeKey2 = new EdgeKeyByIndex(nIndex2, nIndex3);

                                // Find edges in cache

                                if(pdicClippedEdges.ContainsKey(edgeKey1))
                                {
                                    nClippedCacheHits++;
                                    bEdgeKey1  = true;
                                    nNewIndex1 = pdicClippedEdges[edgeKey1].GetFirstIndex(nIndex1);
                                    nNewIndex4 = pdicClippedEdges[edgeKey1].nClippedIndex;
                                }
                                else
                                {
                                    nClippedCacheMisses++;
                                    if(pdicRemappedIndices.ContainsKey(nIndex1)) nNewIndex1 = pdicRemappedIndices[nIndex1];
                                }

                                if(pdicClippedEdges.ContainsKey(edgeKey2))
                                {
                                    nClippedCacheHits++;
                                    bEdgeKey2  = true;
                                    nNewIndex3 = pdicClippedEdges[edgeKey2].GetSecondIndex(nIndex3);
                                    nNewIndex5 = pdicClippedEdges[edgeKey2].nClippedIndex;
                                }
                                else
                                {
                                    nClippedCacheMisses++;
                                    if(pdicRemappedIndices.ContainsKey(nIndex3)) nNewIndex3 = pdicRemappedIndices[nIndex3];
                                }

                                // Clip if not present in clipped edge list

                                EdgeKeyByHash clippedEdgeKeyHash = new EdgeKeyByHash(nHashV1, nHashV2);

                                if(dicClipVerticesHash.ContainsKey(clippedEdgeKeyHash))
                                {
                                    nHashV4 = dicClipVerticesHash[clippedEdgeKeyHash];
                                }
                                else
                                {
                                    nHashV4 = nCurrentVertexHash++;
                                    dicClipVerticesHash.Add(clippedEdgeKeyHash, nHashV4);
                                }

                                clippedEdgeKeyHash = new EdgeKeyByHash(nHashV2, nHashV3);

                                if(dicClipVerticesHash.ContainsKey(clippedEdgeKeyHash))
                                {
                                    nHashV5 = dicClipVerticesHash[clippedEdgeKeyHash];
                                }
                                else
                                {
                                    nHashV5 = nCurrentVertexHash++;
                                    dicClipVerticesHash.Add(clippedEdgeKeyHash, nHashV5);
                                }

                                VertexData vd4 = new VertexData(nHashV4), vd5 = new VertexData(nHashV5);

                                if(bEdgeKey1 == false)
                                {
                                    if(VertexData.ClipAgainstPlane(meshDataIn.aVertexData, nIndex1, nIndex2, v1, v2, planeSplit, ref vd4) == false)
                                    {
                                        return false;
                                    }
                                }

                                if(bEdgeKey2 == false)
                                {
                                    if(VertexData.ClipAgainstPlane(meshDataIn.aVertexData, nIndex2, nIndex3, v2, v3, planeSplit, ref vd5) == false)
                                    {
                                        return false;
                                    }
                                }

                                // Add geometry of one side

                                // Add vertex data for all data not present in remapped list

                                if(nNewIndex1 == -1)
                                {
                                    nNewIndex1 = plistVertexData.Count;
                                    plistVertexData.Add(meshDataIn.aVertexData[nIndex1].Copy());
                                    pdicRemappedIndices[nIndex1] = nNewIndex1;
                                }

                                if(nNewIndex3 == -1)
                                {
                                    nNewIndex3 = plistVertexData.Count;
                                    plistVertexData.Add(meshDataIn.aVertexData[nIndex3].Copy());
                                    pdicRemappedIndices[nIndex3] = nNewIndex3;
                                }

                                if(nNewIndex4 == -1)
                                {
                                    nNewIndex4 = plistVertexData.Count;
                                    plistVertexData.Add(vd4);
                                }

                                if(nNewIndex5 == -1)
                                {
                                    nNewIndex5 = plistVertexData.Count;
                                    plistVertexData.Add(vd5);
                                }

                                if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                                {
                                    pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                                }

                                plistObjectIndices.Add(nNewIndex1);
                                plistObjectIndices.Add(nNewIndex4);
                                plistObjectIndices.Add(nNewIndex5);

                                if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                                {
                                    pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                                }

                                plistObjectIndices.Add(nNewIndex1);
                                plistObjectIndices.Add(nNewIndex5);
                                plistObjectIndices.Add(nNewIndex3);

                                Vector3 v4 = plistVertexData[nNewIndex4].v3Vertex;
                                Vector3 v5 = plistVertexData[nNewIndex5].v3Vertex;

                                if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                                {
                                    pFaceConnectivity.AddEdge(nSubMesh, v1, v4, nHashV1, nHashV4, nNewIndex1, nNewIndex4);
                                    pFaceConnectivity.AddEdge(nSubMesh, v4, v5, nHashV4, nHashV5, nNewIndex4, nNewIndex5);
                                    pFaceConnectivity.AddEdge(nSubMesh, v5, v1, nHashV5, nHashV1, nNewIndex5, nNewIndex1);
                                    pFaceConnectivity.AddEdge(nSubMesh, v1, v5, nHashV1, nHashV5, nNewIndex1, nNewIndex5);
                                    pFaceConnectivity.AddEdge(nSubMesh, v5, v3, nHashV5, nHashV3, nNewIndex5, nNewIndex3);
                                    pFaceConnectivity.AddEdge(nSubMesh, v3, v1, nHashV3, nHashV1, nNewIndex3, nNewIndex1);
                                }

                                // Update cap edges and cache

                                if(plistVertexData == listVertexDataPos && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV4, nHashV5, plistVertexData[nNewIndex4].v3Vertex, plistVertexData[nNewIndex5].v3Vertex);

                                if(pdicClippedEdges.ContainsKey(edgeKey1) == false) pdicClippedEdges.Add(edgeKey1, new ClippedEdge(nIndex1, nIndex2, nNewIndex1, nNewIndex2, nNewIndex4));
                                if(pdicClippedEdges.ContainsKey(edgeKey2) == false) pdicClippedEdges.Add(edgeKey2, new ClippedEdge(nIndex2, nIndex3, nNewIndex2, nNewIndex3, nNewIndex5));

                                // Add geometry of other side

                                if(fSide2 < 0.0f)
                                {
                                    plistVertexData     = listVertexDataNeg;
                                    plistObjectIndices  = listIndicesNeg;
                                    pFaceConnectivity   = faceConnectivityNeg;
                                    pMeshConnectivity   = meshConnectivityNeg;
                                    pdicClippedEdges    = dicClippedEdgesNeg;
                                    pdicRemappedIndices = dicRemappedIndicesNeg;
                                }
                                else
                                {
                                    plistVertexData     = listVertexDataPos;
                                    plistObjectIndices  = listIndicesPos;
                                    pFaceConnectivity   = faceConnectivityPos;
                                    pMeshConnectivity   = meshConnectivityPos;
                                    pdicClippedEdges    = dicClippedEdgesPos;
                                    pdicRemappedIndices = dicRemappedIndicesPos;
                                }

                                nNewIndex1 = -1;
                                nNewIndex2 = -1;
                                nNewIndex3 = -1;
                                nNewIndex4 = -1;
                                nNewIndex5 = -1;
                                bEdgeKey1  = false;
                                bEdgeKey2  = false;

                                // Find edges in cache

                                if(pdicClippedEdges.ContainsKey(edgeKey1))
                                {
                                    nClippedCacheHits++;
                                    bEdgeKey1  = true;
                                    nNewIndex2 = pdicClippedEdges[edgeKey1].GetSecondIndex(nIndex2);
                                    nNewIndex4 = pdicClippedEdges[edgeKey1].nClippedIndex;
                                }
                                else
                                {
                                    nClippedCacheMisses++;
                                    if(pdicRemappedIndices.ContainsKey(nIndex2)) nNewIndex2 = pdicRemappedIndices[nIndex2];
                                }

                                if(pdicClippedEdges.ContainsKey(edgeKey2))
                                {
                                    nClippedCacheHits++;
                                    bEdgeKey2  = true;
                                    nNewIndex2 = pdicClippedEdges[edgeKey2].GetFirstIndex(nIndex2);
                                    nNewIndex5 = pdicClippedEdges[edgeKey2].nClippedIndex;
                                }
                                else
                                {
                                    nClippedCacheMisses++;
                                    if(pdicRemappedIndices.ContainsKey(nIndex2)) nNewIndex2 = pdicRemappedIndices[nIndex2];
                                }

                                // Add vertex data for all data not present in remapped list

                                if(nNewIndex2 == -1)
                                {
                                    nNewIndex2 = plistVertexData.Count;
                                    plistVertexData.Add(meshDataIn.aVertexData[nIndex2].Copy());
                                    pdicRemappedIndices[nIndex2] = nNewIndex2;
                                }

                                if(nNewIndex4 == -1)
                                {
                                    nNewIndex4 = plistVertexData.Count;
                                    plistVertexData.Add(vd4);
                                }

                                if(nNewIndex5 == -1)
                                {
                                    nNewIndex5 = plistVertexData.Count;
                                    plistVertexData.Add(vd5);
                                }

                                if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                                {
                                    pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                                }

                                plistObjectIndices.Add(nNewIndex4);
                                plistObjectIndices.Add(nNewIndex2);
                                plistObjectIndices.Add(nNewIndex5);

                                if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                                {
                                    pFaceConnectivity.AddEdge(nSubMesh, v4, v2, nHashV4, nHashV2, nNewIndex4, nNewIndex2);
                                    pFaceConnectivity.AddEdge(nSubMesh, v2, v5, nHashV2, nHashV5, nNewIndex2, nNewIndex5);
                                    pFaceConnectivity.AddEdge(nSubMesh, v5, v4, nHashV5, nHashV4, nNewIndex5, nNewIndex4);
                                }

                                // Update cap edges and cache

                                if(plistVertexData == listVertexDataPos && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV5, nHashV4, plistVertexData[nNewIndex5].v3Vertex, plistVertexData[nNewIndex4].v3Vertex);

                                if(pdicClippedEdges.ContainsKey(edgeKey1) == false) pdicClippedEdges.Add(edgeKey1, new ClippedEdge(nIndex1, nIndex2, nNewIndex1, nNewIndex2, nNewIndex4));
                                if(pdicClippedEdges.ContainsKey(edgeKey2) == false) pdicClippedEdges.Add(edgeKey2, new ClippedEdge(nIndex2, nIndex3, nNewIndex2, nNewIndex3, nNewIndex5));
                            }
                            else
                            {
                                // ... and v3 on same side as v2

                                if(fSide1 < 0.0f)
                                {
                                    plistVertexData     = listVertexDataNeg;
                                    plistObjectIndices  = listIndicesNeg;
                                    pFaceConnectivity   = faceConnectivityNeg;
                                    pMeshConnectivity   = meshConnectivityNeg;
                                    pdicClippedEdges    = dicClippedEdgesNeg;
                                    pdicRemappedIndices = dicRemappedIndicesNeg;
                                }

                                int  nNewIndex1 = -1;
                                int  nNewIndex2 = -1;
                                int  nNewIndex3 = -1;
                                int  nNewIndex4 = -1;
                                int  nNewIndex5 = -1;
                                int  nHashV4    = -1;
                                int  nHashV5    = -1;
                                bool bEdgeKey1  = false;
                                bool bEdgeKey3  = false;

                                EdgeKeyByIndex edgeKey1 = new EdgeKeyByIndex(nIndex1, nIndex2);
                                EdgeKeyByIndex edgeKey3 = new EdgeKeyByIndex(nIndex1, nIndex3);

                                // Find edges in cache

                                if(pdicClippedEdges.ContainsKey(edgeKey1))
                                {
                                    nClippedCacheHits++;
                                    bEdgeKey1  = true;
                                    nNewIndex1 = pdicClippedEdges[edgeKey1].GetFirstIndex(nIndex1);
                                    nNewIndex4 = pdicClippedEdges[edgeKey1].nClippedIndex;
                                }
                                else
                                {
                                    nClippedCacheMisses++;
                                    if(pdicRemappedIndices.ContainsKey(nIndex1)) nNewIndex1 = pdicRemappedIndices[nIndex1];
                                }

                                if(pdicClippedEdges.ContainsKey(edgeKey3))
                                {
                                    nClippedCacheHits++;
                                    bEdgeKey3  = true;
                                    nNewIndex1 = pdicClippedEdges[edgeKey3].GetFirstIndex(nIndex1);
                                    nNewIndex5 = pdicClippedEdges[edgeKey3].nClippedIndex;
                                }
                                else
                                {
                                    nClippedCacheMisses++;
                                    if(pdicRemappedIndices.ContainsKey(nIndex1)) nNewIndex1 = pdicRemappedIndices[nIndex1];
                                }

                                // Clip if not present in clipped edge list

                                EdgeKeyByHash clippedEdgeKeyHash = new EdgeKeyByHash(nHashV1, nHashV2);

                                if(dicClipVerticesHash.ContainsKey(clippedEdgeKeyHash))
                                {
                                    nHashV4 = dicClipVerticesHash[clippedEdgeKeyHash];
                                }
                                else
                                {
                                    nHashV4 = nCurrentVertexHash++;
                                    dicClipVerticesHash.Add(clippedEdgeKeyHash, nHashV4);
                                }

                                clippedEdgeKeyHash = new EdgeKeyByHash(nHashV1, nHashV3);

                                if(dicClipVerticesHash.ContainsKey(clippedEdgeKeyHash))
                                {
                                    nHashV5 = dicClipVerticesHash[clippedEdgeKeyHash];
                                }
                                else
                                {
                                    nHashV5 = nCurrentVertexHash++;
                                    dicClipVerticesHash.Add(clippedEdgeKeyHash, nHashV5);
                                }

                                VertexData vd4 = new VertexData(nHashV4), vd5 = new VertexData(nHashV5);

                                if(bEdgeKey1 == false)
                                {
                                    if(VertexData.ClipAgainstPlane(meshDataIn.aVertexData, nIndex1, nIndex2, v1, v2, planeSplit, ref vd4) == false)
                                    {
                                        return false;
                                    }
                                }

                                if(bEdgeKey3 == false)
                                {
                                    if(VertexData.ClipAgainstPlane(meshDataIn.aVertexData, nIndex1, nIndex3, v1, v3, planeSplit, ref vd5) == false)
                                    {
                                        return false;
                                    }
                                }

                                // Add geometry of one side

                                // Add vertex data for all data not present in remapped list

                                if(nNewIndex1 == -1)
                                {
                                    nNewIndex1 = plistVertexData.Count;
                                    plistVertexData.Add(meshDataIn.aVertexData[nIndex1].Copy());
                                    pdicRemappedIndices[nIndex1] = nNewIndex1;
                                }

                                if(nNewIndex4 == -1)
                                {
                                    nNewIndex4 = plistVertexData.Count;
                                    plistVertexData.Add(vd4);
                                }

                                if(nNewIndex5 == -1)
                                {
                                    nNewIndex5 = plistVertexData.Count;
                                    plistVertexData.Add(vd5);
                                }

                                if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                                {
                                    pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                                }

                                plistObjectIndices.Add(nNewIndex1);
                                plistObjectIndices.Add(nNewIndex4);
                                plistObjectIndices.Add(nNewIndex5);

                                Vector3 v4 = plistVertexData[nNewIndex4].v3Vertex;
                                Vector3 v5 = plistVertexData[nNewIndex5].v3Vertex;

                                if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                                {
                                    pFaceConnectivity.AddEdge(nSubMesh, v1, v4, nHashV1, nHashV4, nNewIndex1, nNewIndex4);
                                    pFaceConnectivity.AddEdge(nSubMesh, v4, v5, nHashV4, nHashV5, nNewIndex4, nNewIndex5);
                                    pFaceConnectivity.AddEdge(nSubMesh, v5, v1, nHashV5, nHashV1, nNewIndex5, nNewIndex1);
                                }

                                // Update cap edges and cache

                                if(plistVertexData == listVertexDataPos && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV4, nHashV5, plistVertexData[nNewIndex4].v3Vertex, plistVertexData[nNewIndex5].v3Vertex);

                                if(pdicClippedEdges.ContainsKey(edgeKey1) == false) pdicClippedEdges.Add(edgeKey1, new ClippedEdge(nIndex1, nIndex2, nNewIndex1, nNewIndex2, nNewIndex4));
                                if(pdicClippedEdges.ContainsKey(edgeKey3) == false) pdicClippedEdges.Add(edgeKey3, new ClippedEdge(nIndex1, nIndex3, nNewIndex1, nNewIndex3, nNewIndex5));

                                // Add geometry of other side

                                if(fSide2 < 0.0f)
                                {
                                    plistVertexData     = listVertexDataNeg;
                                    plistObjectIndices  = listIndicesNeg;
                                    pFaceConnectivity   = faceConnectivityNeg;
                                    pMeshConnectivity   = meshConnectivityNeg;
                                    pdicClippedEdges    = dicClippedEdgesNeg;
                                    pdicRemappedIndices = dicRemappedIndicesNeg;
                                }
                                else
                                {
                                    plistVertexData     = listVertexDataPos;
                                    plistObjectIndices  = listIndicesPos;
                                    pFaceConnectivity   = faceConnectivityPos;
                                    pMeshConnectivity   = meshConnectivityPos;
                                    pdicClippedEdges    = dicClippedEdgesPos;
                                    pdicRemappedIndices = dicRemappedIndicesPos;
                                }

                                nNewIndex1 = -1;
                                nNewIndex2 = -1;
                                nNewIndex3 = -1;
                                nNewIndex4 = -1;
                                nNewIndex5 = -1;
                                bEdgeKey1  = false;
                                bEdgeKey3  = false;

                                // Find edges in cache

                                if(pdicClippedEdges.ContainsKey(edgeKey1))
                                {
                                    nClippedCacheHits++;
                                    bEdgeKey1  = true;
                                    nNewIndex2 = pdicClippedEdges[edgeKey1].GetSecondIndex(nIndex2);
                                    nNewIndex4 = pdicClippedEdges[edgeKey1].nClippedIndex;
                                }
                                else
                                {
                                    nClippedCacheMisses++;
                                    if(pdicRemappedIndices.ContainsKey(nIndex2)) nNewIndex2 = pdicRemappedIndices[nIndex2];
                                }

                                if(pdicClippedEdges.ContainsKey(edgeKey3))
                                {
                                    nClippedCacheHits++;
                                    bEdgeKey3  = true;
                                    nNewIndex3 = pdicClippedEdges[edgeKey3].GetSecondIndex(nIndex3);
                                    nNewIndex5 = pdicClippedEdges[edgeKey3].nClippedIndex;
                                }
                                else
                                {
                                    nClippedCacheMisses++;
                                    if(pdicRemappedIndices.ContainsKey(nIndex3)) nNewIndex3 = pdicRemappedIndices[nIndex3];
                                }

                                // Add vertex data for all data not present in remapped list

                                if(nNewIndex2 == -1)
                                {
                                    nNewIndex2 = plistVertexData.Count;
                                    plistVertexData.Add(meshDataIn.aVertexData[nIndex2].Copy());
                                    pdicRemappedIndices[nIndex2] = nNewIndex2;
                                }

                                if(nNewIndex3 == -1)
                                {
                                    nNewIndex3 = plistVertexData.Count;
                                    plistVertexData.Add(meshDataIn.aVertexData[nIndex3].Copy());
                                    pdicRemappedIndices[nIndex3] = nNewIndex3;
                                }

                                if(nNewIndex4 == -1)
                                {
                                    nNewIndex4 = plistVertexData.Count;
                                    plistVertexData.Add(vd4);
                                }

                                if(nNewIndex5 == -1)
                                {
                                    nNewIndex5 = plistVertexData.Count;
                                    plistVertexData.Add(vd5);
                                }

                                if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                                {
                                    pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                                }

                                plistObjectIndices.Add(nNewIndex4);
                                plistObjectIndices.Add(nNewIndex2);
                                plistObjectIndices.Add(nNewIndex3);

                                if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                                {
                                    pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                                }

                                plistObjectIndices.Add(nNewIndex4);
                                plistObjectIndices.Add(nNewIndex3);
                                plistObjectIndices.Add(nNewIndex5);

                                if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                                {
                                    pFaceConnectivity.AddEdge(nSubMesh, v4, v2, nHashV4, nHashV2, nNewIndex4, nNewIndex2);
                                    pFaceConnectivity.AddEdge(nSubMesh, v2, v3, nHashV2, nHashV3, nNewIndex2, nNewIndex3);
                                    pFaceConnectivity.AddEdge(nSubMesh, v3, v4, nHashV3, nHashV4, nNewIndex3, nNewIndex4);
                                    pFaceConnectivity.AddEdge(nSubMesh, v4, v3, nHashV4, nHashV3, nNewIndex4, nNewIndex3);
                                    pFaceConnectivity.AddEdge(nSubMesh, v3, v5, nHashV3, nHashV5, nNewIndex3, nNewIndex5);
                                    pFaceConnectivity.AddEdge(nSubMesh, v5, v4, nHashV5, nHashV4, nNewIndex5, nNewIndex4);
                                }

                                // Update cap edges and cache

                                if(plistVertexData == listVertexDataPos && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV5, nHashV4, plistVertexData[nNewIndex5].v3Vertex, plistVertexData[nNewIndex4].v3Vertex);

                                if(pdicClippedEdges.ContainsKey(edgeKey1) == false) pdicClippedEdges.Add(edgeKey1, new ClippedEdge(nIndex1, nIndex2, nNewIndex1, nNewIndex2, nNewIndex4));
                                if(pdicClippedEdges.ContainsKey(edgeKey3) == false) pdicClippedEdges.Add(edgeKey3, new ClippedEdge(nIndex1, nIndex3, nNewIndex1, nNewIndex3, nNewIndex5));
                            }
                        }
                        else if(fSide2 * fSide3 < 0.0f)
                        {
                            // v1 and v2 on same side, and v3 on different side

                            if(fSide1 < 0.0f)
                            {
                                plistVertexData     = listVertexDataNeg;
                                plistObjectIndices  = listIndicesNeg;
                                pFaceConnectivity   = faceConnectivityNeg;
                                pMeshConnectivity   = meshConnectivityNeg;
                                pdicClippedEdges    = dicClippedEdgesNeg;
                                pdicRemappedIndices = dicRemappedIndicesNeg;
                            }

                            int  nNewIndex1 = -1;
                            int  nNewIndex2 = -1;
                            int  nNewIndex3 = -1;
                            int  nNewIndex4 = -1;
                            int  nNewIndex5 = -1;
                            int  nHashV4    = -1;
                            int  nHashV5    = -1;
                            bool bEdgeKey2  = false;
                            bool bEdgeKey3  = false;

                            EdgeKeyByIndex edgeKey2 = new EdgeKeyByIndex(nIndex2, nIndex3);
                            EdgeKeyByIndex edgeKey3 = new EdgeKeyByIndex(nIndex1, nIndex3);

                            // Find edges in cache

                            if(pdicClippedEdges.ContainsKey(edgeKey2))
                            {
                                nClippedCacheHits++;
                                bEdgeKey2  = true;
                                nNewIndex2 = pdicClippedEdges[edgeKey2].GetFirstIndex(nIndex2);
                                nNewIndex5 = pdicClippedEdges[edgeKey2].nClippedIndex;
                            }
                            else
                            {
                                nClippedCacheMisses++;
                                if(pdicRemappedIndices.ContainsKey(nIndex2)) nNewIndex2 = pdicRemappedIndices[nIndex2];
                            }

                            if(pdicClippedEdges.ContainsKey(edgeKey3))
                            {
                                nClippedCacheHits++;
                                bEdgeKey3  = true;
                                nNewIndex1 = pdicClippedEdges[edgeKey3].GetFirstIndex(nIndex1);
                                nNewIndex4 = pdicClippedEdges[edgeKey3].nClippedIndex;
                            }
                            else
                            {
                                nClippedCacheMisses++;
                                if(pdicRemappedIndices.ContainsKey(nIndex1)) nNewIndex1 = pdicRemappedIndices[nIndex1];
                            }

                            // Clip if not present in clipped edge list

                            EdgeKeyByHash clippedEdgeKeyHash = new EdgeKeyByHash(nHashV1, nHashV3);

                            if(dicClipVerticesHash.ContainsKey(clippedEdgeKeyHash))
                            {
                                nHashV4 = dicClipVerticesHash[clippedEdgeKeyHash];
                            }
                            else
                            {
                                nHashV4 = nCurrentVertexHash++;
                                dicClipVerticesHash.Add(clippedEdgeKeyHash, nHashV4);
                            }

                            clippedEdgeKeyHash = new EdgeKeyByHash(nHashV2, nHashV3);

                            if(dicClipVerticesHash.ContainsKey(clippedEdgeKeyHash))
                            {
                                nHashV5 = dicClipVerticesHash[clippedEdgeKeyHash];
                            }
                            else
                            {
                                nHashV5 = nCurrentVertexHash++;
                                dicClipVerticesHash.Add(clippedEdgeKeyHash, nHashV5);
                            }

                            VertexData vd4 = new VertexData(nHashV4), vd5 = new VertexData(nHashV5);

                            if(bEdgeKey2 == false)
                            {
                                if(VertexData.ClipAgainstPlane(meshDataIn.aVertexData, nIndex2, nIndex3, v2, v3, planeSplit, ref vd5) == false)
                                {
                                    return false;
                                }
                            }

                            if(bEdgeKey3 == false)
                            {
                                if(VertexData.ClipAgainstPlane(meshDataIn.aVertexData, nIndex1, nIndex3, v1, v3, planeSplit, ref vd4) == false)
                                {
                                    return false;
                                }
                            }

                            // Add geometry of one side

                            // Add vertex data for all data not present in remapped list

                            if(nNewIndex1 == -1)
                            {
                                nNewIndex1 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex1].Copy());
                                pdicRemappedIndices[nIndex1] = nNewIndex1;
                            }

                            if(nNewIndex2 == -1)
                            {
                                nNewIndex2 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex2].Copy());
                                pdicRemappedIndices[nIndex2] = nNewIndex2;
                            }

                            if(nNewIndex4 == -1)
                            {
                                nNewIndex4 = plistVertexData.Count;
                                plistVertexData.Add(vd4);
                            }

                            if(nNewIndex5 == -1)
                            {
                                nNewIndex5 = plistVertexData.Count;
                                plistVertexData.Add(vd5);
                            }

                            if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                            {
                                pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                            }

                            plistObjectIndices.Add(nNewIndex2);
                            plistObjectIndices.Add(nNewIndex5);
                            plistObjectIndices.Add(nNewIndex4);

                            if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                            {
                                pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                            }

                            plistObjectIndices.Add(nNewIndex2);
                            plistObjectIndices.Add(nNewIndex4);
                            plistObjectIndices.Add(nNewIndex1);

                            Vector3 v4 = plistVertexData[nNewIndex4].v3Vertex;
                            Vector3 v5 = plistVertexData[nNewIndex5].v3Vertex;

                            if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                            {
                                pFaceConnectivity.AddEdge(nSubMesh, v2, v5, nHashV2, nHashV5, nNewIndex2, nNewIndex5);
                                pFaceConnectivity.AddEdge(nSubMesh, v5, v4, nHashV5, nHashV4, nNewIndex5, nNewIndex4);
                                pFaceConnectivity.AddEdge(nSubMesh, v4, v2, nHashV4, nHashV2, nNewIndex4, nNewIndex2);
                                pFaceConnectivity.AddEdge(nSubMesh, v2, v4, nHashV2, nHashV4, nNewIndex2, nNewIndex4);
                                pFaceConnectivity.AddEdge(nSubMesh, v4, v1, nHashV4, nHashV1, nNewIndex4, nNewIndex1);
                                pFaceConnectivity.AddEdge(nSubMesh, v1, v2, nHashV1, nHashV2, nNewIndex1, nNewIndex2);
                            }

                            // Update cap edges and cache

                            if(plistVertexData == listVertexDataPos && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV5, nHashV4, plistVertexData[nNewIndex5].v3Vertex, plistVertexData[nNewIndex4].v3Vertex);

                            if(pdicClippedEdges.ContainsKey(edgeKey2) == false) pdicClippedEdges.Add(edgeKey2, new ClippedEdge(nIndex2, nIndex3, nNewIndex2, nNewIndex3, nNewIndex5));
                            if(pdicClippedEdges.ContainsKey(edgeKey3) == false) pdicClippedEdges.Add(edgeKey3, new ClippedEdge(nIndex1, nIndex3, nNewIndex1, nNewIndex3, nNewIndex4));

                            // Add geometry of other side

                            if(fSide3 < 0.0f)
                            {
                                plistVertexData     = listVertexDataNeg;
                                plistObjectIndices  = listIndicesNeg;
                                pFaceConnectivity   = faceConnectivityNeg;
                                pMeshConnectivity   = meshConnectivityNeg;
                                pdicClippedEdges    = dicClippedEdgesNeg;
                                pdicRemappedIndices = dicRemappedIndicesNeg;
                            }
                            else
                            {
                                plistVertexData     = listVertexDataPos;
                                plistObjectIndices  = listIndicesPos;
                                pFaceConnectivity   = faceConnectivityPos;
                                pMeshConnectivity   = meshConnectivityPos;
                                pdicClippedEdges    = dicClippedEdgesPos;
                                pdicRemappedIndices = dicRemappedIndicesPos;
                            }

                            nNewIndex1 = -1;
                            nNewIndex2 = -1;
                            nNewIndex3 = -1;
                            nNewIndex4 = -1;
                            nNewIndex5 = -1;
                            bEdgeKey2  = false;
                            bEdgeKey3  = false;

                            // Find edges in cache

                            if(pdicClippedEdges.ContainsKey(edgeKey2))
                            {
                                nClippedCacheHits++;
                                bEdgeKey2  = true;
                                nNewIndex3 = pdicClippedEdges[edgeKey2].GetSecondIndex(nIndex3);
                                nNewIndex5 = pdicClippedEdges[edgeKey2].nClippedIndex;
                            }
                            else
                            {
                                nClippedCacheMisses++;
                                if(pdicRemappedIndices.ContainsKey(nIndex3)) nNewIndex3 = pdicRemappedIndices[nIndex3];
                            }

                            if(pdicClippedEdges.ContainsKey(edgeKey3))
                            {
                                nClippedCacheHits++;
                                bEdgeKey3  = true;
                                nNewIndex3 = pdicClippedEdges[edgeKey3].GetSecondIndex(nIndex3);
                                nNewIndex4 = pdicClippedEdges[edgeKey3].nClippedIndex;
                            }
                            else
                            {
                                nClippedCacheMisses++;
                                if(pdicRemappedIndices.ContainsKey(nIndex3)) nNewIndex3 = pdicRemappedIndices[nIndex3];
                            }

                            // Add vertex data for all data not present in remapped list

                            if(nNewIndex3 == -1)
                            {
                                nNewIndex3 = plistVertexData.Count;
                                plistVertexData.Add(meshDataIn.aVertexData[nIndex3].Copy());
                                pdicRemappedIndices[nIndex3] = nNewIndex3;
                            }

                            if(nNewIndex4 == -1)
                            {
                                nNewIndex4 = plistVertexData.Count;
                                plistVertexData.Add(vd4);
                            }

                            if(nNewIndex5 == -1)
                            {
                                nNewIndex5 = plistVertexData.Count;
                                plistVertexData.Add(vd5);
                            }

                            if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoChunkConnectionInfo == false)
                            {
                                pMeshConnectivity.NotifyNewClippedFace(meshDataIn, nSubMesh, i, nSubMesh, plistObjectIndices.Count / 3);
                            }

                            plistObjectIndices.Add(nNewIndex5);
                            plistObjectIndices.Add(nNewIndex3);
                            plistObjectIndices.Add(nNewIndex4);

                            if(fracturedComponent.GenerateIslands && splitOptions.bForceNoIslandGeneration == false)
                            {
                                pFaceConnectivity.AddEdge(nSubMesh, v5, v3, nHashV5, nHashV3, nNewIndex5, nNewIndex3);
                                pFaceConnectivity.AddEdge(nSubMesh, v3, v4, nHashV3, nHashV4, nNewIndex3, nNewIndex4);
                                pFaceConnectivity.AddEdge(nSubMesh, v4, v5, nHashV4, nHashV5, nNewIndex4, nNewIndex5);
                            }

                            // Update cap edges and cache

                            if(plistVertexData == listVertexDataPos && splitOptions.bForceNoCap == false) AddCapEdge(dicCapEdges, nHashV4, nHashV5, plistVertexData[nNewIndex4].v3Vertex, plistVertexData[nNewIndex5].v3Vertex);

                            if(pdicClippedEdges.ContainsKey(edgeKey2) == false) pdicClippedEdges.Add(edgeKey2, new ClippedEdge(nIndex2, nIndex3, nNewIndex2, nNewIndex3, nNewIndex5));
                            if(pdicClippedEdges.ContainsKey(edgeKey3) == false) pdicClippedEdges.Add(edgeKey3, new ClippedEdge(nIndex1, nIndex3, nNewIndex1, nNewIndex3, nNewIndex4));
                        }
                    }
                }
            }

//          Debug.Log("Clipped cache hits " + nClippedCacheHits + " clipped cache misses " + nClippedCacheMisses);

            // Compute transforms

            Vector3 v3CenterPos = Vector3.zero;

            if(listVertexDataPos.Count > 0)
            {
                Vector3 v3Min = Vector3.zero, v3Max = Vector3.zero;
                MeshData.ComputeMinMax(listVertexDataPos, ref v3Min, ref v3Max);
                v3CenterPos = (v3Min + v3Max) * 0.5f;
            }

            Matrix4x4 mtxToLocalPos = Matrix4x4.TRS(v3CenterPos, meshDataIn.qRotation, meshDataIn.v3Scale).inverse;

            if(splitOptions.bVerticesAreLocal)
            {
                mtxToLocalPos = Matrix4x4.TRS(v3CenterPos, Quaternion.identity, Vector3.one).inverse;
            }

            Vector3 v3CenterNeg = Vector3.zero;

            if(listVertexDataNeg.Count > 0)
            {
                Vector3 v3Min = Vector3.zero, v3Max = Vector3.zero;
                MeshData.ComputeMinMax(listVertexDataNeg, ref v3Min, ref v3Max);
                v3CenterNeg = (v3Min + v3Max) * 0.5f;
            }

            Matrix4x4 mtxToLocalNeg = Matrix4x4.TRS(v3CenterNeg, meshDataIn.qRotation, meshDataIn.v3Scale).inverse;

            if(splitOptions.bVerticesAreLocal)
            {
                mtxToLocalNeg = Matrix4x4.TRS(v3CenterNeg, Quaternion.identity, Vector3.one).inverse;
            }

            // Resolve cap outline and add its geometry

            List<List<Vector3>> listlistResolvedCapVertices   = new List<List<Vector3>>();
            List<List<int>>     listlistResolvedCapHashValues = new List<List<int>>();

            bool bNeedsConnectivityPostprocess = false;

            Matrix4x4 mtxPlane = Matrix4x4.TRS(v3PlanePoint, Quaternion.LookRotation(Vector3.Cross(v3PlaneNormal, v3PlaneRight), v3PlaneNormal), Vector3.one);

            if(dicCapEdges.Count > 0 && splitOptions.bForceNoCap == false)
            {
                if(ResolveCap(dicCapEdges, listlistResolvedCapVertices, listlistResolvedCapHashValues, fracturedComponent))
                {
                    if(listlistResolvedCapVertices.Count > 1)
                    {
                        // There's more than one closed cap. We need to postprocess the mesh because there may be more than one object on a side of the plane as a result of the clipping.
                        bNeedsConnectivityPostprocess = (fracturedComponent.GenerateIslands && (splitOptions.bForceNoIslandGeneration == false)) ? true : false;
                    }

                    TriangulateConstrainedDelaunay( listlistResolvedCapVertices, listlistResolvedCapHashValues, splitOptions.bForceCapVertexSoup, fracturedComponent, bNeedsConnectivityPostprocess, faceConnectivityPos, faceConnectivityNeg,
                                                    meshConnectivityPos, meshConnectivityNeg, splitOptions.nForceMeshConnectivityHash, nSplitCloseSubMesh,
                                                    mtxPlane, mtxToLocalPos, mtxToLocalNeg, v3CenterPos, v3CenterNeg,
                                                    alistIndicesPos, listVertexDataPos, alistIndicesNeg, listVertexDataNeg);
                }
                else
                {
                    if(fracturedComponent.Verbose) Debug.LogWarning("Error resolving cap");
                }
            }

            // Postprocess if necessary
            if(bNeedsConnectivityPostprocess)
            {
                // Search for multiple objects inside each meshes

                List<MeshData> listIslandsPos = MeshData.PostProcessConnectivity(meshDataIn, faceConnectivityPos, meshConnectivityPos, alistIndicesPos, listVertexDataPos, nSplitCloseSubMesh, nCurrentVertexHash, false);
                List<MeshData> listIslandsNeg = new List<MeshData>();

                if(splitOptions.bIgnoreNegativeSide == false)
                {
                    listIslandsNeg = MeshData.PostProcessConnectivity(meshDataIn, faceConnectivityNeg, meshConnectivityNeg, alistIndicesNeg, listVertexDataNeg, nSplitCloseSubMesh, nCurrentVertexHash, false);
                }

                // Sometimes we are feed a mesh with multiple islands as input. If this is the case, compute connectivity between islands at this point.

                List<MeshData> listTotalIslands = new List<MeshData>();
                listTotalIslands.AddRange(listIslandsPos);
                listTotalIslands.AddRange(listIslandsNeg);

                if(fracturedComponent.GenerateChunkConnectionInfo && splitOptions.bForceNoIslandConnectionInfo == false)
                {
                    for(int i = 0; i < listTotalIslands.Count; i++)
                    {
                        if(progress != null && listTotalIslands.Count > 10 && splitOptions.bForceNoProgressInfo == false)
                        {
                            progress("Fracturing", "Processing island connectivity...", i / (float)listTotalIslands.Count);
                            if(Fracturer.IsFracturingCancelled()) return false;
                        }

                        for(int j = 0; j < listTotalIslands.Count; j++)
                        {
                            if(i != j)
                            {
                                ComputeIslandsMeshDataConnectivity(fracturedComponent, splitOptions.bVerticesAreLocal, listTotalIslands[i], listTotalIslands[j]);
                            }
                        }
                    }
                }

                listMeshDatasPosOut.AddRange(listIslandsPos);
                listMeshDatasNegOut.AddRange(listIslandsNeg);
            }
            else
            {
                // Create new MeshDatas

                if(listVertexDataPos.Count > 0 && alistIndicesPos.Length > 0)
                {
                    MeshData newMeshData = new MeshData(meshDataIn.aMaterials, alistIndicesPos, listVertexDataPos, nSplitCloseSubMesh, v3CenterPos, meshDataIn.qRotation, meshDataIn.v3Scale, mtxToLocalPos, false, false);
                    newMeshData.meshDataConnectivity = meshConnectivityPos;
                    newMeshData.nCurrentVertexHash   = nCurrentVertexHash;
                    listMeshDatasPosOut.Add(newMeshData);
                }

                if(listVertexDataNeg.Count > 0 && alistIndicesNeg.Length > 0 && splitOptions.bIgnoreNegativeSide == false)
                {
                    MeshData newMeshData = new MeshData(meshDataIn.aMaterials, alistIndicesNeg, listVertexDataNeg, nSplitCloseSubMesh, v3CenterNeg, meshDataIn.qRotation, meshDataIn.v3Scale, mtxToLocalNeg, false, false);
                    newMeshData.meshDataConnectivity = meshConnectivityNeg;
                    newMeshData.nCurrentVertexHash   = nCurrentVertexHash;
                    listMeshDatasNegOut.Add(newMeshData);
                }
            }

            return true;
        }

        private static bool ComputeIslandsMeshDataConnectivity(FracturedObject fracturedComponent, bool bVerticesAreLocal, MeshData meshData1, MeshData meshData2)
        {
            float fMargin = fracturedComponent.ChunkIslandConnectionMaxDistance;

            // Vertices and min/max may be in local space. We want distance checks to be in world space

            Vector3 v3Min1 = meshData1.v3Min; if(bVerticesAreLocal) v3Min1 = Vector3.Scale(v3Min1, meshData1.v3Scale);
            Vector3 v3Max1 = meshData1.v3Max; if(bVerticesAreLocal) v3Max1 = Vector3.Scale(v3Max1, meshData1.v3Scale);
            Vector3 v3Min2 = meshData2.v3Min; if(bVerticesAreLocal) v3Min2 = Vector3.Scale(v3Min2, meshData2.v3Scale);
            Vector3 v3Max2 = meshData2.v3Max; if(bVerticesAreLocal) v3Max2 = Vector3.Scale(v3Max2, meshData2.v3Scale);

            if((v3Min1.x > (v3Max2.x + fMargin)) || (v3Min1.y > (v3Max2.y + fMargin)) || (v3Min1.z > (v3Max2.z + fMargin)))
            {
                return false;
            }

            if((v3Min2.x > (v3Max1.x + fMargin)) || (v3Min2.y > (v3Max1.y + fMargin)) || (v3Min2.z > (v3Max1.z + fMargin)))
            {
                return false;
            }

            bool  bConnected    = false;
            float fDistPlaneMax = fracturedComponent.ChunkIslandConnectionMaxDistance;

            for(int nSubMesh1 = 0; nSubMesh1 < meshData1.aaIndices.Length; nSubMesh1++)
            {
                for(int nFace1 = 0; nFace1 < meshData1.aaIndices[nSubMesh1].Length / 3; nFace1++)
                {
                    Vector3 v1 = meshData1.aVertexData[meshData1.aaIndices[nSubMesh1][nFace1 * 3 + 0]].v3Vertex;
                    Vector3 v2 = meshData1.aVertexData[meshData1.aaIndices[nSubMesh1][nFace1 * 3 + 1]].v3Vertex;
                    Vector3 v3 = meshData1.aVertexData[meshData1.aaIndices[nSubMesh1][nFace1 * 3 + 2]].v3Vertex;

                    if(bVerticesAreLocal)
                    {
                        v1 = Vector3.Scale(v1, meshData1.v3Scale);
                        v2 = Vector3.Scale(v2, meshData1.v3Scale);
                        v3 = Vector3.Scale(v3, meshData1.v3Scale);
                    }

                    Vector3 v3Forward = -Vector3.Cross(v2 - v1, v3 - v1);
                    float   fArea1    = v3Forward.magnitude;

                    if(fArea1 < Parameters.EPSILONCROSSPRODUCT)
                    {
                        continue;
                    }

                    Quaternion qFace     = Quaternion.LookRotation(v3Forward.normalized, (v2 - v1).normalized);
                    Matrix4x4  mtxToFace = Matrix4x4.TRS(v1, qFace, Vector3.one).inverse;

                    Plane planeFace1 = new Plane(v1, v2, v3);

                    for(int nSubMesh2 = 0; nSubMesh2 < meshData2.aaIndices.Length; nSubMesh2++)
                    {
                        for(int nFace2 = 0; nFace2 < meshData2.aaIndices[nSubMesh2].Length / 3; nFace2++)
                        {
                            Vector3 v3Other1 = meshData2.aVertexData[meshData2.aaIndices[nSubMesh2][nFace2 * 3 + 0]].v3Vertex;
                            Vector3 v3Other2 = meshData2.aVertexData[meshData2.aaIndices[nSubMesh2][nFace2 * 3 + 1]].v3Vertex;
                            Vector3 v3Other3 = meshData2.aVertexData[meshData2.aaIndices[nSubMesh2][nFace2 * 3 + 2]].v3Vertex;

                            if(bVerticesAreLocal)
                            {
                                v3Other1 = Vector3.Scale(v3Other1, meshData2.v3Scale);
                                v3Other2 = Vector3.Scale(v3Other2, meshData2.v3Scale);
                                v3Other3 = Vector3.Scale(v3Other3, meshData2.v3Scale);
                            }

                            // Compute distance from face1 to face2

                            float fDist1 = Mathf.Abs(planeFace1.GetDistanceToPoint(v3Other1)); if(fDist1 > fDistPlaneMax) continue;
                            float fDist2 = Mathf.Abs(planeFace1.GetDistanceToPoint(v3Other2)); if(fDist2 > fDistPlaneMax) continue;
                            float fDist3 = Mathf.Abs(planeFace1.GetDistanceToPoint(v3Other3)); if(fDist3 > fDistPlaneMax) continue;

                            // See if they intersect in 2D (face 1 local coordinates)

                            Vector3 v3OtherCenterLocal = (v3Other1 + v3Other2 + v3Other3) / 3.0f;
                            v3OtherCenterLocal = mtxToFace.MultiplyPoint3x4(v3OtherCenterLocal);

                            Vector3 v3Local1 = mtxToFace.MultiplyPoint3x4(v1);
                            Vector3 v3Local2 = mtxToFace.MultiplyPoint3x4(v2);
                            Vector3 v3Local3 = mtxToFace.MultiplyPoint3x4(v3);
                            Vector3 v3Edge2  = v3Local3 - v3Local2;
                            Vector3 v3Edge3  = v3Local1 - v3Local3;

                            bool bFaceConnected = false;

                            // Test the center

                            if(v3OtherCenterLocal.x >= 0.0f)
                            {
                                if(Vector3.Cross(v3Edge2, v3OtherCenterLocal - v3Local2).z <= 0.0f)
                                {
                                    if(Vector3.Cross(v3Edge3, v3OtherCenterLocal - v3Local3).z <= 0.0f)
                                    {
                                        bFaceConnected = true;
                                    }
                                }
                            }

                            if(bFaceConnected == false)
                            {
                                // Try intersecting lines

                                Vector3 v3OtherLocal1 = mtxToFace.MultiplyPoint3x4(v3Other1);
                                Vector3 v3OtherLocal2 = mtxToFace.MultiplyPoint3x4(v3Other2);
                                Vector3 v3OtherLocal3 = mtxToFace.MultiplyPoint3x4(v3Other3);

                                if(bFaceConnected == false) if(IntersectEdges2D(v3OtherLocal1.x, v3OtherLocal1.y, v3OtherLocal2.x, v3OtherLocal2.y, v3Local1.x, v3Local1.y, v3Local2.x, v3Local2.y)) bFaceConnected = true;
                                if(bFaceConnected == false) if(IntersectEdges2D(v3OtherLocal1.x, v3OtherLocal1.y, v3OtherLocal2.x, v3OtherLocal2.y, v3Local2.x, v3Local2.y, v3Local3.x, v3Local3.y)) bFaceConnected = true;
                                if(bFaceConnected == false) if(IntersectEdges2D(v3OtherLocal1.x, v3OtherLocal1.y, v3OtherLocal2.x, v3OtherLocal2.y, v3Local3.x, v3Local3.y, v3Local1.x, v3Local1.y)) bFaceConnected = true;

                                if(bFaceConnected == false) if(IntersectEdges2D(v3OtherLocal2.x, v3OtherLocal2.y, v3OtherLocal3.x, v3OtherLocal3.y, v3Local1.x, v3Local1.y, v3Local2.x, v3Local2.y)) bFaceConnected = true;
                                if(bFaceConnected == false) if(IntersectEdges2D(v3OtherLocal2.x, v3OtherLocal2.y, v3OtherLocal3.x, v3OtherLocal3.y, v3Local2.x, v3Local2.y, v3Local3.x, v3Local3.y)) bFaceConnected = true;
                                if(bFaceConnected == false) if(IntersectEdges2D(v3OtherLocal2.x, v3OtherLocal2.y, v3OtherLocal3.x, v3OtherLocal3.y, v3Local3.x, v3Local3.y, v3Local1.x, v3Local1.y)) bFaceConnected = true;

                                if(bFaceConnected == false) if(IntersectEdges2D(v3OtherLocal3.x, v3OtherLocal3.y, v3OtherLocal1.x, v3OtherLocal1.y, v3Local1.x, v3Local1.y, v3Local2.x, v3Local2.y)) bFaceConnected = true;
                                if(bFaceConnected == false) if(IntersectEdges2D(v3OtherLocal3.x, v3OtherLocal3.y, v3OtherLocal1.x, v3OtherLocal1.y, v3Local2.x, v3Local2.y, v3Local3.x, v3Local3.y)) bFaceConnected = true;
                                if(bFaceConnected == false) if(IntersectEdges2D(v3OtherLocal3.x, v3OtherLocal3.y, v3OtherLocal1.x, v3OtherLocal1.y, v3Local3.x, v3Local3.y, v3Local1.x, v3Local1.y)) bFaceConnected = true;
                            }

                            if(bFaceConnected)
                            {
                                int nHash = MeshDataConnectivity.GetNewHash(); // New hash value to identify the 2 shared faces
                                meshData1.meshDataConnectivity.NotifyNewCapFace(nHash, nSubMesh1, nFace1);
                                meshData2.meshDataConnectivity.NotifyNewCapFace(nHash, nSubMesh2, nFace2);
                                bConnected = true;
                            }
                        }
                    }
                }
            }

            return bConnected;
        }

        public static bool IntersectEdges2D(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            Vector2 p = new Vector2(x1, y1);
            Vector2 q = new Vector2(x3, y3);
            Vector2 r = new Vector2(x2 - x1, y2 - y1);
            Vector2 s = new Vector2(x4 - x3, y4 - y3);

            float fCross   = CrossProduct2D(r, s);

            if(fCross < Parameters.EPSILONCROSSPRODUCT)
            {
                return false;
            }

            float t = CrossProduct2D((q - p), s) / fCross;
            float u = CrossProduct2D((q - p), r) / fCross;

            float fNegativeMargin = Parameters.EPSILONINSIDETRIANGLE;

            if(t >= fNegativeMargin && t <= (1.0f - fNegativeMargin) && u >= fNegativeMargin && u <= (1.0f - fNegativeMargin))
            {
                return true;
            }

            return false;
        }

        private static float CrossProduct2D(Vector2 a, Vector2 b)
        {
            return (a.x * b.y) - (a.y * b.x);
        }
    }
}