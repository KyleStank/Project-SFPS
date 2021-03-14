namespace ProjectSFPS.Core.Variables
{
    [System.Serializable]
    public class SFPSVector3Reference : SFPSBaseVariableReference<UnityEngine.Vector3, Variables.SFPSVector3>
    {
        public SFPSVector3Reference() : base() {}
        public SFPSVector3Reference(UnityEngine.Vector3 value) : base(value) {}

        public static implicit operator SFPSVector3Reference(UnityEngine.Vector3 value)
        {
            return new SFPSVector3Reference(value);
        }
    }
}
