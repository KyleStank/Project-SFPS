using UnityEngine;

namespace ProjectSFPS
{
    /// <summary>
    /// Abstract MonoBehaviour that all Project SFPS components derive from.
    /// </summary>
    public abstract class SFPSBehaviour : MonoBehaviour
    {
         #if UNITY_EDITOR
        [SerializeField]
        private bool m_ShowBaseProps = false;
        [SerializeField]
        private bool m_ShowDerivedProps = false;
        #endif

        [SerializeField]
        private bool m_LoggingEnabled = true;
        public bool LoggingEnabled
        {
            get { return m_LoggingEnabled; }
            set { m_LoggingEnabled = value; }
        }

        private string FormatLogMessage(object message)
        {
            return "[SFPS]: " + message;
        }

        public void Log(object message, Object context = null)
        {
            if (!m_LoggingEnabled) return;

            Debug.Log(FormatLogMessage(message), context == null ? this : context);
        }

        public void LogWarning(object message, Object context = null)
        {
            if (!m_LoggingEnabled) return;

            Debug.LogWarning(FormatLogMessage(message), context == null ? this : context);
        }

        public void LogError(object message, Object context = null)
        {
            if (!m_LoggingEnabled) return;

            Debug.LogError(FormatLogMessage(message), context == null ? this : context);
        }
    }
}
