using UnityEngine;

namespace ProjectSFPS.Variables
{
    /// <summary>
    /// Abstract class that defines a simple ScriptableObject that acts as a wrapper for a value.
    /// </summary>
    /// <typeparam name="T">Value type that ScriptableObject will wrap.</typeparam>
    public abstract class SFPSVariable<T> : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        private T m_InitialValue;
        public T InitialValue
        {
            get { return m_InitialValue; }
            set { m_InitialValue = value; }
        }

        public T RuntimeValue { get; set; }

        public void OnAfterDeserialize() => RuntimeValue = InitialValue;
        public void OnBeforeSerialize() {}
    }
}
