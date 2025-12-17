using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    [Header("변신 스킬")]
    [SerializeField] private float transformCool = 0.5f; // 스킬 쿨타임
    [SerializeField] private ParticleSystem transformEffect;//이펙트

    [Header("디코이 스킬")]
    [SerializeField] private GameObject decoyPrefab; // 디코이 프리팹
    [SerializeField] private GameObject rangeIndicator; // 범위 표시 오브젝트
    [SerializeField] private float decoyRange = 5f; // 디코이 투척 범위
    [SerializeField] private float decoyCooldown = 5f; // 디코이 쿨다운
    [SerializeField] private float throwForce = 10f; // 던지는 힘
    [SerializeField] private float throwAngle = 45f; // 던지는 각도

    private Animator animator;
    private PlayerMove playerMove;


    // 변신 스킬 변수
    private bool isTransformed = false; // 변신 상태
    private float lastTransformTime = -999f; // 마지막 스킬 사용 시간

    // 디코이 스킬 변수
    private bool isDecoyAiming = false; // 디코이 조준 중
    private float lastDecoyTime = -999f;
    private Vector3 decoyTargetPosition;

    private void Awake()
    {
        TryGetComponent(out animator);
        TryGetComponent(out playerMove);

        // 범위 표시 오브젝트 초기화
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        // 디코이 조준 중일 때만 위치 업데이트
        if (isDecoyAiming)
        {
            UpdateDecoyAimPosition();
            UpdateRangeIndicatorPosition();
        }
    }

    // Q키 입력 처리 (PlayerInput에서 호출)
    public void OnTransformSkill()
    {
        // 쿨타임 체크
        if (Time.time < lastTransformTime + transformCool)
        {
            return;
        }

        if (transformEffect != null)
        {
            transformEffect.Stop();
            transformEffect.Play();
        }

        // 애니메이션 트리거
        if (animator != null)
        {
            if (isTransformed)
            {
                animator.SetTrigger("Detransform");
                isTransformed = false;
            }
            else
            {
                animator.SetTrigger("Transform");
                isTransformed = true;
            }
        }

        lastTransformTime = Time.time;
    }

    public void OnDecoySkillStart()
    {
        if (isDecoyAiming)
        {
            CancelDecoySkill();
            return;
        }

        if (Time.time < lastDecoyTime + decoyCooldown)
        {
            Debug.Log("디코이 쿨다운 중...");
            return;
        }

        StartDecoyAim();
    }

    public void OnDecoySkillThrow()
    {
        if (!isDecoyAiming)
        {
            return;
        }

        ThrowDecoy();
    }

    private void StartDecoyAim()
    {
        isDecoyAiming = true;

        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(true);

            float diameter = decoyRange * 2f;
            rangeIndicator.transform.localScale = new Vector3(diameter, 0.01f, diameter);

            UpdateRangeIndicatorPosition();
        }

        Debug.Log("디코이 조준 시작");
    }

    // 범위 표시를 캐릭터 발 위치에 업데이트
    private void UpdateRangeIndicatorPosition()
    {
        if (rangeIndicator == null) return;

        // 캐릭터의 발 위치 (바닥)
        Vector3 footPosition = transform.position;
        footPosition.y = 0.2f; // 바닥에서 살짝 위

        rangeIndicator.transform.position = footPosition;
    }

    //PlayerMove의 mousePos를 활용하여 조준 위치 업데이트
    private void UpdateDecoyAimPosition()
    {
        if (!playerMove.HasMouseWorldPosition())
            return;

        Vector3 mouseWorldPos = playerMove.GetMouseWorldPosition();

        // 플레이어로부터 방향 (Y축 제외)
        Vector3 directionToMouse = mouseWorldPos - transform.position;
        directionToMouse.y = 0f; // Y축 무시

        // 수평 거리
        float distanceToMouse = directionToMouse.magnitude;

        // 범위 내로 제한
        if (distanceToMouse > decoyRange)
        {
            directionToMouse = directionToMouse.normalized * decoyRange;
        }

        // 목표 위치 = 플레이어 위치 + 제한된 방향
        decoyTargetPosition = transform.position + directionToMouse;
        decoyTargetPosition.y = 0.5f; // 바닥 높이로 고정
    }

    private void ThrowDecoy()
    {
        if (decoyPrefab != null)
        {
            // 던지기 시작 위치 (손 위치)
            Vector3 startPosition = transform.position + Vector3.up * 1.5f;
            GameObject decoy = Instantiate(decoyPrefab, startPosition, Quaternion.identity);

            Rigidbody rb = decoy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 던지는 방향과 속도 계산
                Vector3 throwVelocity = CalculateThrowVelocity(startPosition, decoyTargetPosition);
                rb.linearVelocity = throwVelocity;

                Debug.Log($"디코이 투척: 목표={decoyTargetPosition}, 속도={throwVelocity}");
            }
        }

        EndDecoyAim();
        lastDecoyTime = Time.time;
    }


    // 포물선으로 디코이를 던지는 코루틴
    private Vector3 CalculateThrowVelocity(Vector3 startPos, Vector3 targetPos)
    {
        // 수평 방향
        Vector3 horizontalDirection = targetPos - startPos;
        horizontalDirection.y = 0f;
        float horizontalDistance = horizontalDirection.magnitude;

        // 높이 차이
        float heightDifference = targetPos.y - startPos.y;

        // 중력
        float gravity = Mathf.Abs(Physics.gravity.y);

        // 던지는 각도 (라디안)
        float angleRad = throwAngle * Mathf.Deg2Rad;

        // 필요한 초기 속도 계산
        // v = sqrt(g * d / sin(2*angle))를 기본으로 높이 차이 보정
        float velocitySquared = gravity * horizontalDistance / Mathf.Sin(2f * angleRad);

        // 높이 차이 보정
        if (heightDifference != 0)
        {
            velocitySquared += gravity * heightDifference / (2f * Mathf.Sin(angleRad) * Mathf.Sin(angleRad));
        }

        float velocity = Mathf.Sqrt(Mathf.Abs(velocitySquared));

        // throwForce로 속도 스케일링
        velocity = throwForce;

        // 최종 속도 벡터 계산
        Vector3 throwVelocity = horizontalDirection.normalized * velocity * Mathf.Cos(angleRad);
        throwVelocity.y = velocity * Mathf.Sin(angleRad);

        return throwVelocity;
    }

    private void CancelDecoySkill()
    {
        EndDecoyAim();
        Debug.Log("디코이 스킬 취소");
    }

    private void EndDecoyAim()
    {
        isDecoyAiming = false;

        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
    }
    public bool IsDecoyAiming => isDecoyAiming;
}

