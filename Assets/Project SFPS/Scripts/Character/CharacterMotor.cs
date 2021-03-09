using UnityEngine;

namespace ProjectSFPS.Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterMotor : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 10.0f;

        private Rigidbody _rigidbody = null;

        private void Awake()
        {
            Debug.Log("Initialize Character Motor.");

            // Get references.
            _rigidbody = GetComponent<Rigidbody>();
        }
    }
}
