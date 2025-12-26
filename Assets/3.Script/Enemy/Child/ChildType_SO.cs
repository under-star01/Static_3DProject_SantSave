using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ChildType", menuName = "Child Type")]
public class ChildType_SO : ScriptableObject
{
    [Header("Child Info")]
    public string childName;  // UI 표시용 이름
    public Texture2D childImage;  // Child 이미지

    [Header("Prefab")]
    public GameObject childPrefab;  // 생성할 Child 프리팹
}
