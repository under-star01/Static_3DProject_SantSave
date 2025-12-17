using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    [Header("변신 스킬")]
    [SerializeField] private float transformCool = 0.5f;
    [SerializeField] private ParticleSystem transformEffect;

    [Header("디코이 스킬")]
    [SerializeField] private GameObject decoyPrefab;
    [SerializeField] private LineRenderer trajectoryLine; // 궤적 표시
    [SerializeField] private float maxThrowDistance = 5f; // 최대 투척 거리
    [SerializeField] private float minThrowForce = 0.5f; // 최소 던지는 힘
    [SerializeField] private float maxThrowForce = 8f; // 최대 던지는 힘
    [SerializeField] private float minThrowAngle = 30f;
    [SerializeField] private float maxThrowAngle = 65f;
    [SerializeField] private int minTrajectoryPoints = 5; // 가까울 때 점 개수
    [SerializeField] private int maxTrajectoryPoints = 15; // 멀 때 점 개수
    [SerializeField] private float decoyCooldown = 1f; // 쿨다운

    private Animator animator;
    private PlayerMove playerMove;

    // 변신 스킬
    private bool isTransformed = false;
    private float lastTransformTime = -999f;

    // 디코이 스킬
    private bool isDecoyAiming = false;
    private float lastDecoyTime = -999f;
    private float currentThrowAngle;
    private Vector3 targetPosition;

    private void Awake()
    {
        TryGetComponent(out animator);
        TryGetComponent(out playerMove);

        // LineRenderer 초기화
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }
    }

    private void Update()
    {
        // 디코이 조준 중일 때 궤적 업데이트
        if (isDecoyAiming)
        {
            UpdateTrajectory();
        }
    }

    #region 변신 스킬

    public void OnTransformSkill()
    {
        if (Time.time < lastTransformTime + transformCool)
        {
            return;
        }

        if (transformEffect != null)
        {
            transformEffect.Stop();
            transformEffect.Play();
        }

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

    #endregion

    #region 디코이 스킬

    // 디코이 스킬 시작/취소 (우클릭)
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

    // 디코이 투척 (좌클릭)
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

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = true;
        }

        Debug.Log("디코이 조준 시작");
    }

    // 궤적 업데이트 (마우스 위치에 따라)
    private void UpdateTrajectory()
    {
        if (!playerMove.HasMouseWorldPosition())
            return;

        Vector3 mouseWorldPos = playerMove.GetMouseWorldPosition();
        Vector3 startPosition = transform.position + Vector3.up * 1.5f;

        // 마우스까지의 수평 거리 계산
        Vector3 directionToMouse = mouseWorldPos - startPosition;
        directionToMouse.y = 0f;
        float distanceToMouse = directionToMouse.magnitude;

        // 최대 거리 제한
        if (distanceToMouse > maxThrowDistance)
        {
            distanceToMouse = maxThrowDistance;
            directionToMouse = directionToMouse.normalized * maxThrowDistance;
        }

        // 목표 위치 계산
        targetPosition = startPosition + directionToMouse;
        targetPosition.y = mouseWorldPos.y;

        // 정규화된 거리 (0~1)
        float normalizedDistance = distanceToMouse / maxThrowDistance;

        // 거리에 따른 힘 계산
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, normalizedDistance);

        // 거리에 따른 각도 계산
        currentThrowAngle = Mathf.Lerp(minThrowAngle, maxThrowAngle, normalizedDistance);

        // 거리에 따른 궤적 점 개수 계산
        int trajectoryPoints = Mathf.RoundToInt(Mathf.Lerp(minTrajectoryPoints, maxTrajectoryPoints, normalizedDistance));

        // 던지기 속도 계산
        Vector3 throwVelocity = CalculateThrowVelocity(startPosition, targetPosition, throwForce, currentThrowAngle);

        // 궤적 그리기
        DrawTrajectory(startPosition, throwVelocity, trajectoryPoints);

        // 디버그 (선택사항)
        // Debug.Log($"거리: {distanceToMouse:F2}m, 힘: {throwForce:F2}, 각도: {currentThrowAngle:F1}°, 점: {trajectoryPoints}");
    }

    // 던지기 속도 계산
    private Vector3 CalculateThrowVelocity(Vector3 startPos, Vector3 targetPos, float force, float angle)
    {
        Vector3 direction = targetPos - startPos;
        Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z);

        float angleRad = angle * Mathf.Deg2Rad;

        Vector3 velocity = horizontalDirection.normalized * force * Mathf.Cos(angleRad);
        velocity.y = force * Mathf.Sin(angleRad);

        return velocity;
    }

    // 포물선 궤적 그리기
    private void DrawTrajectory(Vector3 startPos, Vector3 initialVelocity, int pointCount)
    {
        if (trajectoryLine == null)
            return;

        trajectoryLine.positionCount = pointCount;

        Vector3 currentPosition = startPos;
        Vector3 currentVelocity = initialVelocity;
        float timeStep = 0.1f;

        for (int i = 0; i < pointCount; i++)
        {
            trajectoryLine.SetPosition(i, currentPosition);

            // 다음 위치 계산
            currentVelocity += Physics.gravity * timeStep;
            currentPosition += currentVelocity * timeStep;

            // 바닥에 닿으면 중단
            if (currentPosition.y < 0f)
            {
                trajectoryLine.positionCount = i + 1;
                trajectoryLine.SetPosition(i, new Vector3(currentPosition.x, 0f, currentPosition.z));
                break;
            }
        }
    }

    // 디코이 던지기
    private void ThrowDecoy()
    {
        if (decoyPrefab != null)
        {
            Vector3 startPosition = transform.position + Vector3.up * 1.5f;
            GameObject decoy = Instantiate(decoyPrefab, startPosition, Quaternion.identity);

            Rigidbody rb = decoy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 거리 계산
                Vector3 directionToTarget = targetPosition - startPosition;
                directionToTarget.y = 0f;
                float distance = directionToTarget.magnitude;

                // 거리 제한
                if (distance > maxThrowDistance)
                {
                    distance = maxThrowDistance;
                }

                float normalizedDistance = distance / maxThrowDistance;

                // 힘과 각도 계산
                float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, normalizedDistance);
                float throwAngle = Mathf.Lerp(minThrowAngle, maxThrowAngle, normalizedDistance);

                // 속도 적용
                Vector3 throwVelocity = CalculateThrowVelocity(startPosition, targetPosition, throwForce, throwAngle);
                rb.linearVelocity = throwVelocity;

                Debug.Log($"디코이 투척: 거리={distance:F2}m, 힘={throwForce:F2}, 각도={throwAngle:F1}°");
            }
        }

        EndDecoyAim();
        lastDecoyTime = Time.time;
    }

    private void CancelDecoySkill()
    {
        EndDecoyAim();
        Debug.Log("디코이 스킬 취소");
    }

    private void EndDecoyAim()
    {
        isDecoyAiming = false;

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }
    }

    #endregion

    public bool IsDecoyAiming => isDecoyAiming;
}