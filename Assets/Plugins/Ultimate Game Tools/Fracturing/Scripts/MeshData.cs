using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UltimateFracturing
{
    static public class Parameters
    {
        static public float EPSILONDISTANCEPLANE     = 0.00001f;
        static public float EPSILONDISTANCEVERTEX    = 0.00001f;
        static public float EPSILONCAPPRECISIONMIN   = 0.0001f;
        static public float EPSILONCROSSPRODUCT      = 0.00001f;
        static public float EPSILONINSIDETRIANGLE    = 0.01f;
        static public int   VERTICESSPACESUBDIVISION = 10;
    }

    public class VertexData
    {
        public int     nVertexHash;
        public Vector3 v3Vertex;
        public Vector3 v3Normal;
        public Vector4 v4Tangent;
        public Color32 color32;
        public Vector2 v2Mapping1;
        public Vector2 v2Mapping2;

        public bool bHasNormal;
        public bool bHasTangent;
        public bool bHasColor32;
        public bool bHasMapping1;
        public bool bHasMapping2;

        public VertexData(int nVertexHash)
        {
            this.nVertexHash = nVertexHash;

            v3Vertex   = Vector3.zero;
            v3Normal   = Vector3.zero;
            v4Tangent  = Vector4.zero;
            color32.r = color32.g = color32.b = color32.a = 255;
            v2Mapping1 = Vector2.zero;
            v2Mapping2 = Vector2.zero;

            bHasNormal   = false;
            bHasTangent  = false;
            bHasColor32  = false;
            bHasMapping1 = false;
            bHasMapping2 = false;
        }

        public VertexData(int nVertexHash, Vector3 v3Vertex, Vector3 v3Normal, Vector3 v4Tangent, Color32 color32, Vector2 v2Mapping1, Vector2 v2Mapping2, bool bHasNormal, bool bHasTangent, bool bHasColor32, bool bHasMapping1, bool bHasMapping2)
        {
            this.nVertexHash  = nVertexHash;
            this.v3Vertex     = v3Vertex;
            this.v3Normal     = v3Normal;
            this.v4Tangent    = v4Tangent;
            this.color32      = color32;
            this.v2Mapping1   = v2Mapping1;
            this.v2Mapping2   = v2Mapping2;

            this.bHasNormal   = bHasNormal;
            this.bHasTangent  = bHasTangent;
            this.bHasColor32  = bHasColor32;
            this.bHasMapping1 = bHasMapping1;
            this.bHasMapping2 = bHasMapping2;
        }

        public VertexData Copy()
        {
            VertexData vdCopy = new VertexData(nVertexHash);

            vdCopy.v3Vertex     = v3Vertex;
            vdCopy.v3Normal     = v3Normal;
            vdCopy.v4Tangent    = v4Tangent;
            vdCopy.color32      = color32;
            vdCopy.v2Mapping1   = v2Mapping1;
            vdCopy.v2Mapping2   = v2Mapping2;

            vdCopy.bHasNormal   = bHasNormal;
            vdCopy.bHasTangent  = bHasTangent;
            vdCopy.bHasColor32  = bHasColor32;
            vdCopy.bHasMapping1 = bHasMapping1;
            vdCopy.bHasMapping2 = bHasMapping2;

            return vdCopy;
        }

        public static VertexData Lerp(int nVertexHash, VertexData vd1, VertexData vd2, float fT)
        {
            VertexData vdResult = new VertexData(nVertexHash);

            // We assume vd1 and vd2 come from the same mesh and have the same data

            vdResult.bHasNormal   = vd1.bHasNormal;
            vdResult.bHasTangent  = vd1.bHasTangent;
            vdResult.bHasColor32  = vd1.bHasColor32;
            vdResult.bHasMapping1 = vd1.bHasMapping1;
            vdResult.bHasMapping2 = vd1.bHasMapping2;

            // Interpolate values

            Vector3 v3Tangent = Vector3.Lerp(vd1.v4Tangent, vd2.v4Tangent, fT);//.normalized;

            vdResult.v3Vertex = Vector3.Lerp(vd1.v3Vertex, vd2.v3Vertex, fT);
            if(vd1.bHasNormal)   vdResult.v3Normal   = Vector3.Lerp(vd1.v3Normal,   vd2.v3Normal,   fT);//.normalized;
            if(vd1.bHasColor32)  vdResult.color32    = Color32.Lerp(vd1.color32,    vd2.color32,    fT);
            if(vd1.bHasMapping1) vdResult.v2Mapping1 = Vector2.Lerp(vd1.v2Mapping1, vd2.v2Mapping1, fT);
            if(vd1.bHasMapping2) vdResult.v2Mapping2 = Vector2.Lerp(vd1.v2Mapping2, vd2.v2Mapping2, fT);
            if(vd1.bHasTangent)  vdResult.v4Tangent  = new Vector4(v3Tangent.x, v3Tangent.y, v3Tangent.z, vd1.v4Tangent.w);

            return vdResult;
        }

        public static bool ClipAgainstPlane(VertexData[] aVertexDataInput, int nIndexA, int nIndexB, Vector3 v3A, Vector3 v3B, Plane planeSplit, ref VertexData clippedVertexDataOut)
        {
            Vector3 vecAB = (v3B - v3A).normalized;
            Ray     rayAB = new Ray(v3A, vecAB);                            
            float   fRayCast;

            if(planeSplit.Raycast(rayAB, out fRayCast) == false)
            {
                Debug.LogWarning("Raycast returned false");
                clippedVertexDataOut = new VertexData(clippedVertexDataOut.nVertexHash);
                return false;
            };

            float fT = fRayCast / (v3B - v3A).magnitude;

            clippedVertexDataOut = VertexData.Lerp(clippedVertexDataOut.nVertexHash, aVertexDataInput[nIndexA], aVertexDataInput[nIndexB], fT);

            return true;
        }

        public static VertexData[] BuildVertexDataArray(Mesh mesh, Matrix4x4 mtxLocalToWorld, bool bTransformVerticesToWorld)
        {
            VertexData[] resultArray = new VertexData[mesh.vertexCount];

            Vector3[] av3Vertices = mesh.vertices;
            Vector3[] av3Normals  = mesh.normals;
            Vector4[] av4Tangents = mesh.tangents;
            Vector2[] av2Mapping1 = mesh.uv;
            Vector2[] av2Mapping2 = mesh.uv2;
            Color[]   acolColors  = mesh.colors;
            Color32[] aColors32   = mesh.colors32;

            for(int i = 0; i < mesh.vertexCount; i++)
            {
                resultArray[i] = new VertexData(-1);

                if(bTransformVerticesToWorld)
                {
                    resultArray[i].v3Vertex = mtxLocalToWorld.MultiplyPoint3x4(av3Vertices[i]);

                    if(av3Normals != null)
                    {
                        if(av3Normals.Length > i)
                        {
                            resultArray[i].v3Normal   = mtxLocalToWorld.MultiplyVector(av3Normals[i]);
                            resultArray[i].bHasNormal = true;
                        }
                    }
                    else resultArray[i].bHasNormal = false; 

                    if(av4Tangents != null)
                    {
                        if(av4Tangents.Length > i)
                        {
                            float w = av4Tangents[i].w;
                            resultArray[i].v4Tangent   = mtxLocalToWorld.MultiplyVector(av4Tangents[i]);
                            resultArray[i].v4Tangent.w = w;
                            resultArray[i].bHasTangent = true;
                        }
                    }
                    else resultArray[i].bHasTangent = false; 
                }
                else
                {
                    resultArray[i].v3Vertex = av3Vertices[i];

                    if(av3Normals != null)
                    {
                        if(av3Normals.Length > i)
                        {
                            resultArray[i].v3Normal   = av3Normals[i];
                            resultArray[i].bHasNormal = true;
                        }
                    }
                    else resultArray[i].bHasNormal = false; 

                    if(av4Tangents != null)
                    {
                        if(av4Tangents.Length > i)
                        {
                            resultArray[i].v4Tangent   = av4Tangents[i];
                            resultArray[i].bHasTangent = true;
                        }
                    }
                    else resultArray[i].bHasTangent = false; 
                }

                if(aColors32 != null)
                {
                    if(aColors32.Length > i)
                    {
                        resultArray[i].color32     = aColors32[i];
                        resultArray[i].bHasColor32 = true;
                    }
                }
                else if(acolColors != null)
                {
                    if(acolColors.Length > i)
                    {
                        resultArray[i].color32     = acolColors[i];
                        resultArray[i].bHasColor32 = true;
                    }
                }
                else resultArray[i].bHasColor32 = false;

                if(av2Mapping1 != null)
                {
                    if(av2Mapping1.Length > i)
                    {
                        resultArray[i].v2Mapping1   = av2Mapping1[i];
                        resultArray[i].bHasMapping1 = true;
                    }
                }
                else resultArray[i].bHasMapping1 = false; 

                if(av2Mapping2 != null)
                {
                    if(av2Mapping2.Length > i)
                    {
                        resultArray[i].v2Mapping2   = av2Mapping2[i];
                        resultArray[i].bHasMapping2 = true;
                    }
                }
                else resultArray[i].bHasMapping2 = false; 
            }

            return resultArray;
        }

        public static void SetMeshDataFromVertexDataArray(MeshFilter meshFilter, MeshData meshData, bool bTransformVertexToLocal)
        {
            VertexData[] aVertexData = meshData.aVertexData;

            Vector3[] aVertices = new Vector3[aVertexData.Length];
            Vector3[] aNormals  = aVertexData[0].bHasNormal   ? new Vector3[aVertexData.Length] : null;
            Vector4[] aTangents = aVertexData[0].bHasTangent  ? new Vector4[aVertexData.Length] : null;
            Color32[] aColors32 = aVertexData[0].bHasColor32  ? new Color32[aVertexData.Length] : null;
            Vector2[] aMapping1 = aVertexData[0].bHasMapping1 ? new Vector2[aVertexData.Length] : null;
            Vector2[] aMapping2 = aVertexData[0].bHasMapping2 ? new Vector2[aVertexData.Length] : null;

            Matrix4x4 mtxWorldToLocal = meshFilter.transform.worldToLocalMatrix;

            for(int i = 0; i < aVertexData.Length; i++)
            {
                if(bTransformVertexToLocal)
                {
                    aVertices[i] = mtxWorldToLocal.MultiplyPoint(aVertexData[i].v3Vertex);
                    if(aNormals  != null) aNormals[i]  = mtxWorldToLocal.MultiplyVector(aVertexData[i].v3Normal);
                    if(aTangents != null)
                    {
                        float w = aVertexData[i].v4Tangent.w;
                        aTangents[i] = mtxWorldToLocal.MultiplyVector(aVertexData[i].v4Tangent);
                        aTangents[i].w = w;
                    }
                }
                else
                {
                    aVertices[i] = aVertexData[i].v3Vertex;
                    if(aNormals  != null) aNormals[i]  = aVertexData[i].v3Normal;
                    if(aTangents != null) aTangents[i] = aVertexData[i].v4Tangent;
                }

                if(aColors32 != null) aColors32[i] = aVertexData[i].color32;
                if(aMapping1 != null) aMapping1[i] = aVertexData[i].v2Mapping1;
                if(aMapping2 != null) aMapping2[i] = aVertexData[i].v2Mapping2;
            }

            meshFilter.sharedMesh.vertices = aVertices;
            meshFilter.sharedMesh.normals  = aNormals;
            meshFilter.sharedMesh.tangents = aTangents;
            meshFilter.sharedMesh.colors32 = aColors32;
            meshFilter.sharedMesh.uv       = aMapping1;
            meshFilter.sharedMesh.uv2      = aMapping2;
        }
    }

    public struct EdgeKeyByIndex
    {
        public int nIndexA;
        public int nIndexB;

        public EdgeKeyByIndex(int nIndexA, int nIndexB)
        {
            this.nIndexA = nIndexA;
            this.nIndexB = nIndexB;
        }

        public void Set(int nIndexA, int nIndexB)
        {
            this.nIndexA = nIndexA;
            this.nIndexB = nIndexB;
        }

        public class EqualityComparer : IEqualityComparer<EdgeKeyByIndex>
        {
            public bool Equals(EdgeKeyByIndex x, EdgeKeyByIndex y)
            {
                return x.CompareTo(y.nIndexA, y.nIndexB);
            }

            public int GetHashCode(EdgeKeyByIndex x)
            {
                return x.nIndexA.GetHashCode() + x.nIndexB.GetHashCode();
            }
        }

        public bool CompareTo(int nIndexA, int nIndexB)
        {
            if(this.nIndexA == nIndexA && this.nIndexB == nIndexB) return true;
            if(this.nIndexA == nIndexB && this.nIndexB == nIndexA) return true;

            return false;
        }
    }

    public struct EdgeKeyByHash
    {
        public int nHashA;
        public int nHashB;

        public EdgeKeyByHash(int nHashA, int nHashB)
        {
            this.nHashA = nHashA;
            this.nHashB = nHashB;
        }

        public void Set(int nHashA, int nHashB)
        {
            this.nHashA = nHashA;
            this.nHashB = nHashB;
        }

        public class EqualityComparer : IEqualityComparer<EdgeKeyByHash>
        {
            public bool Equals(EdgeKeyByHash x, EdgeKeyByHash y)
            {
                return x.CompareTo(y.nHashA, y.nHashB);
            }

            public int GetHashCode(EdgeKeyByHash x)
            {
                return x.nHashA.GetHashCode() + x.nHashB.GetHashCode();
            }
        }

        public bool CompareTo(int nHashA, int nHashB)
        {
            if(this.nHashA == nHashA && this.nHashB == nHashB) return true;
            if(this.nHashA == nHashB && this.nHashB == nHashA) return true;

            return false;
        }
    }

    public struct ClippedEdge
    {
        public int nOldIndexA;
        public int nOldIndexB;

        public int nNewIndexA;
        public int nNewIndexB;

        public int nClippedIndex;

        public ClippedEdge(int nOldIndexA, int nOldIndexB, int nNewIndexA, int nNewIndexB, int nClippedIndex)
        {
            this.nOldIndexA     = nOldIndexA;
            this.nOldIndexB     = nOldIndexB;
            this.nNewIndexA     = nNewIndexA;
            this.nNewIndexB     = nNewIndexB;
            this.nClippedIndex = nClippedIndex;
        }

        public int GetFirstIndex(int nOldIndexA)
        {
            if(this.nOldIndexA == nOldIndexA) return nNewIndexA;
            return nNewIndexB;
        }

        public int GetSecondIndex(int nOldIndexB)
        {
            if(this.nOldIndexB == nOldIndexB) return nNewIndexB;
            return nNewIndexA;
        }
    }

    public struct CapEdge
    {
        public Vector3 v1, v2;
        public int     nHash1, nHash2;
        public float   fLength;

        public CapEdge(int nHash1, int nHash2, Vector3 v1, Vector3 v2, float fLength)
        {
            this.nHash1  = nHash1;
            this.nHash2  = nHash2;
            this.v1      = v1;
            this.v2      = v2;
            this.fLength = fLength;
        }

        public int SharesVertex1Of(CapEdge edge)
        {
            if(nHash1 == edge.nHash1) return 1;
            if(nHash2 == edge.nHash1) return 2;

            return 0;
        }

        public int SharesVertex2Of(CapEdge edge)
        {
            if(nHash1 == edge.nHash2) return 1;
            if(nHash2 == edge.nHash2) return 2;

            return 0;
        }
    }

    // Face connectivity through shared edges
    public class MeshFaceConnectivity
    {
        public struct EdgeData
        {
            public struct SideData
            {
                public SideData(int nFace, int nSubMesh, int nSubMeshFace, int nEdgePos, int nVertexDataV1, int nVertexDataV2)
                {
                    this.nFace         = nFace;         // Global face index
                    this.nSubMesh      = nSubMesh;      // Submesh index
                    this.nSubMeshFace  = nSubMeshFace;  // Face index (inside submesh)
                    this.nEdgePos      = nEdgePos;      // Edge position (0, 1 or 2)
                    this.nVertexDataV1 = nVertexDataV1; // Index in vertex data array for vertex 1
                    this.nVertexDataV2 = nVertexDataV2; // Index in vertex data array for vertex 2
                }

                public int nFace;
                public int nSubMesh;
                public int nSubMeshFace;
                public int nEdgePos;
                public int nVertexDataV1;
                public int nVertexDataV2;
            }

            public EdgeData(int nEdgeIndex, int nFace, int nSubMesh, int nSubMeshFace, int nEdgePos, Vector3 v1, Vector3 v2, int nVertex1Hash, int nVertex2Hash, int nVertexDataV1, int nVertexDataV2)
            {
                this.nEdgeIndex   = nEdgeIndex;
                this.v1           = v1;
                this.v2           = v2;
                this.nVertex1Hash = nVertex1Hash;
                this.nVertex2Hash = nVertex2Hash;

                listSides = new List<SideData>();
                listSides.Add(new SideData(nFace, nSubMesh, nSubMeshFace, nEdgePos, nVertexDataV1, nVertexDataV2));
            }

            public int            nEdgeIndex;     // Edge index for the edge list
            int                   nVertex1Hash;   // Vertex 1 hash (unique ID for vertices with same vertex coordinates)
            int                   nVertex2Hash;   // Vertex 2 hash (unique ID for vertices with same vertex coordinates)
            public Vector3        v1;             // Vertex 1
            public Vector3        v2;             // Vertex 2
            public List<SideData> listSides;      // Adjacent sides (more than 2 can be due to degenerate faces, just in case)

            public bool Compare(int nVertex1Hash, int nVertex2Hash)
            {
                if(this.nVertex1Hash == nVertex1Hash && this.nVertex2Hash == nVertex2Hash) return true;
                if(this.nVertex1Hash == nVertex2Hash && this.nVertex2Hash == nVertex1Hash) return true;

                return false;
            }

            public void AddSideData(int nFace, int nSubMesh, int nSubMeshFace, int nEdgePos, int nVertexDataV1, int nVertexDataV2)
            {
                if(listSides == null)
                {
                    listSides = new List<SideData>();
                }

                listSides.Add(new SideData(nFace, nSubMesh, nSubMeshFace, nEdgePos, nVertexDataV1, nVertexDataV2));
            }

            public bool HasMoreThanOneSide()
            {
                if(listSides == null)
                {
                    return false;
                }

                return listSides.Count > 1;
            }
        }

        public class TriangleData
        {
            public TriangleData(int nSubMesh, int nTriangle)
            {
                this.nSubMesh  = nSubMesh;
                this.nTriangle = nTriangle;

                anEdges                = new int[3];
                alistNeighborSubMeshes = new List<int>[3];
                alistNeighborTriangles = new List<int>[3];

                for(int i = 0; i < 3; i++)
                {
                    anEdges[i] = -1;
                    alistNeighborSubMeshes[i] = new List<int>();
                    alistNeighborTriangles[i] = new List<int>();
                }

                bVisited = false;
            }

            public int         nSubMesh;               // Submesh this triangle belongs to
            public int         nTriangle;              // Triangle index (relative to submesh)
            public int[]       anEdges;                // The 3 edges that conform this triangle
            public List<int>[] alistNeighborSubMeshes; // For each 3 edges, the list of adjacent submeshes
            public List<int>[] alistNeighborTriangles; // For each 3 edges, the list adjacent triangles (indices relative to submesh triangles
            public bool        bVisited;               // Has this triangle been visited when traversing?
        }

        public  List<TriangleData>                  listTriangles;            // The triangles

        private List<EdgeData>                      listEdges;                // Edges for linear traversal
        private List<int>                           listEdgeIndices;          // Indices into the edge list
        private Dictionary<EdgeKeyByHash, EdgeData> dicEdges;                 // Edge dictionary (key -> data)
        private int                                 nEdgeCount;               // Internal edge counter
        private Dictionary<int, int>                dicSubMeshTriangleCount;  // For each submesh, the internal triangle counter

        public MeshFaceConnectivity()
        {
            listTriangles           = new List<TriangleData>();
            listEdges               = new List<EdgeData>();
            listEdgeIndices         = new List<int>();
            dicEdges                = new Dictionary<EdgeKeyByHash, EdgeData>(new EdgeKeyByHash.EqualityComparer());
            dicSubMeshTriangleCount = new Dictionary<int, int>();

            nEdgeCount = 0;
        }

        public void Clear()
        {
            listTriangles.Clear();
            listEdges.Clear();
            listEdgeIndices.Clear();
            dicEdges.Clear();
            dicSubMeshTriangleCount.Clear();

            nEdgeCount = 0;
        }

        public void ResetVisited()
        {
            for(int i = 0; i < listTriangles.Count; i++)
            {
                listTriangles[i].bVisited = false;
            }
        }

        public void AddEdge(int nSubMesh, Vector3 v1, Vector3 v2, int nVertex1Hash, int nVertex2Hash, int nVertexDataIndex1, int nVertexDataIndex2)
        {
            int nEdgePos = nEdgeCount % 3;
            int nFace    = nEdgeCount / 3;

            if(dicSubMeshTriangleCount.ContainsKey(nSubMesh) == false)
            {
                dicSubMeshTriangleCount.Add(nSubMesh, 0);
            }

            int nSubMeshFace = dicSubMeshTriangleCount[nSubMesh];

            // Look for edge

            int nEdgeIndex = -1;

            EdgeKeyByHash edgeKey = new EdgeKeyByHash(nVertex1Hash, nVertex2Hash);

            // Try to find in dictionary, otherwise create new entry

            if(dicEdges.ContainsKey(edgeKey))
            {
                EdgeData edge = dicEdges[edgeKey];
                edge.AddSideData(nFace, nSubMesh, nSubMeshFace, nEdgePos, nVertexDataIndex1, nVertexDataIndex2);
                nEdgeIndex = edge.nEdgeIndex;
            }
            else
            {
                nEdgeIndex = listEdges.Count;
                EdgeData newEdge = new EdgeData(nEdgeIndex, nFace, nSubMesh, nSubMeshFace, nEdgePos, v1, v2, nVertex1Hash, nVertex2Hash, nVertexDataIndex1, nVertexDataIndex2);
                listEdges.Add(newEdge);
                dicEdges.Add(edgeKey, newEdge);
            }

            listEdgeIndices.Add(nEdgeIndex);

            // Closed triangle? create new triangle entry

            nEdgeCount++;

            if(nEdgeCount % 3 == 0)
            {
                TriangleData newTriangle = new TriangleData(nSubMesh, nSubMeshFace);

                EdgeData edge1 = listEdges[listEdgeIndices[listEdgeIndices.Count - 3]];
                EdgeData edge2 = listEdges[listEdgeIndices[listEdgeIndices.Count - 2]];
                EdgeData edge3 = listEdges[listEdgeIndices[listEdgeIndices.Count - 1]];

                newTriangle.anEdges[0] = edge1.nEdgeIndex;
                newTriangle.anEdges[1] = edge2.nEdgeIndex;
                newTriangle.anEdges[2] = edge3.nEdgeIndex;

                // Set connectivity data

                foreach(EdgeData.SideData sideData in edge1.listSides)
                {
                    if(sideData.nFace != nFace)
                    {
                        listTriangles[sideData.nFace].alistNeighborSubMeshes[sideData.nEdgePos].Add(nSubMesh);
                        listTriangles[sideData.nFace].alistNeighborTriangles[sideData.nEdgePos].Add(nSubMeshFace);

                        newTriangle.alistNeighborSubMeshes[0].Add(sideData.nSubMesh);
                        newTriangle.alistNeighborTriangles[0].Add(sideData.nSubMeshFace);
                    }
                }

                foreach(EdgeData.SideData sideData in edge2.listSides)
                {
                    if(sideData.nFace != nFace)
                    {
                        listTriangles[sideData.nFace].alistNeighborSubMeshes[sideData.nEdgePos].Add(nSubMesh);
                        listTriangles[sideData.nFace].alistNeighborTriangles[sideData.nEdgePos].Add(nSubMeshFace);

                        newTriangle.alistNeighborSubMeshes[1].Add(sideData.nSubMesh);
                        newTriangle.alistNeighborTriangles[1].Add(sideData.nSubMeshFace);
                    }
                }

                foreach(EdgeData.SideData sideData in edge3.listSides)
                {
                    if(sideData.nFace != nFace)
                    {
                        listTriangles[sideData.nFace].alistNeighborSubMeshes[sideData.nEdgePos].Add(nSubMesh);
                        listTriangles[sideData.nFace].alistNeighborTriangles[sideData.nEdgePos].Add(nSubMeshFace);

                        newTriangle.alistNeighborSubMeshes[2].Add(sideData.nSubMesh);
                        newTriangle.alistNeighborTriangles[2].Add(sideData.nSubMeshFace);
                    }
                }

                listTriangles.Add(newTriangle);

                dicSubMeshTriangleCount[nSubMesh]++;
            }
        }
    }

    // MeshData->MeshData connectivity through shared faces
    public class MeshDataConnectivity
    {
        public static int s_CurrentSharedFaceHash = 0;

        public struct Face
        {
            public Face(int nSubMesh, int nFaceIndex)
            {
                this.nSubMesh   = nSubMesh;
                this.nFaceIndex = nFaceIndex;
            }

            public class EqualityComparer : IEqualityComparer<Face>
            {
                public bool Equals(Face x, Face y)
                {
                    return (x.nSubMesh == y.nSubMesh && x.nFaceIndex == y.nFaceIndex);
                }

                public int GetHashCode(Face x)
                {
                    return x.nSubMesh.GetHashCode() + x.nFaceIndex.GetHashCode();
                }
            }

            public int nSubMesh;
            public int nFaceIndex;
        }

        public Dictionary<int,  List<Face>> dicHash2FaceList;
        public Dictionary<Face, List<int>>  dicFace2HashList;
        public Dictionary<Face, bool>       dicFace2IsClipped;

        public MeshDataConnectivity()
        {
            dicHash2FaceList  = new Dictionary<int, List<Face>>();
            dicFace2HashList  = new Dictionary<Face, List<int>> (new Face.EqualityComparer());
            dicFace2IsClipped = new Dictionary<Face, bool>(new Face.EqualityComparer());
        }

        public MeshDataConnectivity GetDeepCopy()
        {
            MeshDataConnectivity meshDataCopy = new MeshDataConnectivity();

            foreach(KeyValuePair<int, List<Face>> hash2FaceList in dicHash2FaceList)
            {
                meshDataCopy.dicHash2FaceList.Add(hash2FaceList.Key, new List<Face>());

                foreach(Face face in hash2FaceList.Value)
                {
                    meshDataCopy.dicHash2FaceList[hash2FaceList.Key].Add(face);
                }
            }

            foreach(KeyValuePair<Face, List<int>> face2HashList in dicFace2HashList)
            {
                meshDataCopy.dicFace2HashList.Add(face2HashList.Key, new List<int>());

                foreach(int nHash in face2HashList.Value)
                {
                    meshDataCopy.dicFace2HashList[face2HashList.Key].Add(nHash);
                }
            }

            foreach(KeyValuePair<Face, bool> face2IsClipped in dicFace2IsClipped)
            {
                meshDataCopy.dicFace2IsClipped.Add(face2IsClipped.Key, face2IsClipped.Value);
            }

            return meshDataCopy;
        }

        public void NotifyNewClippedFace(MeshData meshDataSource, int nSourceSubMesh, int nSourceFaceIndex, int nDestSubMesh, int nDestFaceIndex)
        {
            Face faceSrc = new Face(nSourceSubMesh, nSourceFaceIndex);
            Face faceDst = new Face(nDestSubMesh,   nDestFaceIndex);

            if(meshDataSource.meshDataConnectivity.dicFace2HashList.ContainsKey(faceSrc))
            {
                // The source face is a shared face, we must register this information

                foreach(int nHash in meshDataSource.meshDataConnectivity.dicFace2HashList[faceSrc])
                {
                    if(dicHash2FaceList.ContainsKey(nHash) == false)
                    {
                        dicHash2FaceList.Add(nHash, new List<Face>());
                    }

                    if(dicFace2HashList.ContainsKey(faceDst) == false)
                    {
                        dicFace2HashList.Add(faceDst, new List<int>());
                    }

                    dicHash2FaceList[nHash].Add(faceDst);
                    dicFace2HashList[faceDst].Add(nHash);

                    if(dicFace2IsClipped.ContainsKey(faceDst) == false)
                    {
                        dicFace2IsClipped.Add(faceDst, true);
                    }
                    else
                    {
                        dicFace2IsClipped[faceDst] = true;
                    }
                }
            }
        }

        public static int GetNewHash()
        {
            int nNewHash = 0;

            lock(typeof(MeshDataConnectivity))
            {
                nNewHash = s_CurrentSharedFaceHash;
                s_CurrentSharedFaceHash++;
            }

            return nNewHash;
        }

        public void NotifyNewCapFace(int nHash, int nSubMesh, int nFaceIndex)
        {
            Face face = new Face(nSubMesh, nFaceIndex);

            if(dicHash2FaceList.ContainsKey(nHash) == false)
            {
                dicHash2FaceList.Add(nHash, new List<Face>());
            }

            dicHash2FaceList[nHash].Add(face);

            if(dicFace2HashList.ContainsKey(face) == false)
            {
                dicFace2HashList.Add(face, new List<int>());
            }

            dicFace2HashList[face].Add(nHash);
        }

        public void NotifyRemappedFace(MeshDataConnectivity source, int nSourceSubMesh, int nSourceFaceIndex, int nDestSubMesh, int nDestFaceIndex)
        {
            Face faceSrc = new Face(nSourceSubMesh, nSourceFaceIndex);
            Face faceDst = new Face(nDestSubMesh,   nDestFaceIndex);

            if(source.dicFace2HashList.ContainsKey(faceSrc))
            {
                // The source face is a shared face, we must register this information

                foreach(int nHash in source.dicFace2HashList[faceSrc])
                {
                    if(dicHash2FaceList.ContainsKey(nHash) == false)
                    {
                        dicHash2FaceList.Add(nHash, new List<Face>());
                    }

                    if(dicFace2HashList.ContainsKey(faceDst) == false)
                    {
                        dicFace2HashList.Add(faceDst, new List<int>());
                    }

                    dicHash2FaceList[nHash].Add(faceDst);
                    dicFace2HashList[faceDst].Add(nHash);

                    if(source.dicFace2IsClipped.ContainsKey(faceSrc))
                    {
                        if(dicFace2IsClipped.ContainsKey(faceDst) == false)
                        {
                            dicFace2IsClipped.Add(faceDst, true);
                        }
                        else
                        {
                            dicFace2IsClipped[faceDst] = true;
                        }
                    }
                }
            }
        }
    }

    public class MeshData
    {
        public int          nSubMeshCount;      // The number of submeshes present
        public int[][]      aaIndices;          // For each submesh, an array of indices
        public int          nSplitCloseSubMesh; // Index of the submesh that has the split faces

        public VertexData[] aVertexData;        // Vertex data

        public Vector3      v3Position;         // Transform's position
        public Quaternion   qRotation;          // Transform's rotation
        public Vector3      v3Scale;            // Transform's scale

        public Vector3      v3Min;              // BBox min
        public Vector3      v3Max;              // BBox max

        public int          nCurrentVertexHash; // The last vertex hash value used. New values should be incremental values.

        public Material[]   aMaterials;         // The array of materials

        public MeshDataConnectivity meshDataConnectivity;

        public class IncreasingSizeComparer : IComparer<MeshData>
        {
            private int  nSplitAxis;

            public IncreasingSizeComparer(int nSplitAxis)
            {
                this.nSplitAxis = nSplitAxis;
            }

            public int Compare(MeshData a, MeshData b)
            {
                Vector3 v3MinA = a.v3Min;
                Vector3 v3MaxA = a.v3Max;
                Vector3 v3MinB = b.v3Min;
                Vector3 v3MaxB = b.v3Max;

                if(nSplitAxis == 0)
                {
                    return ((v3MaxA.x - v3MinA.x) - (v3MaxB.x - v3MinB.x)) < 0.0f ? -1 : 1;
                }
                else if(nSplitAxis == 1)
                {
                    return ((v3MaxA.y - v3MinA.y) - (v3MaxB.y - v3MinB.y)) < 0.0f ? -1 : 1;
                }
                else if(nSplitAxis == 2)
                {
                    return ((v3MaxA.z - v3MinA.z) - (v3MaxB.z - v3MinB.z)) < 0.0f ? -1 : 1;
                }

                return  Mathf.Max(v3MaxA.x - v3MinA.x, v3MaxA.y - v3MinA.y, v3MaxA.z - v3MinA.z) - 
                        Mathf.Max(v3MaxB.x - v3MinB.x, v3MaxB.y - v3MinB.y, v3MaxB.z - v3MinB.z) < 0.0f ? -1 : 1;
                //return ((v3MaxA - v3MinA).sqrMagnitude - (v3MaxB - v3MinB).sqrMagnitude) < 0.0f ? -1 : 1;
            }
        }

        public class DecreasingSizeComparer : IComparer<MeshData>
        {
            private int  nSplitAxis;

            public DecreasingSizeComparer(int nSplitAxis)
            {
                this.nSplitAxis = nSplitAxis;
            }

            public int Compare(MeshData a, MeshData b)
            {
                Vector3 v3MinA = a.v3Min;
                Vector3 v3MaxA = a.v3Max;
                Vector3 v3MinB = b.v3Min;
                Vector3 v3MaxB = b.v3Max;

                if(nSplitAxis == 0)
                {
                    return ((v3MaxA.x - v3MinA.x) - (v3MaxB.x - v3MinB.x)) < 0.0f ? 1 : -1;
                }
                else if(nSplitAxis == 1)
                {
                    return ((v3MaxA.y - v3MinA.y) - (v3MaxB.y - v3MinB.y)) < 0.0f ? 1 : -1;
                }
                else if(nSplitAxis == 2)
                {
                    return ((v3MaxA.z - v3MinA.z) - (v3MaxB.z - v3MinB.z)) < 0.0f ? 1 : -1;
                }

                return  Mathf.Max(v3MaxA.x - v3MinA.x, v3MaxA.y - v3MinA.y, v3MaxA.z - v3MinA.z) - 
                        Mathf.Max(v3MaxB.x - v3MinB.x, v3MaxB.y - v3MinB.y, v3MaxB.z - v3MinB.z) < 0.0f ? 1 : -1;
                //return ((v3MaxA - v3MinA).sqrMagnitude - (v3MaxB - v3MinB).sqrMagnitude) < 0.0f ? 1 : -1;
            }
        }

        private MeshData()
        {
            meshDataConnectivity = new MeshDataConnectivity();
            aMaterials           = new Material[1];
            aMaterials[0]        = null;
        }

        public MeshData(Material[] aMaterials, List<int>[] alistIndices, List<VertexData> listVertexData, int nSplitCloseSubMesh, Vector3 v3Position, Quaternion qRotation, Vector3 v3Scale, Matrix4x4 mtxTransformVertices, bool bUseTransform, bool bBuildVertexHashData)
        {
            nSubMeshCount = alistIndices.Length;
            aaIndices     = new int[nSubMeshCount][];

            for(int nSubMesh = 0; nSubMesh < nSubMeshCount; nSubMesh++)
            {
                aaIndices[nSubMesh] = alistIndices[nSubMesh].ToArray();
            }

            this.nSplitCloseSubMesh = nSplitCloseSubMesh;

            aVertexData = listVertexData.ToArray();

            if(bUseTransform)
            {
                for(int i = 0; i < aVertexData.Length; i++)
                {
                    aVertexData[i].v3Vertex = mtxTransformVertices.MultiplyPoint3x4(aVertexData[i].v3Vertex);
                }
            }

            ComputeMinMax(aVertexData, ref v3Min, ref v3Max);

            this.v3Position = v3Position;
            this.qRotation  = qRotation;
            this.v3Scale    = v3Scale;

            meshDataConnectivity = new MeshDataConnectivity();

            if(bBuildVertexHashData)
            {
                BuildVertexHashData();
            }

            if(aMaterials != null)
            {
                this.aMaterials = new Material[aMaterials.Length];
                aMaterials.CopyTo(this.aMaterials, 0);
            }
            else
            {
                this.aMaterials    = new Material[1];
                this.aMaterials[0] = null;
            }
        }

        public MeshData(Transform transform, Mesh mesh, Material[] aMaterials, Matrix4x4 mtxLocalToWorld, bool bTransformVerticesToWorld, int nSplitCloseSubMesh, bool bBuildVertexHashData)
            : this(transform.position, transform.rotation, transform.localScale, mesh, aMaterials, mtxLocalToWorld, bTransformVerticesToWorld, nSplitCloseSubMesh, bBuildVertexHashData)
        {

        }

        public MeshData(Vector3 v3Position, Quaternion qRotation, Vector3 v3Scale, Mesh mesh, Material[] aMaterials, Matrix4x4 mtxLocalToWorld, bool bTransformVerticesToWorld, int nSplitCloseSubMesh, bool bBuildVertexHashData)
        {
            nSubMeshCount = mesh.subMeshCount;
            aaIndices     = new int[nSubMeshCount][];

            for(int nSubMesh = 0; nSubMesh < nSubMeshCount; nSubMesh++)
            {
                aaIndices[nSubMesh] = mesh.GetTriangles(nSubMesh);
            }

            this.nSplitCloseSubMesh = nSplitCloseSubMesh;

            aVertexData = VertexData.BuildVertexDataArray(mesh, mtxLocalToWorld, bTransformVerticesToWorld);

            ComputeMinMax(aVertexData, ref v3Min, ref v3Max);

            this.v3Position = v3Position;
            this.qRotation  = qRotation;
            this.v3Scale    = v3Scale;

            if(bTransformVerticesToWorld)
            {
                v3Scale = Vector3.one;
            }

            meshDataConnectivity = new MeshDataConnectivity();

            if(bBuildVertexHashData)
            {
                BuildVertexHashData();
            }

            if(aMaterials != null)
            {
                this.aMaterials = new Material[aMaterials.Length];
                aMaterials.CopyTo(this.aMaterials, 0);
            }
            else
            {
                this.aMaterials    = new Material[1];
                this.aMaterials[0] = null;
            }
        }

        static public MeshData CreateBoxMeshData(Vector3 v3Pos, Quaternion qRot, Vector3 v3Scale, Vector3 v3Min, Vector3 v3Max)
        {
            MeshData box = new MeshData();

            box.nSubMeshCount      = 1;
            box.aaIndices          = new int[1][];
            box.aaIndices[0]       = new int[36] {  1, 0, 3, 1, 3, 2, 4, 5, 7, 5, 6, 7,    0, 4, 3, 4, 7, 3, 7, 2, 3, 7, 6, 2,    5, 0, 1, 5, 4, 0, 6, 1, 2, 6, 5, 1 };
            box.nSplitCloseSubMesh = 0;

            Vector3[] aVertices = new Vector3[8] {  new Vector3(v3Min.x, v3Min.y, v3Min.z), new Vector3(v3Min.x, v3Min.y, v3Max.z), new Vector3(v3Max.x, v3Min.y, v3Max.z), new Vector3(v3Max.x, v3Min.y, v3Min.z), 
                                                    new Vector3(v3Min.x, v3Max.y, v3Min.z), new Vector3(v3Min.x, v3Max.y, v3Max.z), new Vector3(v3Max.x, v3Max.y, v3Max.z), new Vector3(v3Max.x, v3Max.y, v3Min.z) };

            box.aVertexData = new VertexData[aVertices.Length];

            for(int v = 0; v < aVertices.Length; v++)
            {
                box.aVertexData[v] = new VertexData(v);
                box.aVertexData[v].v3Vertex = aVertices[v];
            }

            box.v3Position = v3Pos;
            box.qRotation  = qRot;
            box.v3Scale    = v3Scale;

            box.v3Min = v3Min;
            box.v3Max = v3Max;

            box.nCurrentVertexHash = 8;

            return box;
        }

        public MeshData GetDeepCopy()
        {
            MeshData meshDataCopy = new MeshData();

            meshDataCopy.nSubMeshCount = nSubMeshCount;
            meshDataCopy.aaIndices     = new int[nSubMeshCount][];

            for(int nSubMesh = 0; nSubMesh < nSubMeshCount; nSubMesh++)
            {
                meshDataCopy.aaIndices[nSubMesh] = new int[aaIndices[nSubMesh].Length];
                aaIndices[nSubMesh].CopyTo(meshDataCopy.aaIndices[nSubMesh], 0);
            }

            meshDataCopy.nSplitCloseSubMesh = nSplitCloseSubMesh;

            meshDataCopy.aVertexData = new VertexData[aVertexData.Length];
            aVertexData.CopyTo(meshDataCopy.aVertexData, 0);

            meshDataCopy.v3Position = v3Position;
            meshDataCopy.qRotation  = qRotation;
            meshDataCopy.v3Scale    = v3Scale;

            meshDataCopy.v3Min = v3Min;
            meshDataCopy.v3Max = v3Max;

            meshDataCopy.nCurrentVertexHash = nCurrentVertexHash;

            meshDataCopy.meshDataConnectivity = meshDataConnectivity.GetDeepCopy();

            meshDataCopy.aMaterials = new Material[aMaterials.Length];
            aMaterials.CopyTo(meshDataCopy.aMaterials, 0);

            return meshDataCopy;
        }

        public bool FillMeshFilter(MeshFilter meshFilter, bool bTransformVerticesToLocal)
        {
            meshFilter.transform.position   = v3Position;
            meshFilter.transform.rotation   = qRotation;
            meshFilter.transform.localScale = v3Scale;

            meshFilter.sharedMesh = new Mesh();

            Mesh mesh = meshFilter.sharedMesh;

            VertexData.SetMeshDataFromVertexDataArray(meshFilter, this, bTransformVerticesToLocal);

            mesh.subMeshCount = nSubMeshCount;

            for(int nSubMesh = 0; nSubMesh < nSubMeshCount; nSubMesh++)
            {
                mesh.SetTriangles(aaIndices[nSubMesh], nSubMesh);
            }

            mesh.RecalculateBounds();
            mesh.Optimize();

            return true;
        }

        public static void ComputeMinMax(IEnumerable<VertexData> VertexData, ref Vector3 v3Min, ref Vector3 v3Max)
        {
            v3Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            v3Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach(VertexData vdata in VertexData)
            {
                if(vdata.v3Vertex.x < v3Min.x) v3Min.x = vdata.v3Vertex.x;
                if(vdata.v3Vertex.y < v3Min.y) v3Min.y = vdata.v3Vertex.y;
                if(vdata.v3Vertex.z < v3Min.z) v3Min.z = vdata.v3Vertex.z;
                if(vdata.v3Vertex.x > v3Max.x) v3Max.x = vdata.v3Vertex.x;
                if(vdata.v3Vertex.y > v3Max.y) v3Max.y = vdata.v3Vertex.y;
                if(vdata.v3Vertex.z > v3Max.z) v3Max.z = vdata.v3Vertex.z;
            }
        }

        private void BuildVertexHashData()
        {
            // We are going to build the vertex hash data. This will allow us to know which vertices actually share the same coordinates, but have different vertex data.

            List<Dictionary<int, Vector3>> listdicVertexHash2Vertex; // For each spacial subdivision, a dictionary from hash->vertex
            List<float>                    listYTops;                // This will contain entries where the different space subdivisions (Y level) are made.

            listdicVertexHash2Vertex = new List<Dictionary<int, Vector3>>();
            listYTops                = new List<float>();

            // Subdivide space

            int nSubdivisions = (aVertexData.Length / Parameters.VERTICESSPACESUBDIVISION) + 1;

            for(int nSubDiv = 0; nSubDiv < nSubdivisions; nSubDiv++)
            {
                float fTop = v3Min.y + (((float)(nSubDiv + 1) / (float)nSubdivisions) * (v3Max.y - v3Min.y));

                listdicVertexHash2Vertex.Add(new Dictionary<int, Vector3>());
                listYTops.Add(fTop);
            }

            // Look for replicated vertices

            int[] aSpacesToLookIn = new int[3];

            nCurrentVertexHash = 0;

            int nUniqueVertices = 0;

            for(int nVertexIndex = 0; nVertexIndex < aVertexData.Length; nVertexIndex++)
            {
                Vector3 v3Vertex = aVertexData[nVertexIndex].v3Vertex;

                int nSpaceYIndex = Mathf.FloorToInt(((v3Vertex.y - v3Min.y) / (v3Max.y - v3Min.y)) * listYTops.Count);

                if(nSpaceYIndex < 0) nSpaceYIndex = 0;
                if(nSpaceYIndex >= listYTops.Count) nSpaceYIndex = listYTops.Count - 1;

                aSpacesToLookIn[0] = nSpaceYIndex;
                aSpacesToLookIn[1] = -1;
                aSpacesToLookIn[2] = -1;

                int nSpacesToLookIn = 1;

                // We'll test the nearest one as well in case it's very close to the border and we may have floating point errors

                if(Mathf.Abs(listYTops[nSpaceYIndex] - v3Vertex.y) < Parameters.EPSILONDISTANCEPLANE)
                {
                    // Almost at the top?

                    if(nSpaceYIndex < listYTops.Count - 1)
                    {
                        aSpacesToLookIn[nSpacesToLookIn++] = nSpaceYIndex + 1;
                    }
                }

                if(nSpaceYIndex > 0)                    
                {
                    if(Mathf.Abs(listYTops[nSpaceYIndex - 1] - v3Vertex.y) < Parameters.EPSILONDISTANCEPLANE)
                    {
                        // Almost at the bottom?
                        aSpacesToLookIn[nSpacesToLookIn++] = nSpaceYIndex - 1;
                    }
                }

                int nVertexHash = -1;

                for(int nSpace = 0; nSpace < aSpacesToLookIn.Length; nSpace++)
                {
                    int nSpaceY = aSpacesToLookIn[nSpace];
                    if(nSpaceY == -1) continue;

                    foreach(KeyValuePair<int, Vector3> hash2VertexPair in listdicVertexHash2Vertex[nSpaceY])
                    {
                        if(Vector3.Distance(hash2VertexPair.Value, v3Vertex) < Parameters.EPSILONDISTANCEVERTEX)
                        {
                            nVertexHash = hash2VertexPair.Key;
                            break;
                        }
                    }
                }

                if(nVertexHash == -1)
                {
                    int nNewHash = nCurrentVertexHash++;
                    listdicVertexHash2Vertex[nSpaceYIndex].Add(nNewHash, v3Vertex);
                    aVertexData[nVertexIndex].nVertexHash = nNewHash;

                    nUniqueVertices++;
                }
                else
                {
                    aVertexData[nVertexIndex].nVertexHash = nVertexHash;
                }
            }
        }

        public bool GetSharedFacesArea(FracturedObject fracturedComponent, MeshData meshData2, out float fSharedArea)
        {
            fSharedArea = 0.0f;
            bool bHasSharedFaces = false;

            foreach(int nHash in meshDataConnectivity.dicHash2FaceList.Keys)
            {
                if(meshData2.meshDataConnectivity.dicHash2FaceList.ContainsKey(nHash))
                {
                    // Both faces come from the same split, but the area may be different (due to clipping)

                    foreach(MeshDataConnectivity.Face face1 in meshDataConnectivity.dicHash2FaceList[nHash])
                    {
                        Vector3 v1 = aVertexData[aaIndices[face1.nSubMesh][face1.nFaceIndex * 3 + 0]].v3Vertex;
                        Vector3 v2 = aVertexData[aaIndices[face1.nSubMesh][face1.nFaceIndex * 3 + 1]].v3Vertex;
                        Vector3 v3 = aVertexData[aaIndices[face1.nSubMesh][face1.nFaceIndex * 3 + 2]].v3Vertex;

                        float fArea1 = Vector3.Cross(v3 - v1, v2 - v1).magnitude;

                        foreach(MeshDataConnectivity.Face face2 in meshData2.meshDataConnectivity.dicHash2FaceList[nHash])
                        {
                            Vector3 v3Other1 = meshData2.aVertexData[meshData2.aaIndices[face2.nSubMesh][face2.nFaceIndex * 3 + 0]].v3Vertex;
                            Vector3 v3Other2 = meshData2.aVertexData[meshData2.aaIndices[face2.nSubMesh][face2.nFaceIndex * 3 + 1]].v3Vertex;
                            Vector3 v3Other3 = meshData2.aVertexData[meshData2.aaIndices[face2.nSubMesh][face2.nFaceIndex * 3 + 2]].v3Vertex;

                            float fArea2 = Vector3.Cross(v3Other3 - v3Other1, v3Other2 - v3Other1).magnitude;

                            bool bConnected = false;

                            if(Face2InsideFace1(fracturedComponent, this, meshData2, face1, face2))
                            {
                                bConnected = true;
                            }
                            else if(Face2InsideFace1(fracturedComponent, meshData2, this, face2, face1))
                            {
                                bConnected = true;
                            }

                            if(bConnected)
                            {
                                fSharedArea += Mathf.Min(fArea1, fArea2);
                                bHasSharedFaces = true;
                            }
                        }
                    }
                }
            }

            return bHasSharedFaces;
        }

        private static bool Face2InsideFace1(FracturedObject fracturedComponent, MeshData meshData1, MeshData meshData2, MeshDataConnectivity.Face face1, MeshDataConnectivity.Face face2)
        {
            if(fracturedComponent.FracturePattern == FracturedObject.EFracturePattern.BSP)
            {
                if(meshData1.meshDataConnectivity.dicFace2IsClipped.ContainsKey(face1) == false && meshData2.meshDataConnectivity.dicFace2IsClipped.ContainsKey(face2) == false)
                {
                    // Same face, because both haven't been clipped further
                    return true;
                }
            }

            Vector3 v1 = meshData1.aVertexData[meshData1.aaIndices[face1.nSubMesh][face1.nFaceIndex * 3 + 0]].v3Vertex;
            Vector3 v2 = meshData1.aVertexData[meshData1.aaIndices[face1.nSubMesh][face1.nFaceIndex * 3 + 1]].v3Vertex;
            Vector3 v3 = meshData1.aVertexData[meshData1.aaIndices[face1.nSubMesh][face1.nFaceIndex * 3 + 2]].v3Vertex;

            Vector3 v3Forward = -Vector3.Cross(v2 - v1, v3 - v1);
            float   fArea1    = v3Forward.magnitude;

            if(fArea1 < Parameters.EPSILONCROSSPRODUCT)
            {
                return false;
            }

            Quaternion qFace     = Quaternion.LookRotation(v3Forward.normalized, (v2 - v1).normalized);
            Matrix4x4  mtxToFace = Matrix4x4.TRS(v1, qFace, Vector3.one).inverse;

            Vector3 v3Other1 = meshData2.aVertexData[meshData2.aaIndices[face2.nSubMesh][face2.nFaceIndex * 3 + 0]].v3Vertex;
            Vector3 v3Other2 = meshData2.aVertexData[meshData2.aaIndices[face2.nSubMesh][face2.nFaceIndex * 3 + 1]].v3Vertex;
            Vector3 v3Other3 = meshData2.aVertexData[meshData2.aaIndices[face2.nSubMesh][face2.nFaceIndex * 3 + 2]].v3Vertex;

            // We are going to check if the center is inside the face, doing it in face1 local coordinates

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

            // Test if the two triangles intersect in 2D

            if(bFaceConnected == false)
            {
                // Try intersecting lines

                Vector3 v3OtherLocal1 = mtxToFace.MultiplyPoint3x4(v3Other1);
                Vector3 v3OtherLocal2 = mtxToFace.MultiplyPoint3x4(v3Other2);
                Vector3 v3OtherLocal3 = mtxToFace.MultiplyPoint3x4(v3Other3);

                if(bFaceConnected == false) if(Fracturer.IntersectEdges2D(v3OtherLocal1.x, v3OtherLocal1.y, v3OtherLocal2.x, v3OtherLocal2.y, v3Local1.x, v3Local1.y, v3Local2.x, v3Local2.y)) bFaceConnected = true;
                if(bFaceConnected == false) if(Fracturer.IntersectEdges2D(v3OtherLocal1.x, v3OtherLocal1.y, v3OtherLocal2.x, v3OtherLocal2.y, v3Local2.x, v3Local2.y, v3Local3.x, v3Local3.y)) bFaceConnected = true;
                if(bFaceConnected == false) if(Fracturer.IntersectEdges2D(v3OtherLocal1.x, v3OtherLocal1.y, v3OtherLocal2.x, v3OtherLocal2.y, v3Local3.x, v3Local3.y, v3Local1.x, v3Local1.y)) bFaceConnected = true;

                if(bFaceConnected == false) if(Fracturer.IntersectEdges2D(v3OtherLocal2.x, v3OtherLocal2.y, v3OtherLocal3.x, v3OtherLocal3.y, v3Local1.x, v3Local1.y, v3Local2.x, v3Local2.y)) bFaceConnected = true;
                if(bFaceConnected == false) if(Fracturer.IntersectEdges2D(v3OtherLocal2.x, v3OtherLocal2.y, v3OtherLocal3.x, v3OtherLocal3.y, v3Local2.x, v3Local2.y, v3Local3.x, v3Local3.y)) bFaceConnected = true;
                if(bFaceConnected == false) if(Fracturer.IntersectEdges2D(v3OtherLocal2.x, v3OtherLocal2.y, v3OtherLocal3.x, v3OtherLocal3.y, v3Local3.x, v3Local3.y, v3Local1.x, v3Local1.y)) bFaceConnected = true;

                if(bFaceConnected == false) if(Fracturer.IntersectEdges2D(v3OtherLocal3.x, v3OtherLocal3.y, v3OtherLocal1.x, v3OtherLocal1.y, v3Local1.x, v3Local1.y, v3Local2.x, v3Local2.y)) bFaceConnected = true;
                if(bFaceConnected == false) if(Fracturer.IntersectEdges2D(v3OtherLocal3.x, v3OtherLocal3.y, v3OtherLocal1.x, v3OtherLocal1.y, v3Local2.x, v3Local2.y, v3Local3.x, v3Local3.y)) bFaceConnected = true;
                if(bFaceConnected == false) if(Fracturer.IntersectEdges2D(v3OtherLocal3.x, v3OtherLocal3.y, v3OtherLocal1.x, v3OtherLocal1.y, v3Local3.x, v3Local3.y, v3Local1.x, v3Local1.y)) bFaceConnected = true;
            }

            return bFaceConnected;
        }

        static public List<MeshData> PostProcessConnectivity(MeshData meshDataSource, MeshFaceConnectivity connectivity, MeshDataConnectivity meshConnectivity, List<int>[] alistIndices, List<VertexData> listVertexData, int nSplitCloseSubMesh, int nCurrentVertexHash, bool bTransformToLocal)
        {
            List<MeshData> listMeshData = new List<MeshData>();

            // Build some lists

            List<int>[]      alistFacesRemainingInOut = new List<int>[alistIndices.Length];
            int[]            aLinearFaceIndexStart    = new int[alistIndices.Length];
            int              nLinearFaceIndexStart    = 0;
            List<int>[]      alistIndicesOut          = new List<int>[alistIndices.Length];
            List<VertexData> listVertexDataOut        = new List<VertexData>();

            Dictionary<int, int> dicVertexRemap       = new Dictionary<int, int>();

            for(int nSubMesh = 0; nSubMesh < alistIndices.Length; nSubMesh++)
            {
                // Remaining faces to process

                alistFacesRemainingInOut[nSubMesh] = new List<int>();

                for(int nFace = 0; nFace < alistIndices[nSubMesh].Count / 3; nFace++)
                {
                    alistFacesRemainingInOut[nSubMesh].Add(nFace);
                }

                // Face index start list

                aLinearFaceIndexStart[nSubMesh] = nLinearFaceIndexStart;
                nLinearFaceIndexStart += alistIndices[nSubMesh].Count / 3;

                // Mesh indices out

                alistIndicesOut[nSubMesh] = new List<int>();
            }

            // Process

            while(StillHasFacesToProcess(alistFacesRemainingInOut))
            {
                // Isolete objects

                for(int nSubMesh = 0; nSubMesh < alistFacesRemainingInOut.Length; nSubMesh++)
                {
                    if(alistFacesRemainingInOut[nSubMesh].Count > 0)
                    {
                        dicVertexRemap.Clear();
                        listVertexDataOut.Clear();
                        MeshDataConnectivity meshConnectivityOut = new MeshDataConnectivity();

                        for(int i = 0; i < alistIndices.Length; i++)
                        {
                            alistIndicesOut[i].Clear();
                        }

                        // Start process

                        LookForClosedObjectRecursive(connectivity, meshConnectivity, nSubMesh, alistFacesRemainingInOut[nSubMesh][0], alistIndices, listVertexData, alistFacesRemainingInOut, aLinearFaceIndexStart, alistIndicesOut, listVertexDataOut, dicVertexRemap, meshConnectivityOut);

                        // Build transform

                        Vector3 v3Min = Vector3.zero, v3Max = Vector3.zero;
                        MeshData.ComputeMinMax(listVertexDataOut, ref v3Min, ref v3Max);
                        Vector3 v3Center = (v3Min + v3Max) * 0.5f;

                        Matrix4x4 mtxTransformVertices = Matrix4x4.TRS(v3Center, meshDataSource.qRotation, meshDataSource.v3Scale);

                        // Add new meshData

                        MeshData newMeshData = new MeshData(meshDataSource.aMaterials, alistIndicesOut, listVertexDataOut, nSplitCloseSubMesh, v3Center, meshDataSource.qRotation, meshDataSource.v3Scale, mtxTransformVertices, bTransformToLocal, false);
                        newMeshData.meshDataConnectivity = meshConnectivityOut;
                        newMeshData.nCurrentVertexHash   = nCurrentVertexHash;

                        listMeshData.Add(newMeshData);
                    }
                }
            }

            return listMeshData;
        }

        static private bool StillHasFacesToProcess(List<int>[] alistFacesRemaining)
        {
            foreach(List<int> listFaces in alistFacesRemaining)
            {
                if(listFaces.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        static private void LookForClosedObjectRecursive(MeshFaceConnectivity connectivity, MeshDataConnectivity meshConnectivity, int nSubMeshStart, int nFaceSubMeshStart, List<int>[] alistIndicesIn, List<VertexData> listVertexDataIn, List<int>[] alistFacesRemainingInOut, int[] aLinearFaceIndexStart, List<int>[] alistIndicesOut, List<VertexData> listVertexDataOut, Dictionary<int, int> dicVertexRemap, MeshDataConnectivity meshConnectivityOut)
        {
            MeshFaceConnectivity.TriangleData triangle = connectivity.listTriangles[aLinearFaceIndexStart[nSubMeshStart] + nFaceSubMeshStart];

            // Check if we already visited this triangle

            if(triangle.bVisited)
            {
                return;
            }

            // Add vertex data of this face. Add new indices in new list and remove processed face from remaining face list.

            meshConnectivityOut.NotifyRemappedFace(meshConnectivity, nSubMeshStart, nFaceSubMeshStart, nSubMeshStart, alistIndicesOut[nSubMeshStart].Count / 3);

            for(int i = 0; i < 3; i++)
            {
                int nIndexIn = alistIndicesIn[nSubMeshStart][nFaceSubMeshStart * 3 + i];

                if(dicVertexRemap.ContainsKey(nIndexIn))
                {
                    alistIndicesOut[nSubMeshStart].Add(dicVertexRemap[nIndexIn]);
                }
                else
                {
                    int nNewIndexOut = listVertexDataOut.Count;
                    alistIndicesOut[nSubMeshStart].Add(nNewIndexOut);
                    listVertexDataOut.Add(listVertexDataIn[nIndexIn].Copy());
                    dicVertexRemap.Add(nIndexIn, nNewIndexOut);
                }
            }

            alistFacesRemainingInOut[nSubMeshStart].Remove(nFaceSubMeshStart);

            triangle.bVisited = true;

            // Recurse into adjacent triangles

            for(int i = 0; i < 3; i++)
            {
                MeshFaceConnectivity.TriangleData triangleAdjacent = null;

                for(int nSide = 0; nSide < triangle.alistNeighborSubMeshes[i].Count; nSide++)
                {
                    if(triangle.alistNeighborSubMeshes[i][nSide] != -1 && triangle.alistNeighborTriangles[i][nSide] != -1)
                    {
                        triangleAdjacent = connectivity.listTriangles[aLinearFaceIndexStart[triangle.alistNeighborSubMeshes[i][nSide]] + triangle.alistNeighborTriangles[i][nSide]];
                    }

                    if(triangleAdjacent != null)
                    {
                        if(triangleAdjacent.bVisited != true)
                        {
                            LookForClosedObjectRecursive(connectivity, meshConnectivity, triangleAdjacent.nSubMesh, triangleAdjacent.nTriangle, alistIndicesIn, listVertexDataIn, alistFacesRemainingInOut, aLinearFaceIndexStart, alistIndicesOut, listVertexDataOut, dicVertexRemap, meshConnectivityOut);
                        }
                    }
                }
            }
        }
    }
}