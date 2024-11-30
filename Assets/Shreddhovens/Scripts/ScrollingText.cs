using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScrollingText : MonoBehaviour
{
    TextMeshProUGUI m_text;

    float m_alarm = 0f;
    [SerializeField] float m_changeSpeed = .2f;
    [SerializeField] int m_maxSpaces = 50;
    int m_nbSpaces = 0;

    [SerializeField] bool m_canScroll = false;

    string m_baseText;



    private void Start()
    {
        m_text = GetComponent<TextMeshProUGUI>();   

        if(m_canScroll)
        {
            ActivateScrolling(m_text.text);
        }
    }


    public void ActivateScrolling(string p_text)
    {
        m_canScroll = true;
        m_text.text = p_text;
        m_baseText = p_text;
        
        for(int l_i = 0; l_i < m_nbSpaces; l_i++)
        {
            m_text.text += " ";
        }
    }

    private void Update()
    {
        if (!m_canScroll) return;

        m_alarm += Time.deltaTime;

        if(m_alarm > m_changeSpeed)
        {
            m_alarm = 0f;

            //m_text
        }
    }
}
