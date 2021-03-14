namespace ProjectSFPS.Core.Variables
{
    [System.Serializable]
    public class SFPSIntReference : SFPSBaseVariableReference<int, Variables.SFPSInt>
    {
        public SFPSIntReference() : base() {}
        public SFPSIntReference(int value) : base(value) {}

        public static implicit operator SFPSIntReference(int value) => new SFPSIntReference(value);
        public static implicit operator int(SFPSIntReference value) => value == null ? new SFPSIntReference().Value : value.Value;

        public static SFPSIntReference operator +(SFPSIntReference first, SFPSIntReference second) => first.Value + second.Value;
        public static SFPSIntReference operator -(SFPSIntReference first, SFPSIntReference second) => first.Value - second.Value;
        public static SFPSIntReference operator *(SFPSIntReference first, SFPSIntReference second) => first.Value * second.Value;
        public static SFPSIntReference operator /(SFPSIntReference first, SFPSIntReference second) => first.Value / second.Value;
    }
}
