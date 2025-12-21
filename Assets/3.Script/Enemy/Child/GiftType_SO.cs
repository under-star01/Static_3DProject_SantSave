using UnityEngine;

[CreateAssetMenu(fileName = "GiftType", menuName = "Gift Type")]
public class GiftType_SO : ScriptableObject
{
    [Header("Gift Info")]
    public string giftId;    // 내부 식별용 ID
    public string giftName;  // UI 표시용 이름

    [Header("Prefab")]
    public GameObject giftPrefab;  // 생성할 Child 프리팹
}
