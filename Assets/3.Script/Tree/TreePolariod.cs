using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TreePolaroid : MonoBehaviour
{
    [Header("UI 참조")]
    public Image photoImage;
    public TextMeshProUGUI wishText; 
    public TextMeshProUGUI childNameText;

    [Header("할당된 위시 카드")]
    public string assignedChildName;
    private WishCard assignedWishCard;

    [Header("설정")]
    public bool assignOnStart = true;
    public string specificChildName = ""; // 특정 아이에게 할당하려면

    void Start()
    {
        if (assignOnStart)
        {
            if (!string.IsNullOrEmpty(specificChildName))
            {
                AssignSpecificWish(specificChildName);
            }
            else
            {
                AssignRandomWish();
            }
        }
    }

    public void AssignRandomWish()
    {
        WishManager wishManager = WishManager.Instance;
        if (wishManager != null)
        {
            List<WishCard> allWishes = wishManager.GetAllWishCards();
            if (allWishes.Count > 0)
            {
                int randomIndex = Random.Range(0, allWishes.Count);
                WishCard wish = allWishes[randomIndex];
                ApplyWishToUI(wish);
                assignedChildName = wish.childName;
                assignedWishCard = wish;
            }
            else
            {
                Debug.LogWarning("할당할 위시 카드가 없습니다.");
            }
        }
    }

    public void AssignSpecificWish(string childName)
    {
        WishManager wishManager = WishManager.Instance;
        if (wishManager != null)
        {
            WishCard wish = wishManager.GetWishCard(childName);
            if (wish != null)
            {
                ApplyWishToUI(wish);
                assignedChildName = childName;
                assignedWishCard = wish;
            }
            else
            {
                Debug.LogWarning($"아이 '{childName}'의 위시 카드를 찾을 수 없습니다.");
            }
        }
    }

    void ApplyWishToUI(WishCard wish)
    {
        if (photoImage != null && wish.childPhoto != null)
        {
            photoImage.sprite = wish.childPhoto;
            photoImage.color = Color.white; // 이미지가 보이도록
        }
        else if (photoImage != null)
        {
            photoImage.color = Color.clear; // 사진이 없으면 투명하게
        }

        if (wishText != null)
            wishText.text = wish.wishText;

        if (childNameText != null)
            childNameText.text = wish.childName;
    }

    // 산타가 상호작용 시 호출
    public void OnSantaInteract()
    {
        if (assignedWishCard != null)
        {
            Debug.Log($"산타가 {assignedChildName}의 소원을 확인했습니다!");
            Debug.Log($"선물: {assignedWishCard.giftItem}");


        }
    }

    // 완료 상태 업데이트
    public void MarkAsCompleted()
    {
        if (assignedWishCard != null && !assignedWishCard.isCompleted)
        {
            WishManager.Instance.CompleteWish(assignedChildName);
            assignedWishCard.isCompleted = true;

            // UI 업데이트 (예: 회색 처리)
            if (photoImage != null)
                photoImage.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        }
    }

    // 현재 할당된 위시 카드 정보 가져오기
    public WishCard GetAssignedWish()
    {
        return assignedWishCard;
    }

    // 외부에서 강제로 재할당
    public void ReassignWish(string newChildName)
    {
        AssignSpecificWish(newChildName);
    }
}
