using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [SerializeField] private ChildManager childManager;

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
    }
}
