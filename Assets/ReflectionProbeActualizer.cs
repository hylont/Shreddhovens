using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(HDAdditionalReflectionData))]
public class ReflectionProbeActualizer : MonoBehaviour
{
    [SerializeField] float m_repeatRate = .05f;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(Actualize), 0, m_repeatRate);
    }

    void Actualize()
    {
        GetComponent<HDAdditionalReflectionData>().RequestRenderNextUpdate();
    }
}
