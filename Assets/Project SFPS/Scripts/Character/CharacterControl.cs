using UnityEngine;

using ProjectSFPS.Core;

namespace ProjectSFPS.Character
{
    public class CharacterControl : SFPSBehaviour, ICharacterControl
    {
        private string _horizontalAxis = "Horizontal";
        private string _verticalAxis = "Vertical";

        private float _rawHorizontalInput = 0.0f;
        private float _rawVerticalInput = 0.0f;

        private CharacterInput _characterInput = default;

        public CharacterInput CharacterInput
        {
            get { return _characterInput; }
        }

        private void Awake()
        {
            Log("Initialize Character Input.");
        }

        private void Update()
        {
            // Technically not required as ReadInput() assigned values to _characterInput before returning.
            _characterInput = ReadInput();
        }

        /// <summary>
        /// Reads input from the user and returns the result.
        /// Also assigns value to CharacterInput property.
        /// </summary>
        public CharacterInput ReadInput()
        {
            // Get input values.
            _rawHorizontalInput = Input.GetAxisRaw(_horizontalAxis);
            _rawVerticalInput = Input.GetAxisRaw(_verticalAxis);

            // Assign character input values and return result.
            _characterInput.Horizontal = _rawHorizontalInput;
            _characterInput.Vertical = _rawVerticalInput;
            return _characterInput;
        }
    }
}
