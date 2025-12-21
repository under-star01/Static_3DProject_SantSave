using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class ChildCtrl : MonoBehaviour
{
    [SerializeField] private List<Transform> patrolPos_List = new List<Transform>(); // Child가 패트롤할 위치 리스트
    [SerializeField] private Transform bedPos;

    // Child 오브젝트 데이터 초기화 메소드
    public void Init(BedData bedData)
    {
        // 임시로 패트롤 위치와 침대 위치를 가지고 옴
        if (bedData != null)
        {
            patrolPos_List = bedData.patrolPoints;
            bedPos = bedData.Bed.transform;
        }
    }
}
