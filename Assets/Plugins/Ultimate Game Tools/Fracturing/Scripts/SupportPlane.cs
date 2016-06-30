using UnityEngine;
using System.Collections;

namespace UltimateFracturing
{
    [System.Serializable]
    public class SupportPlane
    {
        public bool            GUIExpanded;
        public string          GUIName;
        public bool            GUIShowInScene;
        public Vector3         v3PlanePosition;
        public Quaternion      qPlaneRotation;
        public Vector3         v3PlaneScale;
        public Mesh            planeMesh;
        public FracturedObject fracturedObject;

        public SupportPlane(FracturedObject fracturedObject)
        {
            GUIExpanded    = true;
            GUIName        = "New Support Plane";
            GUIShowInScene = true;

            this.fracturedObject = fracturedObject;

            planeMesh = new Mesh();

            Vector3[] av3RectVerts = new Vector3[4];
            Vector3[] aRectNormals = new Vector3[4];
            Vector2[] aRectMapping = new Vector2[4];
            int[]     aRectIndices = { 0, 1, 2, 0, 2, 3 };

            av3RectVerts[0] = new Vector3(-1.0f, 0.0f, -1.0f);
            av3RectVerts[1] = new Vector3(-1.0f, 0.0f, +1.0f);
            av3RectVerts[2] = new Vector3(+1.0f, 0.0f, +1.0f);
            av3RectVerts[3] = new Vector3(+1.0f, 0.0f, -1.0f);

            aRectNormals[0] = new Vector3(0.0f, 1.0f, 0.0f);
            aRectNormals[1] = new Vector3(0.0f, 1.0f, 0.0f);
            aRectNormals[2] = new Vector3(0.0f, 1.0f, 0.0f);
            aRectNormals[3] = new Vector3(0.0f, 1.0f, 0.0f);

            bool  bHasRenderer = false;
            float fHeight      = 1.0f;

            if(fracturedObject.SourceObject)
            {
                if(fracturedObject.SourceObject.GetComponent<Renderer>())
                {
                    bHasRenderer = true;
                }
            }
           
            if(bHasRenderer)
            {
                Bounds bounds = fracturedObject.SourceObject.GetComponent<Renderer>().bounds;

                fHeight = bounds.extents.y;

                for(int i = 0; i < av3RectVerts.Length; i++)
                {
                    float fPlaneSize  = 1.3f;
                    float fMaxExtents = Mathf.Max(bounds.extents.z * fPlaneSize, bounds.extents.x * fPlaneSize);
                    av3RectVerts[i]   = Vector3.Scale(av3RectVerts[i], new Vector3(fMaxExtents, fMaxExtents, fMaxExtents)) + fracturedObject.transform.position;
                    av3RectVerts[i]   = fracturedObject.transform.InverseTransformPoint(av3RectVerts[i]);
                }

                v3PlanePosition = fracturedObject.transform.position - new Vector3(0.0f, fHeight - 0.05f, 0.0f);
                v3PlanePosition = fracturedObject.transform.InverseTransformPoint(v3PlanePosition);

                qPlaneRotation  = Quaternion.identity;
            }
            else
            {
                for(int i = 0; i < av3RectVerts.Length; i++)
                {
                    av3RectVerts[i] += fracturedObject.transform.position;
                    av3RectVerts[i]  = fracturedObject.transform.InverseTransformPoint(av3RectVerts[i]);
                }

                v3PlanePosition = new Vector3(0.0f, (-fHeight * 0.5f) + 0.05f, 0.0f);
                qPlaneRotation   = Quaternion.identity;
            }

            v3PlaneScale = Vector3.one;

            planeMesh.vertices  = av3RectVerts;
            planeMesh.normals   = aRectNormals;
            planeMesh.uv        = aRectMapping;
            planeMesh.triangles = aRectIndices;
        }

        public Matrix4x4 GetLocalMatrix()
        {
            return Matrix4x4.TRS(v3PlanePosition, qPlaneRotation, v3PlaneScale);
        }

        public Vector3[] GetBoundingBoxSegments(Bounds bounds)
        {
            Vector3 v3Min = bounds.min;
            Vector3 v3Max = bounds.max;

            Vector3[] av3BoxVertices = new Vector3[8];

            av3BoxVertices[0] = new Vector3(v3Min.x, v3Min.y, v3Min.z);
            av3BoxVertices[1] = new Vector3(v3Min.x, v3Min.y, v3Max.z);
            av3BoxVertices[2] = new Vector3(v3Max.x, v3Min.y, v3Max.z);
            av3BoxVertices[3] = new Vector3(v3Max.x, v3Min.y, v3Min.z);
            av3BoxVertices[4] = new Vector3(v3Min.x, v3Max.y, v3Min.z);
            av3BoxVertices[5] = new Vector3(v3Min.x, v3Max.y, v3Max.z);
            av3BoxVertices[6] = new Vector3(v3Max.x, v3Max.y, v3Max.z);
            av3BoxVertices[7] = new Vector3(v3Max.x, v3Max.y, v3Min.z);

            Vector3[] av3BoxSegments = new Vector3[24];

            for(int i = 0; i < 4; i++)
            {
                av3BoxSegments[i * 2 + 0] = av3BoxVertices[(i + 0) % 4];
                av3BoxSegments[i * 2 + 1] = av3BoxVertices[(i + 1) % 4];
            }

            for(int i = 4; i < 8; i++)
            {
                av3BoxSegments[i * 2 + 0] = av3BoxVertices[((i + 0) % 4) + 4];
                av3BoxSegments[i * 2 + 1] = av3BoxVertices[((i + 1) % 4) + 4];
            }

            for(int i = 8; i < 12; i++)
            {
                av3BoxSegments[i * 2 + 0] = av3BoxVertices[ i % 4];
                av3BoxSegments[i * 2 + 1] = av3BoxVertices[(i % 4) + 4];
            }

            return av3BoxSegments;
        }

