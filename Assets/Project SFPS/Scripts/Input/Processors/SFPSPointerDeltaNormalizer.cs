using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectSFPS.Input.Processors
{
    /// <summary>
    /// Processor designed for pointer delta that converts InputSystem pointer deltas
    /// into values more similar to the traditional Unity Input system.
    /// </summary>
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    public class SFPSPointerDeltaNormalizer : InputProcessor<Vector2>
    {
        #if UNITY_EDITOR
        static SFPSPointerDeltaNormalizer()
        {
            Initialize();
        }
        #endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            InputSystem.RegisterProcessor<SFPSPointerDeltaNormalizer>();
        }

        public override Vector2 Process(Vector2 value, InputControl control)
        {
            // https://forum.unity.com/threads/mouse-delta-input.646606/
            // Multiply value by "magic" numbers.
            // 0.5f - Account for scaling applied directly in Windows code by old input system.
            // 0.1f - Account for sensitivity setting on old Mouse X and Y axes.
            return (value * 0.5f) * 0.1f;
        }
    }
}
