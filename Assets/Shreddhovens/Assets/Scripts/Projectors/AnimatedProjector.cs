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
    public float LightIntensity = 100;

    public List<EAnimation> Animations;

    public float TimeUntilBegin = 0f;
    public List<Transform> Targets, MoveDestinations;
    public int m_targetIdx, m_destIdx;
    public float m_targetChangeSpeed = 0f;
    public float m_destChangeSpeed = 0f;

    public float RotationSpeed = 1f, MovementSpeed = 1f;

    [SerializeField] bool m_canAnimate = false;
    [SerializeField] bool m_activateLightOnBegin = true;

    [Header("FLASH")]
    public float FlashInterval = .1f;

    [Header("Debug")]
    [SerializeField] bool m_debug = false;
    private void Awake()
    {
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

        if (m_activateLightOnBegin)
        {
            m_lightObject.SetActive(false);
            //m_lightArea.SetActive(false;
        }
    }

    public void SetMaterial(Material material)
    {
        m_lightObject.GetComponent<Renderer>().material = material;
        m_lightObject.GetComponent<Renderer>().material.SetFloat("_EmissiveIntensity", 
            LightIntensity + UnityEngine.Random.Range(LightIntensity * -.1f, LightIntensity *.1f));
    }

    void ChangeTargetRepeat()
    {
        if (!m_canAnimate) return;

        if (m_targetIdx < Targets.Count - 1)
        {
            m_targetIdx++;
        }
        else m_targetIdx = 0;
    }
    void ChangetDestRepeat()
    {
        if (!m_canAnimate) return;

        if (m_destIdx < MoveDestinations.Count - 1)
        {
            m_destIdx++;
        }
        else m_destIdx = 0;
    }

    private void Update()
    {
        if (!m_canAnimate) return;        

        foreach (var anim in Animations)
        {
            if(anim != EAnimation.FLASH)
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
                }

                if (anim == EAnimation.ROTATION_TO_POINT)
                {
                    Quaternion lookOnLook =
                    Quaternion.LookRotation(Targets[m_targetIdx].transform.position - transform.position);

                    transform.rotation =
                    Quaternion.Slerp(transform.rotation, lookOnLook, Time.deltaTime * RotationSpeed);
                }
            }            
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

        if(Animations.Contains(EAnimation.FLASH)) InvokeRepeating(nameof(FlashRepeat), 0, FlashInterval);

        if (Targets.Count > 1 && m_targetChangeSpeed > 0)
        {
            InvokeRepeating(nameof(ChangeTargetRepeat), 0, m_targetChangeSpeed);
        }

        if (MoveDestinations.Count > 1 && m_destChangeSpeed > 0)
        {
            InvokeRepeating(nameof(ChangetDestRepeat), 0, m_destChangeSpeed);
        }
    }
    public IEnumerator StartAnimationCoroutine(float p_targetChangeSpeed = 0, float p_destChangeSpeed = 0)
    {
        yield return new WaitForSeconds(TimeUntilBegin);

        StartAnimation(p_targetChangeSpeed, p_destChangeSpeed);
    }

    void FlashRepeat()
    {
        if (!m_canAnimate) return;

        m_lightObject.SetActive(!m_lightObject.activeSelf);
    }

    private void OnDisable()
    {
        m_canAnimate = false;
    }

    internal void StopAnimation()
    {
        m_canAnimate = false;

        m_lightObject.SetActive(false);
        //m_lightArea.SetActive(false);
    }
}
