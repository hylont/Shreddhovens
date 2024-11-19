using UnityEngine;

public class Lerper : MonoBehaviour
{
    private Vector3 m_destination;
    [SerializeField] float m_speed = .5f;
    private void Start()
    {
        m_destination = transform.position;
    }
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, m_destination, UnityEngine.Time.deltaTime * m_speed);
    }
    public void SetDestination(Vector3 p_destination)
    {
        m_destination = p_destination;
    }
}
