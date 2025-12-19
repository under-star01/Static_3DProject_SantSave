using UnityEngine;
using System.Collections.Generic;

public class WishManager : MonoBehaviour
{
    public static WishManager Instance { get; private set; } // 싱글톤

    [SerializeField] private WishDataSO wishDataSettings;

    private Dictionary<string, WishCard> wishCards;
    private List<Sprite> availablePhotos;

    private bool isInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeWishCards();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeWishCards()
    {
        if (isInitialized) return;

        // 리소스 폴더에서 사진 로드
        LoadChildPhotos();

        // 위시 카드 생성
        GenerateWishCards();

        isInitialized = true;
        Debug.Log($"위시 카드 {wishCards.Count}개 생성 완료!");
    }

    void LoadChildPhotos()
    {
        // Resources/ChildPhotos/ 폴더에서 모든 스프라이트 로드
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>(wishDataSettings.photoResourcesPath);
        availablePhotos = new List<Sprite>();

        for (int i = 0; i < loadedSprites.Length; i++)
        {
            availablePhotos.Add(loadedSprites[i]);
        }

        if (availablePhotos.Count == 0)
        {
            Debug.LogWarning("사진 리소스가 없습니다. 기본 사진을 생성합니다.");
            CreateDefaultPhoto();
        }
    }

    void CreateDefaultPhoto()
    {
        // 간단한 기본 사진 생성 (옵션)
        availablePhotos.Add(null); // null로 처리하거나

    }

    void GenerateWishCards()
    {
        wishCards = new Dictionary<string, WishCard>();

        // 사용할 이름과 선물 풀을 복사해서 사용 (원본 유지)
        List<string> namePool = new List<string>();
        List<string> giftPool = new List<string>();
        List<Sprite> photoPool = new List<Sprite>();

        // 리스트 복사
        for (int i = 0; i < wishDataSettings.childNames.Count; i++)
        {
            namePool.Add(wishDataSettings.childNames[i]);
        }

        for (int i = 0; i < wishDataSettings.giftItems.Count; i++)
        {
            giftPool.Add(wishDataSettings.giftItems[i]);
        }

        for (int i = 0; i < availablePhotos.Count; i++)
        {
            photoPool.Add(availablePhotos[i]);
        }

        // 랜덤 셔플
        ShuffleList(namePool);
        ShuffleList(giftPool);
        ShuffleList(photoPool);

        // 생성할 개수 결정
        int count = wishDataSettings.wishCardCount;
        if (count > namePool.Count) count = namePool.Count;
        if (count > giftPool.Count) count = giftPool.Count;
        if (count > photoPool.Count && photoPool.Count > 0)
            count = Mathf.Min(count, photoPool.Count);

        for (int i = 0; i < count; i++)
        {
            string childName = namePool[i];
            Sprite photo = photoPool[i % photoPool.Count];
            string gift = giftPool[i];

            WishCard newCard = new WishCard(childName, photo, gift);
            wishCards.Add(childName, newCard);
        }
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // 모든 위시 카드가 완료되었는지 확인
    public bool AreAllWishesCompleted()
    {
        foreach (var card in wishCards.Values)
        {
            if (!card.isCompleted)
                return false;
        }
        return true;
    }

    // 트리 오브젝트에서 호출할 메서드들
    public WishCard GetWishCard(string childName)
    {
        if (wishCards.ContainsKey(childName))
            return wishCards[childName];
        return null;
    }

    public List<WishCard> GetAllWishCards()
    {
        List<WishCard> allCards = new List<WishCard>();

        foreach (var card in wishCards.Values)
        {
            allCards.Add(card);
        }

        return allCards;
    }

    public void CompleteWish(string childName)
    {
        if (wishCards.ContainsKey(childName))
        {
            wishCards[childName].isCompleted = true;
        }
    }

    // 특정 조건의 위시 카드 찾기
    public WishCard FindWishByGift(string giftName)
    {
        foreach (var card in wishCards.Values)
        {
            if (card.giftItem == giftName && !card.isCompleted)
                return card;
        }
        return null;
    }

    // 미완료된 위시 카드만 가져오기
    public List<WishCard> GetIncompleteWishes()
    {
        List<WishCard> incomplete = new List<WishCard>();

        foreach (var card in wishCards.Values)
        {
            if (!card.isCompleted)
                incomplete.Add(card);
        }

        return incomplete;
    }

    // 게임 재시작 시 (옵션)
    public void ResetWishes()
    {
        isInitialized = false;
        wishCards.Clear();
        InitializeWishCards();
    }

    // 디버그용: 모든 위시 카드 정보 출력
    public void PrintAllWishes()
    {
        Debug.Log("=== 현재 위시 카드 목록 ===");
        int index = 1;

        foreach (var card in wishCards.Values)
        {
            Debug.Log($"{index}. {card.childName}: {card.giftItem} (완료: {card.isCompleted})");
            index++;
        }
    }
}
