using UnityEngine;
using UnityEditor;

using ProjectSFPS.Variables;

namespace ProjectSFPS.Editor.Variables
{
    // TODO: Implement. After implementation, remove CustomEditor attribute and create custom editor for each variable type.
    [CustomEditor(typeof(SFPSVector2))]
    public class SFPSVariableReferenceEditor : SFPSBaseEditor<SFPSVector2>
    {
        private const string PROPNAME_VARIABLE = "m_Variable";

        private SerializedProperty m_PropVariable;

        protected override void Initialize() {}

        protected override void DrawInspector() {}
    }
}
