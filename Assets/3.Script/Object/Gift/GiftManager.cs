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
    private List<GameObject> spawnedGifts = new List<GameObject>();

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

        // 선물 생성
        SpawnGifts();
    }

    private void SpawnGifts()
    {
        if (giftSpawnPoints == null || giftSpawnPoints.Length < 3)
        {
            Debug.LogError("선물 스폰 포인트가 3개 필요합니다!");
            return;
        }

        // 각 아이가 원하는 선물을 스폰
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == null) continue;

            GiftType giftType = children[i].desiredGift;
            GiftItem giftItem = GetGiftItem(giftType);

            if (giftItem != null && giftItem.prefab != null)
            {
                // 선물 생성
                GameObject giftObj = Instantiate(
                    giftItem.prefab,
                    giftSpawnPoints[i].position,
                    giftSpawnPoints[i].rotation
                );

                // GiftPickup 컴포넌트 확인 및 추가
                GiftPickup pickup = giftObj.GetComponent<GiftPickup>();
                if (pickup == null)
                {
                    pickup = giftObj.AddComponent<GiftPickup>();
                }

                // 선물 정보 초기화
                pickup.Initialize(giftType, giftItem.giftName);

                // Layer 설정
                giftObj.layer = LayerMask.NameToLayer("Interactable");

                spawnedGifts.Add(giftObj);

                Debug.Log($"선물 생성: {giftItem.giftName} at {giftSpawnPoints[i].name}");
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

    // 선물 아이템 가져오기
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