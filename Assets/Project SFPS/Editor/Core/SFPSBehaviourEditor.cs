using UnityEditor;

using ProjectSFPS.Core;

namespace ProjectSFPS.Editor.Core
{
    [CustomEditor(typeof(SFPSBehaviour), true)]
    public class SFPSBehaviourEditor : UnityEditor.Editor
    {
        private const string PROPNAME_SCRIPT = "m_Script";
        private const string PROPNAME_LOGGINGENABLED = "_loggingEnabled";

        private readonly string[] _excludedDefaultProperties = new string[]
        {
            PROPNAME_SCRIPT,
            PROPNAME_LOGGINGENABLED
        };

        private SerializedProperty _propScript;
        private SerializedProperty _propLoggingEnabled;

        private void OnEnable()
        {
            _propScript = serializedObject.FindProperty(PROPNAME_SCRIPT);
            _propLoggingEnabled = serializedObject.FindProperty(PROPNAME_LOGGINGENABLED);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Script property.
            EditorGUILayout.PropertyField(_propScript);

            EditorGUILayout.Space();

            // Logging enabled property.
            EditorGUILayout.TextField("Logging", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_propLoggingEnabled);

            EditorGUILayout.Space();

            // Draw remaining properties.
            EditorGUILayout.TextField("Properties", EditorStyles.boldLabel);
            DrawPropertiesExcluding(serializedObject, _excludedDefaultProperties);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
