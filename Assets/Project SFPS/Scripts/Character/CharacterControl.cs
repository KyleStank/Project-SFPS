using UnityEngine;

using ProjectSFPS.Core;

namespace ProjectSFPS.Character
{
    public class CharacterControl : LoggingBehaviour, ICharacterControl
    {
        private string _horizontalAxis = "Horizontal";
        private string _verticalAxis = "Vertical";

        private float _rawHorizontalInput = 0.0f;
        private float _rawVerticalInput = 0.0f;

        private CharacterInput _characterInput = default;

        private void Awake()
        {
            Log("Initialize Character Input.");
        }

        public CharacterInput ReadInput()
        {
            _rawHorizontalInput = Input.GetAxisRaw(_horizontalAxis);
            _rawVerticalInput = Input.GetAxisRaw(_verticalAxis);

            return _characterInput;
        }
    }
}
