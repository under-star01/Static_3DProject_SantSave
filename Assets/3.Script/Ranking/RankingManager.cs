using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingManager : MonoBehaviour
{
    [Header("테스트용 디폴트 JSON (Title 씬에서만 사용 추천)")]
    [SerializeField] private bool useDefaultJsonInTitle = true;
    [SerializeField] private TextAsset defaultRankingJson; // ranking.json(TextAsset)을 여기에 드래그

    [Header("Ranking UI Root")]
    [SerializeField] private GameObject rankingUIRoot;         // Hierarchy의 RankingUI 루트(통째로 On/Off)

    [Header("ScrollView 연결")]
    [SerializeField] private RectTransform contentParent;
    [SerializeField] private GameObject rankingItemPrefab;

    [Header("이름 입력 패널 연결")]
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private TMP_InputField nameInputField;

    [Header("기타 UI")]
    [SerializeField] private TMP_Text noDataText;
    [SerializeField] private ScrollRect scrollRect;


    private List<RankingEntry> rankingList = new List<RankingEntry>();

    private List<RankingItemController> itemPool = new List<RankingItemController>();

    private const int NameMaxLength = 5;
    private int pendingNameIndex = -1;

    [Serializable]
    public class RankingListWrapper
    {
        public RankingEntry[] entries;
    }

    private void Awake()
    {
        if (nameInputField != null)
        {
            nameInputField.characterLimit = NameMaxLength;
        }

        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(false);
        }

        // 타이틀 시작 시 랭킹창은 꺼두고 버튼으로 열기
        if (rankingUIRoot != null)
        {
            rankingUIRoot.SetActive(false);
        }
    }

    // RankingButton에서 이 함수를 OnClick으로 호출.
    public void ToggleRankingUI()
    {
        if (rankingUIRoot == null)
        {
            return;
        }

        bool next = !rankingUIRoot.activeSelf;
        rankingUIRoot.SetActive(next);

        if (next == true)
        {
            // 랭킹창을 열 때: 입력 패널은 기본적으로 닫고, UI 새로고침
            if (nameInputPanel != null)
            {
                nameInputPanel.SetActive(false);
            }
            pendingNameIndex = -1;

            RefreshUI();
        }
        else
        {
            // 랭킹창을 닫을 때 입력 패널도 같이 닫기
            if (nameInputPanel != null)
            {
                nameInputPanel.SetActive(false);
            }
            pendingNameIndex = -1;
        }
    }

    [ContextMenu("DEBUG/Load From Default JSON")]
    public void LoadFromDefaultJson()
    {
        if (defaultRankingJson == null)
        {
            Debug.LogWarning("[Ranking] defaultRankingJson이 비어있습니다. TextAsset을 인스펙터에 넣어주세요.");
            rankingList.Clear();
            return;
        }

        Debug.Log("[Ranking] Default JSON Loaded. bytes=" + defaultRankingJson.bytes.Length);

        RankingListWrapper wrapper = JsonUtility.FromJson<RankingListWrapper>(defaultRankingJson.text);
        rankingList.Clear();

        if (wrapper == null || wrapper.entries == null)
        {
            Debug.LogWarning("[Ranking] JSON 파싱 실패 또는 entries가 null 입니다.");
            return;
        }

        for (int i = 0; i < wrapper.entries.Length; i++)
        {
            if (wrapper.entries[i] != null)
            {
                rankingList.Add(wrapper.entries[i]);
            }
        }

        Debug.Log("[Ranking] entries count = " + rankingList.Count);

        // TODO: 여기서 SortRanking() 호출(점수 내림차순, 동점 날짜 내림차순)
        // SortRanking();
    }

    [ContextMenu("DEBUG/Print Paths")]
    public void DebugPrintPaths()
    {
        Debug.Log("[Ranking] streamingAssetsPath = " + Application.streamingAssetsPath);
        Debug.Log("[Ranking] persistentDataPath  = " + Application.persistentDataPath);
        Debug.Log("[Ranking] dataPath           = " + Application.dataPath);
    }

    public void RefreshUI()
    {
        // 기존 아이템 제거
        // Destroy로 전부 지우면, 에디터(Inspector)가 선택 중인 오브젝트가 사라져서
        // UnityEditor 쪽 NullReference / SerializedObjectNotCreatableException이 발생함.
        // 그래서 "삭제"가 아니라 "재사용(풀링)" 방식으로 변경했음!(빨간 에러가 에디터 쪽이라 문제는 없지만, 거슬려서..)
        Debug.Log("[Ranking] RefreshUI called. rankingList.Count = " + rankingList.Count);
        Debug.Log("[Ranking] contentParent = " + (contentParent != null ? contentParent.name : "NULL") + ", prefab = " + (rankingItemPrefab != null ? rankingItemPrefab.name : "NULL"));

        // 데이터 없는 경우 처리
        if (noDataText != null)
        {
            // 이름 입력 패널이 켜져 있으면 NoData_Text는 무조건 숨김(겹침 방지)
            if (nameInputPanel != null && nameInputPanel.activeSelf == true)
            {
                noDataText.gameObject.SetActive(false);
            }
            else
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
        }

        // 정렬된 리스트를 순회하여 프리팹 생성
        // 필요한 만큼 아이템을 확보(없으면 생성), 있으면 재사용
        for (int i = 0; i < rankingList.Count; i++)
        {
            RankingItemController ctrl = null;

            if (i < itemPool.Count)
            {
                ctrl = itemPool[i];
            }
            else
            {
                GameObject item = Instantiate(rankingItemPrefab, contentParent);
                ctrl = item.GetComponent<RankingItemController>();
                if (ctrl == null)
                {
                    Debug.LogError("rankingItemPrefab에 RankingItemController가 없습니다.");
                    Destroy(item);
                    return;
                }
                itemPool.Add(ctrl);
            }

            ctrl.gameObject.SetActive(true);

            RankingEntry e = rankingList[i];
            ctrl.SetData(i + 1, e);
        }

        // 남는 아이템은 비활성화
        for (int i = rankingList.Count; i < itemPool.Count; i++)
        {
            if (itemPool[i] != null)
            {
                itemPool[i].gameObject.SetActive(false);
            }
        }

        // 스크롤 설정은 ScrollView가 자동으로 처리
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    // Top10 진입 시 호출(인덱스는 정렬된 rankingList 기준)
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
        if (noDataText != null)
        {
            // 입력 패널 켜면 겹침 방지로 NoData 숨김
            noDataText.gameObject.SetActive(false);
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
        if (pendingNameIndex < 0 || pendingNameIndex >= rankingList.Count)
        {
            if (nameInputPanel != null)
            {
                nameInputPanel.SetActive(false);
            }
            pendingNameIndex = -1;
            RefreshUI();
            return;
        }

        string inputName = "";
        if (nameInputField != null)
        {
            inputName = nameInputField.text;
        }

        if (string.IsNullOrWhiteSpace(inputName) == true)
        {
            inputName = "NONAME";
        }

        if (inputName.Length > NameMaxLength)
        {
            inputName = inputName.Substring(0, NameMaxLength);
        }

        rankingList[pendingNameIndex].name = inputName;

        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(false);
        }

        pendingNameIndex = -1;

        RefreshUI();

        // TODO: SaveRanking() 연결
    }
}
