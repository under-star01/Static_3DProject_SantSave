using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WakeUpManager : MonoBehaviour
{
    public static WakeUpManager instance;

    [Header("UI 설정")]
    public Slider WakeUpSlider;
    public TextMeshProUGUI WakeUpCnt;

    [Header("게이지 설정")]
    public float maxGauge = 100f;
    private float currentGauge = 0f;

    [Header("기상 관련 변수")]
    public int wakeUpCnt = 1;
    public int wakeUpMaxCnt;
    private bool isMaxCnt = false; // 최대 아이 수 도달 여부
    private bool isWakeCool = false; // 기상 후, 간격 설정

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

    private void Start()
    {
        // 최대 아이 수 설정
        wakeUpMaxCnt = ChildManager.instance.spawnChild_List.Count;
    }

    private void Update()
    {
        // 슬라이더의 value값에 게이지 값을 넣어줌
        if (WakeUpSlider != null)
        {
            WakeUpSlider.value = currentGauge;
        }
    }

    public void AddAlertness(float amount)
    {
        if (isWakeCool || isMaxCnt) return;

        currentGauge += amount;

        if (currentGauge >= maxGauge)
        {
            currentGauge = maxGauge;
            
            WakeUp();
        }
    }

    void WakeUp()
    {   
        // 일어난 아이 수 및 UI 갱신
        wakeUpCnt++;
        WakeUpCnt.text = $"*{wakeUpCnt}";

        // IsSleeping 상태인 아이중, 한 명을 깨우는 메소드
        ChildManager.instance.WakeChild();

        // 최대 아이수에 도달했을 경우 게이지 증가 중단
        if (wakeUpCnt >= wakeUpMaxCnt)
        {
            isMaxCnt = true;
            return;
        }

        // 깨우고 난 후, 잠시 게이지가 오르지 않음
        StartCoroutine(WakeUpCool(1f));
    }

    private IEnumerator WakeUpCool(float duration)
    {
        isWakeCool = true;
        yield return new WaitForSeconds(duration);

        currentGauge = 0f;
        isWakeCool = false;
    }
}
