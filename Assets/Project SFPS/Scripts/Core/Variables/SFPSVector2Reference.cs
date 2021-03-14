namespace ProjectSFPS.Core.Variables
{
    [System.Serializable]
    public class SFPSVector2Reference : SFPSBaseVariableReference<UnityEngine.Vector2, Variables.SFPSVector2>
    {
        public SFPSVector2Reference() : base() {}
        public SFPSVector2Reference(UnityEngine.Vector2 value) : base(value) {}

        public static implicit operator SFPSVector2Reference(UnityEngine.Vector2 value)
        {
            return new SFPSVector2Reference(value);
        }
    }
}
