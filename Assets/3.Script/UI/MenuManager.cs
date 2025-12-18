using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TitleMenuManager : MonoBehaviour
{
    [SerializeField] private Button startButton; // 스타트메뉴 넣기
    [SerializeField] private Button optionButton; // 옵션메뉴 넣기
    [SerializeField] private Button exitButton; // 종료메뉴 넣기

    private void Start()
    {
        // 각 버튼에 공통 컨트롤러 초기화
        // 스타트
        var startCtrl = startButton.gameObject.GetComponent<ButtonController>();
        if (startCtrl == null)
        { 
            startCtrl = startButton.gameObject.AddComponent<ButtonController>();
        }
        startCtrl.Initialize("START", OnStartClicked); // 스타트 버튼 클릭 이벤트 구독

        // 옵션
        var optionCtrl = optionButton.gameObject.GetComponent<ButtonController>();
        if (optionCtrl == null)
        { 
            optionCtrl = optionButton.gameObject.AddComponent<ButtonController>(); 
        }
        optionCtrl.Initialize("OPTION", OnOptionClicked); // 옵션 버튼 클릭 이벤트 구독

        // 종료
        var exitCtrl = exitButton.gameObject.GetComponent<ButtonController>();
        if (exitCtrl == null) 
        { 
            exitCtrl = exitButton.gameObject.AddComponent<ButtonController>(); 
        }
        exitCtrl.Initialize("EXIT", OnExitClicked); // 종료 버튼 클릭 이벤트 구독
    }

    private void OnStartClicked()
    {
        // 게임 씬 로드 (이름 확정되면 활성화하고 이름 변경)
        // SceneManager.LoadScene("GameScene");
        Debug.Log("Start clicked");
    }

    private void OnOptionClicked()
    {
        Debug.Log("Option clicked");
        // 옵션 화면 토글 로직
    }

    private void OnExitClicked()
    {   // 종료버튼 클릭 시 종료
        Debug.Log("Exit clicked");
        Application.Quit();
    }
}

