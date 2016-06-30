using UnityEngine;
using Pathfinding;

/** Attach this script to any obstacle with a collider to enable dynamic updates of the graphs around it.
 * When the object has moved a certain distance (or actually when it's bounding box has changed by a certain amount) defined by #updateError
 * it will call AstarPath.UpdateGraphs and update the graph around it.
 *
 * Make sure that any children colliders do not extend beyond the bounds of the collider attached to the
 * GameObject that the DynamicGridObstacle component is attached to since this script only updates the graph
 * using the bounds of the collider on the same GameObject.
 *
 * \note This script only works with GridGraph, PointGraph and LayerGridGraph
 *
 * \see AstarPath.UpdateGraphs
 * \see graph-updates
 */
[RequireComponent(typeof(Collider))]
[HelpURL("http://arongranberg.com/astar/docs/class_dynamic_grid_obstacle.php")]
public class DynamicGridObstacle : MonoBehaviour {
	/** Collider to get bounds information from */
	Collider col;

	/** Cached transform component */
	Transform tr;

	/** The minimum change in world units along one of the axis of the bounding box of the collider to trigger a graph update */
	public float updateError = 1;

	/** Time in seconds between bounding box checks.
	 * If AstarPath.batchGraphUpdates is enabled, it is not beneficial to have a checkTime much lower
	 * than AstarPath.graphUpdateBatchingInterval because that will just add unnecessary many graph updates.
	 */
	public float checkTime = 0.2F;

	/** Bounds of the collider the last time the graphs were updated */
	Bounds prevBounds;

	/** Rotation of the collider the last time the graphs were updated */
	Quaternion prevRotation;

	/** True if the collider was enabled last time the graphs were updated */
	bool prevEnabled;

	float lastCheckTime = -9999;

	void Start () {
		col = GetComponent<Collider>();
		tr = transform;
		if (col == null) {
			throw new System.Exception("A collider must be attached to the GameObject for the DynamicGridObstacle to work");
		}
		prevBounds = col.bounds;
		prevEnabled = col.enabled;
		prevRotation = tr.rotation;
	}

	void Update () {
		if (!col) {
			Debug.LogError("Removed collider from DynamicGridObstacle", this);
			enabled = false;
			return;
		}

		while (AstarPath.active == null || AstarPath.active.isScanning) {
			lastCheckTime = Time.realtimeSinceStartup;
		}

		if (Time.realtimeSinceStartup - lastCheckTime < checkTime) {
			return;
		}

		if (col.enabled) {
			// The current bounds of the collider
			Bounds newBounds = col.bounds;
			var newRotation = tr.rotation;

			Vector3 minDiff = prevBounds.min - newBounds.min;
			Vector3 maxDiff = prevBounds.max - newBounds.max;

			var extents = newBounds.extents.magnitude;
			// This is the distance that a point furthest out on the bounding box
			// would have moved due to the changed rotation of the object
			var errorFromRotation = extents*Quaternion.Angle(prevRotation, newRotation)*Mathf.Deg2Rad;

			// If the difference between the previous bounds and the new bounds is greater than some value, update the graphs
			if (minDiff.sqrMagnitude > updateError*updateError || maxDiff.sqrMagnitude > updateError*updateError ||
				errorFromRotation > updateError || !prevEnabled) {
				// Update the graphs as soon as possible
				DoUpdateGraphs();
			}
		} else {
			// Collider is disabled
			if (prevEnabled) {
				DoUpdateGraphs();
			}
		}
	}

	/** Revert graphs when destroyed.
	 * When the DynamicObstacle is destroyed, a last graph update should be done to revert nodes to their original state */
	void OnDestroy () {
		if (AstarPath.active != null) {
			var guo = new GraphUpdateObject(prevBounds);
			AstarPath.active.UpdateGraphs(guo);
		}
	}

	public void DoUpdateGraphs () {
		if (col == null) return;

		if (!col.enabled) {
			// If the collider is not enabled, then col.bounds will empty
			// so just update prevBounds
			AstarPath.active.UpdateGraphs(prevBounds);
		} else {
			Bounds newBounds = col.bounds;

			Bounds merged = newBounds;
			merged.Encapsulate(prevBounds);

			// Check what seems to be fastest, to update the union of prevBounds and newBounds in a single request
			// or to update them separately, the smallest volume is usually the fastest
			if (BoundsVolume(merged) < BoundsVolume(newBounds) + BoundsVolume(prevBounds)) {
				// Send an update request to update the nodes inside the 'merged' volume
				AstarPath.active.UpdateGraphs(merged);
			} else {
				// Send two update request to update the nodes inside the 'prevBounds' and 'newBounds' volumes
				AstarPath.active.UpdateGraphs(prevBounds);
				AstarPath.active.UpdateGraphs(newBounds);
			}

			prevBounds = newBounds;
		}

		prevEnabled = col.enabled;
		prevRotation = tr.rotation;
		lastCheckTime = Time.realtimeSinceStartup;
	}

	/** Returns the volume of a Bounds object. X*Y*Z */
	static float BoundsVolume (Bounds b) {
		return System.Math.Abs(b.size.x * b.size.y * b.size.z);
	}
}
