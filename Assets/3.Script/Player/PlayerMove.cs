using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 20f;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask groundMask;

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
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * moveInput.y + camRight * moveInput.x;

        Vector3 vel = Vector3.zero;
        if (moveDir.sqrMagnitude > 0.0001f)
            vel = moveDir.normalized * moveSpeed;

        vel.y = rb.linearVelocity.y;
        rb.linearVelocity = vel;
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

        // 애니메이션 동기화
        Vector3 worldVel = rb.linearVelocity;
        worldVel.y = 0f;

        float speed = worldVel.magnitude;

        // 마우스(=플레이어 forward) 기준 방향으로 변환
        Vector3 localDir = Vector3.zero;
        if (speed > 0.05f)
        {
            // 속도 방향만 사용 (크기 제거)
            localDir = transform.InverseTransformDirection(worldVel.normalized);
        }

        float moveX = localDir.x; // -1~1
        float moveY = localDir.z; // -1~1

        animator.SetFloat("Speed", speed * moveSpeed, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveX", moveX * moveSpeed, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", moveY * moveSpeed, 0.1f, Time.deltaTime);
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void SetLookInput(Vector2 screenPos)
    {
        mousePos = screenPos;
    }
}
