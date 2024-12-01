using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FadePanel : MonoBehaviour
{
    [SerializeField] float m_fadeSpeed = 1f;
    Image m_image;
    private void Start()
    {
        m_image = GetComponent<Image>();
        m_image.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        m_image.color = new(m_image.color.r, m_image.color.g, m_image.color.b, 
            Mathf.Lerp(m_image.color.a, 0f, Time.deltaTime * m_fadeSpeed));

        if(m_image.color.a < .05f) Destroy(gameObject);
    }
}
