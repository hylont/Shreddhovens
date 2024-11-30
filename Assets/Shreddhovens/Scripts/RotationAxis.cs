using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationAxis : MonoBehaviour
{
    [SerializeField] float m_pivotSpeed = 1f;
    [SerializeField] Vector3 m_axis = Vector3.up;

    private void Update()
    {
        transform.Rotate(m_axis, Time.deltaTime * m_pivotSpeed);
    }
}
