using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 20f;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask wallMask;

    public Vector3 mouseHitPos;
    private Rigidbody rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 mousePos;
    private Vector3 moveDir;
    private Vector3 lookDir;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out animator);

        if (cam == null)
            cam = Camera.main;
    }

    private void FixedUpdate()
    {
        // 이동 방향 및 벽 충돌 처리
        CalculateMoveAndWall();

        // 회전 적용
        CalculateRotation();

        // 애니메이션 적용
        CalculateAnimation();
    }

    private void Update()
    {
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

    public Vector3 GetMouseWorldPosition()
    {
        return lookDir;
    }
}
