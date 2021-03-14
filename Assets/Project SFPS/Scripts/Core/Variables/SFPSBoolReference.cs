namespace ProjectSFPS.Core.Variables
{
    [System.Serializable]
    public class SFPSBoolReference : SFPSBaseVariableReference<bool, Variables.SFPSBool>
    {
        public SFPSBoolReference() : base() {}
        public SFPSBoolReference(bool value) : base(value) {}

        public static implicit operator SFPSBoolReference(bool value)
        {
            return new SFPSBoolReference(value);
        }
    }
}
