using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectSFPS.Core.Input.Processors
{
    /// <summary>
    /// Processor that multiplys a Vector2 value by Time.deltaTime and then multiplys the result by a constant scale factor.
    /// </summary>
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    public class SFPSScaledDeltaTime : InputProcessor<Vector2>
    {
        // This number doesn't mean anything. It just felt right to me.
        private static readonly float SCALE_FACTOR = 7.5f;

        #if UNITY_EDITOR
        static SFPSScaledDeltaTime()
        {
            Initialize();
        }
        #endif

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            InputSystem.RegisterProcessor<SFPSScaledDeltaTime>();
        }

        public override Vector2 Process(Vector2 value, InputControl control)
        {
            return (value * Time.deltaTime) * SCALE_FACTOR;
        }
    }
}
