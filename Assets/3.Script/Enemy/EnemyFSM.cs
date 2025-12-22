using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType
{
    Roamer,
    Obsever
}

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFSM : MonoBehaviour
{
    [Header("타입 설정")]
    public EnemyType enemyType;

    [Header("참조 스크립트")]
    public EnemyFOV fov;

    [Header("공통 설정")]
    public Transform[] patrolPoints;
    public float idleTime = 2f;
    public float lookTime = 1.5f;

    [Header("Roamer 설정")]
    public bool isSleeping;

    [Header("Observer 설정")]
    public float alertAmount = 20f;

    [Header("게임 오버 설정")]
    public float killDistance = 3.0f;
    public float gaugeFillSpeed = 30f;
    public float gaugeDrainSpeed = 10f;

    [Header("디버그")]
    public float currentGauge = 0f;
    private float maxGauge = 100f;

    [Header("소리 관련 변수")]
    private bool isHeard;
    private Vector3 noisePosition;

    private NavMeshAgent agent;
    private Transform targetPlayer;
    private bool isPlayerFound;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (enemyType == EnemyType.Roamer && isSleeping)
        {
            if (fov != null) fov.enabled = false;
        }

        StartCoroutine(FSM_Loop());
    }

    IEnumerator FSM_Loop()
    {
        while (true)
        {
            // 1. Sleep (수면)
            if (isSleeping) yield return StartCoroutine(SleepState());

            // 2. WakeUp (기상)
            // 수면 중 플레이어의 소리를 감지했을 때 실행
            if (isSleeping && isHeard) yield return StartCoroutine(WakeUpState());


            if (!isSleeping)
            {
                // 1. Idle (대기)
                // 발견되지 않았을 때만 실행
                if (!isPlayerFound && !isHeard) yield return StartCoroutine(IdleState());

                // 2. Move (이동)
                // Idle에서 발견됐다면 Move는 건너뜀
                if (!isPlayerFound && !isHeard) yield return StartCoroutine(MoveState());

                // 3. Detect (정밀 감시 - 멈춰서 두리번거림)
                // 이동 중에 발견됐다면 Detect는 건너뜀
                if (!isPlayerFound && !isHeard) yield return StartCoroutine(DetectState());

                // --- [발견 시 실행되는 구간] ---
                // 4. Look (경고/주시)
                if (isHeard && !isPlayerFound)
                {
                    yield return StartCoroutine(LookState());
                }

                // 5. Chase (추격)
                if (isPlayerFound)
                {
                    if (enemyType == EnemyType.Roamer) yield return StartCoroutine(ChaseState());

                    if (enemyType == EnemyType.Obsever) yield return StartCoroutine(AlertState());
                }
            }

        }
    }

    // --- [개별 상태 코루틴] ---

    IEnumerator SleepState()
    {
        agent.isStopped = true; // 이동 불가능 상태로 전환
        yield return null;
    }

    IEnumerator WakeUpState()
    {
        isSleeping = false;
        isHeard = false;    // 자는동안 들었던 소리는 일어나는 동안 잊음
        agent.isStopped = false; // 이동 가능 상태로 전환

        if (fov != null) fov.enabled = true;    // 시야 켜주기

        yield return new WaitForSeconds(0.5f);
    }


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
        Debug.Log("State: Look (두리번/주시)");

        float timer = 0f;
        Quaternion startRot = transform.rotation;

        // 바라볼 목표 지점 결정
        Vector3 targetPos;

        if (isPlayerFound && targetPlayer != null)
        {
            targetPos = targetPlayer.position; // 발견했으면 플레이어
        }
        else if (isHeard)
        {
            targetPos = noisePosition; // 소리만 들었으면 소리 위치
        }
        else
        {
            targetPos = transform.position + transform.forward; // 그냥 앞
        }

        while (timer < lookTime)
        {
            // 회전 로직 (목표 지점을 향해 부드럽게 회전)
            Vector3 dir = (targetPos - transform.position).normalized;
            dir.y = 0; // 기울어짐 방지
            if (dir != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
            }

            // 여기서 플레이어가 FOV에 들어오면 isPlayerFound가 true가 됨
            if (CheckPlayerDetected())
            {
                // 플레이어를 찾았다! -> LookState 즉시 종료 -> FSM 루프가 ChaseState 실행
                isHeard = false; // 소리보다 시각 발견이 우선이므로 소리 플래그 끔
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 시간 내에 발견 못하면 소리 들음 상태 해제 (다시 순찰하러 감)
        isHeard = false;
    }

    IEnumerator ChaseState()
    {
        //Debug.Log("State: Chase (추격 시작)");

        while (true)
        {
            // 1. 추격 종료 조건
            // targetPlayer가 null인지 확인하고, 감지 실패 시 종료
            if (targetPlayer == null || !fov.DetectPlayer(true))
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
                // Time.deltaTime * 속도 -> 이만큼 채워달라고 요청
                // * 2.0f는 여러 마리가 붙었을 때 가중치 (선택 사항)
                float fearAmount = Time.deltaTime * gaugeFillSpeed;

                // FearManager의 감소 속도보다 더 많이 더해야 게이지가 참
                // 예: 감소가 10인데 여기서 30을 더하면 실제로는 20씩 참
                CaughtManager.instance.AddCaught(fearAmount + (CaughtManager.instance.drainSpeed * Time.deltaTime));
            }

            yield return null;
        }
    }


    IEnumerator AlertState()
    {
        yield return null;
    }


    // [헬퍼 함수] 중복되는 감지 로직을 하나로 묶음
    bool CheckPlayerDetected()
    {
        if (fov.DetectPlayer(false))
        {
            isPlayerFound = true;
            targetPlayer = fov.player; // FOV가 찾은 플레이어 저장
            return true;
        }
        return false;
    }

    public void HeardSound(Vector3 noisePos)
    {
        //Chase 상태일 때는 소리감지 X
        if (isPlayerFound) return;

        //Debug.Log("소리 감지! 위치: " + noisePos);

        // Noise 발생 위치(플레이어 위치) 전달
        noisePosition = noisePos;
        isHeard = true;

        // 현재 이동 중이라면 즉시 멈추게 함 (반응 속도 향상)
        if (agent != null) agent.ResetPath();
    }
}