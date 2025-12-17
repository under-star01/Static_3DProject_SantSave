using UnityEngine;

public class Decoy : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f; // 지속 시간
    [SerializeField] private float attractionRadius = 10f; // 유인 범위
    [SerializeField] private ParticleSystem landingEffect; // 착지 이펙트

    private bool hasLanded = false;

    private void Start()
    {
        // 일정 시간 후 삭제
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 처음 착지 시에만
        if (!hasLanded)
        {
            hasLanded = true;

            // 착지 이펙트
            if (landingEffect != null)
            {
                Instantiate(landingEffect, transform.position, Quaternion.identity);
            }

            // 적 유인
            AttractEnemies();
        }
    }

    private void AttractEnemies()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, attractionRadius);

        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log($"적 유인: {enemy.name}");
                // 여기에 적 AI 타겟 변경 로직 추가
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 유인 범위 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}