using UnityEngine;

namespace ProjectSFPS.Core.Variables
{
    [System.Serializable]
    public class SFPSVector2Reference : SFPSBaseVariableReference<Vector2, Variables.SFPSVector2>
    {
        public SFPSVector2Reference() : base() {}
        public SFPSVector2Reference(Vector2 value) : base(value) {}

        public static implicit operator SFPSVector2Reference(Vector2 value) => new SFPSVector2Reference(value);
        public static implicit operator Vector2(SFPSVector2Reference value) => value == null ? new SFPSVector2Reference().Value : value.Value;

        public static SFPSVector2Reference operator +(SFPSVector2Reference first, SFPSVector2Reference second) => first.Value + second.Value;
        public static SFPSVector2Reference operator -(SFPSVector2Reference first, SFPSVector2Reference second) => first.Value - second.Value;
        public static SFPSVector2Reference operator *(SFPSVector2Reference first, SFPSVector2Reference second) => first.Value * second.Value;
        public static SFPSVector2Reference operator /(SFPSVector2Reference first, SFPSVector2Reference second) => first.Value / second.Value;
    }
}
