using UnityEngine;

using ProjectSFPS.Core;

namespace ProjectSFPS.Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterMotor : SFPSBehaviour, ICharacterMotor
    {
        [SerializeField]
        private float _accelerationRate = 10.0f;
        public float AccelerationRate
        {
            get { return _accelerationRate; }
            set { _accelerationRate = value; }
        }

        private Rigidbody _rigidbody = null;

        private void Awake()
        {
            Log("Initialize Character Motor.");

            // Get references.
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Rotate(CalculateRotation());
        }

        private void FixedUpdate()
        {
            Move(CalculateMoveDirection());
        }

        private Vector3 CalculateRotation()
        {
            SFPSInput sfpsInput = SFPSInputManager.Instance.SFPSInput;

            return new Vector3(
                0.0f,
                5.0f * sfpsInput.MouseX, // TODO: Replace hard-coded "5.0f" with sensitivity value.
                0.0f
            );
        }

        public void Rotate(Vector3 rotation)
        {
            transform.Rotate(rotation);
        }

        private Vector3 CalculateMoveDirection()
        {
            SFPSInput sfpsInput = SFPSInputManager.Instance.SFPSInput;

            // Create move direction vector based on input.
            return new Vector3(
                sfpsInput.Horizontal,
                0.0f,
                sfpsInput.Vertical
            );
        }

        public void Move(Vector3 direction)
        {
            float sqrMagnitude = direction.sqrMagnitude;

            // Calculate normalized vector with square magnitude.
            // Assign each axis directly to prevent garbage collection with "new" keyword.
            Vector3 normalizedDirection = direction / Mathf.Sqrt(sqrMagnitude);
            normalizedDirection = new Vector3(
                float.IsNaN(normalizedDirection.x) ? 0.0f : normalizedDirection.x,
                float.IsNaN(normalizedDirection.y) ? 0.0f : normalizedDirection.y,
                float.IsNaN(normalizedDirection.z) ? 0.0f : normalizedDirection.z
            );

            // Calculate velocity change.
            Vector3 velocityChange = normalizedDirection * _accelerationRate;
            velocityChange = transform.TransformDirection(velocityChange); // Convert local point to world space.
            velocityChange -= _rigidbody.velocity;
            velocityChange.y = 0.0f; // Ignore Y velocity change to prevent issues with gravity.

            // Move.
            _rigidbody.AddForce(velocityChange, ForceMode.Impulse);
        }
    }
}
