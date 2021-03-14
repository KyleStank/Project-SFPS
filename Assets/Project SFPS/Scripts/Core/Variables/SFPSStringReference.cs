namespace ProjectSFPS.Core.Variables
{
    [System.Serializable]
    public class SFPSStringReference : SFPSBaseVariableReference<string, Variables.SFPSString>
    {
        public SFPSStringReference() : base() {}
        public SFPSStringReference(string value) : base(value) {}

        public static implicit operator SFPSStringReference(string value) => new SFPSStringReference(value);
        public static implicit operator string(SFPSStringReference value) => value == null ? new SFPSStringReference().Value : value.Value;

        public static SFPSStringReference operator +(SFPSStringReference first, SFPSStringReference second) => first.Value + second.Value;
    }
}
