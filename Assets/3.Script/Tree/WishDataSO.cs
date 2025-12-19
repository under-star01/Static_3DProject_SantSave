using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WishDataSettings", menuName = "SantaGame/WishDataSettings")]
public class WishDataSO : ScriptableObject
{
    [Header("카드 데이터 소스 목록")]
    public List<WishCardDataSO> wishCardDatas;

    [Header("생성할 기본 카드 수")]
    public int defaultCardCount = 3;

    [Header("사진 리소스 경로 (참고)")]
    public string photoResourcesPath = "ChildPhotos/"; // 필요 시 사용

    // 런타임에서 카드 리스트를 메모리에 매핑하는 간단한 팩토리 메서드
    public List<WishCardDataSO> GetAllCardDatas()
    {
        if (wishCardDatas == null) // 카드 만들어진거 없으면
        {
            return new List<WishCardDataSO>(); // 리스트 데이터에서 새로 뽑아서 만들어 저장
        }

        return new List<WishCardDataSO>(wishCardDatas); // 만들어진게 있다면 리스트에 저장하기
    }

    // 이름으로 특정 카드를 찾기
    public WishCardDataSO GetCardDataByName(string name)
    {
        if (wishCardDatas == null) // 소원카드의 정보가 없다면
        { 
            return null; // 돌아가~
        }

        foreach (var d in wishCardDatas) 
        {
            if (d != null && d.childName == name) // 카드 데이터가 null이 아니고(생성되어있고), 데이터의 아이 이름이 name이면
            { 
                return d; // 데이터 리턴
            }
        }
        return null; // 아니면 돌아가~
    }

    // 기본 팩에서 임의의 카드들을 제공
    public List<WishCardDataSO> DrawRandomCards(int count) 
    {
        List<WishCardDataSO> pool = GetAllCardDatas(); // 리스트 풀 생성
        if (pool.Count == 0) // 리스트가 없으면
        {
            return new List<WishCardDataSO>(); // 새로운 리스트 만들기
        }

        // 간단한 셔플
        for (int i = 0; i < pool.Count; i++)
        {
            int r = Random.Range(i, pool.Count);
            var tmp = pool[i];
            pool[i] = pool[r];
            pool[r] = tmp;
        }

        int take = Mathf.Min(count, pool.Count);
        List<WishCardDataSO> result = new List<WishCardDataSO>();
        for (int i = 0; i < take; i++) 
        {
            result.Add(pool[i]); 
        }
        return result;
    }
}
