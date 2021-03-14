using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectSFPS.Core.Input
{
    // TODO: Implement according to MVP document.
    [CreateAssetMenu(fileName = "Input Settings", menuName = "SFPS/Input Settings")]
    public class SFPSInputSettings : ScriptableObject
    {
        [SerializeField]
        private InputActionAsset m_InputActionAsset = null;
        public InputActionAsset InputActionAsset
        {
            get { return m_InputActionAsset; }
        }
    }
}
