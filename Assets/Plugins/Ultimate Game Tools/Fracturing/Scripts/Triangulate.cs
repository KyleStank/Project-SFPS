using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UltimateFracturing
{
    static partial class Fracturer
    {
        static void TriangulateConstrainedDelaunay( List<List<Vector3>> listlistPointsConstrainedDelaunay, List<List<int>> listlistHashValuesConstrainedDelaunay, bool bForceVertexSoup, FracturedObject fracturedComponent,
                                                    bool bConnectivityPostprocess, MeshFaceConnectivity faceConnectivityPos, MeshFaceConnectivity faceConnectivityNeg, MeshDataConnectivity meshConnectivityPos, MeshDataConnectivity meshConnectivityNeg, int nForceMeshConnectivityHash,
                                                    int nSplitCloseSubMesh, Matrix4x4 mtxPlane, Matrix4x4 mtxToLocalPos, Matrix4x4 mtxToLocalNeg, Vector3 v3CenterPos, Vector3 v3CenterNeg,
                                                    List<int>[] aListIndicesPosInOut, List<VertexData> listVertexDataPosInOut, List<int>[] aListIndicesNegInOut, List<VertexData> listVertexDataNegInOut)
        {
            // Pass to two dimensional plane:

            Matrix4x4 mtxPlaneInverse = mtxPlane.inverse;
            List<List<Poly2Tri.Point2D>>      listlistCapPoints        = new List<List<Poly2Tri.Point2D>>();
            List<List<Poly2Tri.PolygonPoint>> listlistCapPolygonPoints = new List<List<Poly2Tri.PolygonPoint>>();
            List<Poly2Tri.Polygon>            listCapPolygons          = new List<Poly2Tri.Polygon>();

            for(int i = 0; i < listlistPointsConstrainedDelaunay.Count; i++)
            {
                List<Poly2Tri.Point2D>      listCapPoints         = new List<Poly2Tri.Point2D>();
                List<Poly2Tri.PolygonPoint> listCapPolygonsPoints = new List<Poly2Tri.PolygonPoint>();

                foreach(Vector3 v3Vertex in listlistPointsConstrainedDelaunay[i])
                {
                    Vector3 v3VertexInPlane = mtxPlaneInverse.MultiplyPoint3x4(v3Vertex);
                    listCapPoints.Add        (new Poly2Tri.Point2D     (v3VertexInPlane.x, v3VertexInPlane.z));
                    listCapPolygonsPoints.Add(new Poly2Tri.PolygonPoint(v3VertexInPlane.x, v3VertexInPlane.z));
                }

                listlistCapPoints.Add(listCapPoints);
                listlistCapPolygonPoints.Add(listCapPolygonsPoints);
                listCapPolygons.Add(new Poly2Tri.Polygon(listCapPolygonsPoints));
            }

            // Remove close vertices

            float fPrecisionFix = Mathf.Max(Parameters.EPSILONCAPPRECISIONMIN, fracturedComponent.CapPrecisionFix);

            if(fPrecisionFix > 0.0f)
            {
                for(int nCap = 0; nCap < listlistCapPolygonPoints.Count; nCap++)
                {
                    double lastX = listlistCapPolygonPoints[nCap][listlistCapPolygonPoints[nCap].Count - 1].X;
                    double lastY = listlistCapPolygonPoints[nCap][listlistCapPolygonPoints[nCap].Count - 1].Y;

                    bool bDeleteCap = false;

                    for(int nVertex = 0; nVertex < listlistCapPolygonPoints[nCap].Count; nVertex++)
                    {
                        double vecX = listlistCapPolygonPoints[nCap][nVertex].X - lastX;
                        double vecY = listlistCapPolygonPoints[nCap][nVertex].Y - lastY;

                        if(System.Math.Sqrt(vecX * vecX + vecY * vecY) < fPrecisionFix)
                        {
                            listlistCapPolygonPoints[nCap].RemoveAt(nVertex);
                            nVertex--;

                            if(listlistCapPolygonPoints[nCap].Count < 3)
                            {
                                bDeleteCap = true;
                                break;
                            }
                        }
                        else
                        {
                            lastX = listlistCapPolygonPoints[nCap][nVertex].X;
                            lastY = listlistCapPolygonPoints[nCap][nVertex].Y;
                        }
                    }

                    if(bDeleteCap)
                    {
                        listlistCapPolygonPoints.RemoveAt(nCap);
                        nCap--;
                    }
                }
            }

            if(listlistCapPolygonPoints.Count == 0)
            {
                return;
            }

            // Search if one of the caps is contained in the other. If this happens we will mark the big one as the polygon and the rest as holes

            int nSuperPolygon = -1;
            Poly2Tri.Polygon polygonContainer = null;

            if(bForceVertexSoup == false)
            {
                for(int i = 0; i < listlistCapPolygonPoints.Count; i++)
                {
                    for(int j = 0; j < listlistCapPolygonPoints.Count; j++)
                    {
                        if(i != j && listlistCapPoints[i].Count >= 3 && listlistCapPoints[j].Count >= 3)
                        {
                            if(Poly2Tri.PolygonUtil.PolygonContainsPolygon(listlistCapPoints[i], listCapPolygons[i].Bounds, listlistCapPoints[j], listCapPolygons[j].Bounds, true))
                            {
                                nSuperPolygon = i;
                                break;
                            }
                            else if(Poly2Tri.PolygonUtil.PolygonContainsPolygon(listlistCapPoints[j], listCapPolygons[j].Bounds, listlistCapPoints[i], listCapPolygons[i].Bounds, true))
                            {
                                nSuperPolygon = j;
                                break;
                            }
                        }
                    }
                }

                // Add holes if this is a cap with holes

                if(nSuperPolygon != -1)
                {
                    polygonContainer = listCapPolygons[nSuperPolygon];

                    for(int i = 0; i < listlistCapPolygonPoints.Count; i++)
                    {
                        if(i != nSuperPolygon && listCapPolygons[i].Count >= 3)
                        {
                            polygonContainer.AddHole(listCapPolygons[i]);
                        }
                    }
                }
            }

            // Triangulate

            bool bTriangulatedWithHoles = false;

            if(polygonContainer != null && bForceVertexSoup == false)
            {
                // Polygon with holes

                try
                {
                    Poly2Tri.P2T.Triangulate(polygonContainer);

                    if(polygonContainer.Triangles != null)
                    {
                        List<Vector3> listMeshVertices = new List<Vector3>();
                        List<int>     listMeshIndices  = new List<int>();

                        CreateIndexedMesh(polygonContainer.Triangles, listMeshVertices, listMeshIndices, mtxPlane, true);

                        Triangulate(    listMeshVertices, listMeshIndices, fracturedComponent, listlistPointsConstrainedDelaunay, listlistHashValuesConstrainedDelaunay,
                                        bConnectivityPostprocess, faceConnectivityPos, faceConnectivityNeg, meshConnectivityPos, meshConnectivityNeg, nForceMeshConnectivityHash,
                                        nSplitCloseSubMesh, mtxPlane, mtxToLocalPos, mtxToLocalNeg, v3CenterPos, v3CenterNeg,
                                        aListIndicesPosInOut, listVertexDataPosInOut, aListIndicesNegInOut, listVertexDataNegInOut);
                    }

                    bTriangulatedWithHoles = true;
                }
                catch(System.Exception e)
                {
                    if(fracturedComponent.Verbose) Debug.LogWarning("Exception (" + e.GetType() + ") using hole triangulation (holes = " + listlistCapPolygonPoints.Count + "). Trying to use constrained delaunay.");
                    bTriangulatedWithHoles = false;
                }
            }

            if(bTriangulatedWithHoles == false)
            {
                if(bForceVertexSoup)
                {
                    // Vertex soup

                    List<Poly2Tri.TriangulationPoint> listPoints = new List<Poly2Tri.TriangulationPoint>();

                    if(listlistCapPolygonPoints.Count > 0)
                    {
                        foreach(Poly2Tri.PolygonPoint polyPoint in listlistCapPolygonPoints[0])
                        {
                            listPoints.Add(polyPoint);
                        }

                        try
                        {
                            if(listPoints.Count >= 3)
                            {
                                Poly2Tri.PointSet ps = new Poly2Tri.PointSet(listPoints);
                                Poly2Tri.P2T.Triangulate(ps);

                                if(ps.Triangles != null)
                                {
                                    List<Vector3> listMeshVertices = new List<Vector3>();
                                    List<int>     listMeshIndices  = new List<int>();

                                    CreateIndexedMesh(ps.Triangles, listMeshVertices, listMeshIndices, mtxPlane, true);

                                    Triangulate(    listMeshVertices, listMeshIndices, fracturedComponent, listlistPointsConstrainedDelaunay, listlistHashValuesConstrainedDelaunay,
                                                    bConnectivityPostprocess, faceConnectivityPos, faceConnectivityNeg, meshConnectivityPos, meshConnectivityNeg, nForceMeshConnectivityHash,
                                                    nSplitCloseSubMesh, mtxPlane, mtxToLocalPos, mtxToLocalNeg, v3CenterPos, v3CenterNeg,
                                                    aListIndicesPosInOut, listVertexDataPosInOut, aListIndicesNegInOut, listVertexDataNegInOut);
                                }
                            }
                        }
                        catch(System.Exception e)
                        {
                            if(fracturedComponent.Verbose) Debug.LogWarning("Exception (" + e.GetType() + ") using vertex soup triangulation.");
                        }
                    }
                }
                else
                {
                    // Use constrained delaunay triangulation

                    int nPoly = 0;

                    foreach(List<Poly2Tri.PolygonPoint> listPolyPoints in listlistCapPolygonPoints)
                    {
                        IList<Poly2Tri.DelaunayTriangle> listTriangles = null;

                        Poly2Tri.Polygon polygon = null;

                        try
                        {
                            if(listPolyPoints.Count >= 3)
                            {
                                polygon = new Poly2Tri.Polygon(listPolyPoints);
                                Poly2Tri.P2T.Triangulate(polygon);
                                listTriangles = polygon.Triangles;
                            }
                        }
                        catch(System.Exception e)
                        {
                            if(fracturedComponent.Verbose) Debug.LogWarning("Exception (" + e.GetType() + ") using polygon triangulation of cap polygon " + nPoly + ". Trying to use non constrained");
                            listTriangles = null;
                        }

                        if(listTriangles == null)
                        {
                            List<Poly2Tri.TriangulationPoint> listPoints = new List<Poly2Tri.TriangulationPoint>();

                            foreach(Poly2Tri.PolygonPoint polyPoint in listPolyPoints)
                            {
                                listPoints.Add(polyPoint);
                            }

                            try
                            {
                                if(listPoints.Count >= 3)
                                {
                                    Poly2Tri.PointSet ps = new Poly2Tri.PointSet(listPoints);
                                    Poly2Tri.P2T.Triangulate(ps);
                                    listTriangles = ps.Triangles;
                                }
                            }
                            catch(System.Exception e)
                            {
                                if(fracturedComponent.Verbose) Debug.LogWarning("Exception (" + e.GetType() + ") using non constrained triangulation of cap polygon " + nPoly + ". Skipping");
                            }
                        }

                        if(listTriangles != null)
                        {
                            List<Vector3> listMeshVertices = new List<Vector3>();
                            List<int>     listMeshIndices  = new List<int>();

                            CreateIndexedMesh(listTriangles, listMeshVertices, listMeshIndices, mtxPlane, true);

                            Triangulate(    listMeshVertices, listMeshIndices, fracturedComponent, listlistPointsConstrainedDelaunay, listlistHashValuesConstrainedDelaunay,
                                            bConnectivityPostprocess, faceConnectivityPos, faceConnectivityNeg, meshConnectivityPos, meshConnectivityNeg, nForceMeshConnectivityHash,
                                            nSplitCloseSubMesh, mtxPlane, mtxToLocalPos, mtxToLocalNeg, v3CenterPos, v3CenterNeg,
                                            aListIndicesPosInOut, listVertexDataPosInOut, aListIndicesNegInOut, listVertexDataNegInOut);
                        }

                        nPoly++;
                    }
                }
            }
        }

        static void CreateIndexedMesh(IList<Poly2Tri.DelaunayTriangle> listTriangles, List<Vector3> listVerticesOut, List<int> listIndicesOut, Matrix4x4 mtxTransform, bool bTransform)
        {
            listVerticesOut.Clear();
            listIndicesOut.Clear();

            Vector3 v3Vertex = Vector3.zero;

            foreach(Poly2Tri.DelaunayTriangle triangle in listTriangles)
            {
                for(int i = 0; i < 3; i++)
                {
                    bool bFound = false;
                    int  nIndex = 0;

                    Poly2Tri.TriangulationPoint point = triangle.PointCWFrom(triangle.Points[i]);
                    v3Vertex.x = point.Xf;
                    v3Vertex.z = point.Yf;

                    foreach(Vector3 v3VertexIndexed in listVerticesOut)
                    {
                        if((v3Vertex - v3VertexIndexed).magnitude < Parameters.EPSILONDISTANCEVERTEX)
                        {
                            bFound = true;
                            break;   
                        }

                        nIndex++;
                    }

                    if(bFound == false)
                    {
                        listIndicesOut.Add(listVerticesOut.Count);
                        listVerticesOut.Add(v3Vertex);
                    }
                    else
                    {
                        listIndicesOut.Add(nIndex);
                    }
                }
            }

            if(bTransform)
            {
                for(int i = 0; i < listVerticesOut.Count; i++)
                {
                    listVerticesOut[i] = mtxTransform.MultiplyPoint3x4(listVerticesOut[i]);
                }
            }
        }

        static void Triangulate(    List<Vector3> listVertices, List<int> listIndices, FracturedObject fracturedComponent, List<List<Vector3>> listlistPointsConstrainedDelaunay, List<List<int>> listlistHashValuesConstrainedDelaunay,
                                    bool bConnectivityPostprocess, MeshFaceConnectivity faceConnectivityPos, MeshFaceConnectivity faceConnectivityNeg, MeshDataConnectivity meshConnectivityPos, MeshDataConnectivity meshConnectivityNeg, int nForceMeshConnectivityHash,
                                    int nSplitCloseSubMesh, Matrix4x4 mtxPlane, Matrix4x4 mtxToLocalPos, Matrix4x4 mtxToLocalNeg, Vector3 v3CenterPos, Vector3 v3CenterNeg,
                                    List<int>[] aListIndicesPosInOut, List<VertexData> listVertexDataPosInOut, List<int>[] aListIndicesNegInOut, List<VertexData> listVertexDataNegInOut)
        {
            int nPositiveSideIndexStart = listVertexDataPosInOut.Count;
            int nNegativeSideIndexStart = listVertexDataNegInOut.Count;

            if(listVertexDataPosInOut.Count < 1 || listVertexDataNegInOut.Count < 1)
            {
                return;
            }

            // Add vertex data

            VertexData[] aVtxDataCapPos = new VertexData[listVertices.Count];
            VertexData[] aVtxDataCapNeg = new VertexData[listVertices.Count];

            Vector3 v3PlaneNormal = mtxPlane.MultiplyVector(Vector3.up);

            float fReversedCapNormals = fracturedComponent.InvertCapNormals ? -1.0f : 1.0f;

            Vector3 v3CapNormalLocalPos = mtxToLocalPos.MultiplyVector(-v3PlaneNormal * fReversedCapNormals);
            Vector3 v3CapNormalLocalNeg = mtxToLocalNeg.MultiplyVector( v3PlaneNormal * fReversedCapNormals);

            Vector3 v3TangentPos = Vector3.right;
            v3TangentPos = mtxPlane.MultiplyVector(v3TangentPos);
            v3TangentPos = mtxToLocalPos.MultiplyVector(v3TangentPos);

            Vector3 v3TangentNeg = Vector3.right;
            v3TangentNeg = mtxPlane.MultiplyVector(v3TangentNeg);
            v3TangentNeg = mtxToLocalNeg.MultiplyVector(v3TangentNeg);

            Matrix4x4 mtxPlaneInverse = mtxPlane.inverse;

            Color32 colWhite  = new Color32(255, 255, 255, 255);
            Vector3 v2Mapping = Vector2.zero;

            for(int i = 0; i < listVertices.Count; i++)
            {
                Vector3 v3Local   = mtxPlaneInverse.MultiplyPoint3x4(listVertices[i]);
                v2Mapping.x = v3Local.x * fracturedComponent.SplitMappingTileU;
                v2Mapping.y = v3Local.z * fracturedComponent.SplitMappingTileV;

                int nVertexHash = ComputeVertexHash(listVertices[i], listlistPointsConstrainedDelaunay, listlistHashValuesConstrainedDelaunay);

                aVtxDataCapPos[i] = new VertexData(nVertexHash, listVertices[i], v3CapNormalLocalPos, v3TangentPos, colWhite, v2Mapping, v2Mapping, true, true, listVertexDataPosInOut[0].bHasColor32, listVertexDataPosInOut[0].bHasMapping1, listVertexDataPosInOut[0].bHasMapping2);
                aVtxDataCapNeg[i] = new VertexData(nVertexHash, listVertices[i], v3CapNormalLocalNeg, v3TangentNeg, colWhite, v2Mapping, v2Mapping, true, true, listVertexDataNegInOut[0].bHasColor32, listVertexDataNegInOut[0].bHasMapping1, listVertexDataNegInOut[0].bHasMapping2);
            }

            listVertexDataPosInOut.AddRange(aVtxDataCapPos);
            listVertexDataNegInOut.AddRange(aVtxDataCapNeg);

            // Add indices

            for(int i = 0; i < listIndices.Count / 3; i++)
            {
                int nTriangleA = listIndices[i * 3 + 0];
                int nTriangleB = listIndices[i * 3 + 1];
                int nTriangleC = listIndices[i * 3 + 2];

                int nHashPosA  = listVertexDataPosInOut[nPositiveSideIndexStart + nTriangleA].nVertexHash;
                int nHashPosB  = listVertexDataPosInOut[nPositiveSideIndexStart + nTriangleB].nVertexHash;
                int nHashPosC  = listVertexDataPosInOut[nPositiveSideIndexStart + nTriangleC].nVertexHash;

                int nHashNegA  = listVertexDataNegInOut[nNegativeSideIndexStart + nTriangleA].nVertexHash;
                int nHashNegB  = listVertexDataNegInOut[nNegativeSideIndexStart + nTriangleB].nVertexHash;
                int nHashNegC  = listVertexDataNegInOut[nNegativeSideIndexStart + nTriangleC].nVertexHash;

                if(nHashPosA != -1 && nHashPosB != -1 && nHashPosC != -1 && nHashNegA != -1 && nHashNegB != -1 && nHashNegC != -1)
                {
                    int nMeshConnectivityHash = nForceMeshConnectivityHash == -1 ? MeshDataConnectivity.GetNewHash() : nForceMeshConnectivityHash; // New hash value to identify the 2 shared faces

                    if(fracturedComponent.GenerateChunkConnectionInfo)
                    {
                        meshConnectivityPos.NotifyNewCapFace(nMeshConnectivityHash, nSplitCloseSubMesh, aListIndicesPosInOut[nSplitCloseSubMesh].Count / 3);
                    }

                    aListIndicesPosInOut[nSplitCloseSubMesh].Add(nPositiveSideIndexStart + nTriangleA);
                    aListIndicesPosInOut[nSplitCloseSubMesh].Add(nPositiveSideIndexStart + nTriangleB);
                    aListIndicesPosInOut[nSplitCloseSubMesh].Add(nPositiveSideIndexStart + nTriangleC);

                    if(bConnectivityPostprocess)
                    {
                        faceConnectivityPos.AddEdge(nSplitCloseSubMesh, listVertices[nTriangleA], listVertices[nTriangleB], nHashPosA, nHashPosB, nPositiveSideIndexStart + nTriangleA, nPositiveSideIndexStart + nTriangleB);
                        faceConnectivityPos.AddEdge(nSplitCloseSubMesh, listVertices[nTriangleB], listVertices[nTriangleC], nHashPosB, nHashPosC, nPositiveSideIndexStart + nTriangleB, nPositiveSideIndexStart + nTriangleC);
                        faceConnectivityPos.AddEdge(nSplitCloseSubMesh, listVertices[nTriangleC], listVertices[nTriangleA], nHashPosC, nHashPosA, nPositiveSideIndexStart + nTriangleC, nPositiveSideIndexStart + nTriangleA);
                    }

                    if(fracturedComponent.GenerateChunkConnectionInfo)
                    {
                        meshConnectivityNeg.NotifyNewCapFace(nMeshConnectivityHash, nSplitCloseSubMesh, aListIndicesNegInOut[nSplitCloseSubMesh].Count / 3);
                    }

                    aListIndicesNegInOut[nSplitCloseSubMesh].Add(nNegativeSideIndexStart + nTriangleA);
                    aListIndicesNegInOut[nSplitCloseSubMesh].Add(nNegativeSideIndexStart + nTriangleC);
                    aListIndicesNegInOut[nSplitCloseSubMesh].Add(nNegativeSideIndexStart + nTriangleB);

                    if(bConnectivityPostprocess)
                    {
                        faceConnectivityNeg.AddEdge(nSplitCloseSubMesh, listVertices[nTriangleA], listVertices[nTriangleC], nHashNegA, nHashNegC, nNegativeSideIndexStart + nTriangleA, nNegativeSideIndexStart + nTriangleC);
                        faceConnectivityNeg.AddEdge(nSplitCloseSubMesh, listVertices[nTriangleC], listVertices[nTriangleB], nHashNegC, nHashNegB, nNegativeSideIndexStart + nTriangleC, nNegativeSideIndexStart + nTriangleB);
                        faceConnectivityNeg.AddEdge(nSplitCloseSubMesh, listVertices[nTriangleB], listVertices[nTriangleA], nHashNegB, nHashNegA, nNegativeSideIndexStart + nTriangleB, nNegativeSideIndexStart + nTriangleA);
                    }
                }
            }
        }

        static int ComputeVertexHash(Vector3 v3Vertex, List<List<Vector3>> listlistPointsConstrainedDelaunay, List<List<int>> listlistHashValuesConstrainedDelaunay)
        {
            float fMinDistanceSqr = float.MaxValue;
            int   nHash           = -1;

            // Find the closest one. We could try to find them with an epsilon margin but found out it generated some errors.

            for(int i = 0; i < listlistPointsConstrainedDelaunay.Count; i++)
            {
                for(int j = 0; j < listlistPointsConstrainedDelaunay[i].Count; j++)
                {
                    float fDistanceSqr = Vector3.SqrMagnitude(listlistPointsConstrainedDelaunay[i][j] - v3Vertex);

                    if(fDistanceSqr < fMinDistanceSqr)
                    {
                        nHash = listlistHashValuesConstrainedDelaunay[i][j];
                        fMinDistanceSqr = fDistanceSqr;
                    }
                }
            }

            float fEpsilonSqr = Parameters.EPSILONCAPPRECISIONMIN * Parameters.EPSILONCAPPRECISIONMIN;

            if(fMinDistanceSqr > fEpsilonSqr)
            {
                return -1; // Invalid vertex, probably due to delaunay triangulation error 
            }

            return nHash;
        }

        static bool AddCapEdge(Dictionary<EdgeKeyByHash, CapEdge> dicCapEdges, int nVertexHash1, int nVertexHash2, Vector3 v3Vertex1, Vector3 v3Vertex2)
        {
            float fLength = Vector3.Distance(v3Vertex1, v3Vertex2);

            EdgeKeyByHash edgeKey = new EdgeKeyByHash(nVertexHash1, nVertexHash2);

            if(dicCapEdges.ContainsKey(edgeKey) == false)
            {
                dicCapEdges.Add(edgeKey, new CapEdge(nVertexHash1, nVertexHash2, v3Vertex1, v3Vertex2, fLength));
            }

            return true;
        }

        static bool ResolveCap(Dictionary<EdgeKeyByHash, CapEdge> dicCapEdges, List<List<Vector3>> listlistResolvedCapVertices, List<List<int>> listlistResolvedCapHashValues, FracturedObject fracturedComponent)
        {
            if(dicCapEdges.Count < 3)
            {
                if(fracturedComponent.Verbose) Debug.LogWarning("Cap has < 3 segments");
                return false;
            }

            listlistResolvedCapVertices.Clear();
            listlistResolvedCapHashValues.Clear();

            List<CapEdge> listResolvedCap   = new List<CapEdge>();
            List<CapEdge> listUnresolvedCap = new List<CapEdge>(dicCapEdges.Values);
            List<Vector3> newListVertices   = new List<Vector3>();
            List<int>     newListHashValues = new List<int>();

            int nTotalSegments = listUnresolvedCap.Count;

            listResolvedCap.Add(listUnresolvedCap[0]);
            newListVertices.Add(listUnresolvedCap[0].v1);
            newListVertices.Add(listUnresolvedCap[0].v2);
            newListHashValues.Add(listUnresolvedCap[0].nHash1);
            newListHashValues.Add(listUnresolvedCap[0].nHash2);
            listUnresolvedCap.RemoveAt(0);

            // Try to match all segments

            while(listUnresolvedCap.Count > 0)
            {
                // Search for a connected segment in the unresolved segment pool

                CapEdge CapEdgeLast  = listResolvedCap[listResolvedCap.Count - 1];
                CapEdge CapEdgeFirst = listResolvedCap[0];

                bool bFound = false;

                for(int nSegmentUnresolved = 0; nSegmentUnresolved < listUnresolvedCap.Count; nSegmentUnresolved++)
                {
                    CapEdge CapEdgeUnresolved = listUnresolvedCap[nSegmentUnresolved];

                    int nSharesVertexEnd   = CapEdgeLast.SharesVertex1Of (CapEdgeUnresolved);
                    int nSharesVertexStart = CapEdgeFirst.SharesVertex2Of(CapEdgeUnresolved);

                    if(nSharesVertexEnd == 2)
                    {
                        newListVertices.Add(CapEdgeUnresolved.v2);
                        newListHashValues.Add(CapEdgeUnresolved.nHash2);
                        listResolvedCap.Add(CapEdgeUnresolved);
                        listUnresolvedCap.RemoveAt(nSegmentUnresolved);
                        bFound = true;
                        break;
                    }
                    else if(nSharesVertexStart == 1)
                    {
                        newListVertices.Insert(0, CapEdgeUnresolved.v1);
                        newListHashValues.Insert(0, CapEdgeUnresolved.nHash1);
                        listResolvedCap.Insert(0, CapEdgeUnresolved);
                        listUnresolvedCap.RemoveAt(nSegmentUnresolved);
                        bFound = true;
                        break;
                    }
                }

                bool bFinish = bFound == false;

                if(listResolvedCap.Count >= 3)
                {
                    if(listResolvedCap[listResolvedCap.Count - 1].SharesVertex1Of(listResolvedCap[0]) == 2)
                    {
                        bFinish = true;
                    }
                }

                if(listUnresolvedCap.Count == 0)
                {
                    bFinish = true;
                }

                if(bFinish)
                {
                    // Finished, new cap vertex group

                    int nTotalAdd = newListVertices.Count;

                    if(Vector3.Distance(newListVertices[0], newListVertices[newListVertices.Count - 1]) < Parameters.EPSILONDISTANCEVERTEX)
                    {
                        nTotalAdd = newListVertices.Count - 1;
                    }

                    if(nTotalAdd > 2)
                    {
                        List<Vector3> listAddVertices = new List<Vector3>();
                        List<int>     listAddHash     = new List<int>();

                        for(int i = 0; i < nTotalAdd; i++)
                        {
                            listAddVertices.Add(newListVertices[i]);
                            listAddHash.Add(newListHashValues[i]);
                        }

                        listlistResolvedCapVertices.Add(listAddVertices);
                        listlistResolvedCapHashValues.Add(listAddHash);
                    }
                    else
                    {
                        if(fracturedComponent.Verbose) Debug.LogWarning("Cap group has less than 3 vertices (" + newListVertices.Count + ")");
                    }

                    if(listUnresolvedCap.Count > 0)
                    {
                        listResolvedCap.Clear();
                        listResolvedCap.Add(listUnresolvedCap[0]);
                        newListVertices.Clear();
                        newListHashValues.Clear();
                        newListVertices.Add(listUnresolvedCap[0].v1);
                        newListVertices.Add(listUnresolvedCap[0].v2);
                        newListHashValues.Add(listUnresolvedCap[0].nHash1);
                        newListHashValues.Add(listUnresolvedCap[0].nHash2);
                        listUnresolvedCap.RemoveAt(0);
                    }
                }
            }

            if(listUnresolvedCap.Count > 0)
            {
                if(fracturedComponent.Verbose) Debug.LogWarning(string.Format("Cap has {0}/{1} unresolved segments left", listUnresolvedCap.Count, nTotalSegments));
            }

            return true;
        }
    }
}
