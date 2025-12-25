using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "GiftType", menuName = "Gift Type")]
public class GiftType_SO : ScriptableObject
{
    [Header("Gift Info")]
    public string giftName;  // UI 표시용 이름
    public string coment; // 선물 관련 코멘트

    [Header("Prefab")]
    public GameObject giftPrefab;  // 생성할 Child 프리팹
}
