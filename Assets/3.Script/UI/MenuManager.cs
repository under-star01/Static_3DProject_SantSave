using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TitleMenuManager : MonoBehaviour
{
    [SerializeField] private Button startButton; // 스타트메뉴 넣기
    [SerializeField] private Button optionButton; // 옵션메뉴 넣기
    [SerializeField] private Button exitButton; // 종료메뉴 넣기

    private OptionPanelController optionPanelController;

    private void Start()
    {
        // Canvas에서 OptionPanelController 찾기
        optionPanelController = FindObjectOfType<OptionPanelController>();

        // 각 버튼에 공통 컨트롤러 초기화
        // 스타트
        var sc = startButton.gameObject.GetComponent<ButtonController>();
        if (sc == null)
        {
            sc = startButton.gameObject.AddComponent<ButtonController>();
        }
            sc.Initialize("START", OnStartClicked); // 클릭시 이벤트 발생

        var oc = optionButton.gameObject.GetComponent<ButtonController>();
        if (oc == null)
        {
            oc = optionButton.gameObject.AddComponent<ButtonController>();
        }

        oc.Initialize("OPTION", OnOptionClicked); // 클릭시 이벤트 발생

        var ec = exitButton.gameObject.GetComponent<ButtonController>();
        if (ec == null)
        {
            ec = exitButton.gameObject.AddComponent<ButtonController>();
        }
            ec.Initialize("EXIT", OnExitClicked); // 클릭시 이벤트 발생
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
        if (optionPanelController != null)
        {
            optionPanelController.OpenOptionPanel();         
        }
    }

    private void OnExitClicked()
    {   // 종료버튼 클릭 시 종료
        Debug.Log("Exit clicked");
        Application.Quit();
    }
}

