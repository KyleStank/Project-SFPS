using UnityEngine;

namespace ProjectSFPS.Core.Variables
{
    /// <summary>
    /// Do not use this class.
    /// It only exists to create property drawers for every sub-class of the generic class SFPSBaseVariableReference.
    /// </summary>
    public abstract class SFPSBaseVariableReference {}


    /// <summary>
    /// Abstract class that acts as a wrapper for SFPSBaseVariable.
    /// Contains a constant value of type TValue and a SFPSBaseVariable reference of type TVariable.
    /// Since a custom property drawer exists for this class, either the constant or reference variable value can be used.
    /// </summary>
    /// <typeparam name="TValue">Value type that class represents.</typeparam>
    /// <typeparam name="TVariable">Value type that reference variable represents.</typeparam>
    public abstract class SFPSBaseVariableReference<TValue, TVariable> : SFPSBaseVariableReference where TVariable : SFPSBaseVariable<TValue>
    {
        public SFPSBaseVariableReference() {}
        public SFPSBaseVariableReference(TValue value)
        {
            m_UseConstantValue = true;
            m_ConstantValue = value;
        }

        [SerializeField]
        protected bool m_UseConstantValue = true;
        [SerializeField]
        protected TValue m_ConstantValue = default(TValue);
        [SerializeField]
        protected TVariable m_Variable = default(TVariable);

        /// <summary>
        /// The value of the variable.
        /// The constant value or reference variable value is used based on what was chosen in the Unity Editor.
        /// </summary>
        public TValue Value
        {
            get => m_UseConstantValue || m_Variable == null ? m_ConstantValue : m_Variable.Value;
            set
            {
                if (!m_UseConstantValue && m_Variable != null)
                {
                    m_Variable.Value = value;
                }
                else
                {
                    m_UseConstantValue = true;
                    m_ConstantValue = value;
                }
            }
        }
    }
}
