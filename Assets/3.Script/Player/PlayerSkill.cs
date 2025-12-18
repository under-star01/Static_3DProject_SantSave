using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    [Header("변신 스킬")]
    [SerializeField] private float transformCool = 0.5f;
    [SerializeField] private ParticleSystem transformEffect;
    [SerializeField] private GameObject[] transformPrefabs;
    [SerializeField] private Vector3 transformOffset = new Vector3(0, 0.5f, 0);

    [Header("디코이 스킬")]
    [SerializeField] private GameObject decoyPrefab;
    [SerializeField] private LineRenderer trajectoryLine; // 궤적 표시
    [SerializeField] private GameObject decoyRangeIndicator; // 디코이 범위 표시
    [SerializeField] private float decoyAttractionRadius = 5f; //디코이 유인 범위
    [SerializeField] private float rangeIndicatorHeight = 0.5f; //범위 표시 높이
    [SerializeField] private float maxThrowDistance = 5f; // 최대 투척 거리
    [SerializeField] private float minThrowForce = 0.1f; // 최소 던지는 힘
    [SerializeField] private float maxThrowForce = 8f; // 최대 던지는 힘
    [SerializeField] private float minThrowAngle = 30f; //최소 각도
    [SerializeField] private float maxThrowAngle = 65f; //최대 각도
    [SerializeField] private int minTrajectoryPoints = 3; // 가까울 때 점 개수
    [SerializeField] private int maxTrajectoryPoints = 16; // 멀 때 점 개수
    [SerializeField] private float decoyCooldown = 1f; // 쿨다운
    [SerializeField] private float trajectoryDistanceMultiplier = 0.9f; // 거리 게수
    [SerializeField] private LayerMask wallLayer; //벽 레이어
    [SerializeField] private float throwAnimationDelay = 0.7f; // 던지기 타이밍
    [SerializeField] private float throwAnimationDuration = 1f; // 애니메이션 전체 길이

    private Animator animator;
    private PlayerMove playerMove;
    private Renderer playerRenderer;
    private Rigidbody rb;

    // 변신 스킬
    private bool isTransformed = false;
    private float lastTransformTime = -999f;
    private GameObject currentTransformObject;
    private Quaternion currentrotation; // 회전값 저장

    // 디코이 스킬
    private bool isDecoyAiming = false;
    private bool isThrowing = false; // 던지는 중
    private bool lockPlayerRotation = false;
    private float lastDecoyTime = -999f;
    private float currentThrowAngle;
    private Vector3 targetPosition;
    private Vector3 finalLandingPosition;
    private Quaternion lockedPlayerRotation;

    private void Awake()
    {
        TryGetComponent(out animator);
        TryGetComponent(out playerMove);
        TryGetComponent(out rb);
        playerRenderer = GetComponentInChildren<Renderer>();

        // LineRenderer 초기화
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }

        // 범위 표시 초기화
        if (decoyRangeIndicator != null)
        {
            decoyRangeIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        // 디코이 조준 중일 때 궤적 업데이트
        if (isDecoyAiming && !isThrowing)
        {
            UpdateTrajectory();
        }

        if (isTransformed)
        {
            CheckMovementWhileTransformed();
            UpdateTransformObjectPosition();
        }

        // 던지는 동안 회전 고정
        if (lockPlayerRotation)
        {
            transform.rotation = lockedPlayerRotation;
        }
    }

    #region 변신 스킬

    public void OnTransformSkill()
    {
        if (Time.time < lastTransformTime + transformCool)
        {
            return;
        }

        if (isTransformed)
        {
            // 변신 해제
            Detransform();
        }
        else
        {
            // 변신 시작
            Transform();
        }

        lastTransformTime = Time.time;
    }

    private void Transform()
    {
        if (transformPrefabs == null || transformPrefabs.Length == 0)
        {
            Debug.LogWarning("변신 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 이펙트 재생
        if (transformEffect != null)
        {
            transformEffect.Stop();
            transformEffect.Play();
        }

        int randomIndex = UnityEngine.Random.Range(0, transformPrefabs.Length);
        GameObject selectedPrefab = transformPrefabs[randomIndex];

        // 독립 오브젝트로 생성
        Vector3 spawnPosition = transform.position + transformOffset;
        currentTransformObject = Instantiate(selectedPrefab, spawnPosition, transform.rotation);

        // Rigidbody 설정 (날아가지 않게)
        Rigidbody transformRb = currentTransformObject.GetComponent<Rigidbody>();
        if (transformRb != null)
        {
            transformRb.isKinematic = true;
            transformRb.useGravity = false;
            transformRb.linearVelocity = Vector3.zero;
            transformRb.angularVelocity = Vector3.zero;
        }

        // 플레이어와 충돌 무시
        Collider playerCollider = GetComponent<Collider>();
        Collider transformCollider = currentTransformObject.GetComponent<Collider>();
        if (playerCollider != null && transformCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, transformCollider);
        }

        // 초기 회전값 저장
        currentrotation = transform.rotation;

        if (playerRenderer != null)
        {
            playerRenderer.enabled = false;
        }

        // 이동 불가
        if (playerMove != null)
        {
            playerMove.canMove = false;
        }

        isTransformed = true;

        Debug.Log($"변신 완료: {selectedPrefab.name}");
    }

    private void Detransform()
    {
        // 이펙트 재생
        if (transformEffect != null)
        {
            transformEffect.Stop();
            transformEffect.Play();
        }

        // 변신 오브젝트 제거
        if (currentTransformObject != null)
        {
            Destroy(currentTransformObject);
            currentTransformObject = null;
        }

        // 플레이어 메시 다시 보이기
        if (playerRenderer != null)
        {
            playerRenderer.enabled = true;
        }

        // 이동 가능
        if (playerMove != null)
        {
            playerMove.canMove = true;
        }


        isTransformed = false;

        Debug.Log("변신 해제");
    }

    // 변신 오브젝트 위치만 업데이트 (회전은 고정)
    private void UpdateTransformObjectPosition()
    {
        if (currentTransformObject == null)
            return;

        currentTransformObject.transform.position = transform.position + transformOffset;
        currentTransformObject.transform.rotation = currentrotation;
    }

    private void CheckMovementWhileTransformed()
    {
        if (!isTransformed)
            return;

        // PlayerMove에서 입력 체크
        if (playerMove != null)
        {
            Vector2 moveInput = playerMove.GetMoveInput();

            if (moveInput.sqrMagnitude > 0.01f) // 입력이 있으면
            {
                Debug.Log("이동 감지 - 변신 해제");
                Detransform();
            }
        }
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

        // 연속 입력 방지
        if (isThrowing)
        {
            return;
        }
        StartThrowAnimation();
    }

    private void StartDecoyAim()
    {
        isDecoyAiming = true;

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = true;
        }

        if (decoyRangeIndicator != null)
        {
            decoyRangeIndicator.SetActive(true);

            // 범위 크기 설정
            float diameter = decoyAttractionRadius * 2f;
            decoyRangeIndicator.transform.localScale = new Vector3(diameter, 0.01f, diameter);
        }
        Debug.Log("디코이 조준 시작");
    }

    // 던지기 애니메이션 시작
    private void StartThrowAnimation()
    {
        isThrowing = true;

        // 이동 불가
        if (playerMove != null)
        {
            playerMove.canMove = false;
        }

        // 현재 바라보는 방향 저장
        lockedPlayerRotation = transform.rotation;
        lockPlayerRotation = true;

        // 던지기 애니메이션 트리거
        if (animator != null)
        {
            animator.SetTrigger("Throw");
        }

        StartCoroutine(ThrowAfterAnimation());
    }

    // 애니메이션 타이밍에 맞춰 던지기
    private IEnumerator ThrowAfterAnimation()
    {
        yield return new WaitForSeconds(throwAnimationDelay);

        ThrowDecoy();

        yield return new WaitForSeconds(throwAnimationDuration - throwAnimationDelay);

        isThrowing = false;
        lockPlayerRotation = false;

        // 이동 가능
        if (playerMove != null)
        {
            playerMove.canMove = true;
        }
    }

    // 궤적 업데이트 (마우스 위치에 따라)
    private void UpdateTrajectory()
    {
        Vector3 mouseWorldPos = playerMove.mouseHitPos;
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
        DrawTrajectoryWithWallCheck(startPosition, throwVelocity * trajectoryDistanceMultiplier, trajectoryPoints);

        UpdateRangeIndicator();
    }

    // 벽 충돌 체크하며 궤적 그리기
    private void DrawTrajectoryWithWallCheck(Vector3 startPos, Vector3 initialVelocity, int pointCount)
    {
        if (trajectoryLine == null)
            return;

        List<Vector3> trajectoryPoints = new List<Vector3>();

        Vector3 currentPosition = startPos;
        Vector3 currentVelocity = initialVelocity;
        float timeStep = Time.fixedDeltaTime; // 0.1f → Time.fixedDeltaTime (더 정확)

        for (int i = 0; i < 200; i++)
        {
            trajectoryPoints.Add(currentPosition);

            // 중력 적용
            Vector3 gravity = Physics.gravity;
            Vector3 nextVelocity = currentVelocity + gravity * timeStep;
            Vector3 nextPosition = currentPosition + currentVelocity * timeStep;

            Vector3 direction = nextPosition - currentPosition;
            float distance = direction.magnitude;

            if (Physics.Raycast(currentPosition, direction.normalized, out RaycastHit wallHit, distance, wallLayer))
            {
                trajectoryPoints.Add(wallHit.point);
                finalLandingPosition = new Vector3(wallHit.point.x, 0f, wallHit.point.z);
                break;
            }

            currentVelocity = nextVelocity;
            currentPosition = nextPosition;

            if (currentPosition.y <= 0f)
            {
                finalLandingPosition = new Vector3(currentPosition.x, 0f, currentPosition.z);
                trajectoryPoints.Add(finalLandingPosition);
                break;
            }
        }

        trajectoryLine.positionCount = trajectoryPoints.Count;
        for (int i = 0; i < trajectoryPoints.Count; i++)
        {
            trajectoryLine.SetPosition(i, trajectoryPoints[i]);
        }
    }

    // 디코이 범위 표시 위치 업데이트
    private void UpdateRangeIndicator()
    {
        if (decoyRangeIndicator == null) return;

        // 궤적의 마지막 지점 (착지 지점) 계산
        Vector3 landingPosition = finalLandingPosition;
        landingPosition.y = rangeIndicatorHeight;

        decoyRangeIndicator.transform.position = landingPosition;
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
        // 던지는 중에는 취소 불가
        if (isThrowing)
        {
            return;
        }


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
        // 범위 표시 비활성화
        if (decoyRangeIndicator != null)
        {
            decoyRangeIndicator.SetActive(false);
        }
    }

    #endregion

    public bool IsDecoyAiming => isDecoyAiming;
    public bool IsTransformed => isTransformed;
    public bool IsThrowing => isThrowing;
}