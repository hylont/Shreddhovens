using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Button))]
public class ResponsiveButton : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] AudioClip m_hoverClip;
    [SerializeField] AudioClip m_clickClip;
    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<AudioSource>().clip = m_hoverClip;
        GetComponent<AudioSource>().Play();
    }
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            GetComponent<AudioSource>().clip = m_clickClip;
            GetComponent<AudioSource>().Play();
        });
    }
}
