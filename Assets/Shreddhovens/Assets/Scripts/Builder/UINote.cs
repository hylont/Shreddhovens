using System.Collections;
using UnityEngine;

namespace Assets.Shreddhovens.Assets.Scripts.Builder
{
    public class UINote : MonoBehaviour
    {
        public int SixteenthPosition = 0;
        public int BPM = 60;
        [SerializeField] float m_YAutoDestroy = -5000;

        void Update()
        {
            transform.Translate(new(0,- BPM * Time.deltaTime * .1f));

            if(transform.localPosition.y < m_YAutoDestroy)
            {
                Destroy(gameObject);
            }
        }
    }
}