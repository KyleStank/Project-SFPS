using UnityEngine;

namespace ProjectSFPS.Core
{
    public abstract class SFPSBehaviour : MonoBehaviour
    {
        [Header("Logging")]
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(message, context == null ? gameObject : context);
#endif
        }

        public void LogWarning(object message, Object context = null)
        {
            if (!_loggingEnabled) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(message, context == null ? gameObject : context);
#endif
        }

        public void LogError(object message, Object context = null)
        {
            if (!_loggingEnabled) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(message, context == null ? gameObject : context);
#endif
        }
    }
}
