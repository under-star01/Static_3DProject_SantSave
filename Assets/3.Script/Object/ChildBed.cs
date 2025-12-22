using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildBed : MonoBehaviour, IInteractable
{
    [Header("아이 정보")]
    [SerializeField] private ChildData childData;

    [Header("선물 놓을 위치")]
    [SerializeField] private Transform giftPlacementPoint;

    private bool hasReceivedGift = false;

    public void Interact()
    {
        if (hasReceivedGift)
        {
            Debug.Log($"{childData.childName}은 이미 선물을 받았습니다!");
            return;
        }

        if (PlayerInventory.Instance == null || !PlayerInventory.Instance.HasGiftEquipped())
        {
            Debug.Log("선물을 들고 있지 않습니다!");
            return;
        }

        if (PlayerInventory.Instance.IsAnimating)
        {
            Debug.Log("애니메이션 중입니다!");
            return;
        }

        // 선물 정보 가져오기
        GiftType currentGift = PlayerInventory.Instance.GetEquippedGift();

        // 게임 매니저에 기록
        GiftManager.Instance.RecordDelivery(childData, currentGift);

        // 선물 놓기 애니메이션 시작
        PlayerInventory.Instance.PlaceGiftOnBed();

        hasReceivedGift = true;

        Debug.Log($"{childData.childName}에게 {currentGift}를 놓았습니다!");
    }
}
