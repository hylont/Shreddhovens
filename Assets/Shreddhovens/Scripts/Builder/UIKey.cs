using MusicXml.Domain;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIKey : MonoBehaviour
{
    public Note Note;
    public int Octave;

    public Color PlayingColor = Color.cyan;
    Color m_baseColor;

    private void Start()
    {
        m_baseColor = GetComponent<Image>().color;
    }

    public void Play()
    {
        GetComponent<Image>().color = PlayingColor;

        StartCoroutine(ReleasePlayCoroutine());
    }

    IEnumerator ReleasePlayCoroutine()
    {
        yield return new WaitForSeconds(.5f);
        GetComponent<Image>().color = m_baseColor;
    }
}