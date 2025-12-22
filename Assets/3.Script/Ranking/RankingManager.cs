using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingManager : MonoBehaviour
{
    [Header("ScrollView 연결")]
    [SerializeField] private RectTransform contentParent;      // ScrollView/Viewport/Content
    [SerializeField] private GameObject rankingItemPrefab;     // RankingItem 프리팹

    [Header("이름 입력 패널 연결")]
    [SerializeField] private GameObject nameInputPanel;        // Panel_NameInput (처음엔 비활성)
    [SerializeField] private TMP_InputField nameInputField;    // Panel_NameInput/InputField_Name

    [Header("기타 UI")]
    [SerializeField] private TMP_Text noDataText;              // 데이터 없을 때 표시 텍스트
    [SerializeField] private ScrollRect scrollRect;            // ScrollView의 ScrollRect

    private List<RankingEntry> rankingList = new List<RankingEntry>();

    private const int NameMaxLength = 5;

    // 이름 입력을 적용해야 하는 대상 인덱스(Top10 진입한 새 기록)
    private int pendingNameIndex = -1;

    private void Awake()
    {
        if (nameInputField != null)
        {
            // 최대 5글자 입력 제한
            nameInputField.characterLimit = NameMaxLength;
        }

        if (nameInputPanel != null)
        {
            // 시작 시에는 꺼두기 (RankingButton 눌러 열거나, Top10 기록 시 켜기)
            nameInputPanel.SetActive(false);
        }
    }

    public void RefreshUI()
    {
        // 기존 기록 제거
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        // 데이터 없는 경우 처리
        if (noDataText != null)
        {
            if (rankingList.Count <= 0)
            {
                noDataText.gameObject.SetActive(true);
                noDataText.text = "랭킹 데이터가 없습니다.";
            }
            else
            {
                noDataText.gameObject.SetActive(false);
            }
        }

        // 정렬된 리스트를 순회하여 순서대로 프리팹 생성
        for (int i = 0; i < rankingList.Count; i++)
        {
            RankingEntry e = rankingList[i];

            GameObject item = Instantiate(rankingItemPrefab, contentParent);
            RankingItemController ctrl = item.GetComponent<RankingItemController>();
            if (ctrl != null)
            {
                // i + 1 : 순위(1부터)(리스트 위치를 순위로 빼야하므로 +1)
                ctrl.SetData(i + 1, e);
            }
        }

        // 스크롤 설정은 ScrollView가 자동으로 처리
        // 랭킹창을 열 때 항상 맨 위부터 보여줌
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    // Top10 진입 시 호출할 함수(게임 종료 후 점수 확정 시)
    // 여기서는 "Top10 진입한 새 기록"이라고 정하고 이름 입력 패널을 띄우는 부분만.
    public void OpenNameInputForIndex(int index)
    {
        if (index < 0)
        {
            return;
        }
        if (index >= rankingList.Count)
        {
            return;
        }
        if (index >= 10)
        {
            return;
        }

        pendingNameIndex = index;

        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);
        }
        if (nameInputField != null)
        {
            nameInputField.text = "";
            nameInputField.ActivateInputField();
        }
    }

    // 이름 입력 처리 예시
    public void OnConfirmNameInput()
    {
        // 입력 값 처리 및 랭킹 갱신 로직 연결
        if (pendingNameIndex < 0)
        {
            // 입력할 대상이 없는 상태
            if (nameInputPanel != null)
            {
                nameInputPanel.SetActive(false);
            }
            return;
        }
        if (pendingNameIndex >= rankingList.Count)
        {
            if (nameInputPanel != null)
            {
                nameInputPanel.SetActive(false);
            }
            pendingNameIndex = -1;
            return;
        }

        string inputName = "";
        if (nameInputField != null)
        {
            inputName = nameInputField.text;
        }

        // 공백/빈 문자열 방지: 최소 1글자 ("NONAME" 기본값 사용)
        if (string.IsNullOrWhiteSpace(inputName) == true)
        {
            inputName = "NONAME";
        }

        // TMP_InputField characterLimit이 있어도 혹시 모르니 한 번 더 컷
        if (inputName.Length > NameMaxLength)
        {
            inputName = inputName.Substring(0, NameMaxLength);
        }

        rankingList[pendingNameIndex].name = inputName;

        // 입력 패널 닫기
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(false);
        }

        pendingNameIndex = -1;

        // 이름 적용 후 UI 갱신
        RefreshUI();

        // TODO: 여기서 SaveRanking() 같은 저장 호출을 연결하면 됨
    }

    // 외부에서 데이터 세팅하려고 필요할 수 있음 (로더/게임매니저에서 주입)
    public void SetRankingList(List<RankingEntry> list)
    {
        rankingList = list;
        if (rankingList == null)
        {
            rankingList = new List<RankingEntry>();
        }
        RefreshUI();
    }
}
