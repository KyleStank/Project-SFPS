using UnityEngine;
using System.Collections;

namespace UltimateFracturing
{
    public class DieTimer : MonoBehaviour
    {
        public float SecondsToDie      = Mathf.Infinity;
        public float OffscreenLifeTime = Mathf.Infinity;

        float m_fTimer = 0.0f;

        void Start()
        {
            m_fTimer = 0.0f;
        }
    
        void Update()
        {
            m_fTimer += Time.deltaTime;

            if(m_fTimer > SecondsToDie)
            {
                Destroy(gameObject);
            }
        }
    }
}