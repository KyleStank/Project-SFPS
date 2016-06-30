using UnityEngine;
using System.Collections;
using Pathfinding;

/** Moves a grid graph to follow a target.
 *
 * Attach this to some object in the scene and assign the target to e.g the player.
 * Then the graph will follow that object around as it moves.
 *
 * This is useful if pathfinding is only necessary in a small region around an object (for example the player).
 * It makes it possible to have vast open worlds (maybe procedurally generated) and still be able to use pathfinding on them.
 *
 * When the graph is moved you may notice an fps drop.
 * If this grows too large you can try a few things:
 * - Reduce the #updateDistance. This will make the updates smaller but more frequent.
 *   This only works to some degree however since an update has an inherent overhead.
 * - Turn off erosion on the grid graph. This will reduce the number of nodes that need updating.
 * - Reduce the grid size.
 * - Turn on multithreading (A* Inspector -> Settings)
 * - Disable Height Testing or Collision Testing in the grid graph. This can give a minor performance boost.
 *
 * \see Take a look at the example scene called "Procedural" for an example of how to use this script
 *
 * \version Since 3.6.8 this class can handle graph rotation other options such as isometric angle and aspect ratio.
 */
[HelpURL("http://arongranberg.com/astar/docs/class_procedural_grid_mover.php")]
public class ProceduralGridMover : MonoBehaviour {
	/** Graph will be updated if the target is more than this number of nodes from the graph center.
	 * Note that this is in nodes, not world units.
	 *
	 * \version The unit was changed to nodes instead of world units in 3.6.8.
	 */
	public float updateDistance = 10;

	/** Graph will be moved to follow this target */
	public Transform target;

	/** Flood fill the graph after updating.
	 * If this is set to false, areas of the graph will not be recalculated.
	 * Enable this only if the graph will only have a single area (i.e
	 * from all walkable nodes there is a valid path to every other walkable
	 * node). One case where this might be appropriate is a large
	 * outdoor area such as a forrest.
	 * If there are multiple areas in the graph and this
	 * is not enabled, pathfinding could fail later on.
	 *
	 * Enabling it will make the graph updates faster.
	 */
	public bool floodFill;

	/** Grid graph to update */
	GridGraph graph;

	/** Temporary buffer */
	GridNode[] tmp;

	/** True while the graph is being updated by this script */
	public bool updatingGraph { get; private set; }

	public void Start () {
		if (AstarPath.active == null) throw new System.Exception("There is no AstarPath object in the scene");

		graph = AstarPath.active.astarData.gridGraph;

		if (graph == null) throw new System.Exception("The AstarPath object has no GridGraph");
		UpdateGraph();
	}

	/** Update is called once per frame */
	void Update () {
		// Calculate where the graph center and the target position is in graph space
		var graphCenterInGraphSpace = PointToGraphSpace(graph.center);
		var targetPositionInGraphSpace = PointToGraphSpace(target.position);

		// Check the distance in graph space
		// We only care about the X and Z axes since the Y axis is the "height" coordinate of the nodes (in graph space)
		// We only care about the plane that the nodes are placed in
		if (VectorMath.SqrDistanceXZ(graphCenterInGraphSpace, targetPositionInGraphSpace) > updateDistance*updateDistance) {
			UpdateGraph();
		}
	}

	/** Transforms a point from world space to graph space.
	 * In graph space, (0,0,0) is bottom left corner of the graph
	 * and one unit along the X and Z axes equals distance between two nodes
	 * the Y axis still uses world units
	 */
	Vector3 PointToGraphSpace (Vector3 p) {
		// Multiply with the inverse matrix of the graph
		// to get the point in graph space
		return graph.inverseMatrix.MultiplyPoint(p);
	}

	/** Updates the graph asynchronously.
	 * This will move the graph so that the target's position is the center of the graph.
	 * If the graph is already being updated, the call will be ignored.
	 */
	public void UpdateGraph () {
		if (updatingGraph) {
			// We are already updating the graph
			// so ignore this call
			return;
		}

		updatingGraph = true;

		// Start a work item for updating the graph
		// This will pause the pathfinding threads
		// so that it is safe to update the graph
		// and then do it over several frames
		// (hence the IEnumerator coroutine)
		// to avoid too large FPS drops
		IEnumerator ie = UpdateGraphCoroutine();
		AstarPath.active.AddWorkItem(new AstarPath.AstarWorkItem(
				force => {
			// If force is true we need to calculate all steps at once
			if (force) while (ie.MoveNext()) {}

			// Calculate one step. True will be returned when there are no more steps
			bool done = !ie.MoveNext();

			if (done) {
				updatingGraph = false;
			}
			return done;
		}));
	}

