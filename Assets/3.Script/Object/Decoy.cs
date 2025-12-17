using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoy : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private float lifetime = 5f; // 디코이 지속 시간

    [Header("적 유인")]
    [SerializeField] private float attractionRadius = 10f; // 적 유인 범위
    [SerializeField] private LayerMask enemyLayer; // 적 레이어

    [Header("이펙트")]
    [SerializeField] private ParticleSystem spawnEffect; // 생성 이펙트
    [SerializeField] private ParticleSystem landingEffect; // 착지 이펙트
    [SerializeField] private ParticleSystem destroyEffect; // 소멸 이펙트

    [Header("사운드")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip landingSound;
    [SerializeField] private AudioClip destroySound;

    private AudioSource audioSource;
    private bool hasLanded = false;

    private void Awake()
    {
        // AudioSource 컴포넌트 가져오기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // 생성 이펙트 재생
        if (spawnEffect != null)
        {
            spawnEffect.Play();
        }

        // 생성 사운드 재생
        if (audioSource != null && spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        // 일정 시간 후 삭제
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 처음 착지 시에만
        if (!hasLanded)
        {
            hasLanded = true;

            OnLanding();
        }
    }

    /// <summary>
    /// 착지 시 호출
    /// </summary>
    private void OnLanding()
    {
        // 착지 이펙트
        if (landingEffect != null)
        {
            ParticleSystem effect = Instantiate(landingEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }

        // 착지 사운드
        if (audioSource != null && landingSound != null)
        {
            audioSource.PlayOneShot(landingSound);
        }

        // 적 유인
        AttractEnemies();
    }

    /// <summary>
    /// 범위 내 적들을 유인
    /// </summary>
    private void AttractEnemies()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, attractionRadius, enemyLayer);

        foreach (var enemy in enemies)
        {
            // 적 AI에게 디코이 위치 전달
            // EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            // if (enemyAI != null)
            // {
            //     enemyAI.SetTarget(transform);
            // }

            Debug.Log($"적 유인: {enemy.name}");
        }

        Debug.Log($"총 {enemies.Length}명의 적을 유인했습니다.");
    }

    private void OnDestroy()
    {
        // 소멸 이펙트 (파괴되기 전)
        if (destroyEffect != null)
        {
            ParticleSystem effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }

        // 소멸 사운드
        if (audioSource != null && destroySound != null)
        {
            // AudioSource는 오브젝트가 파괴되면 사라지므로 별도 처리
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 에디터에서 유인 범위 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}
