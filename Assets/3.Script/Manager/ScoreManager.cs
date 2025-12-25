using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance = null;

    [Header("현재 점수")]
    // 플레이어 점수
    [SerializeField] private int score = 1000; 

    [Header("차감 점수 목록")]
    [SerializeField] private int wrongScore; // 오답시 차감 점수
    [SerializeField] private int caughtedScore; // 발각시 차감 점수
    [SerializeField] private int wakeScore; // 아이를 기상시 차감 점수

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // 데이터 초기화 및 생성 메소드
    public void InitializeData()
    {
        // 여기서 UI 연결해주면 될 것 같아!
    }

    // 점수 변경 메소드
    private void ApplyScore(int value)
    {
        // 점수 계산
        score += value;
        score = Mathf.Clamp(score, 0, 1000);

        // 변경 점수 UI 적용
        UIManager.instance.SetScore(score, value);
    }

    // 정답 선물 제출시 메소드
    public void OnCorrectSubmit()
    {
        Debug.Log("정답을 맞췄어용");
    }

    // 오답 선물 제출시 점수 차감 메소드
    public void OnWrongSubmit()
    {
        ApplyScore(-wrongScore);
    }

    // NPC에게 발견되었을 때 점수 차감 메소드
    public void OnCaughted()
    {
        ApplyScore(-caughtedScore);
    }

    // 아이를 깨웠을 때 점수 차감 메소드
    public void OnWakeChild()
    {
        ApplyScore(-wakeScore);
    }
}
