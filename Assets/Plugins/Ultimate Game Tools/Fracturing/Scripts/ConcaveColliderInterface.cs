using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace UltimateFracturing
{
    public static class ConcaveColliderInterface
    {
        [StructLayout(LayoutKind.Sequential)]
        struct SConvexDecompositionInfoInOut
        {
            public uint     uMaxHullVertices;
            public uint     uMaxHulls;
            public float    fPrecision;
            public float    fBackFaceDistanceFactor;
            public uint     uLegacyDepth;
            public uint     uNormalizeInputMesh;
            public uint     uUseFastVersion;

            public uint     uTriangleCount;
            public uint     uVertexCount;

            // Out parameters

            public int      nHullsOut;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SConvexDecompositionHullInfo
        {
            public int      nVertexCount;
            public int      nTriangleCount;
        };

        public delegate void LogDelegate([MarshalAs(UnmanagedType.LPStr)]string message);

        [DllImport("ConvexDecompositionDll")]
        private static extern void DllInit(bool bUseMultithreading);

        [DllImport("ConvexDecompositionDll")]
        private static extern void DllClose();

        [DllImport("ConvexDecompositionDll")]
        private static extern void SetLogFunctionPointer(IntPtr pfnUnity3DLog);

        [DllImport("ConvexDecompositionDll")]
        private static extern void SetProgressFunctionPointer(IntPtr pfnUnity3DProgress);

        [DllImport("ConvexDecompositionDll")]
        private static extern void CancelConvexDecomposition();

        [DllImport("ConvexDecompositionDll")]
        private static extern bool DoConvexDecomposition(ref SConvexDecompositionInfoInOut infoInOut, Vector3[] pfVertices, int[] puIndices);

        [DllImport("ConvexDecompositionDll")]
        private static extern bool GetHullInfo(uint uHullIndex, ref SConvexDecompositionHullInfo infoOut);

        [DllImport("ConvexDecompositionDll")]
        private static extern bool FillHullMeshData(uint uHullIndex, ref float pfVolumeOut, int[] pnIndicesOut, Vector3[] pfVerticesOut);

        public static int ComputeHull(GameObject gameObject, FracturedObject fracturedObject)
        {
            int nTotalTriangles = 0;

            if(ComputeHull(gameObject, fracturedObject.ConcaveColliderAlgorithm, fracturedObject.ConcaveColliderMaxHulls, fracturedObject.ConcaveColliderMaxHullVertices, fracturedObject.ConcaveColliderLegacySteps, fracturedObject.Verbose, out nTotalTriangles) == false)
            {
                if(fracturedObject.ConcaveColliderAlgorithm == FracturedObject.ECCAlgorithm.Fast)
                {
                    // Fast failed. Try with normal.
                    if(fracturedObject.Verbose)
                    {
                        Debug.Log(gameObject.name + ": Falling back to normal convex decomposition algorithm");
                    }

                    if(ComputeHull(gameObject, FracturedObject.ECCAlgorithm.Normal, fracturedObject.ConcaveColliderMaxHulls, fracturedObject.ConcaveColliderMaxHullVertices, fracturedObject.ConcaveColliderLegacySteps, fracturedObject.Verbose, out nTotalTriangles) == false)
                    {
                        if(fracturedObject.Verbose)
                        {
                            Debug.Log(gameObject.name + ": Falling back to box collider");
                        }
                    }
                }
            }

            return nTotalTriangles;
        }

        private static bool ComputeHull(GameObject gameObject, FracturedObject.ECCAlgorithm eAlgorithm, int nMaxHulls, int nMaxHullVertices, int nLegacySteps, bool bVerbose, out int nTotalTrianglesOut)
        {
            MeshFilter theMesh = (MeshFilter)gameObject.GetComponent<MeshFilter>();

            DllInit(true);
            SetLogFunctionPointer(Marshal.GetFunctionPointerForDelegate(new LogDelegate(Log)));

            SConvexDecompositionInfoInOut info = new SConvexDecompositionInfoInOut();

            nTotalTrianglesOut = 0;

            if(theMesh)
            {
                if(theMesh.sharedMesh)
                {
                    uint uLegacySteps = (uint)(Mathf.Max(1, nLegacySteps));

                    info.uMaxHullVertices        = (uint)(Mathf.Max(3, nMaxHullVertices));
                    info.uMaxHulls               = (uint)nMaxHulls;
                    info.fPrecision              = 0.2f;
                    info.fBackFaceDistanceFactor = 0.2f;
                    info.uLegacyDepth            = eAlgorithm == FracturedObject.ECCAlgorithm.Legacy ? uLegacySteps : 0;
                    info.uNormalizeInputMesh     = 0;
                    info.uUseFastVersion         = eAlgorithm == FracturedObject.ECCAlgorithm.Fast ? (uint)1 : (uint)0;

                    info.uTriangleCount          = (uint)theMesh.sharedMesh.triangles.Length / 3;
                    info.uVertexCount            = (uint)theMesh.sharedMesh.vertexCount;

                    Vector3[] av3Vertices = theMesh.sharedMesh.vertices;

                    if(DoConvexDecomposition(ref info, av3Vertices, theMesh.sharedMesh.triangles))
                    {
                        for(int nHull = 0; nHull < info.nHullsOut; nHull++)
                        {
                            SConvexDecompositionHullInfo hullInfo = new SConvexDecompositionHullInfo();
                            GetHullInfo((uint)nHull, ref hullInfo);

                            if(hullInfo.nTriangleCount > 0)
                            {
                                Vector3[] hullVertices = new Vector3[hullInfo.nVertexCount];
                                int[]     hullIndices  = new int[hullInfo.nTriangleCount * 3];
                            
                                float fHullVolume = -1.0f;

                                FillHullMeshData((uint)nHull, ref fHullVolume, hullIndices, hullVertices);

                                Mesh hullMesh = new Mesh();
                                hullMesh.vertices  = hullVertices;
                                hullMesh.triangles = hullIndices;
                                hullMesh.uv        = new Vector2[hullVertices.Length];
                                hullMesh.RecalculateNormals();

                                GameObject goNewHull = new GameObject("Hull " + (nHull + 1));
                                goNewHull.transform.position   = gameObject.transform.position;
                                goNewHull.transform.rotation   = gameObject.transform.rotation;
                                goNewHull.transform.localScale = gameObject.transform.localScale;
                                goNewHull.transform.parent     = gameObject.transform;
                                goNewHull.layer                = gameObject.layer;
                                MeshCollider meshCollider = goNewHull.AddComponent<MeshCollider>() as MeshCollider;

                                meshCollider.sharedMesh = null;
                                meshCollider.sharedMesh = hullMesh;
                                meshCollider.convex     = true;

                                nTotalTrianglesOut += hullInfo.nTriangleCount;
                            }
                            else
                            {
                                if(bVerbose)
                                {
                                    Debug.LogWarning(gameObject.name + ": Error generating collider. ComputeHull() returned 0 triangles.");
                                }
                            }
                        }

                        if(info.nHullsOut < 0 && bVerbose)
                        {
                            Debug.LogWarning(gameObject.name + ": Error generating collider. ComputeHull() returned no hulls.");
                        }
                    }
                    else
                    {
                        if(bVerbose)
                        {
                            Debug.LogWarning(gameObject.name + ": Error generating collider. ComputeHull() returned false.");
                        }
                    }
                }
            }

            DllClose();
            return nTotalTrianglesOut > 0;
        }

        static void Log(string message)
        {
            Debug.Log(message);
        }
    }
}