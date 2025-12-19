using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionPanelController : MonoBehaviour
{
    // 직렬화
    [Header("UI References")] // 옵션 창에 뜨는 것들 연결
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private Button closeButton;

    [Header("Sliders")] // 볼륨 조절 슬라이더들 연결
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider systemSlider;

    [Header("Volume Labels")] // 볼륨 조절 숫자들 연결
    [SerializeField] private TextMeshProUGUI musicValueText;
    [SerializeField] private TextMeshProUGUI sfxValueText;
    [SerializeField] private TextMeshProUGUI systemValueText;

    private void Start()
    {
        // 초기 비활성화
        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }

        // 닫기 버튼 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseOptionPanel);
        }

        // 슬라이더 이벤트 연결 3종
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (systemSlider != null)
        {
            systemSlider.onValueChanged.AddListener(OnSystemVolumeChanged);
        }
    }
 
    // 옵션 창 켜기 및 이전에 설정된 볼륨 불러오기
    public void OpenOptionPanel()
    {
        if (optionPanel != null)
        {
            optionPanel.SetActive(true);
            LoadCurrentVolumes();
        }
    }

    // 옵션창 닫기
    public void CloseOptionPanel()
    {
        if (optionPanel != null)
            optionPanel.SetActive(false);
    }

    // 저장되어있는 볼륨 불러오기
    private void LoadCurrentVolumes()
    {
        if (AudioManager.Instance == null)
        {
            // 처음 키는거면 그냥 켜져랏!
            return;
        }

        // 킨 적 있다면 저장된 볼륨 가져와랏! 그 값으로 업데이트 해라!
        if (musicSlider != null)
        {
            float musicVol = AudioManager.Instance.GetMusicVolume();
            musicSlider.value = musicVol;
            UpdateVolumeText(musicValueText, musicVol);
        }

        if (sfxSlider != null)
        {
            float sfxVol = AudioManager.Instance.GetSFXVolume();
            sfxSlider.value = sfxVol;
            UpdateVolumeText(sfxValueText, sfxVol);
        }

        if (systemSlider != null)
        {
            float sysVol = AudioManager.Instance.GetSystemVolume();
            systemSlider.value = sysVol;
            UpdateVolumeText(systemValueText, sysVol);
        }
    }

    // 볼륨 바뀌었다! 갱신해라!
    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }

        UpdateVolumeText(musicValueText, value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }

        UpdateVolumeText(sfxValueText, value);
    }

    private void OnSystemVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSystemVolume(value);
        }

        UpdateVolumeText(systemValueText, value);
    }

    // 볼륨의 텍스트들은 자동으로 0~1 값에 100을 곱하고 % 기호를 붙여줘라!
    private void UpdateVolumeText(TextMeshProUGUI text, float value)
    {
        if (text != null)
        {
            text.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }
}
