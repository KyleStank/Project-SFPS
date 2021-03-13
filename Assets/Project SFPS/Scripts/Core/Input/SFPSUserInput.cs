using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectSFPS.Core.Input
{
    internal static class BindingPaths
    {
        public const string POINTER_DELTA = "<Pointer>/delta";
    }

    internal static class SFPSProcessorNames
    {
        public const string POINTER_DELTA_NORMALIZER = "SFPSPointerDeltaNormalizer";
        public const string SCALED_DELTA_TIME = "SFPSScaledDeltaTime";
    }

    public class SFPSUserInput : SFPSBehaviour
    {
        [Header("References")]
        [SerializeField]
        private InputActionAsset m_InputActions = null;

        [Header("Configuration")]
        [SerializeField]
        private string m_DefaultActionMap = "Player";

        private InputActionMap m_ActiveActionMap = null;
        public InputActionMap ActiveActionMap
        {
            get { return m_ActiveActionMap; }
        }

        // private static SFPSUserInput s_Instance = null;
        // public static SFPSUserInput Instance
        // {
        //     get
        //     {
        //         if (s_Instance == null)
        //         {
        //             GameObject go = new GameObject("SFPSUserInput (Singleton)");
        //             s_Instance = go.AddComponent<SFPSUserInput>();
        //         }

        //         return s_Instance;
        //     }
        // }

        private void Awake()
        {
            // // Make sure only one instance ever exists.
            // if (s_Instance == null)
            // {
            //     Log("Initialize SFPSUserInput");
            //     s_Instance = this;
            // }
            // else if (s_Instance != this)
            // {
            //     LogWarning("Only one SFPSUserInput can exist at a time. Destroying extra...");
            //     Destroy(gameObject);
            // }

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

            if (m_InputActions == null)
            {
                LogError("Cannot set InputActionMap [" + actionMapName + "] as active because no InputActions asset is assigned");
                return false;
            }

            // Try to find action map.
            InputActionMap actionMap = m_InputActions.FindActionMap(actionMapName);

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

                // TODO: Finish this later.
                // // Look through actions and setup.
                // var actions = actionMap.actions;
                // for (int i = 0; i < actions.Count; i++)
                // {
                //     var action = actions[i];
                //     var bindings = action.bindings;
                //     for (int j = 0; j < bindings.Count; j++)
                //     {
                //         // Loop through each binding and add required processors as needed.
                //         var binding = bindings[j];
                //         switch (binding.path)
                //         {
                //             case BindingPaths.POINTER_DELTA:
                //                 if (!binding.processors.Contains(SFPSProcessorNames.SCALED_DELTA_TIME))
                //                 {
                //                     binding.processors += ',' + SFPSProcessorNames.SCALED_DELTA_TIME + ',';
                //                 }
                //                 break;

                //             default:
                //                 break;
                //         }

                //         // Apply modified bindings.
                //         action.ApplyBindingOverride(
                //             j,
                //             new InputBinding(
                //                 binding.path,
                //                 binding.action,
                //                 binding.groups,
                //                 binding.processors,
                //                 binding.interactions,
                //                 binding.name
                //             )
                //         );
                //     }
                // }
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
