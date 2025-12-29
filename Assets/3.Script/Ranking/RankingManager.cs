using System;
using System.Linq;
using System.IO;
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

    private void Start()
    {
        LoadRanking();
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

        SaveRanking();  // 이름 확정 후 즉시 저장
        pendingNameIndex = -1;
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(false);
        }
        RefreshUI();
    }

    // Order By Linq (SQL 문법 중 하나인 Order By 쿼리문을 VS에서 사용할 수 있음)
    public void SortRanking()
    {
        // 랭킹 리스트의 값을 내림차순(Descending)으로 설정. 점수가 같다면 날짜 순으로 정렬
        rankingList = rankingList.OrderByDescending(e => e.score).ThenByDescending(e => {
            // 문자열을 실제 DateTime 객체로 변환하여 비교 (포맷이 바뀌어도 안전함)
            if (DateTime.TryParse(e.date, out DateTime parsedDate))
                return parsedDate;
            return DateTime.MinValue;
        }).ToList();
    }

    public void SaveRanking()
    {
        // root배열인 json 파일을 파싱하기 위한 wrapper을 선언
        RankingListWrapper wrapper = new RankingListWrapper();

        // jsonUtility 사용을 위해 파싱 할 데이터를 배열에 저장
        wrapper.entries = rankingList.ToArray();
        
        // Json 문자열로 변환하여 넣어줌
        string json = JsonUtility.ToJson(wrapper, true);

        // 3. 저장할 파일 경로 설정
        string path = Path.Combine(Application.persistentDataPath, "ranking.json");

        // Unity의 대표적인 저장방식 dataPath와 persistentDataPath 중 persistentDataPath 사용
        // 위 방법을 사용하면 Application.persistentDataPath 경로로 사용중인 OS의 쓰기 권한이 있는 안전한 폴더로 연결해줌
        // 데이터 저장 흐름 : 데이터 리스트 -> Wrapper 클래스 -> Json 문자열 변환(파싱) -> 파일 저장 (직렬화 및 쓰기)
        File.WriteAllText(path, json);
    }

    public void LoadRanking()
    {
        string path = Path.Combine(Application.persistentDataPath, "ranking.json");

        // 파일이 존재하는지 확인
        if (File.Exists(path))
        {
            // 랭킹 데이터 파일에서 Json 문자열을 읽어 들여옴
            string json = File.ReadAllText(path);

            // 텍스트 형태의 Json 데이터를 다시 C# 객체(object)로 변환 (역직렬화)
            RankingListWrapper wrapper = JsonUtility.FromJson<RankingListWrapper>(json);

            // 변환한 데이터를 리스트에 담음
            // rankingList 변수를 통해 랭킹 파일에 저장된 데이터에 접근 할 수 있음
            rankingList = new List<RankingEntry>(wrapper.entries);

            Debug.Log("[Ranking] 로컬 파일로부터 데이터 로드 완료.");
        }
        else
        {
            Debug.Log("[Ranking] 저장된 파일이 없습니다. 기본 데이터를 로드합니다.");
            LoadFromDefaultJson(); // 파일이 없으면 에디터에서 설정한 기본값 로드
        }
    }

    public void ProcessNewScore(int newScore)
    {
        // 1. 임시 이름으로 새로운 데이터 생성 (날짜는 현재 시간)
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        RankingEntry newEntry = new RankingEntry("TEMP_PLAYER", newScore, currentDate);

        // 2. 리스트에 추가
        rankingList.Add(newEntry);

        // 3. 정렬 (점수 내림차순, 점수 같으면 날짜 내림차순)
        SortRanking();

        // 4. 최대 랭킹 개수 유지 (예: 10등까지만 보관)
        if (rankingList.Count > 10)
        {
            rankingList.RemoveRange(10, rankingList.Count - 10);
        }

        // 5. 새 점수가 몇 위인지 확인
        int rankIndex = rankingList.IndexOf(newEntry);

        // 6. 만약 Top 10 진입했다면 이름 입력창 띄우기
        if (rankIndex >= 0 && rankIndex < 10)
        {
            OpenNameInputForIndex(rankIndex);
        }
        else
        {
            // Top 10이 아니면 바로 저장
            SaveRanking();
            RefreshUI();
        }
    }

    [ContextMenu("DEBUG/Clear Ranking Data")]
    public void ClearRankingData()
    {
        rankingList.Clear();
        string path = Path.Combine(Application.persistentDataPath, "ranking.json");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        RefreshUI();
        Debug.Log("모든 랭킹 데이터가 삭제되었습니다.");
    }

    [ContextMenu("DEBUG/Add Dummy Data")]
    public void AddDummyData()
    {
        rankingList.Add(new RankingEntry("ALICE", 1500, "2025-12-25 10:00"));
        rankingList.Add(new RankingEntry("BOB", 3000, "2025-12-26 11:30"));
        SortRanking();
        SaveRanking();
        RefreshUI();
    }
}
