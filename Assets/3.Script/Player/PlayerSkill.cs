using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Skill
{
    Transform,
    Decoy
}

public class PlayerSkill : MonoBehaviour
{
    [Header("변신 스킬")]
    [SerializeField] private float transformCool = 0.5f;
    [SerializeField] private ParticleSystem transformEffect;
    [SerializeField] private GameObject[] transformPrefabs;

    [Header("디코이 스킬")]
    [SerializeField] private GameObject decoyPrefab;
    [SerializeField] private LineRenderer trajectoryLine; // 궤적 표시
    [SerializeField] private GameObject decoyRangeIndicator; // 디코이 범위 표시
    [SerializeField] private float throwForce = 8f;
    [SerializeField] private float throwAngle = 45f;
    [SerializeField] private int trajectoryPoints = 20;
    [SerializeField] private float decoyCooldown = 1f; // 쿨다운
    [SerializeField] private LayerMask wallLayer; //벽 레이어
    [SerializeField] private float throwAnimationDelay = 0.7f; // 던지기 타이밍
    [SerializeField] private float throwAnimationDuration = 1f; // 애니메이션 전체 길이

    private Animator animator;
    private PlayerMove playerMove;
    private Renderer playerRenderer;
    private Rigidbody rb;
    [SerializeField] private Light sight;

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

        if (playerMove != null)
        {
            Vector2 moveInput = playerMove.GetMoveInput();

            if (moveInput.sqrMagnitude > 0.01f) // 입력이 있으면
            {
                return;
            }
        }

        rb.linearVelocity = Vector3.zero;

        if (isThrowing)
        {
            return;
        }

        if (isDecoyAiming)
        {
            //조준중 캔슬
            CancelDecoySkill();
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

        AudioManager.Instance.PlayTransformSFX();

        // 불끄기
        sight.enabled = false;

        // 레이어 및 태그 변경
        gameObject.tag = "Decoy";
        gameObject.layer = LayerMask.NameToLayer("Decoy");

        int randomIndex = UnityEngine.Random.Range(0, transformPrefabs.Length);
        GameObject selectedPrefab = transformPrefabs[randomIndex];

        // 독립 오브젝트로 생성
        currentTransformObject = Instantiate(selectedPrefab, transform.position, transform.rotation);

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

        UIManager.instance.ChangeSkillColor(Skill.Transform, true);
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

        //불켜기
        sight.enabled = true;

        // 레이어 및 태그 복구
        gameObject.tag = "Player";
        gameObject.layer = LayerMask.NameToLayer("Player");

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
        lastTransformTime = Time.time;

        // 쿨타임 UI 실행
        UIManager.instance.ChangeSkillColor(Skill.Transform, false);
        UIManager.instance.CoolTimeStart(Skill.Transform, transformCool);
     
        Debug.Log("변신 해제");
    }

    // 변신 오브젝트 위치만 업데이트 (회전은 고정)
    private void UpdateTransformObjectPosition()
    {
        if (currentTransformObject == null)
            return;

        currentTransformObject.transform.position = transform.position;
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

    public void OnDecoySkillStart()
    {
        if (isTransformed)
        {//변신 중 사용 불가
            return;
        }

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
        if (isTransformed)
        {//변신 중 사용 불가
            return;
        }

        if (!isDecoyAiming)
        {
            return;
        }

        if (isThrowing)
        {
            return;
        }

        StartThrowAnimation();
    }

    private void StartDecoyAim()
    {
        isDecoyAiming = true;

        AudioManager.Instance.PlayClickSFX();

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = true;
        }

        if (decoyRangeIndicator != null)
        {
            decoyRangeIndicator.SetActive(true);

            float diameter = 8f;//유인표시범위
            decoyRangeIndicator.transform.localScale = new Vector3(diameter, 0.01f, diameter);
        }

        // 스킬 UI 색상 변경
        UIManager.instance.ChangeSkillColor(Skill.Decoy, true);

        Debug.Log("디코이 조준 시작");
    }

    private void StartThrowAnimation()
    {
        isThrowing = true;

        if (playerMove != null)
        {
            playerMove.canMove = false;
        }

        lockedPlayerRotation = transform.rotation;
        lockPlayerRotation = true;

        if (animator != null)
        {
            animator.SetTrigger("Throw");
        }

        StartCoroutine(ThrowAfterAnimation());
    }

    private IEnumerator ThrowAfterAnimation()
    {
        yield return new WaitForSeconds(throwAnimationDelay);

        ThrowDecoy();

        yield return new WaitForSeconds(throwAnimationDuration - throwAnimationDelay);

        isThrowing = false;
        lockPlayerRotation = false;

        if (playerMove != null)
        {
            playerMove.canMove = true;
        }
    }

