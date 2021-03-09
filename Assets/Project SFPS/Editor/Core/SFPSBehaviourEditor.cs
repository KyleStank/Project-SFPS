using System.Reflection;

using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

using ProjectSFPS.Core;

namespace ProjectSFPS.Editor.Core
{
    [CustomEditor(typeof(SFPSBehaviour), true)]
    public class SFPSBehaviourEditor : UnityEditor.Editor
    {
        private const string PROPNAME_SCRIPT = "m_Script";
        private const string PROPNAME_LOGGINGENABLED = "_loggingEnabled";
        private const string PROPNAME_SHOWBASEPROPS = "_showBaseProps";
        private const string PROPNAME_SHOWDERIVEDPROPS = "_showDerivedProps";

        private readonly string[] _excludedDefaultProperties = new string[]
        {
            PROPNAME_SCRIPT,
            PROPNAME_LOGGINGENABLED,
            PROPNAME_SHOWBASEPROPS,
            PROPNAME_SHOWDERIVEDPROPS
        };

        private SerializedProperty _propScript;
        private SerializedProperty _propLoggingEnabled;
        private SerializedProperty _propShowBaseProps;
        private SerializedProperty _propShowDerivedProps;

        private bool _hasDerivedProperties = false;

        private AnimBool _basePropsAnim;
        private AnimBool _derivedPropsAnim;

        private void OnEnable()
        {
            // Grab serialized properties.
            _propScript = serializedObject.FindProperty(PROPNAME_SCRIPT);
            _propLoggingEnabled = serializedObject.FindProperty(PROPNAME_LOGGINGENABLED);
            _propShowBaseProps = serializedObject.FindProperty(PROPNAME_SHOWBASEPROPS);
            _propShowDerivedProps = serializedObject.FindProperty(PROPNAME_SHOWDERIVEDPROPS);

            // Check if there are any derived propertfdies.
            // NOTE: Always use the last serialized property in the order they are declared in SFPSBehaviour.
            _hasDerivedProperties = _propShowDerivedProps.Copy().CountRemaining() > 0;

            // Setup animation values.
            _basePropsAnim = new AnimBool(_propShowBaseProps.isExpanded, Repaint);
            _derivedPropsAnim = new AnimBool(_propShowDerivedProps.isExpanded, Repaint);
        }

        private void OnDisable()
        {
            // Clean up event listeners.
            _basePropsAnim.valueChanged.RemoveAllListeners();
            _derivedPropsAnim.valueChanged.RemoveAllListeners();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Script property.
            EditorGUILayout.PropertyField(_propScript);

            // Draw inheritied properties.
            EditorGUILayout.Space();
            DrawInheritedProperties();

            // Draw derived properties, if there are any.
            if (_hasDerivedProperties)
            {
                EditorGUILayout.Space();
                DrawDerivedProperties();
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the inherited properties.
        /// </summary>
        private void DrawInheritedProperties()
        {
            // Create foldout.
            _propShowBaseProps.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(
                _propShowBaseProps.isExpanded,
                "SFPSBehaviour Settings",
                EditorStyles.foldoutHeader
            );
            _basePropsAnim.target = _propShowBaseProps.isExpanded;

            // Fade foldout content in/out.
            if (EditorGUILayout.BeginFadeGroup(_basePropsAnim.faded))
            {
                EditorGUI.indentLevel++;

                // Logging property.
                EditorGUILayout.TextField("Logging", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_propLoggingEnabled);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// Draws the derived properties.
        /// </summary>
        private void DrawDerivedProperties()
        {
            // Create foldout.
            _propShowDerivedProps.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(
                _propShowDerivedProps.isExpanded,
                target.name + " Settings",
                EditorStyles.foldoutHeader
            );
            _derivedPropsAnim.target = _propShowDerivedProps.isExpanded;

            // Fade foldout content in/out.
            if (EditorGUILayout.BeginFadeGroup(_derivedPropsAnim.faded))
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.TextField("Properties", EditorStyles.boldLabel);
                DrawPropertiesExcluding(serializedObject, _excludedDefaultProperties);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
