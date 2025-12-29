using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // 싱글톤 패턴
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("오디오소스")]
    [SerializeField] private AudioSource bgmSource;      // BGM 전용
    [SerializeField] private AudioSource sfxSource;      // 효과음 전용
    [SerializeField] private AudioSource loopSfxSource;  // 루프 효과음 전용

    [Header("BGM")]
    [SerializeField] private AudioClip titleBGM;
    [SerializeField] private AudioClip SelectBGM;
    [SerializeField] private AudioClip stageBGM;

    [Header("효과음")]
    [SerializeField] private AudioClip ThrowSFX;
    [SerializeField] private AudioClip BellSFX;
    [SerializeField] private AudioClip FootstepSFX;
    [SerializeField] private AudioClip TransformSFX;
    [SerializeField] private AudioClip PickupSFX;
    [SerializeField] private AudioClip EnterSFX;
    [SerializeField] private AudioClip ClickSFX;
    [SerializeField] private AudioClip AlertSFX;
    [SerializeField] private AudioClip WakeupSFX;
    [SerializeField] private AudioClip DropSFX;


    [Header("UI 소리")]
    [SerializeField] private AudioClip menuSelectSFX;
    [SerializeField] private AudioClip ButtonSFX;

    // Mixer Group 이름
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";
    private const string SYSTEM_VOLUME = "SystemVolume";

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource 초기화
        if (bgmSource != null)
        {
            bgmSource.loop = true;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
        }

        if (loopSfxSource != null)
        {
            loopSfxSource.loop = true;
        }
    }

    private void Start()
    {
        // 저장된 볼륨 로드
        LoadVolumes();
    }

    #region 볼륨 설정 (Audio Mixer 기반)

    // 볼륨 설정 (0~1 범위를 -80~0 dB로 변환)
    public void SetMusicVolume(float volume)
    {
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        //audioMixer.SetFloat(MUSIC_VOLUME, dB);
        PlayerPrefs.SetFloat(MUSIC_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        //audioMixer.SetFloat(SFX_VOLUME, dB);
        PlayerPrefs.SetFloat(SFX_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public void SetSystemVolume(float volume)
    {
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        //audioMixer.SetFloat(SYSTEM_VOLUME, dB);
        PlayerPrefs.SetFloat(SYSTEM_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME, 0.8f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME, 0.8f);
    }

    public float GetSystemVolume()
    {
        return PlayerPrefs.GetFloat(SYSTEM_VOLUME, 0.8f);
    }

    private void LoadVolumes()
    {
        SetMusicVolume(GetMusicVolume());
        SetSFXVolume(GetSFXVolume());
        SetSystemVolume(GetSystemVolume());
    }

    #endregion

    #region BGM 제어

    // BGM 재생
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // BGM 정지
    public void StopBGM()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
    }

    // BGM 일시정지
    public void PauseBGM()
    {
        if (bgmSource != null) bgmSource.Pause();
    }

    // BGM 재개
    public void ResumeBGM()
    {
        if (bgmSource != null) bgmSource.UnPause();
    }

    // BGM 바로가기
    public void PlayTitleBGM() => PlayBGM(titleBGM);
    public void PlaySelectBGM() => PlayBGM(SelectBGM);
    public void PlayStageBGM() => PlayBGM(stageBGM);

    #endregion

    #region 효과음 제어

    // 효과음 재생 (기본)
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // 효과음 재생 (볼륨 조절)
    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale);
    }

    // 효과음 바로가기
    public void PlayThrowSFX() => PlaySFX(ThrowSFX);
    public void PlayBellSFX() => PlaySFX(BellSFX);
    public void PlayFootstepSFX() => PlaySFX(FootstepSFX);
    public void PlayTransformSFX() => PlaySFX(TransformSFX);
    public void PlayPickupSFX() => PlaySFX(PickupSFX);
    public void PlayEnterSFX() => PlaySFX(EnterSFX);
    public void PlayClickSFX() => PlaySFX(ClickSFX);
    public void PlayAlertSFX() => PlaySFX(AlertSFX);
    public void PlayWakeupSFX() => PlaySFX(WakeupSFX);
    public void PlayDropSFX() => PlaySFX(DropSFX);


    // UI 효과음 바로가기
    public void PlayMenuSelectSFX() => PlaySFX(menuSelectSFX);
    public void PlayButtonSFX() => PlaySFX(ButtonSFX);

    #endregion

    #region 루프 효과음 제어

    // 루프 효과음 재생 시작
    public void PlayLoopSFX(AudioClip clip)
    {
        if (loopSfxSource == null || clip == null) return;

        loopSfxSource.clip = clip;
        loopSfxSource.Play();
    }

    // 루프 효과음 정지
    public void StopLoopSFX()
    {
        if (loopSfxSource == null) return;
        loopSfxSource.Stop();
    }

    #endregion

    #region 유틸리티

    // 모든 소리 정지
    public void StopAll()
    {
        if (bgmSource != null) bgmSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
        if (loopSfxSource != null) loopSfxSource.Stop();
    }

    #endregion
}