    // 단순화된 궤적 업데이트
    private void UpdateTrajectory()
    {
        Vector3 mouseWorldPos = playerMove.mouseHitPos;
        Vector3 startPosition = transform.position + Vector3.up * 1.5f;

        // 마우스 방향으로 목표 설정
        targetPosition = mouseWorldPos;

        // 단순한 던지기 속도 계산
        Vector3 throwVelocity = CalculateSimpleThrowVelocity(startPosition, targetPosition);

        // 궤적 그리기 (벽 체크 포함)
        DrawTrajectoryWithWallCheck(startPosition, throwVelocity);

        UpdateRangeIndicator();
    }

    // 단순화된 속도 계산 (고정 각도 사용)
    private Vector3 CalculateSimpleThrowVelocity(Vector3 start, Vector3 target)
    {
        Vector3 direction = target - start;
        Vector3 horizontalDir = new Vector3(direction.x, 0, direction.z);

        float angleRad = throwAngle * Mathf.Deg2Rad;

        Vector3 velocity = horizontalDir.normalized * throwForce * Mathf.Cos(angleRad);
        velocity.y = throwForce * Mathf.Sin(angleRad);

        return velocity;
    }

    // 벽 체크 포함된 궤적 그리기
    private void DrawTrajectoryWithWallCheck(Vector3 startPos, Vector3 initialVelocity)
    {
        if (trajectoryLine == null)
            return;

        List<Vector3> points = new List<Vector3>();
        Vector3 currentPos = startPos;
        Vector3 currentVel = initialVelocity;
        Vector3 previousPos = startPos;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            points.Add(currentPos);

            // 간단한 중력 적용
            currentVel += Physics.gravity * 0.1f;
            Vector3 nextPos = currentPos + currentVel * 0.1f;

            // 현재 위치에서 다음 위치로 가는 방향
            Vector3 direction = nextPos - currentPos;
            float distance = direction.magnitude;

            // 벽 체크
            if (Physics.Raycast(currentPos, direction.normalized, out RaycastHit hit, distance, wallLayer))
            {
                // 벽에 닿은 지점
                points.Add(hit.point);
                finalLandingPosition = new Vector3(hit.point.x, 0f, hit.point.z);

                trajectoryLine.positionCount = points.Count;
                trajectoryLine.SetPositions(points.ToArray());
                return;
            }

            previousPos = currentPos;
            currentPos = nextPos;

            // 땅에 닿으면 종료
            if (currentPos.y <= 0f)
            {
                Vector3 groundPos = new Vector3(currentPos.x, 0f, currentPos.z);
                points.Add(groundPos);
                finalLandingPosition = groundPos;

                trajectoryLine.positionCount = points.Count;
                trajectoryLine.SetPositions(points.ToArray());
                return;
            }
        }

        trajectoryLine.positionCount = points.Count;
        trajectoryLine.SetPositions(points.ToArray());

        // 최대 거리까지 갔을 때의 착지 지점
        if (points.Count > 0)
        {
            Vector3 lastPoint = points[points.Count - 1];
            finalLandingPosition = new Vector3(lastPoint.x, 0f, lastPoint.z);
        }
    }

    private void UpdateRangeIndicator()
    {
        if (decoyRangeIndicator == null) return;

        // 최종 착지 지점에 범위 표시
        Vector3 indicatorPos = finalLandingPosition;
        indicatorPos.y = 0.5f;
        decoyRangeIndicator.transform.position = indicatorPos;
    }

    // 디코이 던지기
    private void ThrowDecoy()
    {
        AudioManager.Instance.PlayThrowSFX();

        if (decoyPrefab != null)
        {
            Vector3 startPosition = transform.position + Vector3.up * 1.5f;
            GameObject decoy = Instantiate(decoyPrefab, startPosition, Quaternion.identity);

            Rigidbody rb = decoy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 고정된 힘과 각도로 던지기
                Vector3 throwVelocity = CalculateSimpleThrowVelocity(startPosition, targetPosition);
                rb.linearVelocity = throwVelocity;

                Debug.Log($"디코이 투척");
            }
        }

        EndDecoyAim();
        lastDecoyTime = Time.time;

        // 스킬 UI 쿨타임 적용
        UIManager.instance.CoolTimeStart(Skill.Decoy, decoyCooldown);
    }

    private void CancelDecoySkill()
    {
        if (isThrowing)
        {
            return;
        }

        AudioManager.Instance.PlayClickSFX();

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

        if (decoyRangeIndicator != null)
        {
            decoyRangeIndicator.SetActive(false);
        }

        // 색상 복구
        UIManager.instance.ChangeSkillColor(Skill.Decoy, false);
    }

    #endregion

    public bool IsDecoyAiming => isDecoyAiming;
    public bool IsTransformed => isTransformed;
    public bool IsThrowing => isThrowing;
}