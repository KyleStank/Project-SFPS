using UnityEngine;

namespace ProjectSFPS.Core
{
    public abstract class SFPSBehaviour : MonoBehaviour
    {
        [SerializeField]
        private bool _loggingEnabled = true;

        public bool LoggingEnabled
        {
            get { return _loggingEnabled; }
            set { _loggingEnabled = value; }
        }

        public void Log(object message, Object context = null)
        {
            if (!_loggingEnabled) return;

            Debug.Log(message, context == null ? gameObject : context);
        }

        public void LogWarning(object message, Object context = null)
        {
            if (!_loggingEnabled) return;

            Debug.LogWarning(message, context == null ? gameObject : context);
        }

        public void LogError(object message, Object context = null)
        {
            if (!_loggingEnabled) return;

            Debug.LogError(message, context == null ? gameObject : context);
        }
    }
}
