namespace ProjectSFPS.Core.Variables
{
    [System.Serializable]
    public class SFPSBoolReference : SFPSBaseVariableReference<bool, Variables.SFPSBool>
    {
        public SFPSBoolReference() : base() {}
        public SFPSBoolReference(bool value) : base(value) {}

        public static implicit operator SFPSBoolReference(bool value) => new SFPSBoolReference(value);
        public static implicit operator bool(SFPSBoolReference value) => value == null ? new SFPSBoolReference().Value : value.Value;
    }
}
