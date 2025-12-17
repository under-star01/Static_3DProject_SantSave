using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; 
    [SerializeField] private float currentSpeed = 5f; 
    [SerializeField] private float turnSpeed = 20f;
    [SerializeField] private Camera cam;
    [SerializeField] LayerMask groundMask;

    private Rigidbody rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 mousePos;

    private void Awake()
    {
        // 컴포넌트 연결
        TryGetComponent(out rb);
        TryGetComponent(out animator);

        // 외부 연결
        if(cam != null)
        {
            cam = Camera.main;
        }
    }

    private void FixedUpdate()
    {
        // 카메라 기준 이동 방향으로 설정
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * moveInput.y + camRight * moveInput.x;

        // 속도 적용
        Vector3 speed = Vector3.zero;

        if (moveDir.sqrMagnitude > 0.0001f)
            speed = moveDir.normalized * moveSpeed;

        speed.y = rb.linearVelocity.y;
        rb.linearVelocity = speed;
    }

    private void Update()
    {
        // 마우스 위치로 회전
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, 999f, groundMask))
        {
            Vector3 dir = hit.point - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        // 애니메이션 동기화
        float speed = rb.linearVelocity.magnitude;
        animator.SetFloat("Speed", speed);
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
