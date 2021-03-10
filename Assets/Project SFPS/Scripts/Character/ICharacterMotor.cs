using UnityEngine;

namespace ProjectSFPS.Character
{
    public interface ICharacterMotor
    {
        float AccelerationRate { get; set; }

        void Move(Vector3 direction);
        void Rotate(Vector3 rotation);
    }
}
