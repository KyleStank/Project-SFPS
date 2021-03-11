using UnityEngine;

using ProjectSFPS.Core;

namespace ProjectSFPS.Character
{
    [RequireComponent(typeof(SFPSUserInput))]
    public class SFPSCharacter : SFPSBehaviour
    {
        private SFPSUserInput m_UserInput = null;
        private SFPSCharacterMotor m_CharacterMotor = null;

        private void Awake()
        {
            Log("Initialize Character");

            m_UserInput = GetComponent<SFPSUserInput>();
            m_CharacterMotor = GetComponent<SFPSCharacterMotor>();

            if (m_CharacterMotor == null)
            {
                LogWarning("Character movement and rotation will not be processed because no CharacterMotor is attached");
            }
        }

        private void Update()
        {
            if (m_CharacterMotor != null)
            {
                m_CharacterMotor.Rotate(SFPSInputManager.Instance.SFPSInput.MouseX);
            }
        }

        private void FixedUpdate()
        {
            if (m_CharacterMotor != null)
            {
                m_CharacterMotor.Move(SFPSInputManager.Instance.SFPSInput.Horizontal, SFPSInputManager.Instance.SFPSInput.Vertical);
            }
        }
    }
}
