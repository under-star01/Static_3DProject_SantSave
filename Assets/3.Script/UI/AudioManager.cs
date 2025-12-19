using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")] // 직렬화
    [SerializeField] private AudioMixer audioMixer;

    // Mixer Group 이름 (Audio Mixer에 설정한 이름과 동일해야 인식됨)
    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";
    private const string SYSTEM_VOLUME = "SystemVolume";

    private void Awake()
    {
        // 싱글톤 패턴
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
    }

    private void Start()
    {
        // 저장된 볼륨 로드
        LoadVolumes();
    }

    // 볼륨 설정 (0~1 범위를 -80~0 dB로 변환)
    public void SetMusicVolume(float volume)
    {
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f; // 볼륨 조절의 선형값을 데시벨 단위로 변환
        // 볼륨의 값이 0.0001보다 크면 *20을, 아니면 * -80을 적용함
        // volume 0.5 = Log10(0.5) = -0.30103... *20 = -6.02 dB (최대볼륨의 절반값)
        // volume 1 = Log10(1.0) = 0 * 20 = 0dB (최대 볼륨)
        // volume의 값이 0에 수렴한다고 볼 수 있는 0.0001f보다 작을 경우 -80dB(최대 볼륨의 약 1억분의 1)로 설정함
        // why? Log10(0)은 음의 무한대 값이 되어 오류가 발생하기 때문에 최대치에 제한을 둠.

        audioMixer.SetFloat(MUSIC_VOLUME, dB); // 오디오 믹서의 특정 값에 변환된 데시벨을 실제로 적용

        PlayerPrefs.SetFloat(MUSIC_VOLUME, volume); //변환한 볼륨 슬라이더의 값을 저장(껐다 켜도 볼륨 설정 유지)

        PlayerPrefs.Save(); // 변경 사항을 즉시 저장하여 조절하다 튕기거나 기타 사항에서도 유지됨
    }

    public void SetSFXVolume(float volume)
    {
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat(SFX_VOLUME, dB);
        PlayerPrefs.SetFloat(SFX_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public void SetSystemVolume(float volume)
    {
        float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat(SYSTEM_VOLUME, dB);
        PlayerPrefs.SetFloat(SYSTEM_VOLUME, volume);
        PlayerPrefs.Save();
    }

    // 이하는 모두 현재 설정되어 있던 볼륨 가져오기
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
}
