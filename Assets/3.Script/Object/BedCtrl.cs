using System.Collections.Generic;
using UnityEngine;

public class BedCtrl : MonoBehaviour
{
    [SerializeField] private ChildType_SO childData; // Child 관련 데이터
    [SerializeField] private GiftType_SO giftData; // Gift 관련 데이터
    [SerializeField] private Transform summitPos; // 선물을 놓을 위치
    
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
}
