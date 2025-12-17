using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFSM : MonoBehaviour
{
    [Header("참조 스크립트")]
    public EnemyFOV fov;

    [Header("설정")]
    public Transform[] patrolPoints;
    public float idleTime = 2f;
    public float lookTime = 1.5f;

    [Header("게임 오버 설정")]
    public float killDistance = 3.0f;
    public float gaugeFillSpeed = 30f;
    public float gaugeDrainSpeed = 10f;

    //[Header("디버그")]
    //public float currentGauge = 0f;    
    //private float maxGauge = 100f;      

    private NavMeshAgent agent;
    private Transform targetPlayer;
    private bool isPlayerFound;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(FSM_Loop());
    }

    IEnumerator FSM_Loop()
    {
        while (true)
        {
            // 1. Idle (대기)
            // 발견되지 않았을 때만 실행
            if (!isPlayerFound) yield return StartCoroutine(IdleState());

            // 2. Move (이동)
            // Idle에서 발견됐다면 Move는 건너뜀
            if (!isPlayerFound) yield return StartCoroutine(MoveState());

            // 3. Detect (정밀 감시 - 멈춰서 두리번거림)
            // 이동 중에 발견됐다면 Detect는 건너뜀
            if (!isPlayerFound) yield return StartCoroutine(DetectState());

            // --- [발견 시 실행되는 구간] ---
            if (isPlayerFound)
            {
                // 4. Look (경고/주시)
                yield return StartCoroutine(LookState());

                // 5. Chase (추격)
                yield return StartCoroutine(ChaseState());
            }
        }
    }

    // --- [개별 상태 코루틴] ---

    IEnumerator IdleState()
    {
        //Debug.Log("State: Idle");
        float timer = 0f;

        while (timer < idleTime)
        {
            // [수정] 대기 중에도 계속 감시
            if (CheckPlayerDetected()) yield break; // 발견하면 즉시 종료

            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator MoveState()
    {
        //Debug.Log("State: Move");

        if (patrolPoints.Length > 0)
        {
            int randIndex = Random.Range(0, patrolPoints.Length);
            agent.SetDestination(patrolPoints[randIndex].position);

            // [수정] 이동 중에도 계속 감시
            while (agent.pathPending || agent.remainingDistance > 0.5f)
            {
                if (CheckPlayerDetected())
                {
                    agent.ResetPath(); // 이동 즉시 멈춤
                    yield break;       // MoveState 강제 종료 -> FSM 루프에서 Chase로 연결됨
                }
                yield return null;
            }
        }
    }

    IEnumerator DetectState()
    {
        //Debug.Log("State: Detect");
        agent.ResetPath();

        // 기존 로직 유지 (멈춰서 확실하게 한 번 더 체크)
        if (CheckPlayerDetected())
        {
            yield break;
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator LookState()
    {
        Debug.Log("State: Look (발견!)");

        float timer = 0f;
        while (timer < lookTime)
        {
            if (targetPlayer != null)
            {
                // 플레이어 방향 보정
                Vector3 dir = (targetPlayer.position - transform.position).normalized;
                dir.y = 0;
                Quaternion lookRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
            }

            // [중요] Look 도중에 플레이어가 시야에서 사라져도, 
            // 일단 발견했으므로 isPlayerFound를 false로 만들지 않음 (Chase에서 처리)

            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ChaseState()
    {
        Debug.Log("State: Chase (추격 시작)");

        while (true)
        {
            // 1. 추격 종료 조건
            // [수정] targetPlayer가 null인지 확인하고, 감지 실패 시 종료
            if (targetPlayer == null || !fov.DetectPlayer())
            {
                Debug.Log("Chase: 놓침 -> 다시 순찰 복귀");
                isPlayerFound = false;
                targetPlayer = null;
                break;
            }

            // 2. 플레이어 따라가기
            agent.SetDestination(targetPlayer.position);

            // 3. 게이지 로직 (거리 체크)
            float dist = Vector3.Distance(transform.position, targetPlayer.position);
            if (dist <= killDistance)
            {
                // 게이지 증가 로직...
            }

            yield return null;
        }
    }

    // [헬퍼 함수] 중복되는 감지 로직을 하나로 묶음
    bool CheckPlayerDetected()
    {
        if (fov.DetectPlayer())
        {
            isPlayerFound = true;
            targetPlayer = fov.player; // FOV가 찾은 플레이어 저장
            return true;
        }
        return false;
    }
}