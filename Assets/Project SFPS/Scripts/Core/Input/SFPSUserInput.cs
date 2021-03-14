using UnityEngine;
using UnityEngine.InputSystem;

using ProjectSFPS.Core.Variables;

namespace ProjectSFPS.Core.Input
{
    public class SFPSUserInput : SFPSBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SFPSInputSettings m_InputSettings = null;

        [Header("Configuration")]
        [SerializeField]
        private SFPSStringReference m_DefaultActionMap = "Player";

        private InputActionMap m_ActiveActionMap = null;
        public InputActionMap ActiveActionMap
        {
            get { return m_ActiveActionMap; }
        }

        private void Awake()
        {
            TrySetActiveActionMap(m_DefaultActionMap);
        }

        /// <summary>
        /// Tries to set a new active action map.
        /// If a new action map is set, returns true.
        /// </summary>
        /// <param name="actionMapName">Name of action map.</param>
        public bool TrySetActiveActionMap(string actionMapName)
        {
            if (actionMapName == null)
            {
                LogError("Cannot retrieve an InputActionMap when the provided map name is null");
                return false;
            }

            if (m_InputSettings == null)
            {
                LogError("Cannot set InputActionMap [" + actionMapName + "] as active because SFPSInputSettings is null");
                return false;
            }

            if (m_InputSettings.InputActionAsset == null)
            {
                LogError("Cannot set InputActionMap [" + actionMapName + "] as active because no InputActionAsset is assigned");
                return false;
            }

            // Try to find action map.
            InputActionMap actionMap = m_InputSettings.InputActionAsset.FindActionMap(actionMapName);

            bool isValid = actionMap != null;
            if (!isValid)
            {
                LogError("InputActionMap [" + actionMapName + "] was not found");
            }
            else // Update active action map and enable.
            {
                m_ActiveActionMap?.Disable(); // Disable current before enabling new.

                // Enable action map and set as active.
                actionMap.Enable();
                m_ActiveActionMap = actionMap;
            }

            return isValid;
        }

        /// <summary>
        /// Finds an action within the current active action map.
        /// </summary>
        /// <param name="actionName">Name of action.</param>
        public InputAction GetAction(string actionName)
        {
            if (actionName == null)
            {
                LogError("Cannot retrieve an InputAction when the provided action name is null");
                return null;
            }

            if (m_ActiveActionMap == null)
            {
                LogError("Cannot retrieve InputAction [" + actionName + "] because there is no active action map");
                return null;
            }

            InputAction action = m_ActiveActionMap.FindAction(actionName);
            return action;
        }
    }
}
