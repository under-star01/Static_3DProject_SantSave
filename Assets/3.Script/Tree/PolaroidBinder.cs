using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PolaroidBinder : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image photoImage;
    [SerializeField] private TextMeshProUGUI childNameText;
    [SerializeField] private TextMeshProUGUI wishText;

    public void Bind(WishCardDataSO card)
    {
        if (card == null)
        {
            Clear();
            return;
        }

        if (photoImage != null)
        {
            photoImage.sprite = card.childPhoto;

            if (card.childPhoto != null)
            {
                photoImage.color = Color.white;
            }
            else
            {
                photoImage.color = Color.clear;
            }
        }

        if (childNameText != null)
        {
            childNameText.text = card.childName;
        }

        if (wishText != null)
        {
            if (string.IsNullOrEmpty(card.wishText) == false)
            {
                wishText.text = card.wishText;
            }
            else
            {
                wishText.text = $"산타님, {card.giftItem}을 주세요!";
            }
        }
    }

    public void Clear()
    {
        if (photoImage != null)
        {
            photoImage.sprite = null;
            photoImage.color = Color.clear;
        }

        if (childNameText != null)
        {
            childNameText.text = "";
        }

        if (wishText != null)
        {
            wishText.text = "";
        }
    }
}
