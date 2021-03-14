using UnityEngine;

namespace ProjectSFPS.Core.Variables
{
    /// <summary>
    /// Do not use this class.
    /// It only exists to create property drawers for every sub-class of the generic class SFPSBaseVariable.
    /// </summary>
    public abstract class SFPSBaseVariable : ScriptableObject {}

    /// <summary>
    /// Abstract ScriptableObject that contains a value of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SFPSBaseVariable<T> : SFPSBaseVariable
    {
        [SerializeField]
        private T m_Value = default(T);

        /// <summary>
        /// The value. Can be set with a regular type T value, or with a SFPSBaseVariable of type T value.
        /// </summary>
        /// <value></value>
        public T Value
        {
            get => m_Value;
            set => SetValue(value);
        }

        /// <summary>
        /// Sets the value to the value of an SFPSBaseVariable of type T.
        /// </summary>
        /// <param name="value">SFPSBaseVariable of type T to assign value from.</param>
        /// <returns>Newly assigned value of type T.</returns>
        public virtual T SetValue(SFPSBaseVariable<T> value) => SetValue(value.Value);

        /// <summary>
        /// Sets the value to another value of type T.
        /// </summary>
        /// <param name="value">New value to assign.</param>
        /// <returns>Newly assigned value of type T.</returns>
        public virtual T SetValue(T value)
        {
            m_Value = value;
            return value;
        }
    }
}
