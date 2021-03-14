using UnityEngine;

namespace ProjectSFPS.Variables
{
    /// <summary>
    /// Serializable class that simply wraps any value. Contains an initial and runtime value.
    /// For the serialization to work, the type must also be serializable by Unity.
    /// </summary>
    /// <typeparam name="T">Value type that SFPSVariable will wrap.</typeparam>
    [System.Serializable]
    public struct SFPSVariable<T> : ISerializationCallbackReceiver
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
