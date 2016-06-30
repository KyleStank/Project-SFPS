using UnityEngine;
using System.Collections;

public class ExplosionSource : MonoBehaviour
{
    public float   InfluenceRadius         = 1.0f;
    public float   Force                   = 1.0f;
    public bool    CheckStructureIntegrity = false;
    public bool    MoveManually            = false;
    public Vector3 PosStart;
    public Vector3 PosEnd;
    public float   MoveDuration            = 2.0f;

    FracturedObject[] m_aFracturedObjects = null;
    float m_fStartTime = 0.0f;

    void Start()
    {
        m_aFracturedObjects = FindObjectsOfType(typeof(FracturedObject)) as FracturedObject[];
        m_fStartTime        = Time.time;
    }
    
    void Update()
    {
        if(MoveManually == false)
        {
            transform.position = Vector3.Lerp(PosStart, PosEnd, Mathf.Clamp01((Time.time - m_fStartTime) / MoveDuration));
        }

        foreach(FracturedObject fracturedObject in m_aFracturedObjects)
        {
            fracturedObject.Explode(transform.position, Force, InfluenceRadius, false, true, false, CheckStructureIntegrity);
        }
    }
}
