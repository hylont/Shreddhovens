using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PianoKey : MonoBehaviour, IComparable<PianoKey>
{
    [SerializeField] float m_defaultVolume = .6f;
    float m_duration;
    Coroutine m_releaseCoroutine;
    [SerializeField] bool m_logEnabled = false;

    private void Awake()
    {
        AudioSource l_source = gameObject.AddComponent<AudioSource>();
        l_source.loop = false;
        l_source.volume = m_defaultVolume;

        GetComponent<Animator>().enabled = true;
    }

    public void SetClip(AudioClip p_clip)
    {
        GetComponent<AudioSource>().clip = p_clip;
    }

    public void Play(float p_duration = 1f)
    {
        if(m_logEnabled) print("Playing " + gameObject.name);
        m_duration = p_duration;

        GetComponent<AudioSource>().time = .1f;
        GetComponent<AudioSource>().Play();

        GetComponent<Animator>().SetBool("IsPressed", true);

        if(m_releaseCoroutine != null) StopCoroutine(m_releaseCoroutine);

        m_releaseCoroutine = StartCoroutine(nameof(Stop));
    }
    private IEnumerator Stop()
    {
        yield return new WaitForSeconds(m_duration);
        GetComponent<AudioSource>().Stop();
        GetComponent<Animator>().SetBool("IsPressed", false);
    }

    public int CompareTo(PianoKey other)
    {
        return other.name.CompareTo(other.name);
    }
}
