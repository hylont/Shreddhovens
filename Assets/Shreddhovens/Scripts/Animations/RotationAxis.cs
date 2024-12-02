using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationAxis : MonoBehaviour
{
    [SerializeField] float m_pivotSpeed = 1f;
    [SerializeField] Vector3 m_axis = Vector3.up;

    [SerializeField] float m_invertSpeedDelay = 0f;

    private void Start()
    {
        if(m_invertSpeedDelay > 0f)
        {
            InvokeRepeating(nameof(InvertSpeed), 0, m_invertSpeedDelay);
        }
    }

    private void Update()
    {
        transform.Rotate(m_axis, Time.deltaTime * m_pivotSpeed);
    }

    void InvertSpeed()
    {
        m_pivotSpeed *= -1;
    }
}
