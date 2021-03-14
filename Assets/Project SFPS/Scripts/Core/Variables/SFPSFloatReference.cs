namespace ProjectSFPS.Core.Variables
{
    [System.Serializable]
    public class SFPSFloatReference : SFPSBaseVariableReference<float, Variables.SFPSFloat>
    {
        public SFPSFloatReference() : base() {}
        public SFPSFloatReference(float value) : base(value) {}

        public static implicit operator SFPSFloatReference(float value)
        {
            return new SFPSFloatReference(value);
        }
    }
}
