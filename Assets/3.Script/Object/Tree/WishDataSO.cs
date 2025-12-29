using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WishDataSettings", menuName = "SantaGame/WishDataSettings")]
public class WishDataSO : ScriptableObject
{
    [Header("아이 이름 풀")]
    public List<string> childNames = new List<string>
    {
        "범근", "명훈", "혜림"
    };

    [Header("선물 아이템 풀")]
    public List<string> giftItems = new List<string>
    {
        "레고 블록", "인형", "로봇 장난감", "그림책",
        "축구공", "색연필 세트", "자전거", "퍼즐",
        "블록셋", "과학 실험 키트", "악기", "보드게임"
    };

    [Header("사진 리소스 경로")]
    public string photoResourcesPath = "ChildPhotos/";

    [Header("생성할 위시 카드 수")]
    public int wishCardCount = 3;

    [Header("문장 템플릿")]
    public string wishTemplate = "산타님, {0}을 주세요!";
}


