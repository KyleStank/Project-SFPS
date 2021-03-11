using UnityEngine;

namespace ProjectSFPS.Core
{
    public abstract class SFPSBehaviour : MonoBehaviour
    {
        [SerializeField]
        private bool m_LoggingEnabled = true;
        public bool LoggingEnabled
        {
            get { return m_LoggingEnabled; }
            set { m_LoggingEnabled = value; }
        }

#if UNITY_EDITOR
        [SerializeField]
        private bool m_ShowBaseProps = false;
        [SerializeField]
        private bool m_ShowDerivedProps = false;
#endif

        private string FormatLogMessage(object message)
        {
            return "[SFPS]: " + message;
        }

        public void Log(object message, Object context = null)
        {
            if (!m_LoggingEnabled) return;

            Debug.Log(FormatLogMessage(message), context == null ? gameObject : context);
        }

        public void LogWarning(object message, Object context = null)
        {
            if (!m_LoggingEnabled) return;

            Debug.LogWarning(FormatLogMessage(message), context == null ? gameObject : context);
        }

        public void LogError(object message, Object context = null)
        {
            if (!m_LoggingEnabled) return;

            Debug.LogError(FormatLogMessage(message), context == null ? gameObject : context);
        }
    }
}
