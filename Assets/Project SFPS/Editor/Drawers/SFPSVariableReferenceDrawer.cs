using UnityEngine;
using UnityEditor;

using ProjectSFPS.Core.Variables;

namespace ProjectSFPS.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SFPSBaseVariableReference), true)]
    public sealed class SFPSVariableReferenceDrawer : PropertyDrawer
    {
        private static class Styles
        {
            static Styles()
            {
                PopupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"))
                {
                    imagePosition = ImagePosition.ImageOnly
                };
            }

            public static GUIStyle PopupStyle { get; set; }
        }

        private static readonly string[] m_PopupOptions = new string[]
        {
            "Use Constant",
            "Use SFPS Variable"
        };

        private const string PROPNAME_USECONSTANTVALUE = "m_UseConstantValue";
        private const string PROPNAME_CONSTANTVALUE = "m_ConstantValue";
        private const string PROPNAME_VARIABLE = "m_Variable";

        private SerializedProperty m_Prop;
        private SerializedProperty m_UseConstantValueProp;
        private SerializedProperty m_ConstantValueProp;
        private SerializedProperty m_VariableProp;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Grab properties.
            m_Prop = property;
            m_UseConstantValueProp = m_Prop.FindPropertyRelative(PROPNAME_USECONSTANTVALUE);
            m_ConstantValueProp = m_Prop.FindPropertyRelative(PROPNAME_CONSTANTVALUE);
            m_VariableProp = m_Prop.FindPropertyRelative(PROPNAME_VARIABLE);

            // Don't make child fields be indented.
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect fieldRect = EditorGUI.PrefixLabel(position, label); // Returns the rect containing space for the control after the prefix.
            Rect valueRect = DrawField(position, fieldRect); // Returns the combined rect of the value and the popup button.

            // Draw constant or reference field.
            EditorGUI.PropertyField(valueRect, m_UseConstantValueProp.boolValue ? m_ConstantValueProp : m_VariableProp, GUIContent.none);

            // Set indent back to what it was.
            EditorGUI.indentLevel = indent;

            m_Prop.serializedObject.ApplyModifiedProperties();
        }

        private Rect DrawField(Rect position, Rect fieldRect)
        {
            Rect buttonRect = GetPopupButtonRect(fieldRect);
            Rect valueRect = GetValueRect(fieldRect, buttonRect);

            int result = DrawPopupButton(buttonRect, m_UseConstantValueProp.boolValue ? 0 : 1);
            m_UseConstantValueProp.boolValue = result == 0;

            return valueRect;
        }

        private Rect GetPopupButtonRect(Rect fieldRect)
        {
            Rect buttonRect = new Rect(fieldRect);
            buttonRect.yMin += Styles.PopupStyle.margin.top;
            buttonRect.width = Styles.PopupStyle.fixedWidth + Styles.PopupStyle.margin.right;
            buttonRect.height = Styles.PopupStyle.fixedHeight + Styles.PopupStyle.margin.top;
            return buttonRect;
        }

        private Rect GetValueRect(Rect fieldRect, Rect buttonRect)
        {
            Rect valueRect = new Rect(fieldRect);
            valueRect.x += buttonRect.width;
            valueRect.width -= buttonRect.width;
            return valueRect;
        }

        private int DrawPopupButton(Rect buttonRect, int value)
        {
            return EditorGUI.Popup(buttonRect, value, m_PopupOptions, Styles.PopupStyle);
        }
    }
}
