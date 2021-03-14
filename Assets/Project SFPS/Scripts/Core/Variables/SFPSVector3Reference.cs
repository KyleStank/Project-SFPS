using UnityEngine;

namespace ProjectSFPS.Core.Variables
{
    [System.Serializable]
    public class SFPSVector3Reference : SFPSBaseVariableReference<Vector3, Variables.SFPSVector3>
    {
        public SFPSVector3Reference() : base() {}
        public SFPSVector3Reference(Vector3 value) : base(value) {}

        public static implicit operator SFPSVector3Reference(Vector3 value) => new SFPSVector3Reference(value);
        public static implicit operator Vector3(SFPSVector3Reference value) => value == null ? new SFPSVector3Reference().Value : value.Value;

        public static SFPSVector3Reference operator +(SFPSVector3Reference first, SFPSVector3Reference second) => first.Value + second.Value;
        public static SFPSVector3Reference operator -(SFPSVector3Reference first, SFPSVector3Reference second) => first.Value - second.Value;
    }
}
