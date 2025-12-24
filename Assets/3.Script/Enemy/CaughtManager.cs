using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaughtManager : MonoBehaviour
{
    public static CaughtManager instance;

    public RankingManager rankingManager;

    [Header("UI 설정")]
    public Slider CaughtSilder;

    [Header("게이지 설정")]
    public float maxGauge = 100f;
    public float drainSpeed = 10f;
    private float currentCauge = 0f;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        rankingManager = GetComponent<RankingManager>();
    }

    private void Update()
    {
        if(currentCauge > 0)
        {
            currentCauge -= Time.deltaTime * drainSpeed;
        }

        if(CaughtSilder != null)
        {
            CaughtSilder.value = currentCauge;
            CaughtSilder.gameObject.SetActive(currentCauge > 0.1f);
        }
    }

    public void AddCaught(float amount)
    {
        currentCauge += amount;

        if(currentCauge >= maxGauge)
        {
            currentCauge = maxGauge;
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0;
        rankingManager.ProcessNewScore(500);
    }

}