        public bool IntersectsWith(GameObject otherGameObject, bool bBelowIsAlsoValid = false)
        {
            MeshFilter meshFilter = otherGameObject.GetComponent<MeshFilter>();

            if(planeMesh == null || meshFilter == null)
            {
                return false;
            }

            Vector3[] av3RectVerts = planeMesh.vertices;
            Matrix4x4 mtxWorld = (fracturedObject.transform.localToWorldMatrix * GetLocalMatrix());

            for(int i = 0; i < 4; i++)
            {
                // World -> other object local
                av3RectVerts[i] = mtxWorld.MultiplyPoint3x4(av3RectVerts[i]);
                av3RectVerts[i] = otherGameObject.transform.InverseTransformPoint(av3RectVerts[i]);
            }

            Plane localPlane = new Plane(av3RectVerts[0], av3RectVerts[1], av3RectVerts[2]);
            Vector3 v3Forward = (av3RectVerts[1] - av3RectVerts[0]).normalized;
            Vector3 v3Right   = (av3RectVerts[2] - av3RectVerts[1]).normalized;
            Matrix4x4 mtxToPlaneLocal = Matrix4x4.TRS(av3RectVerts[0], Quaternion.LookRotation(v3Forward, Vector3.Cross(v3Forward, v3Right)), Vector3.one).inverse;

            float fLimitRight   = (av3RectVerts[2] - av3RectVerts[1]).magnitude;
            float fLimitUp      = meshFilter.sharedMesh.bounds.max.y - meshFilter.sharedMesh.bounds.min.y;
            float fLimitForward = (av3RectVerts[1] - av3RectVerts[0]).magnitude;

            // Build box segment table

            Vector3[] av3BoxSegments = GetBoundingBoxSegments(meshFilter.sharedMesh.bounds);

            // Iterate through segments and test them with the plane

            for(int i = 0; i < 12; i++)
            {
                if(TestSegmentVsPlane(av3BoxSegments[i * 2 + 0], av3BoxSegments[i * 2 + 1], localPlane, mtxToPlaneLocal, fLimitRight, fLimitUp, fLimitForward))
                {
                    // This test only checks if the plane intersects a bounding box
                    return true;
                }
            }

            if(bBelowIsAlsoValid)
            {
                float fDistToPlane = localPlane.GetDistanceToPoint(meshFilter.sharedMesh.bounds.center);

                if(fDistToPlane < 0.0f)
                {
                    // This tests if also it is below the plane

                    for(int i = 0; i < 24; i++)
                    {
                        // Move the bounding box to intersect the plane
                        av3BoxSegments[i] = av3BoxSegments[i] + (localPlane.normal * -fDistToPlane);
                    }

                    for(int i = 0; i < 12; i++)
                    {
                        if(TestSegmentVsPlane(av3BoxSegments[i * 2 + 0], av3BoxSegments[i * 2 + 1], localPlane, mtxToPlaneLocal, fLimitRight, fLimitUp, fLimitForward))
                        {
                            // This tests if the plane intersects the translated bounding box
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool TestSegmentVsPlane(Vector3 v1, Vector3 v2, Plane plane, Matrix4x4 mtxToPlaneLocal, float fLimitRight, float fLimitUp, float fLimitForward)
        {
            float fSide1 = v1.x * plane.normal.x + v1.y * plane.normal.y + v1.z * plane.normal.z + plane.distance;
            float fSide2 = v2.x * plane.normal.x + v2.y * plane.normal.y + v2.z * plane.normal.z + plane.distance;

            if(fSide1 * fSide2 > 0.0f)
            {
                return false;
            }

            float fDistance = 0.0f;
            Ray ray = new Ray(v1, (v2 - v1).normalized);

            if(plane.Raycast(ray, out fDistance))
            {
                Vector3 v3IntersectionPoint = v1 + ((v2 - v1).normalized * fDistance);

                // It intersects the plane, now see if it is inside the rect

                Vector3 v3IntersectionPointInPlaneLocal = mtxToPlaneLocal.MultiplyPoint3x4(v3IntersectionPoint);

                if(fDistance <= fLimitUp)
                {
                    if(v3IntersectionPointInPlaneLocal.x >= 0.0f && v3IntersectionPointInPlaneLocal.x <= fLimitRight)
                    {
                        if(v3IntersectionPointInPlaneLocal.z >= 0.0f && v3IntersectionPointInPlaneLocal.z <= fLimitForward)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}