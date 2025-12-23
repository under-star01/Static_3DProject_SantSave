using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WakeUpManager : MonoBehaviour
{
    public static WakeUpManager instance;

    [Header("UI 설정")]
    public Slider WakeUpSlider;

    [Header("게이지 설정")]
    public float maxGauge = 100f;
    private float currentGauge = 0f;

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
        currentGauge += amount;

        if (currentGauge >= maxGauge)
        {
            currentGauge = maxGauge;
            WakeUp();
        }
    }

    void WakeUp()
    {
        Debug.Log("Wake Up!");
    }
}
