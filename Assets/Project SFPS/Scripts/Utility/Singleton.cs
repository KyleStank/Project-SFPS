using UnityEngine;

namespace ProjectSFPS.Utility
{
    // TODO: Fix issue where if Singleton is accessed in a OnDestroy(), sometimes it will be created, sometimes it won't be.
    /// <summary>
    /// Abstract implementation of the Singleton pattern.
    /// The Singleton instance will be automatically created when accessed if an instance doesn't already exist.
    /// If the instance is accessed while the application is closing, the instance will not be created.
    /// The execution order is manually marked as -1. This shouldn't interfere with any other scripts.
    /// The execution order was manually marked because the rest Project SFPS uses the default execution order
    /// and we don't want to force project settings changes for the Singleton to work properly.
    /// </summary>
    /// <typeparam name="T">MonoBehaviour that the instance will be assigned as.</typeparam>
    [DefaultExecutionOrder(-1)]
    public abstract class Singleton<T> : SFPSBehaviour where T : SFPSBehaviour
    {
        private static string m_TypeName
        {
            get { return typeof(T).Name; }
        }

        private static bool m_ShuttingDown = false;
        private static readonly object m_Lock = new object();
        private static T m_Instance = null;
        public static T Instance
        {
            get
            {
                if (m_ShuttingDown)
                {
                    if (m_Instance != null)
                        m_Instance.LogWarning("Singleton [" + m_Instance.name + "] destroyed");

                    return null;
                }

                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        GameObject go = new GameObject(m_TypeName + " (Singleton)");
                        m_Instance = go.AddComponent<T>();
                    }

                    return m_Instance;
                }
            }
        }

        protected virtual void Awake()
        {
            lock (m_Lock)
            {
                T instance = m_Instance != null ? m_Instance : GetComponent<T>();
                if (instance == null)
                {
                    instance = gameObject.AddComponent<T>();
                    m_Instance = instance;
                }
                else if (instance != this) // Make sure only one instance ever exists.
                {
                    LogWarning("Only one Singleton<" + m_TypeName + "> can exist at a time. Destroying [" + name + "]");
                    Destroy(gameObject);
                }
            }
        }

        protected void OnDestroy()
        {
            m_ShuttingDown = true;
        }

        protected void OnApplicationQuit()
        {
            m_ShuttingDown = true;
        }
    }
}
