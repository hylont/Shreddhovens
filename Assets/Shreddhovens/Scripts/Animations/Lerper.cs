using UnityEngine;

public class Lerper : MonoBehaviour
{
    private Vector3 m_destination;
    [SerializeField] float m_speed = .5f;
    [SerializeField] bool m_keepBaseOffset = false;
    public Vector3 Offset = Vector3.zero;
    private void Start()
    {
        m_destination = transform.position;
        if(m_keepBaseOffset)
        {
            Offset = transform.localPosition;
        }
    }
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, m_destination + Offset, UnityEngine.Time.deltaTime * m_speed);
    }
    public void SetDestination(Vector3 p_destination)
    {
        m_destination = p_destination;
    }
}
