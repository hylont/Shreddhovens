using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EAnimation
{
    ROTATION_TO_POINT, FLASH, LERP}

public class AnimatedProjector : MonoBehaviour
{
    [SerializeField] GameObject m_lightObject;
    [SerializeField] GameObject m_lightOriginObject;
    public float LightIntensity = 100;

    public List<EAnimation> Animations;

    public float TimeUntilBegin = 0f;
    public List<Transform> Targets, MoveDestinations;
    public int m_targetIdx, m_destIdx;
    public float m_targetChangeSpeed = 0f;
    public float m_destChangeSpeed = 0f;

    //Timers
    float m_targetChangeTimer = 0;
    float m_destChangeTimer = 0;
    float m_flashChangeTimer = 0;

    public float RotationSpeed = 1f, MovementSpeed = 1f;
    [SerializeField] Transform m_beamTransform;
    [SerializeField] Transform m_supportTransform;
    [SerializeField] Vector3 m_supportRotationOffset = Vector3.zero;

    [SerializeField] bool m_canAnimate = false;
    [SerializeField] bool m_activateLightOnBegin = true;

    [SerializeField] bool m_finiteRay = false;
    [SerializeField] float m_rayLengthScale = 10;

    [Header("FLASH")]
    public float FlashInterval = .1f;

    [Header("Debug")]
    [SerializeField] bool m_debug = false;
    private void Awake()
    {
        if (!m_finiteRay)
        {
            m_lightOriginObject.transform.localScale = new(1, 1, m_rayLengthScale);
        }

        if (m_canAnimate)
        {
            if(TimeUntilBegin > 0f)
            {
                m_canAnimate = false;
                StartCoroutine(StartAnimationCoroutine());
            }
            else
            {
                StartAnimation();
            }
        }

        m_lightObject.SetActive(m_activateLightOnBegin);
    }

    public void SetMaterial(Material material)
    {
        m_lightObject.GetComponent<Renderer>().material = material;
        m_lightObject.GetComponent<Renderer>().material.SetFloat("_EmissiveIntensity", 
            LightIntensity + UnityEngine.Random.Range(LightIntensity * -.1f, LightIntensity *.1f));
    }

    private void Update()
    {
        if (!m_canAnimate) return;

        foreach (var anim in Animations)
        {
            if (anim != EAnimation.FLASH)
            {
                if (MoveDestinations.Count > 0 && m_destIdx >= MoveDestinations.Count)
                {
                    Debug.LogError("[PROJECTOR] Shouldn't exceed destinations");
                    return;
                }

                if (Targets.Count > 0 && m_targetIdx >= Targets.Count)
                {
                    Debug.LogError("[PROJECTOR] Shouldn't exceed targets");
                    return;
                }

                if (anim == EAnimation.LERP)
                {
                    transform.position = Vector3.Lerp(transform.position, MoveDestinations[m_destIdx].position, Time.deltaTime * MovementSpeed);

                    m_destChangeTimer += Time.deltaTime;
                    if(m_destChangeTimer > m_destChangeSpeed)
                    {
                        if (m_destIdx < MoveDestinations.Count - 1)
                        {
                            m_destIdx++;
                        }
                        else m_destIdx = 0;

                        m_destChangeTimer = 0;
                    }
                }

                if (anim == EAnimation.ROTATION_TO_POINT)
                {
                    Vector3 l_directionToTarget = Targets[m_targetIdx].position - m_supportTransform.position;
                    Quaternion l_targetRotationSupport = Quaternion.LookRotation(l_directionToTarget);
                    Quaternion _smoothedRotationSupport = Quaternion.Slerp(m_supportTransform.rotation, l_targetRotationSupport, Time.deltaTime * RotationSpeed);

                    m_supportTransform.rotation = _smoothedRotationSupport;
                    Vector3 l_supportEuler = m_supportTransform.eulerAngles;
                    l_supportEuler.x = m_supportRotationOffset.x;
                    l_supportEuler.z = m_supportRotationOffset.z;
                    m_supportTransform.eulerAngles = l_supportEuler;

                    l_directionToTarget = Targets[m_targetIdx].position - m_beamTransform.position;
                    Quaternion l_targetRotationBeam = Quaternion.LookRotation(l_directionToTarget);
                    Quaternion l_smoothedRotationBeam = Quaternion.Slerp(m_beamTransform.rotation, l_targetRotationBeam, Time.deltaTime * RotationSpeed);

                    m_beamTransform.rotation = l_smoothedRotationBeam;

                    m_targetChangeTimer += Time.deltaTime;
                    if(m_targetChangeTimer > m_targetChangeSpeed)
                    {
                        if (m_targetIdx < Targets.Count - 1)
                        {
                            m_targetIdx++;
                        }
                        else m_targetIdx = 0;

                        m_targetChangeTimer = 0;
                    }
                }
            }
            else
            {
                m_flashChangeTimer += Time.deltaTime;
                if(m_flashChangeTimer > FlashInterval)
                {
                    m_lightObject.SetActive(!m_lightObject.activeSelf);

                    m_flashChangeTimer = 0;
                }
            }
        }
        if (m_finiteRay)
        {
            m_lightOriginObject.transform.localScale =
                    new(m_lightOriginObject.transform.localScale.x, m_lightOriginObject.transform.localScale.y,
                    Targets.Count > 0 ? Vector3.Distance(m_lightOriginObject.transform.position, Targets[m_targetIdx].transform.position)*.5f : 5);
        }        
    }

    public void StartAnimation(float p_targetChangeSpeed = 0, float p_destChangeSpeed = 0)
    {
        if (p_targetChangeSpeed == 0) p_targetChangeSpeed = m_targetChangeSpeed;
        m_targetChangeSpeed = p_targetChangeSpeed;

        if (p_destChangeSpeed == 0) p_destChangeSpeed = m_destChangeSpeed;
        m_destChangeSpeed = p_destChangeSpeed;

        //Light
        m_lightObject.SetActive(true);

        if (m_debug) print("[PROJECTOR] " + name + " activated !");
        m_canAnimate = true;
    }
    public IEnumerator StartAnimationCoroutine(float p_targetChangeSpeed = 0, float p_destChangeSpeed = 0)
    {
        yield return new WaitForSeconds(TimeUntilBegin);

        StartAnimation(p_targetChangeSpeed, p_destChangeSpeed);
    }

    private void OnDisable()
    {
        m_canAnimate = false;
    }

    internal void StopAnimation()
    {
        m_canAnimate = false;

        m_lightObject.SetActive(false);
    }
}
