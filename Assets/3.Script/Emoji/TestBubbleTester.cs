using UnityEngine;
using System.Collections;

public class TestBubbleTester : MonoBehaviour
{
    public TalkBubbleController bubbleController;

    // 테스트용 순서: Idle -> Move
    private string[] testStates = new string[] { "Idle", "Move" };
    private int index = 0;
    public float intervalBetweenStates = 2f; // 상태 간 전환 간격

    void Start()
    {
        if (bubbleController == null)
        {
            Debug.LogError("TalkBubbleController를 연결해 주세요.");
            return;
        }
        // 초기 상태
        StartCoroutine(RunTestSequence());
    }

    IEnumerator RunTestSequence()
    {
        // 초기 상태
        yield return new WaitForSeconds(0.5f);
        TriggerNextState();

        // 순환 테스트: Idle, Move 두 상태를 번갈아 가며 3초짜리 지속 확인
        for (int i = 0; i < 4; i++) // 4회 반복으로 충분한 테스트
        {
            yield return new WaitForSeconds(intervalBetweenStates);
            TriggerNextState();
        }
    }

    void TriggerNextState()
    {
        string nextState = testStates[index % testStates.Length];
        index++;
        bubbleController.OnStateChanged(nextState);
        Debug.Log("TestBubbleTester: StateChanged -> " + nextState);
    }
}
