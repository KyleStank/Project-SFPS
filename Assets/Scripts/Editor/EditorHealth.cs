/*
using UnityEngine;
using UnityEditor;

namespace Shooter.CustomEditors {
    [CustomEditor(typeof(Misc.Health))]
    public class EditorHealth : Editor {
        private string[] tooltips = new string[2] { //This is an array of all of the tooptips used in the inspector
            "Amount of health object has.",
            "Amount of time before this game object is destroyed when it is \"killed\"."
        };

        public override void OnInspectorGUI() {
            Misc.Health health = (Misc.Health)target; //Gets the target, and casts it to a correct object type
            
            Do all of the GUI stuff
            health.SetHealth(EditorGUILayout.FloatField(new GUIContent("Health", tooltips[0]), health.GetHealth()));

            health.SetDestroyTime(EditorGUILayout.FloatField(new GUIContent("Destroy Time", tooltips[1]), health.GetDestroyTime()));

            EditorUtility.SetDirty(target); //Makes sure that Editor saves everything
        }
    }
}
*/