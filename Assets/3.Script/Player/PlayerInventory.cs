using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private GiftType? equippedGift = null;

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
    }

    // 선물 줍기
    public void PickupGift(GiftType gift)
    {
        equippedGift = gift;
        Debug.Log($"선물을 집었습니다: {gift}");

        // UI 업데이트
        InventoryUI.Instance?.UpdateInventory(gift);
    }

    // 선물 내려놓기
    public void PlaceGift()
    {
        Debug.Log($"선물을 놓았습니다: {equippedGift}");
        equippedGift = null;

        // UI 클리어
        InventoryUI.Instance?.ClearInventory();
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
