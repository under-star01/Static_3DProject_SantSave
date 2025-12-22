using UnityEngine;

[CreateAssetMenu(fileName = "ChildType", menuName = "Child Type")]
public class ChildType_SO : ScriptableObject
{
    [Header("Child Info")]
    public string childId;    // 내부 식별용 ID
    public string childName;  // UI 표시용 이름

    [Header("Prefab")]
    public GameObject childPrefab;  // 생성할 Child 프리팹
}
