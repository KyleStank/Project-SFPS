using UnityEngine;
using System.Collections;
using UltimateFracturing;

public class LogicArcsAndColumns : MonoBehaviour
{
	void Start()
    {
	
	}
	
	void Update()
    {

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

        GUILayout.Label("This scene shows a simulated use case scenario with:");
        GUILayout.Label("-BSP fracturing");
        GUILayout.Label("-Chunk structural interconnection");
        GUILayout.Label("-Use of support planes to link pillars to the ground");
        GUILayout.Label("-Collision particles");
        GUILayout.Label("-Collision sounds");
        GUILayout.Label("-Raycasting to trigger explosions with the weapon");
        GUILayout.Label("");

        GUILayout.Label("Hold down the left mouse button and move to aim.");
        GUILayout.Label("Press spacebar to fire.");
        GUILayout.Space(20);
	}
}
