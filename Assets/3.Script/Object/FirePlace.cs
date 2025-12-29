using UnityEngine;

public class FirePlace : MonoBehaviour,IInteractable
{
    // 상호작용 Interact 메소드에서, 현재 GameManager에서 isAllCorrect가 true인지 판단하고, 아니면 실행 못하게, 맞으면 GameOver 이벤트 호출하게 만들면 끝이야!
    public void Interact(PlayerInteract playerInteract)
    {
        // 모든 침대에 선물 제출시 게임 종료 가능
        if (GameManager.instance.submitCnt >= ChildManager.instance.spawnChild_List.Count)
        {
            // 게임 종료 이벤트 호출
            GameManager.instance.gameOver?.Invoke();
            AudioManager.Instance.PlayEnterSFX();
        }
        else
        {
            Debug.LogWarning("아직 모든 침대에 선물을 전달하지 않았습니다!");
        }
    }
}
