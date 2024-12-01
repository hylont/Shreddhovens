using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoPlayer : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;

    [SerializeField] List<AnimatedProjector> m_projectors = new();
    [SerializeField] float m_projectorActiveTime = 1f;

    List<PianoKey> m_playedKeys = new();

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (Input.GetMouseButton(0))
            {
                PianoKey l_key = hit.collider.GetComponentInParent<PianoKey>();
                if (l_key && !m_playedKeys.Contains(l_key))
                {
                    l_key.Play();
                    StartCoroutine(FreeKey(l_key));
                    StartCoroutine(LightUpProjector(m_projectors[Random.Range(0, m_projectors.Count)]));
                }
            }
        }
    }

    IEnumerator FreeKey(PianoKey p_key)
    {
        m_playedKeys.Add(p_key);
        yield return new WaitForSeconds(.25f);
        m_playedKeys.Remove(p_key);
    }

    IEnumerator LightUpProjector(AnimatedProjector p_proj)
    {
        p_proj.StartAnimation();
        yield return new WaitForSeconds(m_projectorActiveTime);
        p_proj.StopAnimation();
    }
}
