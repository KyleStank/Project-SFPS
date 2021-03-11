using UnityEngine;

namespace ProjectSFPS.Core
{
    public class SFPSInputManager : SFPSBehaviour
    {
        [Header("Keyboard")]
        [SerializeField]
        private string m_HorizontalAxis = "Horizontal";
        [SerializeField]
        private string m_VerticalAxis = "Vertical";
        [SerializeField]
        private string m_JumpButton = "Jump";

        [Header("Mouse")]
        [SerializeField]
        private string m_MouseXAxis = "Mouse X";
        [SerializeField]
        private string m_MouseYAxis = "Mouse Y";

        private static SFPSInputManager s_Instance = null;
        public static SFPSInputManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    GameObject go = new GameObject("SFPSInputManager (Singleton)");
                    s_Instance = go.AddComponent<SFPSInputManager>();
                }

                return s_Instance;
            }
        }

        private SFPSInputData _sfpsInput = default;
        public SFPSInputData SFPSInput
        {
            get { return _sfpsInput; }
        }

        private void Awake()
        {
            Log("Initialize SFPSInputManager");

            // Make sure only one instance ever exists.
            if (s_Instance == null)
            {
                s_Instance = this;
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            ReadKeyboardAxisInput();
            ReadMouseAxisInput();
        }

        /// <summary>
        /// Reads input from the keyboard.
        /// </summary>
        public void ReadKeyboardAxisInput()
        {
            _sfpsInput.Horizontal = Input.GetAxisRaw(m_HorizontalAxis);
            _sfpsInput.Vertical = Input.GetAxisRaw(m_VerticalAxis);
        }

        /// <summary>
        /// Reads input from the mouse.
        /// </summary>
        public void ReadMouseAxisInput()
        {
            _sfpsInput.MouseX = Input.GetAxisRaw(m_MouseXAxis);
            _sfpsInput.MouseY = Input.GetAxisRaw(m_MouseYAxis);
        }
    }
}
