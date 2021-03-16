using UnityEngine;
using UnityEngine.InputSystem;

using ProjectSFPS.Core.Input;
using ProjectSFPS.Core.Variables;
using ProjectSFPS.Characters;

namespace ProjectSFPS.Cameras
{
    [RequireComponent(typeof(SFPSUserInput))]
    public class SFPSCameraController : SFPSBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SFPSCamera m_Camera = null;
        [SerializeField]
        private SFPSStringReference m_CameraTag = "MainCamera";
        [SerializeField]
        private SFPSCharacter m_CharacterTarget = null;
        [SerializeField]
        private SFPSStringReference m_CharacterTag = "Player";

        [Header("Input Actions")]
        [SerializeField]
        private SFPSStringReference m_LookAction = "Look";

        private SFPSUserInput m_UserInput = null;
        private InputAction m_LookInputAction = null;

        private Vector2 m_CurrentInput = Vector2.zero;

        private void Awake()
        {
            Log("Initialize Camera Controller");

            m_UserInput = GetComponent<SFPSUserInput>();

            // Find camera reference.
            if (m_Camera == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag(m_CameraTag);
                if (go != null)
                {
                    m_Camera = go.GetComponent<SFPSCamera>();
                    if (m_Camera == null)
                        LogError("Could not find component [Camera] on GameObject [" + go.name + "]");
                }
                else
                {
                    LogError("Could not find GameObject with tag [" + m_CameraTag + "]");
                }
            }

            // Find character reference.
            if (m_CharacterTarget == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag(m_CharacterTag);
                if (go != null)
                {
                    m_CharacterTarget = go.GetComponent<SFPSCharacter>();
                    if (m_CharacterTarget == null)
                        LogError("Could not find component [Character] on GameObject [" + go.name + "]");
                }
                else
                {
                    LogError("Could not find GameObject with tag [" + m_CharacterTag + "]");
                }
            }

            // Camera or character could still be null, so we check again.
            if (m_Camera != null && m_CharacterTarget != null)
                m_Camera.SetTarget(m_CharacterTarget.transform);
        }

        private void Start()
        {
            // Look input action.
            m_LookInputAction = m_UserInput.GetAction(m_LookAction);
            if (m_LookInputAction == null)
                LogError("InputAction [" + m_LookAction + "] not found. User input will be ignored on [" + name + "].");
        }

        private void Update()
        {
            if (m_LookInputAction == null) return;

            m_CurrentInput = m_LookInputAction.ReadValue<Vector2>();
        }

        private void LateUpdate()
        {
            if (m_Camera == null)
            {
                LogError("Unable to process SFPSCameraController because no camera is assigned");
                return;
            }

            // Update position and rotation.
            m_Camera.Move();
            m_Camera.Rotate(m_CurrentInput.x, m_CurrentInput.y);
        }
    }
}
