using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance = null;

    //[SerializeField] private Light sun;
    [SerializeField] private Light moon;
    [SerializeField] private Light all;
    [SerializeField] private Light sun;
    [SerializeField] private Transform window;


    [SerializeField] private float totalTime = 300f; //전체 시간

    public float currentTime;// 현재시간 0이되면 게임오버
    public bool isTimer = true; // 타이머가 작동 중인지 여부

    // 시간 종료 이벤트
    public event Action OnTimeEnd;

    private Color nightColor = new Color(0.1f, 0.1f, 0.4f);
    private Color sunriseColor = new Color(0.5f, 0.5f, 0.4f);
    private bool isTimeEnd = false; // 이벤트 중복 방지

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        currentTime = totalTime;
        all.intensity = 5f;
        all.color = nightColor;
        sun.intensity = 0f;
        sun.enabled = false;
        isTimeEnd = false;
    }

    private void Update()
    {
        // 타이머가 작동 중일 때만 실행
        if (!isTimer) return;

        // 시간 누적 및 GameOver 판정
        currentTime -= Time.deltaTime;

        if (currentTime <= 0f && !isTimeEnd)
        {
            IsTimeOver();
            return;
        }

        // 햇빛, 달빛 Light 조정
        float normalizedTime = 1f - (currentTime / totalTime); //전체시간을 0~1로 변환한 시간

        if (normalizedTime < 0.6f) //시간이 60퍼가 지나기전
        {
            MoonMove();
        }
        else //60퍼가 지나면
        {
            Sunrise((normalizedTime - 0.6f) / 0.4f); // 남은시간을 0~1로 변환
        }
    }

    private void MoonMove()
    {
        moon.transform.LookAt(window);
        moon.transform.position += Vector3.right * 0.1f * Time.deltaTime;
    }

    private void Sunrise(float time)
    {
        //Debug.Log("일출");

        time = Mathf.Clamp01(time);

        // 전체빛 색 변화
        all.color = Color.Lerp(nightColor, sunriseColor, time);

        // 달빛은 서서히 끔
        moon.intensity = Mathf.Lerp(moon.intensity, 0f, Time.deltaTime);

        // 햇빛 온
        sun.enabled = true;
        sun.transform.LookAt(window);
        sun.intensity = Mathf.Lerp(sun.intensity, 50f, time);
        sun.transform.position += Vector3.up * 0.3f * Time.deltaTime;
    }

    public float GetTotalTime()
    {
        return totalTime;
    }

    // 시간 오버로 인한 게임 종료 메소드
    private void IsTimeOver()
    {
        currentTime = 0f;
        isTimeEnd = true;
        Debug.Log("시간 종료!");
        isTimer = false;
        GameManager.instance.isTimeOver = true; // 시간 오버로 인한 게임종료
        GameManager.instance.gameOver?.Invoke() ; // 게임 오버 이벤트 발생
    }
}
