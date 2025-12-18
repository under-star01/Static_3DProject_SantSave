using UnityEngine;

public class Decoy : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f; // 지속 시간
    [SerializeField] private float attractionRadius = 4f; // 유인 범위
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

        foreach (var col in enemies)
        {
            if (!col.CompareTag("Enemy")) continue;

            EnemyFSM enemyFSM = col.GetComponent<EnemyFSM>();
            if (enemyFSM != null)
            {
                enemyFSM.HeardSound(transform.position);
                Debug.Log($"디코이 소리 유인: {col.name}");
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