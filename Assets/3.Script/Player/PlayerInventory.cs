using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform giftHoldPoint; // 손 위치
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMove playerMove;

    private GiftType? equippedGift = null;
    private GameObject currentGiftVisual;
    private bool isAnimating = false;

    public bool IsAnimating => isAnimating;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    // 선물 줍기 시작
    public void StartPickupGift(GiftType gift)
    {
        if (isAnimating || HasGiftEquipped()) return;

        equippedGift = gift;
        isAnimating = true;

        // 이동 불가
        if (playerMove != null)
        {
            playerMove.canMove = false;
        }

        // Lift 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger("Lift");
        }

        Debug.Log($"선물 줍기 시작: {gift}");
    }

    // Lift 애니메이션 끝날 때 호출 (Animation Event)
    public void OnLiftComplete()
    {
        isAnimating = false;

        // 이동 가능
        if (playerMove != null)
        {
            playerMove.canMove = true;
        }

        // IsCarrying = true
        if (animator != null)
        {
            animator.SetBool("IsCarrying", true);
        }

        // 손에 선물 생성
        CreateGiftVisual();

        // UI 업데이트
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.UpdateInventory(equippedGift.Value);
        }

        Debug.Log($"선물을 들었습니다: {equippedGift}");
    }

    // 선물 내려놓기 시작 (G키)
    public void StartPutDownGift()
    {
        if (isAnimating || !HasGiftEquipped()) return;

        isAnimating = true;

        // 이동 불가
        if (playerMove != null)
        {
            playerMove.canMove = false;
        }

        // PutDown 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger("PutDown");
        }

        Debug.Log($"선물 내려놓기 시작: {equippedGift}");
    }

    // PutDown 애니메이션 끝날 때 호출 (Animation Event)
    public void OnPutDownComplete()
    {
        isAnimating = false;

        // 이동 가능
        if (playerMove != null)
        {
            playerMove.canMove = true;
        }

        // IsCarrying = false
        if (animator != null)
        {
            animator.SetBool("IsCarrying", false);
        }

        // 손에서 선물 제거
        DestroyGiftVisual();

        Debug.Log($"선물을 내려놓았습니다: {equippedGift}");
        equippedGift = null;

        // UI 클리어
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.ClearInventory();
        }
    }

    // 침대에 선물 놓기
    public void PlaceGiftOnBed()
    {
        if (isAnimating || !HasGiftEquipped()) return;

        isAnimating = true;

        // 이동 불가
        if (playerMove != null)
        {
            playerMove.canMove = false;
        }

        // PutDown 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger("PutDown");
        }
    }

    // 손에 선물 비주얼 생성
    private void CreateGiftVisual()
    {
        if (giftHoldPoint == null || !equippedGift.HasValue) return;

        GiftItem giftItem = GiftManager.Instance.GetGiftItem(equippedGift.Value);

        if (giftItem != null && giftItem.prefab != null)
        {
            currentGiftVisual = Instantiate(giftItem.prefab, giftHoldPoint);
            currentGiftVisual.transform.localPosition = new Vector3(0.001f, 0f, 0f);
            currentGiftVisual.transform.localRotation = Quaternion.identity;
            currentGiftVisual.transform.localScale = Vector3.one * 0.007f;

            // Collider 비활성화
            Collider col = currentGiftVisual.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // GiftPickup 비활성화
            GiftPickup pickup = currentGiftVisual.GetComponent<GiftPickup>();
            if (pickup != null) pickup.enabled = false;
        }
    }

    // 손에서 선물 제거
    private void DestroyGiftVisual()
    {
        if (currentGiftVisual != null)
        {
            Destroy(currentGiftVisual);
            currentGiftVisual = null;
        }
    }

    public bool HasGiftEquipped()
    {
        return equippedGift.HasValue;
    }

    public GiftType GetEquippedGift()
    {
        return equippedGift ?? GiftType.RTX_5090;
    }
}