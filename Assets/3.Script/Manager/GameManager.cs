using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public Action gameOver; // 게임 종료 이벤트

    public bool isAllCorrect = false; // 전체 정답 여부
    public bool isTimeOver = false; // 타임 오버 여부
    public int submitCnt = 0; // 제출한 선물 개수

    [SerializeField] private CaughtManager caughtManager;
    [SerializeField] private ChildManager childManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private RankingManager rankingManager;

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Canvas canvas;
    [SerializeField] private List<VideoClip> videoClips;

    private int clipIndex; // 재생할 영상 Index (0: AllCorrect, 1: DisCorrect, 2: TimeOver)

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

        // Manager 일괄 초기화

        if (caughtManager != null)
        {
            // CaughtManager 초기화
            caughtManager.InitializeData();
        }

        if (childManager != null)
        {
            // ChildManager 초기화
            childManager.InitializeData();
        }

        if (scoreManager != null)
        {
            // ChildManager 초기화
            scoreManager.InitializeData();
        }
    }

    private void OnEnable()
    {
        gameOver += GameOver;
    }

    private void OnDisable()
    {
        gameOver -= GameOver;
    }

    // 게임 종료 메소드
    public void GameOver()
    {
        // 상태에 따른 영상 설정
        if (!isTimeOver)
        {
            // 정답을 모두 맞췄을 경우, AllCorrect 영상 재생
            if (isAllCorrect)
            {
                clipIndex = 0;
            }
            // 오답이 하나라도 있을 경우, DisCorrect 영상 재생
            else
            {
                clipIndex = 1;
            }
        }
        // 시간 오버로 인한 게임 오버의 경우, TimeOver 영상 재생
        else
        {
            clipIndex = 2;
        }

        // GameOve UI 코루틴 실행
        if (canvas != null && videoPlayer != null)
        {
            StartCoroutine(GameOverUI(videoClips[clipIndex]));
        }
        else
        {
            Debug.Log("GameManager애 Canvas와 VideoPlayer를 연결해주세요!");
        }
    }

    // 영상 재생 후 랭킹 표시 코루틴
    private IEnumerator GameOverUI(VideoClip clip)
    {
        // 암전 효과 On
        yield return UIManager.instance.ActiveBlackOut_co(true, 1f);

        // 영상 재생
        videoPlayer.gameObject.SetActive(true);
        videoPlayer.Stop();
        videoPlayer.clip = clip;
        videoPlayer.Play();
        yield return new WaitForSeconds(0.25f);

        // UI 비활성화;
        foreach (Transform child in canvas.transform)
        {
            child.gameObject.SetActive(false);
        }

        // 재생이 끝날때까지 대기
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        // Ranking UI 표시
        UIManager.instance.ShowRankingUI(true);
        if (rankingManager != null)
        {
            // 점수 저장 -> 내용 적용 메소드 실행
        }
    }
}
