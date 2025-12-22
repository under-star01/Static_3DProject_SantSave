using TMPro;
using UnityEngine;

public class RankingItemController : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text dateText;

    public void SetData(int rank, RankingEntry entry)
    {
        if (rankText != null) rankText.text = rank.ToString();
        if (nameText != null) nameText.text = entry.name;
        if (scoreText != null) scoreText.text = entry.score.ToString();
        if (dateText != null) dateText.text = entry.date;
    }
}
