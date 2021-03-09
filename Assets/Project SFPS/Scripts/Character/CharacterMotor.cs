using UnityEngine;

using ProjectSFPS.Core;

namespace ProjectSFPS.Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterMotor : SFPSBehaviour, ICharacterMotor
    {
        [SerializeField]
        private float _speed = 10.0f;

        private Rigidbody _rigidbody = null;

        private void Awake()
        {
            Log("Initialize Character Motor.");

            // Get references.
            _rigidbody = GetComponent<Rigidbody>();
        }
    }
}
