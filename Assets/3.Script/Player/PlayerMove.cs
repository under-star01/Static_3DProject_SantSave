using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Vector3 moveDir;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 20f;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask wallMask;

    private Rigidbody rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 mousePos;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out animator);

        if (cam == null)
            cam = Camera.main;
    }

    private void FixedUpdate()
    {
        // 카메라 기준으로 xz 평면을 이동하도록 설정
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // 크기가 1인 방향 벡터에, x축으로 input.x, z축으로 input.y의 값을 적용해 기준 벡터 설정 
        moveDir = camRight * moveInput.x + camForward * moveInput.y;

        // 벽에 닿았는지 체크
        Vector3 desired = moveDir.normalized * moveSpeed;

        if (CheckWall(out Vector3 wallNormal))
        {
            // 벽을 향해 파고드는 이동 성분 제거 → 슬라이딩
            desired = Vector3.ProjectOnPlane(desired, wallNormal);
        }
        desired.y = rb.linearVelocity.y;
        rb.linearVelocity = desired;

        // 상태에 따른 속도 적용
        Vector3 vel;

        if (moveDir.sqrMagnitude < 0.0001f) // 정지시
        {
            vel = rb.linearVelocity;
            vel.x = 0f;
            vel.z = 0f;
            rb.linearVelocity = vel;
        }
        else // 이동시
        {
            vel = moveDir.normalized * moveSpeed;
            vel.y = rb.linearVelocity.y;
            rb.linearVelocity = vel;
        }

        // 애니메이션 동기화
        float speed = vel.magnitude;

        animator.SetFloat("Speed", speed);
        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);
    }

    private void Update()
    {
        // 마우스 위치로 회전
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            Vector3 dir = hit.point - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void SetLookInput(Vector2 screenPos)
    {
        mousePos = screenPos;
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
}
