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

    public void Initialize(GiftType type, string name)
    {
        giftType = type;
        giftName = name;

        if (visualModel == null)
        {
            visualModel = gameObject;
        }
    }

    public void Interact()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("PlayerInventory.Instance가 null입니다!");
            return;
        }

        // 애니메이션 중이거나 이미 들고 있으면 못 집음
        if (PlayerInventory.Instance.IsAnimating || PlayerInventory.Instance.HasGiftEquipped())
        {
            Debug.Log("이미 선물을 들고 있거나 애니메이션 중입니다!");
            return;
        }

        if (isPickedUp)
        {
            Debug.Log("이미 집은 선물입니다!");
            return;
        }

        // 선물 줍기 시작 (애니메이션)
        PlayerInventory.Instance.StartPickupGift(giftType);

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
