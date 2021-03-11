using UnityEngine;

using ProjectSFPS.Core;

namespace ProjectSFPS.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class SFPSCamera : SFPSBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField]
        private Vector3 m_Offset = Vector3.zero;
        [SerializeField]
        private Vector2 m_Sensitivity = new Vector2(5.0f, 5.0f);

        [SerializeField]
        private float m_TopClamp = -65.0f;
        [SerializeField]
        private float m_BottomClamp = 65.0f;

        private UnityEngine.Camera m_Camera = null;
        private Transform m_Target = null;
        private Quaternion m_OriginalRotation = Quaternion.identity;

        private void Awake()
        {
            Log("Initialize Camera");

            m_Camera = GetComponent<UnityEngine.Camera>();

            m_OriginalRotation = transform.rotation;
        }

        public void SetTarget(Transform target)
        {
            m_Target = target;
        }

        public void Move()
        {
            if (m_Target == null)
            {
                LogError("Cannot move SFPSCamera because the target is null");
                return;
            }

            transform.position = m_Target.position + m_Target.TransformDirection(m_Offset);
        }

        public void Rotate()
        {
            Quaternion rot = transform.rotation;

            // Detect input.
            SFPSInputData sfpsInput = SFPSInputManager.Instance.SFPSInput;
            if (sfpsInput.MouseX != 0 || sfpsInput.MouseY != 0)
            {
                Vector3 eulerAngles = rot.eulerAngles;

                // Calculate rotation.
                float xRot = eulerAngles.x - SFPSInputManager.Instance.SFPSInput.MouseY * m_Sensitivity.y;
                float yRot = eulerAngles.y + SFPSInputManager.Instance.SFPSInput.MouseX * m_Sensitivity.x;
                rot = Quaternion.Euler(
                    new Vector3(
                        Mathf.Clamp(
                            xRot > 180.0f ? xRot - 360 : xRot,
                            m_TopClamp,
                            m_BottomClamp
                        ),
                        yRot,
                        m_OriginalRotation.z
                    )
                );
            }

            transform.rotation = rot;
        }
    }
}
