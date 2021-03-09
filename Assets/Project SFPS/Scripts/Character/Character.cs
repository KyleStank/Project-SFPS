using UnityEngine;

namespace ProjectSFPS.Character
{
    public class Character : MonoBehaviour
    {
        private CharacterControl _characterControl = null;
        private CharacterMotor _characterMotor = null;

        private void Awake()
        {
            Debug.Log("Initialize Character.");

            // Get references.
            _characterControl = GetComponent<CharacterControl>();
            _characterMotor = GetComponent<CharacterMotor>();
        }

        private void Update()
        {
            
        }
    }
}
