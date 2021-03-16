using UnityEngine;

using ProjectSFPS.Core.Variables;

namespace ProjectSFPS.Characters
{
    [RequireComponent(typeof(Rigidbody))]
    public class SFPSCharacterMotor : SFPSBehaviour
    {
        [SerializeField]
        private SFPSFloatReference _accelerationRate = 10.0f;
        [SerializeField]
        private SFPSVector2Reference m_TurnSensitivity = new Vector2(5.0f, 5.0f);

        public float AccelerationRate
        {
            get { return _accelerationRate; }
            set { _accelerationRate = value; }
        }

        private Rigidbody _rigidbody = null;

        private void Awake()
        {
            Log("Initialize Character Motor");

            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Move(float horizontal, float vertical)
        {
            // Create movement direction from input.
            Vector3 direction =  new Vector3(horizontal, 0.0f, vertical);

            // Calculate normalized vector with square magnitude.
            // Assign each axis directly to prevent garbage collection with "new" keyword.
            Vector3 normalizedDirection = direction / Mathf.Sqrt(direction.sqrMagnitude);
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
            _rigidbody.AddForce(velocityChange, ForceMode.Impulse);
        }

        public void Rotate(float horizontal)
        {
            // Calculate rotation.
            Vector3 rot = transform.rotation.eulerAngles;
            rot = new Vector3(
                0.0f,
                rot.y + (horizontal * m_TurnSensitivity.Value.x),
                0.0f
            );

            transform.rotation = Quaternion.Euler(rot);
        }
    }
}
