using UnityEngine;

using ProjectSFPS.Core;

namespace ProjectSFPS.Character
{
    [RequireComponent(typeof(Camera))]
    public class CharacterCamera : SFPSBehaviour
    {
        [SerializeField]
        private Character _character = null;
        [SerializeField]
        private string _characterTag = "Player";

        private Camera _camera = null;

        private void Awake()
        {
            Log("Initialize Character Camera.");

            // Get references.
            _camera = GetComponent<Camera>();

            if (_character == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag(_characterTag);
                if (go != null)
                {
                    _character = go.GetComponent<Character>();
                    if (_character == null)
                    {
                        Debug.LogError("Could not find component [Character] on GameObject [" + go.name + "].");
                    }
                }
                else
                {
                    Debug.LogError("Could not find GameObject with tag [" + _characterTag + "].");
                }
            }
        }
    }
}
