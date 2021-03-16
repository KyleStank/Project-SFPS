using UnityEngine;
using UnityEngine.InputSystem;

using ProjectSFPS.Core.Input;
using ProjectSFPS.Core.Variables;

namespace ProjectSFPS.Characters
{
    [RequireComponent(typeof(SFPSUserInput))]
    public class SFPSCharacter : SFPSBehaviour
    {
        [Header("Input Actions")]
        [SerializeField]
        private SFPSStringReference m_MoveAction = "Move";
        [SerializeField]
        private SFPSStringReference m_LookAction = "Look";

        private InputAction m_MoveInputAction = null;
        private InputAction m_LookInputAction = null;

        private SFPSUserInput m_UserInput = null;
        private SFPSCharacterMotor m_CharacterMotor = null;

        private Vector2 m_CurrentLookInput = Vector2.zero;
        private Vector2 m_CurrentMoveInput = Vector2.zero;

        private void Awake()
        {
            Log("Initialize Character");

            m_UserInput = GetComponent<SFPSUserInput>();
            m_CharacterMotor = GetComponent<SFPSCharacterMotor>();

            if (m_CharacterMotor == null)
                LogWarning("Character movement and rotation will not be processed because no CharacterMotor is attached");
        }

        private void Start()
        {
            // Move input action.
            m_MoveInputAction = m_UserInput.GetAction(m_MoveAction);
            if (m_MoveInputAction == null)
                LogError("InputAction [" + m_MoveAction + "] not found. Movement input will be ignored on [" + name + "].");

            // Look input action.
            m_LookInputAction = m_UserInput.GetAction(m_LookAction);
            if (m_LookInputAction == null)
                LogError("InputAction [" + m_LookAction + "] not found. Rotation input will be ignored on [" + name + "].");
        }

        private void ReadInput()
        {
            if (m_LookInputAction != null)
                m_CurrentLookInput = m_LookInputAction.ReadValue<Vector2>();

            if (m_MoveInputAction != null)
                m_CurrentMoveInput = m_MoveInputAction.ReadValue<Vector2>();
        }

        private void Update()
        {
            ReadInput();

            if (m_CharacterMotor != null)
                m_CharacterMotor.Rotate(m_CurrentLookInput.x);
        }

        private void FixedUpdate()
        {
            if (m_CharacterMotor != null)
                m_CharacterMotor.Move(m_CurrentMoveInput.x, m_CurrentMoveInput.y);
        }
    }
}
