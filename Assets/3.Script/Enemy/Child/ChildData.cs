using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ChildData
{
    public string childName; // Child 이름
    public List<Transform> patrolPos_List; // Child가 패트롤할 위치 리스트
    public GameObject targetBed; // 목표 침대 오브젝트
    public int targetGiftId; // 목표 선물 Id
}
