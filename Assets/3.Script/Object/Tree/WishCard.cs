// WishCard.cs
using UnityEngine;

[System.Serializable]
public class WishCard
{
    public string childName;
    public Sprite childPhoto;
    public string giftItem;
    public string wishText;
    public bool isCompleted; // 선물 전달 완료 여부

    public WishCard(string name, Sprite photo, string gift)
    {
        childName = name;
        childPhoto = photo;
        giftItem = gift;
        wishText = string.Format("산타님, {0}을 주세요!", gift);
        isCompleted = false;
    }
}
