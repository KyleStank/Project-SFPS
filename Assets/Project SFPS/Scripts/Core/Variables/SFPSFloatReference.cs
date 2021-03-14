namespace ProjectSFPS.Core.Variables
{
    [System.Serializable]
    public class SFPSFloatReference : SFPSBaseVariableReference<float, Variables.SFPSFloat>
    {
        public SFPSFloatReference() : base() {}
        public SFPSFloatReference(float value) : base(value) {}

        public static implicit operator SFPSFloatReference(float value) => new SFPSFloatReference(value);
        public static implicit operator float(SFPSFloatReference value) => value == null ? new SFPSFloatReference().Value : value.Value;

        public static SFPSFloatReference operator +(SFPSFloatReference first, SFPSFloatReference second) => first.Value + second.Value;
        public static SFPSFloatReference operator -(SFPSFloatReference first, SFPSFloatReference second) => first.Value - second.Value;
        public static SFPSFloatReference operator *(SFPSFloatReference first, SFPSFloatReference second) => first.Value * second.Value;
        public static SFPSFloatReference operator /(SFPSFloatReference first, SFPSFloatReference second) => first.Value / second.Value;
    }
}
