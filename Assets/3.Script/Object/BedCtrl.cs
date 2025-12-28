using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BedCtrl : MonoBehaviour, IInteractable
{
    [SerializeField] private ChildType_SO childData; // Child 관련 데이터
    [SerializeField] private GiftType_SO giftData; // Gift 관련 데이터
    [SerializeField] private Transform submitPos; // 선물을 놓을 위치
    [SerializeField] private TextMeshProUGUI childName; // 표시할 이름 오브젝트
    [SerializeField] private int spawnIndex; // 생성 순서

    private bool isBusy = false; // 현재 실행중인지 여부
    private bool isSubmit = false; // 선물 제출 여부

    // Child 오브젝트 데이터 초기화 메소드
    public void Init(ChildType_SO childData, GiftType_SO giftData, BedData bedData)
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
        if(bedData != null)
        {
            this.submitPos = bedData.summitPos;
            this.spawnIndex = bedData.spawnIndex;
            Debug.Log("BedData 데이터 적용 준비 완료");
        }
    }

    public void Interact(PlayerInteract playerInteract)
    {
        if (isSubmit || isBusy || playerInteract == null) return;

        // 선물을 안 들고 있으면 제출 불가
        if (!playerInteract.hasGift)
        {
            Debug.Log("선물을 들고 있지 않아서 제출할 수 없습니다!");
            return;
        }

        // 선물 제출 상태 갱신 및 배치
        isSubmit = true;
        GameManager.instance.submitCnt++;
        Gift gift = playerInteract.PlaceCarriedGift(submitPos);

        // 정답 비교
        bool isCorrect = (gift != null && gift.currentGift == giftData);

        if (isCorrect)
        {
            Debug.LogWarning("정답 선물 제출!");
            UIManager.instance.DeActivatePolarroid(spawnIndex);
            ScoreManager.instance.OnCorrectSubmit();
        }
        else
        {
            Debug.LogWarning("오답 선물 제출!");
            UIManager.instance.DeActivatePolarroid(spawnIndex);
            ScoreManager.instance.OnWrongSubmit();
        }

        isBusy = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") &&  childName != null)
        {
            childName.text = childData.childName;
            childName.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && childName != null)
        {
            childName.gameObject.SetActive(false);
        }
    }
}
