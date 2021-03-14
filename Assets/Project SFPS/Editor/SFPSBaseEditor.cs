using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace ProjectSFPS.Editor
{
    /// <summary>
    /// Abstract class that provides common functionality to custom editors.
    /// </summary>
    public abstract class SFPSBaseEditor<T> : UnityEditor.Editor where T : Object
    {
        /// <summary>
        /// Name of Unity's default "Script" property that is drawn on every MonoBehaviour/ScriptableObject.
        /// </summary>
        protected const string PROPNAME_SCRIPT = "m_Script";

        /// <summary>
        /// Collection of strings containing properties that should be excluded when drawing default properties.
        /// Pass as parameter into DrawPropertiesExcluding().
        /// By default, contains the "m_Script" property.
        /// </summary>
        public List<string> ExcludedDefaultProps { get; set; } = new List<string>()
        {
            PROPNAME_SCRIPT
        };

        /// <summary>
        /// Reference to the "m_Script" property.
        /// </summary>
        protected SerializedProperty m_PropScript = null;

        /// <summary>
        /// When true, Unity's default "Script" property will be drawn.
        /// If set to false, it will not be drawn.
        /// </summary>
        protected bool m_DrawDefaultScriptProperty = true;

        /// <summary>
        /// Returns reference to custom editor target, converted into type T.
        /// </summary>
        protected T Target
        {
            get { return target as T; }
        }

        /// <summary>
        /// Returns true if type T is a ScriptableObject.
        /// </summary>
        protected bool IsScriptableObject
        {
            get { return typeof(T).IsSubclassOf(typeof(ScriptableObject)); }
        }

        /// <summary>
        /// Returns true if type T is a MonoBehaviour.
        /// </summary>
        protected bool IsMonoBehaviour
        {
            get { return typeof(T).IsSubclassOf(typeof(MonoBehaviour)); }
        }

        /// <summary>
        /// Abstract method that should initialize the custom editor.
        /// Invoked inside Awake().
        /// </summary>
        protected abstract void Initialize();
        protected void Awake()
        {
            m_PropScript = serializedObject.FindProperty(PROPNAME_SCRIPT);
            Initialize();
        }

        /// <summary>
        /// Abstract method that should draw the custom editor.
        /// Invoked inside OnInspectorGUI().
        /// </summary>
        protected abstract void DrawInspector();
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw default "Script" field.
            if (m_DrawDefaultScriptProperty)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField(
                    "Script:",
                    IsScriptableObject ?
                        MonoScript.FromScriptableObject(Target as ScriptableObject) :
                        MonoScript.FromMonoBehaviour(Target as MonoBehaviour),
                    typeof(T),
                    false
                );
                GUI.enabled = true;

                EditorGUILayout.Space();
            }

            // Draw abstracted inspector.
            DrawInspector();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the base inspector of SFPSBaseEditor.
        /// </summary>
        protected void DrawBaseInspector() => base.OnInspectorGUI();

        /// <summary>
        /// Draws the default properties of the custom editor.
        /// </summary>
        /// <param name="excludeDefaultProperties">
        /// When true, excludes properties on the ExcludedDefaultProps list.
        /// Otherwise, draws all properties.
        /// </param>
        protected void DrawDefaultProperties(bool excludeDefaultProperties = true) =>
            DrawPropertiesExcluding(serializedObject, excludeDefaultProperties ? ExcludedDefaultProps.ToArray() : new string[0]);
    }
}
