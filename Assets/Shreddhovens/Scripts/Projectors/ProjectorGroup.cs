using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProjectorGroup : MonoBehaviour
{
    public AnimatedProjector[] Projectors = null;
    [SerializeField] int m_projectorsToActivateAtEachStart = 1;
    [SerializeField] List<Material> m_emissiveMaterials = new();
    [SerializeField] bool m_flashIntervalEqualsTargetChange = false;

    public bool m_debug = false;

    [SerializeField] bool m_animateOnStart;

    [Header("Canvas")]
    [SerializeField] Canvas m_onlineCanvas;
    [SerializeField] Canvas m_offlineCanvas;
    [SerializeField] TextMeshProUGUI m_proceduresText;
    [SerializeField] TextMeshProUGUI m_targetChangeText;
    [SerializeField] TextMeshProUGUI m_destChangeText;
    [SerializeField] TextMeshProUGUI m_colorText;

    public float ActivationDelay = 0, TargetChangeSpeed = 0, DestChangeSpeed = 0, FlashInterval = .1f;

    void Awake()
    {
        Projectors = GetComponentsInChildren<AnimatedProjector>();

        if (m_flashIntervalEqualsTargetChange) FlashInterval = TargetChangeSpeed;

        if (Projectors == null) Debug.LogError("[PROJECTOR GROUP] No child projectors found !");
    }

    private void Start()
    {
        if (!m_animateOnStart)
        {
            enabled = false;
        }
    }

    public void SetNewMaterial()
    {
        Material l_chosenMaterial = m_emissiveMaterials[Random.Range(0, m_emissiveMaterials.Count)];
        for (int l_idxProjector = 0; l_idxProjector < Projectors.Length; l_idxProjector++)
        {
            Projectors[l_idxProjector].SetMaterial(l_chosenMaterial);
        }
    }

    void StartAnimations()
    {
        m_offlineCanvas.gameObject.SetActive(false);

        m_proceduresText.text = "";

        int l_stepsUntilDelay = m_projectorsToActivateAtEachStart;
        int l_projectorSetCounter = 0;

        Material l_chosenMaterial = m_emissiveMaterials[Random.Range(0, m_emissiveMaterials.Count)];

            List<EAnimation> l_includedAnims = new();

        for (int l_idxProjector = 0; l_idxProjector < Projectors.Length; l_idxProjector++)
        {
            AnimatedProjector l_projector = Projectors[l_idxProjector];
            l_projector.TimeUntilBegin = l_projectorSetCounter * ActivationDelay;

            if (m_flashIntervalEqualsTargetChange)
            {
                l_projector.FlashInterval = TargetChangeSpeed;
            }
            else
            {
                l_projector.FlashInterval = FlashInterval;
            }

            l_projector.SetMaterial(l_chosenMaterial);

                l_projector.StartAnimation(TargetChangeSpeed, DestChangeSpeed);

            if(m_debug) print($"[PROJECTOR GROUP] {l_projector.name} animation enabled !");

            foreach(EAnimation anim in l_projector.Animations)
            {
                if(!l_includedAnims.Contains(anim)) l_includedAnims.Add(anim);
            }

            l_stepsUntilDelay--;
            if(l_stepsUntilDelay == 0)
            {
                l_stepsUntilDelay = m_projectorsToActivateAtEachStart;
                l_projectorSetCounter++;
            }
        }

        m_onlineCanvas.gameObject.SetActive(true);

        foreach (EAnimation anim in l_includedAnims) m_proceduresText.text += $"{anim} / ";

        m_targetChangeText.text = "Target change speed = "+TargetChangeSpeed.ToString("0.00");
        m_destChangeText.text = "Destination change speed = "+DestChangeSpeed.ToString("0.00");
        m_colorText.text = l_chosenMaterial.name;
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
        if(m_debug) print("[PROJECTOR GROUP] Disabling");
        m_onlineCanvas.gameObject.SetActive(false);
        m_offlineCanvas.gameObject.SetActive(true);
        foreach(AnimatedProjector l_projector in Projectors)
        {
            l_projector.StopAnimation();
        }
    }
}
