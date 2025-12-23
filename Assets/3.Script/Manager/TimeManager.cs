using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    //[SerializeField] private Light sun;
    [SerializeField] private Light moon;
    [SerializeField] private Transform moonLook;
    [SerializeField] private Light all;


    [SerializeField] private float totalTime = 60f;

    private float currentTime;
    private Color nightColor = new Color(0.2f, 0.2f, 0.5f);
    private Color sunriseColor = new Color(0.3f, 0.2f, 0.2f);


    private void Start()
    {
        currentTime = totalTime;
        all.intensity = 0.1f;
    }


    private void Update()
    {
        currentTime -= Time.deltaTime;

        float normalizedTime = 1f - (currentTime / totalTime);

        if (normalizedTime < 0.6f)
        {
            MoonMove();
        }
        else
        {
            Sunrise((normalizedTime - 0.6f) / 0.4f);
        }
    }

    private void MoonMove()
    {
        moon.transform.LookAt(moonLook);
        moon.transform.position += Vector3.right * 1f * Time.deltaTime;
    }

    private void Sunrise(float t)
    {
        Debug.Log("일출한다");
        
        t = Mathf.Clamp01(t);

        // 색 변화
        all.color = Color.Lerp(nightColor, sunriseColor, t);

        // 밝기 증가
        all.intensity = Mathf.Lerp(0.1f, 0.5f, t);

        // 달빛은 서서히 약해짐
        moon.intensity = Mathf.Lerp(moon.intensity, 0f, t);
    }
}
