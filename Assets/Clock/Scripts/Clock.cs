using UnityEngine;
using System.Collections;

public class Clock : MonoBehaviour
{
    // 시작 시간을 밤 10시로 설정
    public int minutes = 0;
    public int hour = 22; // 22시 (밤 10시)

    public GameObject pointerMinutes;
    public GameObject pointerHours;

    // TimeManager 참조
    private TimeManager timeManager;

    private float startTime = 22f; // 22시 (밤 10시)
    private float endTime = 6f; // 6시 (아침 6시)
    private float totalHours = 8f; // 22시부터 다음날 6시까지 = 8시간

    void Start()
    {
        // TimeManager 찾기
        timeManager = FindObjectOfType<TimeManager>();

        if (timeManager == null)
        {
            Debug.LogError("TimeManager를 찾을 수 없습니다!");
        }

        // 시작 시간을 밤 10시로 설정
        hour = 22;
        minutes = 0;
    }

    void Update()
    {
        if (timeManager == null) return;

        // TimeManager의 시간 진행도를 기반으로 시계 시간 계산
        float normalizedTime = 1f - (timeManager.currentTime / timeManager.GetTotalTime());
        normalizedTime = Mathf.Clamp01(normalizedTime);

        // 0~1 범위를 22시~6시(다음날) 범위로 변환
        float currentHourFloat = startTime + (normalizedTime * totalHours);

        // 24시를 넘으면 다음날로 (예: 25시 -> 1시)
        if (currentHourFloat >= 24f)
        {
            currentHourFloat -= 24f;
        }

        // 시, 분으로 분해
        hour = Mathf.FloorToInt(currentHourFloat);
        float remainingMinutes = (currentHourFloat - hour) * 60f;
        minutes = Mathf.FloorToInt(remainingMinutes);

        // 시계 바늘 각도 계산 (시계방향)
        float rotationMinutes = (360.0f / 60.0f) * minutes;
        float rotationHours = ((360.0f / 12.0f) * (hour % 12)) + ((360.0f / (60.0f * 12.0f)) * minutes);

        // 시계 바늘 회전 (양수로 시계방향)
        pointerMinutes.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationMinutes);
        pointerHours.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationHours);
    }
}