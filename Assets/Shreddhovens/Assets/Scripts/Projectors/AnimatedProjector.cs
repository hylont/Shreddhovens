using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EAnimation
{
    ROTATION_TO_POINT, LOOKAT, FLASH, LERP
}

public class AnimatedProjector : MonoBehaviour
{
    [SerializeField] Light m_light;
    public List<EAnimation> Animations;

    public float TimeUntilBegin = 0f;
    public Transform Target, MoveDestination;

    public float RotationSpeed = 1f, MovementSpeed = 1f;

    [SerializeField] bool m_canAnimate = false;

    [Header("FLASH")]
    public float FlashInterval = .1f;

    private void Awake()
    {
        if(m_canAnimate) StartAnimation();
    }

    private void Update()
    {
        if (!m_canAnimate) return;

        foreach(var anim in Animations)
        {
            if (anim == EAnimation.LERP)
            {
                transform.position = Vector3.Lerp(transform.position, MoveDestination.position, Time.deltaTime * MovementSpeed);
            }
            if(anim == EAnimation.LOOKAT)
            {
                transform.LookAt(Target.position);
            }
            if(anim == EAnimation.ROTATION_TO_POINT)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, 
                    Quaternion.FromToRotation(transform.position, Target.position), Time.deltaTime * RotationSpeed);
            }
        }
    }

    public void StartAnimation()
    {
        m_canAnimate = true;

        if(Animations.Contains(EAnimation.FLASH)) InvokeRepeating(nameof(FlashRepeat), 0, FlashInterval);
    }

    void FlashRepeat()
    {
        if (!m_canAnimate) return;

        m_light.enabled = !m_light.enabled;
    }

    private void OnDisable()
    {
        m_canAnimate = false;
    }
}
