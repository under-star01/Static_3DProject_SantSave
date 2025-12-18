//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    [Header("설정")]
    public LayerMask enemyLayer;       // 적 레이어
    public LayerMask obstacleMask;     // 벽 레이어 (소리 차단용)

    [Header("노이즈 설정")]
    [Tooltip("현재 발생하는 소음의 크기 (반경)")]
    public float noise = 0f;

    [Tooltip("초당 소음 감소 속도")]
    public float noiseDecaySpeed = 5.0f;

    // 코루틴 제어용 변수
    private Coroutine runningCoroutine;

    private void Update()
    {
        // 노이즈는 지속적으로 감소
        if (noise > 0)
        {
            noise -= noiseDecaySpeed * Time.deltaTime;

            // 0 이하로 떨어지지 않게 보정
            if (noise < 0) noise = 0f;
        }
    }

    // 애니메이션 이벤트에서 이 함수를 호출합니다.
    public void MakeNoiseOnce(float amount)
    {
        // 기존 노이즈보다 작은 값의 노이즈 발생 시 무시됨
        noise = Mathf.Max(noise, amount);
        // 내 주변 radius 반경 내의 적들을 모두 찾음
        FindEnemys(amount);
    }

    private IEnumerator MakeNoise_co(float amount, float interval)
    {
        while (true) // 무한 반복
        {
            noise = Mathf.Max(noise, amount);
            FindEnemys(amount);   // 적 감지

            // 인터벌만큼 대기 (이 시간 동안 Update에서 noise는 줄어듦)
            yield return new WaitForSeconds(interval);
        }
    }

    // 외부에서 호출할 땐 이 함수를 사용 (중복 방지 로직 포함)
    public void StartNoiseCoroutine(float amount, float interval)
    {
        // 이미 돌고 있는 코루틴이 있다면 끄고 시작
        StopNoiseCoroutine();

        // 코루틴 시작 후 변수에 저장
        runningCoroutine = StartCoroutine(MakeNoise_co(amount, interval));
    }

    // 멈출 때 호출할 함수
    public void StopNoiseCoroutine()
    {
        // 이미 돌고있는 코루틴이 있는지 확인
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
            // 코루틴 변수 초기화
            runningCoroutine = null;
        }
    }

    public void FindEnemys(float currNoise)
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, currNoise, enemyLayer);

        foreach (Collider enemyCol in enemies)
        {
            // 2. EnemyFSM 컴포넌트 찾기
            EnemyFSM enemy = enemyCol.GetComponent<EnemyFSM>();

            if (enemy != null)
            {
                // 3. 적에게 "내 위치"를 소음 위치로 전달
                enemy.HeardSound(transform.position);
            }
        }
    }

    // 디버그용: 소음 반경 그리기
    void OnDrawGizmos()
    {
        if (noise > 0)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f); // 붉은색 반투명
            Gizmos.DrawSphere(transform.position, noise);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, noise);
        }
    }
}
