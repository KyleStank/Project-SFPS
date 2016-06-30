using UnityEngine;
using System.Collections;

public class LogicCollidingSphere : MonoBehaviour
{
    public Rigidbody ObjectToDrop = null;

    bool bDropped = false;
    int  nChunksDetached  = 0;
    int  nChunkCollisions = 0;

    void Start()
    {
        nChunksDetached  = 0;
        nChunkCollisions = 0;
    }

    void Update()
    {
        if(bDropped && ObjectToDrop.isKinematic == true)
        {
            ObjectToDrop.isKinematic = false;
            ObjectToDrop.WakeUp();
        }
    }

	void OnGUI()
	{
        if(LogicGlobalFracturing.HelpVisible == false)
        {
            return;
        }

        // Draw global instructions

        LogicGlobalFracturing.GlobalGUI();

        // Draw instructions

        GUILayout.Label("This scene shows:");
        GUILayout.Label("-Voronoi fracturing");
        GUILayout.Label("-Fracturing on physical contact");
        GUILayout.Label("-Collision custom notifications");
        GUILayout.Label("-Collision particles");
        GUILayout.Label("-Collision sounds");
        GUILayout.Label("");
        GUILayout.Label("Press the button below to drop the object.");

        if(GUILayout.Button("Drop"))
        {
            bDropped = true;
        }

        GUILayout.Label("Collision notifications:");
        GUILayout.Label(nChunksDetached  + " chunks detached");
        GUILayout.Label(nChunkCollisions + " chunk collisions");
	}

    void OnChunkDetach(FracturedChunk.CollisionInfo info)
    {
        // We can cancel the collision processing here
        info.bCancelCollisionEvent = false;

        // Simply increase a counter for each chunk detached
        nChunksDetached++;
    } 

    void OnFreeChunkCollision(FracturedChunk.CollisionInfo info)
    {
        // We can cancel the collision processing here
        info.bCancelCollisionEvent = false;

        // Simply increase a counter for each collision
        nChunkCollisions++;
    } 
}
