using System;
using UnityEngine;
using System.Collections;

public class Stairs : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform targetPos; // 이동 위치
    private bool isInteract = false; // 상호작용 중인지 여부

    public void Interact(PlayerInteract playerInteract)
    {
        if(isInteract) return;

        Debug.Log("실행!");
        isInteract = true;

        // 층이동 코루틴 실행
        StartCoroutine(MoveFloor(playerInteract.gameObject));
    }

    // 층이동 코루틴
    private IEnumerator MoveFloor(GameObject player)
    {
        // 입력 제한 및 암전 효과 실행

        AudioManager.Instance.PlayEnterSFX();

        PlayerInput playerInput;
        player.TryGetComponent(out playerInput);
        playerInput.enabled = false;
        TimeManager.instance.isTimer = false;

        yield return UIManager.instance.ActiveBlackOut_co(true, 1f);

        // 플레이어 위치 이동
        if(targetPos != null)
        {
            player.transform.position = targetPos.position;
        }

        // 입력 복귀 및 암전 효과 해제
        yield return UIManager.instance.ActiveBlackOut_co(false, 1f);

        playerInput.enabled = true;
        TimeManager.instance.isTimer = true;
        isInteract = false;
    }
}
