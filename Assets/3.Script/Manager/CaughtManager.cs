using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CaughtManager : MonoBehaviour
{
    public static CaughtManager instance;

    [Header("UI 설정")]
    public Slider CaughtSilder;

    [Header("게이지 설정")]
    public float maxGauge = 100f;
    public float drainSpeed = 10f;
    private float currentGauge = 0f;

    [Header("발각 관련 변수")]
    [SerializeField] private Transform returnPos;
    [SerializeField] private float caughtTime; // 발각시 증가될 시간
    private PlayerInput playerInput;
    private bool isCaught = false; // 잡힌 상태 여부
    private Coroutine returnRoutine; // 복귀 코루틴

    private void Awake()
    {
        // 싱글톤 구조
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
        }
    }

    public void InitializeData()
    {
        playerInput = FindAnyObjectByType<PlayerInput>();
    }

    private void Update()
    {
        // 게이지는 지속적으로 감소
        if (currentGauge > 0)
        {
            currentGauge -= Time.deltaTime * drainSpeed;
        }

        // 슬라이더의 value값에 게이지 값을 넣어줌
        if (CaughtSilder != null)
        {
            CaughtSilder.value = currentGauge;
            //게이지 값이 존재할 때만 슬라이더 표시
            CaughtSilder.gameObject.SetActive(currentGauge > 0.1f);
        }
    }

    public void AddCaught(float amount)
    {
        currentGauge += amount;

        if (currentGauge >= maxGauge && !isCaught)
        {
            isCaught = true;

            currentGauge = maxGauge;

            if(returnRoutine != null)
            {
                StopCoroutine(returnRoutine);
            }
            returnRoutine = StartCoroutine(ReturnToTree());
        }
    }

    // 1층 트리 복귀 메소드
    private IEnumerator ReturnToTree()
    {
        // 암전 효과 on + 시간 멈춤
        playerInput.enabled = false;
        TimeManager.instance.isTimer = false;
        yield return UIManager.instance.ActiveBlackOut_co(true, 1f);

        // 플레이어 위치 이동
        playerInput.gameObject.transform.position = returnPos.position;

        // 암전 효과 off
        yield return UIManager.instance.ActiveBlackOut_co(false, 1f);

        // 상태 복구 + 시간 복구
        playerInput.enabled = true;
        TimeManager.instance.isTimer = true;
        currentGauge = 0f;
        isCaught = false;

        // 시간 증가 및 점수 감소
        TimeManager.instance.currentTime -= caughtTime;
        ScoreManager.instance.OnCaughted();
    }
}
