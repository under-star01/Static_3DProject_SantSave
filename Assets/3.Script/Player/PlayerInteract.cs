using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interact")]
    [SerializeField] private float interactRadius = 1.5f;
    [SerializeField] private LayerMask interactLayer;

    [Header("Carry")]
    [SerializeField] private Transform giftAttachPoint; // 픽업시 선물 위치
    [SerializeField] private Animator animator;

    private PlayerMove playerMove;

    public Gift carriedGift;
    public bool hasGift => carriedGift != null; // 선물 보유 여부에 따라 상태 변경
    private bool ispickuping= false;

    private void Awake()
    {
        TryGetComponent(out playerMove);
    }


    // 상호작용 시도 메소드
    public void TryInteract()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            interactRadius,
            interactLayer
        );

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(this);
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }

    // 선물 pick up 시도 메소드
    public void TryPickUp_Gift(Gift gift)
    {
        if (gift == null) return;

        // 같은 선물을 다시 집으려는 경우 방지
        if (carriedGift == gift) return;

        // 약간 앞, 위로 위치하도록 설정
        DropCarriedGift();

        // 선물 pick up
        PickGift(gift);
    }

    // 선물 pick up 메소드
    private void PickGift(Gift gift)
    {
        StartCoroutine(PickupAniDelay_co());

        carriedGift = gift;

        // 선물 충돌 해제
        carriedGift.SetPicked(true);

        // 플레이어 손 위치에 붙이기
        Transform t = carriedGift.transform;
        t.SetParent(giftAttachPoint);
        t.localPosition = new Vector3(0f,-0.0017f,0.003f);
        t.localRotation = Quaternion.identity;

        // 애니메이션 반영
        if (animator != null)
            animator.SetBool("IsCarrying", true);
    }

    private IEnumerator PickupAniDelay_co()
    {
        animator.SetTrigger("Pickup");
        ispickuping = true;
        if (playerMove != null)
        {
            playerMove.canMove = false;
        }

        yield return new WaitForSeconds(1f);

        if (playerMove != null)
        {
            playerMove.canMove = true;
        }
        ispickuping = false;
    }

    // 현재 선물 drop 메소드
    public void DropCarriedGift()
    {
        if (!hasGift) return;
        if (ispickuping) return;

        Gift dropped = carriedGift;

        // 부모 해제
        Transform t = dropped.transform;
        t.SetParent(null);
        t.position = transform.position + transform.forward * 0.5f + transform.up * 0.5f;

        // 충돌 비활성화 
        dropped.SetPicked(false);
        carriedGift = null;

        if (animator != null)
            animator.SetBool("IsCarrying", false);
    }

    // 특정 위치에 선물 drop 메소드
    public Gift PlaceCarriedGift(Transform placePoint)
    {
        if (!hasGift || placePoint == null) return null;

        Gift placed = carriedGift;

        // 부모/위치/회전 고정
        Transform t = placed.transform;
        t.SetParent(placePoint);
        t.localPosition = Vector3.zero + Vector3.up * 1.5f; // 올려놓을 높이 설정
        t.localRotation = Quaternion.identity;

        // 충돌 활성화
        placed.SetPicked(true);
        carriedGift = null;

        if (animator != null)
            animator.SetBool("IsCarrying", false);

        return placed;
    }
}
