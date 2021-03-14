namespace ProjectSFPS.Core.Variables
{
    [System.Serializable]
    public class SFPSStringReference : SFPSBaseVariableReference<string, Variables.SFPSString>
    {
        public SFPSStringReference() : base() {}
        public SFPSStringReference(string value) : base(value) {}

        public static implicit operator SFPSStringReference(string value)
        {
            return new SFPSStringReference(value);
        }
    }
}
