using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftManager : MonoBehaviour
{
    public static GiftManager Instance { get; private set; }

    [Header("아이 정보")]
    [SerializeField] private ChildData[] children = new ChildData[3];

    [Header("선물 정보")]
    [SerializeField] private GiftItem[] gifts = new GiftItem[3];

    [Header("선물 스폰포인트")]
    [SerializeField] private Transform[] giftSpawnPoints = new Transform[3];

    private Dictionary<ChildData, GiftType> deliveredGifts = new Dictionary<ChildData, GiftType>();

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
        InitializeGame();
    }

    private void InitializeGame()
    {
        Debug.Log("게임 시작");

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null)
            {
                Debug.Log($"{i + 1}. {children[i].childName}는 {children[i].desiredGift}를 원합니다");
            }
        }
    }

    // 선물 배달 기록
    public void RecordDelivery(ChildData child, GiftType gift)
    {
        deliveredGifts[child] = gift;
        Debug.Log($"{gift}을 {child.childName}에게 전달");

        // 3개 모두 배달했는지 체크
        if (deliveredGifts.Count >= 3)
        {
            ShowResults();
        }
    }

    // 결과 표시
    private void ShowResults()
    {
        Debug.Log("선물 전달 완료");

        int correctCount = 0;

        foreach (var child in children)
        {
            if (deliveredGifts.ContainsKey(child))
            {
                GiftType delivered = deliveredGifts[child];
                bool isCorrect = child.desiredGift == delivered;

                if (isCorrect) correctCount++;

                Debug.Log($"{child.childName}: {delivered} - {(isCorrect ? "정답" : "틀림")}");
            }
        }

        Debug.Log($"점수: {correctCount}/3");
    }

    // 이미 배달했는지 확인
    public bool IsDelivered(ChildData child)
    {
        return deliveredGifts.ContainsKey(child);
    }

    // 배달 개수
    public int GetDeliveredCount()
    {
        return deliveredGifts.Count;
    }

    public GiftItem GetGiftItem(GiftType type)
    {
        foreach (var gift in gifts)
        {
            if (gift.giftType == type)
            {
                return gift;
            }
        }
        return null;
    }
}
