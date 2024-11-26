using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Shreddhovens.Assets.Scripts.Builder
{
    [RequireComponent(typeof(Image))]
    public class UINote : MonoBehaviour
    {
        public int SixteenthPosition = 0;
        public int BPM = 60;
        public float Speed = .025f;
        public UIKey KeyMatch = null;

        Color m_startColorImage = Color.white;
        Color m_startColorText = Color.white;

        [SerializeField] float m_YAutoDestroy = -5000;
        [SerializeField] float m_canvasHeight = 2.2f;

        [SerializeField] TextMeshProUGUI m_text;

        [SerializeField] bool m_debugMode = false;
        [SerializeField] Image m_image;

        private void Start()
        {
            if (m_debugMode) return;

            m_startColorImage = m_image.color;
            m_image.color =
                new(m_startColorImage.r, m_startColorImage.g, m_startColorImage.b, 0);
            m_image.enabled = false;

            m_startColorText = m_text.color;
            m_text.color =
                new(m_startColorText.r, m_startColorText.g, m_startColorText.b, 0);
            m_text.gameObject.SetActive(false);
        }

        void Update()
        {
            transform.Translate(new(0,- BPM * Time.deltaTime * Speed));

            if (!m_debugMode)
            {
                if(transform.position.y < m_YAutoDestroy + m_canvasHeight)
                {
                    m_image.enabled = true;
                    m_text.gameObject.SetActive(true);
                    float l_alpha = (m_YAutoDestroy + m_canvasHeight) - transform.position.y;
                    m_image.color = new(m_startColorImage.r, m_startColorImage.g, m_startColorImage.b, l_alpha);
                    m_text.alpha = l_alpha;
                }
            }


            if(transform.position.y < m_YAutoDestroy)
            {
                if(KeyMatch != null)
                {
                    KeyMatch.Play();
                }

                Destroy(gameObject);
            }
        }
    }
}