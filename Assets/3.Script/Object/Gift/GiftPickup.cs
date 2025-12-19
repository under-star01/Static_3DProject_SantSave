using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftPickup : MonoBehaviour, IInteractable
{
    [Header("선물 정보")]
    [SerializeField] private GiftType giftType;
    [SerializeField] private string giftName;

    [Header("비주얼")]
    [SerializeField] private GameObject visualModel;

    private bool isPickedUp = false;

    // GiftManager에서 호출할 초기화 메서드
    public void Initialize(GiftType type, string name)
    {
        giftType = type;
        giftName = name;

        // Visual Model이 설정 안되어 있으면 자기 자신으로 설정
        if (visualModel == null)
        {
            visualModel = gameObject;
        }
    }

    public void Interact()
    {
        // 이미 선물을 들고 있으면 못 집음
        if (PlayerInventory.Instance.HasGiftEquipped())
        {
            Debug.Log("이미 선물을 들고 있습니다!");
            return;
        }

        // 이미 집은 선물이면 못 집음
        if (isPickedUp)
        {
            Debug.Log("이미 집은 선물입니다!");
            return;
        }

        // 선물 줍기
        PlayerInventory.Instance.PickupGift(giftType);

        // 선물 숨기기
        isPickedUp = true;
        if (visualModel != null)
        {
            visualModel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}