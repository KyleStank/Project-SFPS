namespace ProjectSFPS.Core.Variables
{
    [System.Serializable]
    public class SFPSIntReference : SFPSBaseVariableReference<int, Variables.SFPSInt>
    {
        public SFPSIntReference() : base() {}
        public SFPSIntReference(int value) : base(value) {}

        public static implicit operator SFPSIntReference(int value)
        {
            return new SFPSIntReference(value);
        }
    }
}
