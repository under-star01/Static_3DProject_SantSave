using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TalkBubbleController : MonoBehaviour
{
    [Header("고정된 말풍선 높이 (NPC 중심에서 위로)")]
    public float headHeight = 1.8f;

    [Header("머리 위 말풍선의 높이 오프셋")]
    public float bubbleOffsetUp = 0.0f;

    [Header("상태별 말풍선 매핑")]
    public TalkBubbleData[] stateBubbles;

    private GameObject currentBubble;
    private string currentState;
    private float bubbleLifeSeconds = 3f; // 3초 후 제거

    // FSM에서 상태 변경 시 이 메서드를 호출
    public void OnStateChanged(string newState)
    {
        if (newState == currentState) return;
        currentState = newState;

        // 기존 말풍선 제거
        if (currentBubble != null)
        {
            Destroy(currentBubble);
            currentBubble = null;
        }

        // 새로운 프리팹 할당
        GameObject prefab = GetBubblePrefabForState(currentState);
        if (prefab != null)
        {
            currentBubble = Instantiate(prefab, transform);
            PositionBubble();

            // 3초 후 자동 제거 코루틴 시작
            StopAllCoroutines();
            StartCoroutine(DestroyBubbleAfterDelay(bubbleLifeSeconds));
        }
    }

    private IEnumerator DestroyBubbleAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (currentBubble != null)
        {
            Destroy(currentBubble);
            currentBubble = null;
        }
        currentState = null; // 상태 해제 여부는 필요에 따라 조정
    }

    private GameObject GetBubblePrefabForState(string state)
    {
        if (state == null) return null;
        foreach (var data in stateBubbles)
        {
            if (data.stateName == state) return data.bubblePrefab;
        }
        return null;
    }

    private void LateUpdate()
    {
        PositionBubble();
    }

    private void PositionBubble()
    {
        if (currentBubble == null) return;

        // NPC의 중심 위치를 기준으로 말풍선 위치를 계산
        Vector3 basePos = transform.position;
        Vector3 bubblePos = basePos + Vector3.up * (headHeight + bubbleOffsetUp);

        // 필요 시 앞쪽으로 살짝 이동해 가독성 향상
        bubblePos += transform.forward * 0.15f;

        currentBubble.transform.position = bubblePos;

        // 말풍선이 카메라를 바라보게 하려면 주석 해제
        if (Camera.main != null)
        {
            currentBubble.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }

        // 필요시 SortingOrder 조정 가능
        // SpriteRenderer sr = currentBubble.GetComponent<SpriteRenderer>();
        // if (sr != null) sr.sortingOrder = 100;
    }
}
