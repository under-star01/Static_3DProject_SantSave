using System.Collections.Generic;
using UnityEngine;

public class PolaroidManager : MonoBehaviour
{
    [SerializeField] private WishDataSO wishDataSettings;
    [SerializeField] private int sessionCardCount = 3;

    private List<WishCardDataSO> sessionCards = new List<WishCardDataSO>();
    private int currentIndex = 0;

    public bool HasSession
    {
        get
        {
            if (sessionCards == null)
            {
                return false;
            }
            if (sessionCards.Count <= 0)
            {
                return false;
            }
            return true;
        }
    }

    public int CurrentIndex
    {
        get { return currentIndex; }
    }

    public int SessionCount
    {
        get
        {
            if (sessionCards == null)
            {
                return 0;
            }
            return sessionCards.Count;
        }
    }

    public WishCardDataSO GetCurrentCard()
    {
        if (HasSession == false)
        {
            return null;
        }

        if (currentIndex < 0 || currentIndex >= sessionCards.Count)
        {
            return null;
        }

        return sessionCards[currentIndex];
    }

    public void CreateNewSession()
    {
        if (wishDataSettings == null)
        {
            Debug.LogError("[PolaroidManager] wishDataSettings가 비어있습니다.");
            return;
        }

        sessionCards = wishDataSettings.DrawRandomCards(sessionCardCount);
        currentIndex = 0;

        for (int i = 0; i < sessionCards.Count; i++)
        {
            if (sessionCards[i] != null)
            {
                sessionCards[i].RefreshWishText();
            }
        }
    }

    public void Next()
    {
        if (HasSession == false)
        {
            return;
        }

        currentIndex = currentIndex + 1;
        if (currentIndex >= sessionCards.Count)
        {
            currentIndex = 0;
        }
    }

    public void Prev()
    {
        if (HasSession == false)
        {
            return;
        }

        currentIndex = currentIndex - 1;
        if (currentIndex < 0)
        {
            currentIndex = sessionCards.Count - 1;
        }
    }
}
