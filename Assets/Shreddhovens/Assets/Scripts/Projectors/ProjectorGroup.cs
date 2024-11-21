using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorGroup : MonoBehaviour
{
    AnimatedProjector[] m_projectors = null;
    [SerializeField] int m_projectorsToActivateAtEachStart = 1;
    [SerializeField] List<Material> m_emissiveMaterials = new();

    public float ActivationDelay = 0, TargetChangeSpeed = 0, DestChangeSpeed = 0, FlashInterval = .1f;

    void Awake()
    {
        m_projectors = GetComponentsInChildren<AnimatedProjector>();

        if (m_projectors == null) Debug.LogError("[PROJECTOR GROUP] No child projectors found !");
    }

    void StartAnimations()
    {
        int l_stepsUntilDelay = m_projectorsToActivateAtEachStart;
        int l_projectorSetCounter = 0;

        Material l_chosenMaterial = m_emissiveMaterials[Random.Range(0, m_emissiveMaterials.Count)];

        for (int l_idxProjector = 0; l_idxProjector < m_projectors.Length; l_idxProjector++)
        {
            AnimatedProjector l_projector = m_projectors[l_idxProjector];
            l_projector.TimeUntilBegin = l_projectorSetCounter * ActivationDelay;

            l_projector.FlashInterval = FlashInterval;

            l_projector.SetMaterial(l_chosenMaterial);

            l_projector.StartAnimation(TargetChangeSpeed, DestChangeSpeed);

            l_stepsUntilDelay--;
            if(l_stepsUntilDelay == 0)
            {
                l_stepsUntilDelay = m_projectorsToActivateAtEachStart;
                l_projectorSetCounter++;
            }
        }
    }

    public void Init(float p_activationDelay, float p_targetChangeSpeed, float p_destChangeSpeed)
    {
        ActivationDelay = p_activationDelay;
        TargetChangeSpeed = p_targetChangeSpeed;
        DestChangeSpeed = p_destChangeSpeed;
    }

    public void Init(float p_beatDelay)
    {
        Init(p_beatDelay, p_beatDelay, p_beatDelay);
    }

    private void OnEnable()
    {
        StartAnimations();
    }

    private void OnDisable()
    {
        foreach(AnimatedProjector l_projector in m_projectors)
        {
            l_projector.StopAnimation();
        }
    }
}
