using UnityEngine;
using System.Collections;

public class LogicExplodingGoblin : MonoBehaviour
{
    public float           ExplosionForce        = 4.0f;
    public FracturedObject TargetFracturedObject = null;

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
        GUILayout.Label("-Triggered explosion");
        GUILayout.Label("-Collision particles");
        GUILayout.Label("-Collision sounds");
        GUILayout.Label("");
        GUILayout.Label("Press the button below to explode the object.");

        if(GUILayout.Button("Explode"))
        {
            TargetFracturedObject.Explode(TargetFracturedObject.transform.position, ExplosionForce);
        }
	}
}
