using UnityEngine;

namespace ProjectSFPS.Variables
{
    /// <summary>
    /// Abstract class that defines a simple ScriptableObject that acts as a wrapper for a value.
    /// Underneath the abstraction, the value provided is converted into an SFPSVariable.
    /// </summary>
    /// <typeparam name="T">Value type that ScriptableObject will wrap.</typeparam>
    public abstract class SFPSVariableReference<T> : ScriptableObject
    {
        [SerializeField]
        private SFPSVariable<T> m_Variable;
    }
}
