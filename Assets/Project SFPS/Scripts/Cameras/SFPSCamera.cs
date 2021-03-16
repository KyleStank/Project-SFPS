using UnityEngine;

using ProjectSFPS.Core.EventSystem;
using ProjectSFPS.Core.Variables;

namespace ProjectSFPS.Cameras
{
    [RequireComponent(typeof(Camera))]
    public class SFPSCamera : SFPSBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField]
        private SFPSVector3Reference m_Offset = Vector3.zero;
        [SerializeField]
        private SFPSVector2Reference m_Sensitivity = new Vector2(5.0f, 5.0f);

        [SerializeField]
        private SFPSFloatReference m_TopClamp = -65.0f;
        [SerializeField]
        private SFPSFloatReference m_BottomClamp = 65.0f;

        private Camera m_Camera = null;
        private Transform m_Target = null;
        private Quaternion m_OriginalRotation = Quaternion.identity;

        private void Awake()
        {
            Log("Initialize Camera");

            m_Camera = GetComponent<Camera>();

            m_OriginalRotation = transform.rotation;
        }

        private void OnEnable()
        {
            EventSystem.Subscribe<Vector3>("OnCharacterMove", OnCharacterMove);
        }

        private void OnDisable()
        {
            EventSystem.Unsubscribe<Vector3>("OnCharacterMove", OnCharacterMove);
        }

        private void OnCharacterMove(Vector3 velocity)
        {
            Debug.Log("Vel: " + velocity);
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

        public void Rotate(float horizontal, float vertical)
        {
            // Detect input.
            Quaternion rot = transform.rotation;
            if (horizontal != 0 || vertical != 0)
            {
                Vector3 eulerAngles = rot.eulerAngles;

                // Calculate rotation.
                float xRot = eulerAngles.x - vertical * m_Sensitivity.Value.y;
                float yRot = eulerAngles.y + horizontal * m_Sensitivity.Value.x;
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
