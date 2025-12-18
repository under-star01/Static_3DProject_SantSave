using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        if (currentGauge >= maxGauge)
        {
            currentGauge = maxGauge;
            GameOver();
        }
    }

    void GameOver()
    {
        //임시 게임오버
        Debug.Log("Game Over!");
        Time.timeScale = 0;
    }

}
