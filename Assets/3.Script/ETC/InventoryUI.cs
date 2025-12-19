using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("UI 요소")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Image giftIcon;
    [SerializeField] private TextMeshProUGUI giftNameText;

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

    private void Start()
    {
        ClearInventory();
    }

    public void UpdateInventory(GiftType gift)
    {
        // 패널 활성화
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        // GiftManager에서 선물 정보 가져오기
        GiftItem giftItem = GiftManager.Instance.GetGiftItem(gift);

        if (giftItem != null)
        {
            // 아이콘 설정
            if (giftIcon != null && giftItem.icon != null)
            {
                giftIcon.sprite = giftItem.icon;
            }

            // 이름 설정
            if (giftNameText != null)
            {
                giftNameText.text = giftItem.giftName;
            }
        }
    }

    public void ClearInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }
}
