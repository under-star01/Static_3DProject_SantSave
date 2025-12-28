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

    [SerializeField] private CaughtManager caughtManager;
    [SerializeField] private ChildManager childManager;
    [SerializeField] private ScoreManager scoreManager;

    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Canvas canvas;
    [SerializeField] private List<VideoClip> videoClips;

    private int clipIndex; // 재생할 영상 Index (0: AllCorrect, 1: DisCorrect, 2: TimeOver)

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

        // Manager 일괄 초기화
        
        if(caughtManager != null)
        {
            // CaughtManager 초기화
            caughtManager.InitializeData();
        }
        
        if(childManager != null)
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
        // 굴둑 탈출로 인한 게임 오버 -> 영상 재생 / 점수 계산 -> 점수 표시
        if (!isTimeOver)
        {
            // 정답을 모두 맞췄을 경우, 영상 재생
            if (isAllCorrect)
            {
                clipIndex = 0;
            }
            // 오답이 하나라도 있을 경우, 영상 재생
            else
            {
                clipIndex = 1;
            }
        }
        // 시간 오버로 인한 게임 오버 -> 영상 재생 / 점수 0점 -> 점수 표시
        else
        {
            clipIndex = 2;
        }

        // Canvas 비활성화 및 클립 영상 재생
        if (canvas != null && videoPlayer != null)
        {
            canvas.gameObject.SetActive(false);
            
            videoPlayer.gameObject.SetActive(true);
            StartCoroutine(GameOverUI(videoClips[clipIndex]));
        }
    }

    // 영상 재생 후 랭킹 표시 코루틴
    private IEnumerator GameOverUI(VideoClip clip)
    {
        // 영상 재생
        videoPlayer.Stop();
        videoPlayer.clip = clip;
        videoPlayer.Play();

        // 재생이 끝날때까지 대기
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        // 데이터 저장 내용 여기에 넣으면 될 것 같아!

        // Ranking UI 표시
        //rankingUI.SetActive(true);
    }
}
