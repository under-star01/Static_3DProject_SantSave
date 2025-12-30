using Unity.Cinemachine;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 20f;
    [SerializeField] private float zoomSpeed = 0.5f;
    [SerializeField] private CinemachineCamera cinemachine;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask wallMask;

    public bool canMove = true;
    public Vector3 mouseHitPos;
    
    private Rigidbody rb;
    private Animator animator;
    private PlayerNoise playerNoise;
    private Camera cam;

    private Vector2 moveInput; 
    private Vector2 mousePos;
    private Vector3 moveDir;
    private Vector3 lookDir = Vector3.right;
    private float zoomInput = 0;
    private bool isRun = false;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out animator);
        TryGetComponent(out playerNoise);

        if (cam == null)
            cam = Camera.main;
    }

    private void FixedUpdate()
    {
        // 이동 제한시 리턴
        if (!canMove)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        // 이동 방향 및 벽 충돌 처리
        CalculateMoveAndWall();

        // 회전 적용
        CalculateRotation();

        // 애니메이션 적용
        CalculateAnimation();
    }

    private void Update()
    {
        // 이동 제한시 리턴
        if (!canMove)
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);
            animator.SetFloat("Speed", 0f);
            return;
        }

        // 마우스 회전 방향 계산
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            mouseHitPos = hit.point;
            Vector3 dir = mouseHitPos - transform.position;
            
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
            {
                lookDir = dir.normalized;
            }
        }
    }

    private void LateUpdate()
    {
        if (Mathf.Abs(zoomInput) < 0.01f)
            return;

        var lens = cinemachine.Lens;

        // 휠 ↑ → 확대(OrthoSize 감소)
        lens.OrthographicSize -= zoomInput * zoomSpeed;

        // 1 ~ 5로 제한
        lens.OrthographicSize = Mathf.Clamp(lens.OrthographicSize, 1f, 5f);

        cinemachine.Lens = lens;

        // 이번 프레임 입력 소비
        zoomInput = 0f;
    }

    private void CalculateMoveAndWall()
    {
        // 카메라 기준 이동 방향 설정
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveDir = camRight * moveInput.x + camForward * moveInput.y;

        // 이동 속도 계산 + 벽 슬라이딩
        Vector3 desired = Vector3.zero;

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            desired = moveDir.normalized * moveSpeed;

            if (CheckWall(out Vector3 wallNormal))
            {
                desired = Vector3.ProjectOnPlane(desired, wallNormal);
            }
        }
        desired.y = rb.linearVelocity.y;
        rb.linearVelocity = desired;
    }

    private void CalculateRotation()
    {
        // 회전 적용
        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);

        rb.MoveRotation(newRot);
    }

    private void CalculateAnimation()
    {
        // 애니메이션 파라미터 계산 -> 마우스 기준
        Vector3 horizontalVel = rb.linearVelocity;
        horizontalVel.y = 0f;

        float speed = horizontalVel.magnitude;
        animator.SetFloat("Speed", speed);

        if (speed < 0.01f)
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);
            return;
        }

        Vector3 forward = transform.forward;
        Vector3 move = horizontalVel;

        forward.y = 0f;
        move.y = 0f;

        forward.Normalize();
        move.Normalize();

        float dot = Mathf.Clamp(Vector3.Dot(forward, move), -1f, 1f);
        float crossY = Vector3.Cross(forward, move).y;

        float angle = Mathf.Acos(dot);
        if (crossY < 0f) angle = -angle;

        float moveX = Mathf.Sin(angle);
        float moveY = Mathf.Cos(angle);

        animator.SetFloat("MoveX", moveX);
        animator.SetFloat("MoveY", moveY);
    }

    public void SetMoveInput(Vector2 input)
    {
        // 정지시 -> 소리 발생 Stop
        if (input.Equals(Vector2.zero))
        {
            playerNoise.StopNoiseCoroutine();
        }
        // 이동시 -> 소리 발생 Start
        else
        {
            // Run 상태 (Shift)
            if (isRun)
            {
                playerNoise.StartNoiseCoroutine(6f, 0.5f);
            }
        }

        moveInput = input;
    }

    public void SetLookInput(Vector2 screenPos)
    {
        mousePos = screenPos;
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public void RunStart()
    {
        isRun = true;
        moveSpeed = 4.5f;
    }

    public void RunStop()
    {
        isRun = false;
        moveSpeed = 3.0f;
    }

    public Vector3 GetMouseWorldPosition()
    {
        return lookDir;
    }

    public void SetCameraZoom(float value)
    {
        zoomInput += value;
    }

    // 벽 체크 메소드 (충돌 시, 떨림 및 통과 방지)
    private bool CheckWall(out Vector3 wallNormal)
    {
        wallNormal = Vector3.zero;

        Vector3 center = rb.position + Vector3.up * 0.9f; // 캡슐 중심
        float radius = 0.3f;
        float distance = 0.1f;

        if (Physics.SphereCast(center, radius, moveDir.normalized, out RaycastHit hit, distance, wallMask))
        {
            wallNormal = hit.normal;
            return true;
        }
        return false;
    }

    private void Footstep()
    {
        AudioManager.Instance.PlayFootstepSFX();
    }

    private void LoudFootstep()
    {
        AudioManager.Instance.PlayLoudFootstepSFX();
    }
}
