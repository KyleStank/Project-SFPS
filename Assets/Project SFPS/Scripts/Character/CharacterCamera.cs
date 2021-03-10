using UnityEngine;

using ProjectSFPS.Core;

namespace ProjectSFPS.Character
{
    [RequireComponent(typeof(Camera))]
    public class CharacterCamera : SFPSBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform _target = null;
        [SerializeField]
        private string _targetTag = "Player";

        [Header("Camera Settings")]
        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [SerializeField]
        private Vector2 _sensitivity = new Vector2(5.0f, 5.0f);

        [SerializeField]
        private float _topClamp = -65.0f;
        [SerializeField]
        private float _bottomClamp = 65.0f;

        private Camera _camera = null;

        private Quaternion _originalRotation = Quaternion.identity;
        private Quaternion _rotation = Quaternion.identity;

        private void Awake()
        {
            Log("Initialize Character Camera.");

            // Get references.
            _camera = GetComponent<Camera>();

            if (_target == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag(_targetTag);
                if (go != null)
                {
                    _target = go.GetComponent<Transform>();
                    if (_target == null)
                    {
                        LogError("Could not find component [Character] on GameObject [" + go.name + "].");
                    }
                }
                else
                {
                    LogError("Could not find GameObject with tag [" + _targetTag + "].");
                }
            }

            _originalRotation = transform.rotation;
        }

        private void Update()
        {
            // Calculate rotation.
            Vector3 rot = transform.rotation.eulerAngles;
            float xRot = rot.x - SFPSInputManager.Instance.SFPSInput.MouseY * _sensitivity.y;
            float yRot = rot.y + SFPSInputManager.Instance.SFPSInput.MouseX * _sensitivity.x;

            rot = new Vector3(
                Mathf.Clamp(
                     xRot > 180.0f ? xRot - 360 : xRot,
                     _topClamp,
                     _bottomClamp
                 ),
                yRot,
                _originalRotation.z
            );
            _rotation = Quaternion.Euler(rot);
        }

        private void LateUpdate()
        {
            transform.position = _target.position + _target.TransformDirection(_offset);
            transform.rotation = _rotation;
        }
    }
}
