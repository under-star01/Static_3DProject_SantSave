using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Material m_Default;   // 기본 문 Material
    [SerializeField] private Material m_HighLight; // 강조 문 Material

    private Renderer doorRenderer;

    private void Awake()
    {
        TryGetComponent(out doorRenderer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            doorRenderer.material = m_HighLight;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            doorRenderer.material = m_Default;
        }

    }
}