	/** Async method for moving the graph */
	IEnumerator UpdateGraphCoroutine () {
		// Find the direction
		// that we want to move the graph in.
		// Calcuculate this in graph space (where a distance of one is the size of one node)
		Vector3 dir = PointToGraphSpace(target.position) - PointToGraphSpace(graph.center);

		// Snap to a whole number of nodes
		dir.x = Mathf.Round(dir.x);
		dir.z = Mathf.Round(dir.z);
		dir.y = 0;

		// Nothing do to
		if (dir == Vector3.zero) yield break;

		// Number of nodes to offset in each direction
		Int2 offset = new Int2(-Mathf.RoundToInt(dir.x), -Mathf.RoundToInt(dir.z));

		// Move the center (this is in world units, so we need to convert it back from graph space)
		graph.center += graph.matrix.MultiplyVector(dir);
		graph.GenerateMatrix();

		// Create a temporary buffer
		// required for the calculations
		if (tmp == null || tmp.Length != graph.nodes.Length) {
			tmp = new GridNode[graph.nodes.Length];
		}

		// Cache some variables for easier access
		int width = graph.width;
		int depth = graph.depth;
		GridNode[] nodes = graph.nodes;

		// Check if we have moved
		// less than a whole graph
		// width in any direction
		if (Mathf.Abs(offset.x) <= width && Mathf.Abs(offset.y) <= depth) {
			// Offset each node by the #offset variable
			// nodes which would end up outside the graph
			// will wrap around to the other side of it
			for (int z = 0; z < depth; z++) {
				int pz = z*width;
				int tz = ((z+offset.y + depth)%depth)*width;
				for (int x = 0; x < width; x++) {
					tmp[tz + ((x+offset.x + width) % width)] = nodes[pz + x];
				}
			}

			yield return null;

			// Copy the nodes back to the graph
			// and set the correct indices
			for (int z = 0; z < depth; z++) {
				int pz = z*width;
				for (int x = 0; x < width; x++) {
					GridNode node = tmp[pz + x];
					node.NodeInGridIndex = pz + x;
					nodes[pz + x] = node;
				}
			}


			IntRect r = new IntRect(0, 0, offset.x, offset.y);
			int minz = r.ymax;
			int maxz = depth;

			// If offset.x < 0, adjust the rect
			if (r.xmin > r.xmax) {
				int tmp2 = r.xmax;
				r.xmax = width + r.xmin;
				r.xmin = width + tmp2;
			}

			// If offset.y < 0, adjust the rect
			if (r.ymin > r.ymax) {
				int tmp2 = r.ymax;
				r.ymax = depth + r.ymin;
				r.ymin = depth + tmp2;

				minz = 0;
				maxz = r.ymin;
			}

			// Make sure erosion is taken into account
			// Otherwise we would end up with ugly artifacts
			r = r.Expand(graph.erodeIterations + 1);

			// Makes sure the rect stays inside the grid
			r = IntRect.Intersection(r, new IntRect(0, 0, width, depth));

			yield return null;

			// Update all nodes along one edge of the graph
			// With the same width as the rect
			for (int z = r.ymin; z < r.ymax; z++) {
				for (int x = 0; x < width; x++) {
					graph.UpdateNodePositionCollision(nodes[z*width + x], x, z, false);
				}
			}

			yield return null;

			// Update all nodes along the other edge of the graph
			// With the same width as the rect
			for (int z = minz; z < maxz; z++) {
				for (int x = r.xmin; x < r.xmax; x++) {
					graph.UpdateNodePositionCollision(nodes[z*width + x], x, z, false);
				}
			}

			yield return null;

			// Calculate all connections for the nodes
			// that might have changed
			for (int z = r.ymin; z < r.ymax; z++) {
				for (int x = 0; x < width; x++) {
					graph.CalculateConnections(x, z, nodes[z*width+x]);
				}
			}

			yield return null;

			// Calculate all connections for the nodes
			// that might have changed
			for (int z = minz; z < maxz; z++) {
				for (int x = r.xmin; x < r.xmax; x++) {
					graph.CalculateConnections(x, z, nodes[z*width+x]);
				}
			}

			yield return null;

			// Calculate all connections for the nodes along the boundary
			// of the graph, these always need to be updated
			/** \todo Optimize to not traverse all nodes in the graph, only those at the edges */
			for (int z = 0; z < depth; z++) {
				for (int x = 0; x < width; x++) {
					if (x == 0 || z == 0 || x >= width-1 || z >= depth-1) graph.CalculateConnections(x, z, nodes[z*width+x]);
				}
			}
		} else {
			// Just update all nodes
			for (int z = 0; z < depth; z++) {
				for (int x = 0; x < width; x++) {
					graph.UpdateNodePositionCollision(nodes[z*width + x], x, z, false);
				}
			}

			// Recalculate the connections of all nodes
			for (int z = 0; z < depth; z++) {
				for (int x = 0; x < width; x++) {
					graph.CalculateConnections(x, z, nodes[z*width+x]);
				}
			}
		}

		if (floodFill) {
			yield return null;
			// Make sure the areas for the graph
			// have been recalculated
			// not doing this can cause pathfinding to fail
			AstarPath.active.QueueWorkItemFloodFill();
		}
	}
}
