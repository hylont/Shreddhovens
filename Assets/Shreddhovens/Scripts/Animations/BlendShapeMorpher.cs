using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class BlendShapeMorpher : MonoBehaviour
{
    float m_targetWeight = 0f;
    [SerializeField] int m_blendShapeIdx = 0;
    SkinnedMeshRenderer m_renderer;

    public float Speed = 1f;

    private void Start()
    {
        m_renderer = GetComponent<SkinnedMeshRenderer>();
    }

    public void RequestWeight(float weight)
    {
        m_renderer.SetBlendShapeWeight(m_blendShapeIdx, 0);
        m_targetWeight = weight;
    }

    private void Update()
    {
        if(m_renderer.GetBlendShapeWeight(m_blendShapeIdx) != m_targetWeight)
        {
            m_renderer.SetBlendShapeWeight(m_blendShapeIdx,
                Mathf.Lerp(m_renderer.GetBlendShapeWeight(m_blendShapeIdx), m_targetWeight, Time.deltaTime * Speed));
        }
        else
        {
            m_targetWeight = 0;
        }
    }
}
