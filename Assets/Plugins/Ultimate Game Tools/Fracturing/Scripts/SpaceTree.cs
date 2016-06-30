using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UltimateFracturing
{
    public static partial class Fracturer
    {
        // We use this class to preprocess and subdivide the mesh to speed up Voronoi cells computation
        private class SpaceTreeNode
        {
            public SpaceTreeNode()
            {
                nodeOneSide        = null;
                nodeOtherSide      = null;
                listMeshDatasSpace = new List<MeshData>();
                nLevel             = 0;
                nSplitsX           = 0;
                nSplitsY           = 0;
                nSplitsZ           = 0;
            }

            public bool ContainsCompletely(Vector3 v3Min, Vector3 v3Max)
            {
                if((this.v3Min.x <= v3Min.x && this.v3Max.x >= v3Max.x) || nSplitsX == 0)
                {
                    if((this.v3Min.y <= v3Min.y && this.v3Max.y >= v3Max.y) || nSplitsY == 0)
                    {
                        if((this.v3Min.z <= v3Min.z && this.v3Max.z >= v3Max.z) || nSplitsZ == 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public static List<MeshData> GetSmallestPossibleMeshData(SpaceTreeNode root, Vector3 v3Min, Vector3 v3Max)
            {
                if(root.ContainsCompletely(v3Min, v3Max) == false)
                {
                    return root.listMeshDatasSpace;
                }

                SpaceTreeNode nodeCurrent = root;

                while(true)
                {
                    if(nodeCurrent.nodeOneSide != null)
                    {
                        if(nodeCurrent.nodeOneSide.ContainsCompletely(v3Min, v3Max))
                        {
                            nodeCurrent = nodeCurrent.nodeOneSide;
                            continue;
                        }
                    }

                    if(nodeCurrent.nodeOtherSide != null)
                    {
                        if(nodeCurrent.nodeOtherSide.ContainsCompletely(v3Min, v3Max))
                        {
                            nodeCurrent = nodeCurrent.nodeOtherSide;
                            continue;
                        }
                    }

                    break;
                }

                return nodeCurrent.listMeshDatasSpace;
            }

            public static SpaceTreeNode BuildSpaceTree(MeshData meshDataIn, int nSubdivisionLevels, FracturedObject fracturedComponent, ProgressDelegate progress = null)
            {
                if(nSubdivisionLevels < 1)
                {
                    return null;
                }

                SplitOptions splitOptions = new SplitOptions();

                splitOptions.bForceNoIslandGeneration     = true;
                splitOptions.bForceNoChunkConnectionInfo  = true;            
                splitOptions.bForceNoIslandConnectionInfo = true;
                splitOptions.bForceNoCap                  = false;
                splitOptions.bVerticesAreLocal            = true;

                SpaceTreeNode nodeRoot = new SpaceTreeNode();

                nodeRoot.listMeshDatasSpace = new List<MeshData>();
                nodeRoot.listMeshDatasSpace.Add(meshDataIn);
                nodeRoot.nLevel   = 0;
                nodeRoot.nSplitsX = 0;
                nodeRoot.nSplitsY = 0;
                nodeRoot.nSplitsZ = 0;
                nodeRoot.v3Min    = meshDataIn.v3Min;
                nodeRoot.v3Max    = meshDataIn.v3Max;

                Queue<SpaceTreeNode> queueNodes = new Queue<SpaceTreeNode>();
                queueNodes.Enqueue(nodeRoot);

                int nTotalSubdivisions   = 0;
                int nCurrentSubdivisions = 0;
                int nSplitsX             = 0;
                int nSplitsY             = 0;
                int nSplitsZ             = 0;
                
                for(int i = 0; i < nSubdivisionLevels; i++)
                {
                    nTotalSubdivisions += Mathf.RoundToInt(Mathf.Pow(2, i));
                }

                while(queueNodes.Count > 0)
                {
                    SpaceTreeNode nodeCurrent = queueNodes.Dequeue();

                    List<MeshData> listMeshDataPos;
                    List<MeshData> listMeshDataNeg;

                    if(nodeCurrent.nLevel < nSubdivisionLevels)
                    {
                        if(progress != null)
                        {
                            progress("Fracturing", string.Format("Pre computing space volume (split {0}/{1}, Depth {2})", nCurrentSubdivisions + 1, nTotalSubdivisions, nodeCurrent.nLevel + 1), Mathf.Clamp01((float)nCurrentSubdivisions / (float)nTotalSubdivisions));
                        }

                        if(Fracturer.IsFracturingCancelled())
                        {
                            return null;
                        }

                        Vector3 v3Normal = Vector3.up;
                        Vector3 v3Right  = Vector3.right;
                        Vector3 v3Pos    = (nodeCurrent.v3Min + nodeCurrent.v3Max) * 0.5f;

                        float fSizeX = nodeCurrent.v3Max.x - nodeCurrent.v3Min.x;
                        float fSizeY = nodeCurrent.v3Max.y - nodeCurrent.v3Min.y;
                        float fSizeZ = nodeCurrent.v3Max.z - nodeCurrent.v3Min.z;

                        Vector3 v3MinNeg = nodeCurrent.v3Min;
                        Vector3 v3MaxNeg = nodeCurrent.v3Max;
                        Vector3 v3MinPos = nodeCurrent.v3Min;
                        Vector3 v3MaxPos = nodeCurrent.v3Max;

                        if(fSizeX >= fSizeY && fSizeX >= fSizeZ)
                        {
                            v3Normal = Vector3.right;
                            v3Right  = Vector3.forward;

                            v3MaxNeg.x = v3Pos.x;
                            v3MinPos.x = v3Pos.x;

                            nSplitsX++;
                        }
                        else if(fSizeY >= fSizeX && fSizeY >= fSizeZ)
                        {
                            v3Normal = Vector3.up;
                            v3Right  = Vector3.right;

                            v3MaxNeg.y = v3Pos.y;
                            v3MinPos.y = v3Pos.y;

                            nSplitsY++;
                        }
                        else
                        {
                            v3Normal = Vector3.forward;
                            v3Right  = Vector3.right;

                            v3MaxNeg.z = v3Pos.z;
                            v3MinPos.z = v3Pos.z;

                            nSplitsZ++;
                        }

                        foreach(MeshData meshData in nodeCurrent.listMeshDatasSpace)
                        {
                            if(SplitMeshUsingPlane(meshData, fracturedComponent, splitOptions, v3Normal, v3Right, v3Pos, out listMeshDataPos, out listMeshDataNeg, progress) == true)
                            {
                                nodeCurrent.nodeOneSide = new SpaceTreeNode();
                                nodeCurrent.nodeOneSide.listMeshDatasSpace = listMeshDataNeg;
                                nodeCurrent.nodeOneSide.v3Min    = v3MinNeg;
                                nodeCurrent.nodeOneSide.v3Max    = v3MaxNeg;
                                nodeCurrent.nodeOneSide.nLevel   = nodeCurrent.nLevel + 1;
                                nodeCurrent.nodeOneSide.nSplitsX = nSplitsX;
                                nodeCurrent.nodeOneSide.nSplitsY = nSplitsY;
                                nodeCurrent.nodeOneSide.nSplitsZ = nSplitsZ;

                                queueNodes.Enqueue(nodeCurrent.nodeOneSide);

                                nodeCurrent.nodeOtherSide = new SpaceTreeNode();
                                nodeCurrent.nodeOtherSide.listMeshDatasSpace = listMeshDataPos;
                                nodeCurrent.nodeOtherSide.v3Min    = v3MinPos;
                                nodeCurrent.nodeOtherSide.v3Max    = v3MaxPos;
                                nodeCurrent.nodeOtherSide.nLevel   = nodeCurrent.nLevel + 1;
                                nodeCurrent.nodeOtherSide.nSplitsX = nSplitsX;
                                nodeCurrent.nodeOtherSide.nSplitsY = nSplitsY;
                                nodeCurrent.nodeOtherSide.nSplitsZ = nSplitsZ;

                                queueNodes.Enqueue(nodeCurrent.nodeOtherSide);
                            }
                        }

                        nCurrentSubdivisions++;
                    }
                }

                return nodeRoot;
            }

            SpaceTreeNode   nodeOneSide;         // The subdivided space node of one side
            SpaceTreeNode   nodeOtherSide;       // The subdivided space node of the other side
            int             nLevel;              // The depth level in the tree, range [0, nLevels)
            int             nSplitsX;            // The number of splits in X done to come to this volume
            int             nSplitsY;            // The number of splits in Y done to come to this volume
            int             nSplitsZ;            // The number of splits in Z done to come to this volume
            Vector3         v3Min;               // The bounding box min of this space
            Vector3         v3Max;               // The bounding box max of this space
            List<MeshData>  listMeshDatasSpace;  // The meshes contained in this volume (the list should have 1 item because we don't detect islands for this, but to remain consistent with our way of doing things...)
        }
    }
}