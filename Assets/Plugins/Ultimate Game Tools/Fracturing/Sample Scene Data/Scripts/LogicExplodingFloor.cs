using UnityEngine;
using System.Collections;

public class LogicExplodingFloor : MonoBehaviour
{
	void OnGUI()
	{
        if(LogicGlobalFracturing.HelpVisible == false)
        {
            return;
        }

	      // Draw global instructions

        LogicGlobalFracturing.GlobalGUI();

        // Draw instructions

        GUILayout.Label("This scene shows a floor with:");
        GUILayout.Label("-BSP fracturing");
        GUILayout.Label("-Moving explosion source with radius");
        GUILayout.Label("");

        if(GUILayout.Button("Restart"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("02 Sample Scene - Exploding Floor");
        }
	}
}
