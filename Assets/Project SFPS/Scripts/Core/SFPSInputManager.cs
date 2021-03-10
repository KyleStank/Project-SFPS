using UnityEngine;

namespace ProjectSFPS.Core
{
    public class SFPSInputManager : SFPSBehaviour
    {
        [Header("Keyboard")]
        [SerializeField]
        private string _horizontalAxis = "Horizontal";
        [SerializeField]
        private string _verticalAxis = "Vertical";
        [SerializeField]
        private string _jumpButton = "Jump";

        [Header("Mouse")]
        [SerializeField]
        private string _mouseXAxis = "Mouse X";
        [SerializeField]
        private string _mouseYAxis = "Mouse Y";

        private static SFPSInputManager _instance = null;
        public static SFPSInputManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SFPSInputManager (Singleton)");
                    _instance = go.AddComponent<SFPSInputManager>();
                }

                return _instance;
            }
        }

        private SFPSInput _sfpsInput = default;
        public SFPSInput SFPSInput
        {
            get { return _sfpsInput; }
        }

        private void Awake()
        {
            Log("Initialize SFPSInputManager.");

            // Make sure only one instance ever exists.
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
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
            _sfpsInput.Horizontal = Input.GetAxisRaw(_horizontalAxis);
            _sfpsInput.Vertical = Input.GetAxisRaw(_verticalAxis);
        }

        /// <summary>
        /// Reads input from the mouse.
        /// </summary>
        public void ReadMouseAxisInput()
        {
            _sfpsInput.MouseX = Input.GetAxisRaw(_mouseXAxis);
            _sfpsInput.MouseY = Input.GetAxisRaw(_mouseYAxis);
        }
    }
}
