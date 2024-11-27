using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct CameraTransitions
{
    public float transitionDelay;
    public CinemachineVirtualCamera destination;
}

public class TravellingScenario : MonoBehaviour
{
    [SerializeField] private CameraTransitions baseCamera;
    [SerializeField] private List<CameraTransitions> camerasTransitions = new();
    [SerializeField] float m_loopDelay = 20f;
    [SerializeField] bool m_loopMode = false;
    private int _idxCamera;
    // Start is called before the first frame update
    void OnEnable()
    {
        ResetCameras();

        StartCoroutine(StartScenario());
    }

    private void ResetCameras()
    {
        foreach (CameraTransitions tr in camerasTransitions)
        {
            tr.destination.enabled = false;
        }
        baseCamera.destination.enabled = true;
        _idxCamera = 0;
    }

    private IEnumerator StartScenario()
    {
        if (camerasTransitions.Count > 0)
        {
            while (true)
            {
                if (_idxCamera == 0)
                {
                    yield return new WaitForSeconds(baseCamera.transitionDelay);
                }
                if (_idxCamera >= camerasTransitions.Count)
                {
                    if (m_loopMode) ResetCameras();
                    else yield break;
                    //StartCoroutine(ResetCamerasCoroutine());
                }
                else
                {
                    camerasTransitions[_idxCamera].destination.enabled = true;
                    yield return new WaitForSeconds(camerasTransitions[_idxCamera].transitionDelay);
                    _idxCamera++;
                }
            }
        }
    }

    private IEnumerator ResetCamerasCoroutine()
    {
        yield return new WaitForSeconds(m_loopDelay);

        ResetCameras();
    }
}
