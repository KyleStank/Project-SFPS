using UnityEngine;

using ProjectSFPS.Core;

namespace ProjectSFPS.Character
{
    public class Character : LoggingBehaviour
    {
        private CharacterControl _characterControl = null;
        private CharacterMotor _characterMotor = null;

        private void Awake()
        {
            Log("Initialize Character.");

            // Get references.
            _characterControl = GetComponent<CharacterControl>();
            _characterMotor = GetComponent<CharacterMotor>();
        }

        private void Update()
        {

        }
    }
}
