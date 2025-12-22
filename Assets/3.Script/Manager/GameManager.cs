using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [SerializeField] private ChildManager childManager;
    [SerializeField] private ScoreManager scoreManager;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Manager 일괄 초기화
        if(childManager != null)
        {
            // ChildManager 초기화
            childManager.InitializeData();
        }

        if (scoreManager != null)
        {
            // ChildManager 초기화
            scoreManager.InitializeData();
        }
    }
}
