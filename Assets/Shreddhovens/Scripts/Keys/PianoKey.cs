using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PianoKey : MonoBehaviour, IComparable<PianoKey>
{
    [SerializeField] float m_defaultVolume = .8f;
    float m_duration;
    Coroutine m_releaseCoroutine;
    [SerializeField] bool m_logEnabled = false;

    private void Awake()
    {
        AudioSource l_source = gameObject.AddComponent<AudioSource>();
        l_source.loop = false;
        l_source.volume = m_defaultVolume;
        l_source.spatialBlend = 1;
        l_source.minDistance = 4;
        l_source.maxDistance = 18;

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
        if (name[0] == other.name[0]) return 0;

        if (name[name.Length - 1] < other.name[other.name.Length - 1])
        {
            //print("octave " + name + " < " + other.name);
            return -1;
        }
        else if (name[name.Length - 1] > other.name[other.name.Length - 1])
        {
            //print("octave " + name + " > " + other.name);
            return 1;
        }        
        else
        {
            if (name[0] < other.name[0])
            {
                //print("note "+name + " > " + other.name);
                return -1;
            }
            else if (name[0] > other.name[0])
            {
                //print("note "+name + " < " + other.name);
                return 1;
            }
            else
            {
                if (name.Contains('b') && !other.name.Contains('b'))
                {
                    //print("bemol "+name + " > " + other.name);
                    return -1;
                }

                if (!name.Contains('b') && other.name.Contains('b'))
                {
                    //print("bemol "+name + " < " + other.name);
                    return 1;
                }

                return 0;
            }
        }
    }
}
