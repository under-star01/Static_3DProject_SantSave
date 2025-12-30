using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUIManager : MonoBehaviour
{   
    [SerializeField] private GameObject selectLevelUI;
    [SerializeField] private GameObject settingUI;

    // 난이도 설정 UI 조절 메소드
    public void OnStartClicked()
    {
        AudioManager.Instance.PlayButtonSFX();
        if (selectLevelUI != null)
        {
            if (selectLevelUI.activeSelf)
            {
                selectLevelUI.gameObject.SetActive(false);
            }
            else
            {
                selectLevelUI.gameObject.SetActive(true);
            }
        }
    }

    // 설정창 UI 조절 메소드
    public void OnOptionClicked()
    {
        AudioManager.Instance.PlayButtonSFX();
        if (settingUI != null)
        {
            if (settingUI.activeSelf)
            {
                settingUI.gameObject.SetActive(false);
            }
            else
            {
                settingUI.gameObject.SetActive(true);
            }
        }
    }

    // Exit 버튼 호출 메소드
    public void OnExitClicked()
    {
        AudioManager.Instance.PlayButtonSFX();
#if UNITY_EDITOR
        // 에디터에서 실행 중일 때
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서 실행 중일 때
        Application.Quit();
#endif
    }

    // 씬 로드 메소드
    public void LoadTargetScene(string sceneName)
    {
        if (sceneName.Equals("Stage_Normal"))
        {
            AudioManager.Instance.PlayNormalBGM();
        }
        if (sceneName.Equals("Stage_Hard"))
        {
            AudioManager.Instance.PlayHardBGM();
        }

        SceneManager.LoadScene(sceneName);
    }
}
