using UnityEngine;

[CreateAssetMenu(fileName = "WishCardData", menuName = "SantaGame/WishCardData")]
public class WishCardDataSO : ScriptableObject
{
    public string childName;
    public Sprite childPhoto;
    public string giftItem;
    [TextArea] public string wishTemplate; // "산타님, {0}을 주세요!"
    [HideInInspector] public string wishText; // 런타임에 계산된 소원 텍스트
    [HideInInspector] public bool isCompleted;

    // 에디터에서 기본 템플릿으로 초기화
    public void InitializeFromTemplate(string name, Sprite photo, string gift, string template = null)
    {
        childName = name;
        childPhoto = photo;
        giftItem = gift;
        wishTemplate = template ?? "산타님, {0}을 주세요!";
        wishText = string.Format(wishTemplate.Replace("{0}", gift), gift); // 템플릿이 단순 포맷일 때 안전하게 처리
        isCompleted = false;
    }

    // 런타임에 텍스트를 재생성해도 되는 편의 메서드
    public void RefreshWishText()
    {
        if (string.IsNullOrEmpty(wishTemplate))
        {
            wishText = $"산타님, {giftItem}을 주세요!";
        }
        else
        {
            // 템플릿이 일반 포맷일 때 처리
            wishText = string.Format(wishTemplate.Replace("{0}", giftItem), giftItem);
        }
    }
}
