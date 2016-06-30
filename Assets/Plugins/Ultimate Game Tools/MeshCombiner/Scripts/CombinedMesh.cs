using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CombinedMesh : MonoBehaviour
{
    public bool         SaveMeshAsset = false;
    public bool         KeepPosition  = true;
    public EPivotMode   PivotMode     = EPivotMode.Center;
    public MeshFilter[] MeshObjects   = null;
    public GameObject   RootNode      = null;

    public delegate void CombineProgressDelegate(string strMessage, float fT);

#if UNITY_EDITOR
    [SerializeField]
    private string m_strMeshAssetFileName = "";
#endif

    private bool m_bCancelled = false;

    [Serializable]
    public class ObjectInfo
    {
        public ObjectInfo(Material[] aMaterials, Mesh mesh, Transform transform, Matrix4x4 mtxLocal)
        {
            this.aMaterials = new Material[aMaterials.Length];
            aMaterials.CopyTo(this.aMaterials, 0);
            this.mesh = Instantiate(mesh) as Mesh;

            v3LocalPosition = transform.localPosition;
            qLocalRotation  = transform.localRotation;
            v3LocalScale    = transform.localScale;

            this.mtxLocal = mtxLocal;
            this.mtxWorld = transform.localToWorldMatrix;

            if(mesh.normals != null)
            {
                av3NormalsWorld = mesh.normals;

                for(int nVertex = 0; nVertex < av3NormalsWorld.Length; nVertex++)
                {
                    av3NormalsWorld[nVertex] = mtxWorld.MultiplyVector(av3NormalsWorld[nVertex]);
                }
            }

            if(mesh.tangents != null)
            {
                av4TangentsWorld = mesh.tangents;

                for(int nVertex = 0; nVertex < av4TangentsWorld.Length; nVertex++)
                {
                    Vector3 v3Tangent = new Vector3(av4TangentsWorld[nVertex].x, av4TangentsWorld[nVertex].y, av4TangentsWorld[nVertex].z);
                    v3Tangent = mtxWorld.MultiplyVector(v3Tangent);
                    av4TangentsWorld[nVertex] = new Vector4(v3Tangent.x, v3Tangent.y, v3Tangent.z, av4TangentsWorld[nVertex].w);
                }
            }
        }

        public Material[] aMaterials;
        public Mesh       mesh;
        public Vector3    v3LocalPosition;
        public Quaternion qLocalRotation;
        public Vector3    v3LocalScale;
        public Matrix4x4  mtxLocal;
        public Matrix4x4  mtxWorld;
        public Vector3[]  av3NormalsWorld;
        public Vector4[]  av4TangentsWorld;
    }

    class MaterialMeshInfo
    {
        public MaterialMeshInfo(Transform transform, Mesh mesh, int nSubMesh)
        {
            this.transform = transform;
            this.mesh      = mesh;
            this.nSubMesh  = nSubMesh;
        }

        public Transform transform;
        public Mesh      mesh;
        public int       nSubMesh;
    }

    public enum EPivotMode
    {
        Keep,
        Center,
        BottomCenter,
        TopCenter,
        Min,
        Max
    }

    [SerializeField]
    private List<ObjectInfo> m_listObjectInfo = new List<ObjectInfo>();
    private Dictionary<Material, List<MaterialMeshInfo>> m_dicMeshEntries = new Dictionary<Material, List<MaterialMeshInfo>>();

    public void CancelCombining()
    {
        m_bCancelled = true;
    }

    public bool CombiningCancelled()
    {
        return m_bCancelled;
    }

    public void TransformObjInfoMeshVectorsToLocal(Transform newTransform)
    {
        foreach(ObjectInfo objInfo in m_listObjectInfo)
        {
            if(objInfo.mesh.normals != null && objInfo.av3NormalsWorld != null)
            {
                Vector3[] av3Normals = new Vector3[objInfo.av3NormalsWorld.Length];
                objInfo.av3NormalsWorld.CopyTo(av3Normals, 0);

                for(int nVertex = 0; nVertex < av3Normals.Length; nVertex++)
                {
                    av3Normals[nVertex] = newTransform.InverseTransformDirection(av3Normals[nVertex]);
                }

                objInfo.mesh.normals = av3Normals;
            }

            if(objInfo.mesh.tangents != null)
            {
                Vector4[] av4Tangents = new Vector4[objInfo.av4TangentsWorld.Length];
                objInfo.av4TangentsWorld.CopyTo(av4Tangents, 0);

                for(int nVertex = 0; nVertex < av4Tangents.Length; nVertex++)
                {
                    Vector3 v3Tangent = new Vector3(av4Tangents[nVertex].x, av4Tangents[nVertex].y, av4Tangents[nVertex].z);
                    v3Tangent = newTransform.InverseTransformDirection(v3Tangent);
                    av4Tangents[nVertex] = new Vector4(v3Tangent.x, v3Tangent.y, v3Tangent.z, av4Tangents[nVertex].w);
                }

                objInfo.mesh.tangents = av4Tangents;
            }
        }
    }

    public int GetObjectCount()
    {
        return m_listObjectInfo.Count;
    }

    public ObjectInfo GetObjectInfo(int nIndex)
    {
        return m_listObjectInfo[nIndex];
    }

	public void Combine(CombineProgressDelegate progress)
    {
        m_listObjectInfo.Clear();
        m_dicMeshEntries.Clear();
        m_bCancelled = false;

#if UNITY_EDITOR

        if(SaveMeshAsset)
        {
            if(m_strMeshAssetFileName.Length == 0 || System.IO.File.Exists(m_strMeshAssetFileName) == false)
            {
                m_strMeshAssetFileName = UnityEditor.EditorUtility.SaveFilePanelInProject("Save mesh asset", "mesh_" + this.name + this.GetInstanceID().ToString() + ".asset", "asset", "Please enter a file name to save the mesh asset to");
            }
        }

#endif

        // Build dictionary

        bool bUseNormals   = false;
        bool bUseTangents  = false;
        bool bUseColors    = false;
        bool bUseMapping1  = false;
        bool bUseMapping2  = false;

        Vector3 v3Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 v3Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        int nMeshFilter = 0;

        foreach(MeshFilter meshFilter in MeshObjects)
        {
            if(progress != null)
            {
                progress("Preprocessing object " + meshFilter.name + "...", (float)nMeshFilter / (float)MeshObjects.Length);
            }

            if(m_bCancelled)
            {
                return;
            }

            if(meshFilter == null)
            {
                continue;
            }

            if(meshFilter.GetComponent<Renderer>() == null)
            {
                Debug.LogWarning(meshFilter.name + " has no mesh renderer available");
                continue;
            }

            Mesh mesh = meshFilter.sharedMesh;

            Vector3[] aVertices = mesh.vertices;

            for(int v = 0; v < aVertices.Length; v++)
            {
                Vector3 v3Pos = meshFilter.transform.TransformPoint(aVertices[v]);

                if(v3Pos.x < v3Min.x) v3Min.x = v3Pos.x;
                if(v3Pos.y < v3Min.y) v3Min.y = v3Pos.y;
                if(v3Pos.z < v3Min.z) v3Min.z = v3Pos.z;
                if(v3Pos.x > v3Max.x) v3Max.x = v3Pos.x;
                if(v3Pos.y > v3Max.y) v3Max.y = v3Pos.y;
                if(v3Pos.z > v3Max.z) v3Max.z = v3Pos.z;
            }

            if(mesh.normals  != null) if(mesh.normals.Length  > 0) bUseNormals  = true;
            if(mesh.tangents != null) if(mesh.tangents.Length > 0) bUseTangents = true;
            if(mesh.colors   != null) if(mesh.colors.Length   > 0) bUseColors   = true;
            if(mesh.colors32 != null) if(mesh.colors32.Length > 0) bUseColors   = true;
            if(mesh.uv       != null) if(mesh.uv.Length       > 0) bUseMapping1 = true;
            if(mesh.uv2      != null) if(mesh.uv2.Length      > 0) bUseMapping2 = true;

            for(int nSubMesh = 0; nSubMesh < mesh.subMeshCount; nSubMesh++)
            {
                MaterialMeshInfo newMaterialMeshInfo = new MaterialMeshInfo(meshFilter.transform, mesh, nSubMesh);
                Material         material            = meshFilter.GetComponent<Renderer>().sharedMaterials[nSubMesh];

                if(m_dicMeshEntries.ContainsKey(material) == false)
                {
                    m_dicMeshEntries.Add(material, new List<MaterialMeshInfo>());
                }

                m_dicMeshEntries[material].Add(newMaterialMeshInfo);
            }

            // Add object info. Matrix will be later transformed to local.
            m_listObjectInfo.Add(new ObjectInfo(meshFilter.GetComponent<Renderer>().sharedMaterials, mesh, meshFilter.transform, meshFilter.transform.localToWorldMatrix));
        }

        if(m_dicMeshEntries.Count > 0)
        {
            // Position new

            Vector3 v3Position = transform.position;

            switch(PivotMode)
            {
                case EPivotMode.Keep:         v3Position = transform.position; break;
                case EPivotMode.Center:       v3Position = (v3Max + v3Min) * 0.5f; break;
                case EPivotMode.BottomCenter: v3Position = (v3Max + v3Min) * 0.5f; v3Position.y = v3Min.y; break;
                case EPivotMode.TopCenter:    v3Position = (v3Max + v3Min) * 0.5f; v3Position.y = v3Max.y; break;
                case EPivotMode.Min:          v3Position = v3Min; break;
                case EPivotMode.Max:          v3Position = v3Max; break;
            }

            Vector3    v3OriginalPosition = transform.position;
            Quaternion qOriginalRotation  = transform.rotation;
            Vector3    v3OriginalScale    = transform.localScale;

            transform.position   = v3Position;
            transform.rotation   = Quaternion.identity;
            transform.localScale = Vector3.one;

            Matrix4x4 mtxWorldToNewLocal = this.transform.worldToLocalMatrix;

            if(KeepPosition)
            {
                transform.position   = v3OriginalPosition;
                transform.rotation   = qOriginalRotation;
                transform.localScale = v3OriginalScale;
            }            

            Material[] aMaterials = new Material[m_dicMeshEntries.Keys.Count];
            m_dicMeshEntries.Keys.CopyTo(aMaterials, 0);

            // Transform object information matrices to local

            foreach(ObjectInfo objInfo in m_listObjectInfo)
            {
                objInfo.mtxLocal = mtxWorldToNewLocal * objInfo.mtxLocal;
            }

            // Build mesh

            List<int>[] aListIndices = new List<int>[m_dicMeshEntries.Count];

            List<Vector3> listVertices = new List<Vector3>();
            List<Vector3> listNormals  = new List<Vector3>();
            List<Vector4> listTangents = new List<Vector4>();
            List<Color32> listColors   = new List<Color32>();
            List<Vector2> listMapping1 = new List<Vector2>();
            List<Vector2> listMapping2 = new List<Vector2>();

            Dictionary<GameObject, int> dicObject2IndexStart = new Dictionary<GameObject, int>();

            int nMaterial = 0;

            foreach(List<MaterialMeshInfo> listMaterialMeshInfo in m_dicMeshEntries.Values)
            {
                // Each entry in this list shares the same material, so it will generate a new submesh entry

                aListIndices[nMaterial] = new List<int>();

                int nMaterialMeshInfoIndex = 0;

                foreach(MaterialMeshInfo materialMeshInfo in listMaterialMeshInfo)
                {
                    if(progress != null)
                    {
                        progress("Combining submesh for material " + aMaterials[nMaterial].name + "...", (float)nMaterialMeshInfoIndex / (float)listMaterialMeshInfo.Count);
                    }

                    if(m_bCancelled)
                    {
                        return;
                    }

                    int nIndexStart = listVertices.Count;

                    // We'll find or initialize the index start, which is the position in the vertex data where this mesh will start when combined

                    if(dicObject2IndexStart.ContainsKey(materialMeshInfo.transform.gameObject))
                    {
                        nIndexStart = dicObject2IndexStart[materialMeshInfo.transform.gameObject];
                    }
                    else
                    {
                        // Now we need to add the mesh vertex data (we'll transform it to the new object local space

                        Matrix4x4 mtxLocalToWorld = materialMeshInfo.transform.localToWorldMatrix;
                        Matrix4x4 mtxCombined     = mtxWorldToNewLocal * mtxLocalToWorld;

                        dicObject2IndexStart.Add(materialMeshInfo.transform.gameObject, nIndexStart);

                        int nVertexCount = materialMeshInfo.mesh.vertexCount;

                        Vector3[] aVertices = materialMeshInfo.mesh.vertices;

                        for(int nVertex = 0; nVertex < aVertices.Length; nVertex++)
                        {
                            aVertices[nVertex] = mtxCombined.MultiplyPoint3x4(aVertices[nVertex]);
                        }

                        listVertices.AddRange(aVertices);

                        if(bUseNormals)
                        {
                            bool bCreateNormals = true;
                            if(materialMeshInfo.mesh.normals != null) if(materialMeshInfo.mesh.normals.Length > 0) bCreateNormals = false;

                            if(bCreateNormals)
                            {
                                Debug.LogWarning(string.Format("Object {0} has mesh with no vertex normals, and some other objects have them. Dummy normals have been added", materialMeshInfo.transform.name));
                            }

                            Vector3[] av3Normals = bCreateNormals ? new Vector3[nVertexCount] : materialMeshInfo.mesh.normals;

                            for(int nVertex = 0; nVertex < av3Normals.Length; nVertex++)
                            {
                                av3Normals[nVertex] = materialMeshInfo.transform.TransformDirection(av3Normals[nVertex]);
                                av3Normals[nVertex] = this.transform.InverseTransformDirection(av3Normals[nVertex]);
                            }

                            listNormals.AddRange(av3Normals);
                        }

                        if(bUseTangents)
                        {
                            bool bCreateTangents = true;
                            if(materialMeshInfo.mesh.tangents != null) if(materialMeshInfo.mesh.tangents.Length > 0) bCreateTangents = false;

                            if(bCreateTangents)
                            {
                                Debug.LogWarning(string.Format("Object {0} has mesh with no vertex tangents, and some other objects have them. Dummy tangents have been added", materialMeshInfo.transform.name));
                            }

                            Vector4[] av4Tangents = bCreateTangents ? new Vector4[nVertexCount] : materialMeshInfo.mesh.tangents;

                            for(int nVertex = 0; nVertex < av4Tangents.Length; nVertex++)
                            {
                                Vector3 v3Tangent = new Vector3(av4Tangents[nVertex].x, av4Tangents[nVertex].y, av4Tangents[nVertex].z);
                                v3Tangent = materialMeshInfo.transform.TransformDirection(v3Tangent);
                                v3Tangent = transform.InverseTransformDirection(v3Tangent);
                                av4Tangents[nVertex] = new Vector4(v3Tangent.x, v3Tangent.y, v3Tangent.z, bCreateTangents ? 1.0f : av4Tangents[nVertex].w);
                            }

                            listTangents.AddRange(av4Tangents);
                        }

                        if(bUseColors)
                        {
                            bool bColors       = false;
                            bool bColors32     = false;
                            bool bCreateColors = true;

                            if(materialMeshInfo.mesh.colors   != null) if(materialMeshInfo.mesh.colors.Length   > 0) { bColors   = true; bCreateColors = false; }
                            if(materialMeshInfo.mesh.colors32 != null) if(materialMeshInfo.mesh.colors32.Length > 0) { bColors32 = true; bCreateColors = false; }

                            if(bCreateColors)
                            {
                                Debug.LogWarning(string.Format("Object {0} has mesh with no vertex colors, and some other objects have them. Dummy colors have been added", materialMeshInfo.transform.name));
                            }

                            Color32[] aColors32 = null;
                                
                            if(bCreateColors)
                            {
                                aColors32 = new Color32[nVertexCount];
                            }
                            else if(bColors)
                            {
                                aColors32 = new Color32[nVertexCount];
                                Color[] aColors = materialMeshInfo.mesh.colors;

                                for(int nColor = 0; nColor < nVertexCount; nColor++)
                                {
                                    aColors32[nColor] = aColors[nColor];
                                }
                            }
                            else if(bColors32)
                            {
                                aColors32 = materialMeshInfo.mesh.colors32;
                            }

                            listColors.AddRange(aColors32);
                        }

                        if(bUseMapping1)
                        {
                            bool bCreateMapping1 = true;
                            if(materialMeshInfo.mesh.uv != null) if(materialMeshInfo.mesh.uv.Length > 0) bCreateMapping1 = false;

                            if(bCreateMapping1)
                            {
                                Debug.LogWarning(string.Format("Object {0} has mesh with no vertex mapping (uv), and some other objects have them. Dummy mapping has been added", materialMeshInfo.transform.name));
                            }

                            Vector2[] av2Mapping1 = bCreateMapping1 ? new Vector2[nVertexCount] : materialMeshInfo.mesh.uv;

                            listMapping1.AddRange(av2Mapping1);
                        }

                        if(bUseMapping2)
                        {
                            bool bCreateMapping2 = true;
                            if(materialMeshInfo.mesh.uv2 != null) if(materialMeshInfo.mesh.uv2.Length > 0) bCreateMapping2 = false;

                            if(bCreateMapping2)
                            {
                                Debug.LogWarning(string.Format("Object {0} has mesh with no vertex mapping (uv2), and some other objects have them. Dummy mapping has been added", materialMeshInfo.transform.name));
                            }

                            Vector2[] av2Mapping2 = bCreateMapping2 ? new Vector2[nVertexCount] : materialMeshInfo.mesh.uv2;

                            listMapping2.AddRange(av2Mapping2);
                        }
                    }

                    // Now add the indices

                    int[] aSubMeshIndices = materialMeshInfo.mesh.GetTriangles(materialMeshInfo.nSubMesh);

                    for(int nIndex = 0; nIndex < aSubMeshIndices.Length; nIndex++)
                    {
                        aListIndices[nMaterial].Add(aSubMeshIndices[nIndex] + nIndexStart);
                    }

                    nMaterialMeshInfoIndex++;
                }

                nMaterial++;
            }

            // Build the mesh

            if(m_bCancelled == false)
            {
                if(progress != null)
                {
                    progress("Building mesh...", 1.0f);
                }

                // Create meshFilter if necessary

                MeshFilter combinedMeshFilter = gameObject.GetComponent<MeshFilter>();

                if(combinedMeshFilter == null)
                {
                    combinedMeshFilter = gameObject.AddComponent<MeshFilter>();
                }

                // Set material list

                if(GetComponent<Renderer>() == null)
                {
                    gameObject.AddComponent<MeshRenderer>();
                }

                GetComponent<Renderer>().sharedMaterials = aMaterials;

                // Build mesh

                int nVertexLimit = 65000;

                if(listVertices.Count > nVertexLimit)
                {
                    Debug.LogWarning("Warning! vertex count = " + listVertices.Count + ". You may be hitting Unity's vertex count limit (" + nVertexLimit + "). Please try combining less objects.");
                }

                Mesh newMesh = new Mesh();

                newMesh.vertices = listVertices.ToArray();
                if(bUseNormals)  newMesh.normals  = listNormals.ToArray();
                if(bUseTangents) newMesh.tangents = listTangents.ToArray();
                if(bUseColors)   newMesh.colors32 = listColors.ToArray();
                if(bUseMapping1) newMesh.uv       = listMapping1.ToArray();
                if(bUseMapping2) newMesh.uv2      = listMapping2.ToArray();

                newMesh.subMeshCount = aListIndices.Length;

                for(int nSubMesh = 0; nSubMesh < aListIndices.Length; nSubMesh++)
                {
                    newMesh.SetTriangles(aListIndices[nSubMesh].ToArray(), nSubMesh);
                }

                combinedMeshFilter.sharedMesh = newMesh;

#if UNITY_EDITOR

                // Save them to disc

                if(SaveMeshAsset && m_strMeshAssetFileName.Length > 0)
                {
                    UnityEditor.AssetDatabase.CreateAsset(newMesh, m_strMeshAssetFileName);
                    UnityEditor.AssetDatabase.Refresh();
                }
#endif
            }
        }
        else
        {
            Debug.LogWarning("No meshes were combined because none were found.");
        }
    }
}
