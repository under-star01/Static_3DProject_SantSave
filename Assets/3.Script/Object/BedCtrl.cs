using System.Collections.Generic;
using UnityEngine;

public class BedCtrl : MonoBehaviour, IInteractable
{
    [SerializeField] private ChildType_SO childData; // Child 관련 데이터
    [SerializeField] private GiftType_SO giftData; // Gift 관련 데이터
    [SerializeField] private Transform summitPos; // 선물을 놓을 위치

    private bool isBusy = false; // 현재 실행중인지 여부
    
    // Child 오브젝트 데이터 초기화 메소드
    public void Init(ChildType_SO childData, GiftType_SO giftData, Transform summitPos)
    {
        // Child Data 적용
        if (childData != null)
        {
            this.childData = childData;
            Debug.Log("childData 데이터 적용 준비 완료");
        }

        // Gift Data 적용 
        if (giftData != null)
        {
            this.giftData = giftData;
            Debug.Log("GiftData 데이터 적용 준비 완료");
        }

        // 선물 놓을 위치 적용
        if(summitPos != null)
        {
            this.summitPos = summitPos;
            Debug.Log("제출 위치 준비 완료");
        }
    }

    public void Interact(PlayerInteract playerInteract)
    {
        if (isBusy || playerInteract == null) return;

        // 선물을 안 들고 있으면 제출 불가
        if (!playerInteract.hasGift)
        {
            Debug.Log("선물을 들고 있지 않아서 제출할 수 없습니다!");
            return;
        }

        // 선물 배치
        Gift gift = playerInteract.PlaceCarriedGift(summitPos);

        // 정답 비교
        bool isCorrect = (gift != null && gift.currentGift == giftData);

        if (isCorrect)
        {
            ScoreManager.instance.OnCorrectSubmit();
        }
        else
        {
            ScoreManager.instance.OnWrongSubmit();
        }
        Debug.Log(isCorrect ? "정답 선물 제출!" : "오답 선물 제출!");

        // 점수 전달 내용 이후에 추가
        isBusy = false;
    }
}
