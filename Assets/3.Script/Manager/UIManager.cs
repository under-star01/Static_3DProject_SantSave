using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    [SerializeField] private GameObject checkList;          // 선물 목록 UI
    [SerializeField] private List<GameObject> polaroid_List = new(); // 폴라로이드 UI 리스트
    [SerializeField] private List<Slider> skillUI_List = new();      // 스킬 Slider 리스트
    [SerializeField] private TextMeshProUGUI scoreUI;                // 점수 Text UI 
    [SerializeField] private TextMeshProUGUI addScoreUI;             // 추가 점수 Text UI 
    [SerializeField] private Color changeUIColor;                    // 변경시 UI 색상
    
    private Dictionary<Skill, Image> skillFillImages = new(); // 색상을 변경 Skill Image 딕셔너리
    private int spawnCnt = 0; // 활성화 폴라로이드 UI 개수 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Skill UI의 Image를 미리 캐싱해둠!
        for (int i = 0; i < skillUI_List.Count; i++)
        {
            Transform fill = skillUI_List[i].transform.Find("Fill Area/Fill");
            if (fill != null)
                skillFillImages[(Skill)i] = fill.GetComponent<Image>();
        }
    }

    public void SetScore(int score, int addScore)
    {
        if(scoreUI != null && addScoreUI != null)
        {
            // 점수 적용
            scoreUI.text = $"Score : {score}";

            // 점수 추가 연출
            StartCoroutine(ShowAddScore(addScore, 0.5f));
        }
        else
        {
            Debug.Log("점수 UI 연결하세요!");
        }
    }

    // 점수 추가 연출 메소드
    private IEnumerator ShowAddScore(int score, float duration)
    {
        // UI 상태 초기화 및 활성화
        Color color = addScoreUI.color;
        color.a = 1f;
        addScoreUI.color = color;
        addScoreUI.text = score.ToString();
        yield return new WaitForSeconds(1f);

        // 사라지는 효과
        float elapsed = 0f;

        while (elapsed < duration)
        {
            color.a = (duration - elapsed) / duration;
            addScoreUI.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 마지막으로 결과 값을 보정
        color.a = 0f;
        addScoreUI.color = color;
    }


    public void Init(ChildType_SO childData, GiftType_SO giftData)
    {
        // 폴라로이드 UI 리스트가 비어있으면 리턴
        if (polaroid_List.Count == 0) return;

        if(polaroid_List[spawnCnt] != null)
        {
            polaroid_List[spawnCnt].SetActive(true);
            Texture2D childImage = polaroid_List[spawnCnt].transform.GetChild(0).GetComponent<Texture2D>();
            TextMeshProUGUI childIName = polaroid_List[spawnCnt].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI childIWish = polaroid_List[spawnCnt].transform.GetChild(2).GetComponent<TextMeshProUGUI>();

            childImage = childData.childImage;
            childIName.text = childData.childName;
            childIWish.text = giftData.coment;

            // 해당 데이터에서 필요한 부분 연결하면 돼.
            // 밑에 Canvas 구조를 정리해서 자식 순서로 요소를 연결하자.
            // 연결할 UI : 아이들 Image / 아이 이름 / 선물 관련 코멘트
            Debug.LogWarning($"{childData.childName} / {giftData.giftName} 내용을 선물 목록 UI에 추가합니다.");

            spawnCnt++;
        }
    }

    // 완료 CheckList 비활성화 메소드
    public void DeActivatePolarroid(int index)
    {
        // 이건 임시로만 해둔거야. 색깔을 흐리게 하면 좋을 것 같아.
        polaroid_List[index].SetActive(false);
    }

    // 변신 스킬 쿨타임 UI 적용 메소드
    public void CoolTimeStart(Skill index, float duration)
    {
        // 스킬 UI 리스트가 비어있으면 리턴
        if (skillUI_List.Count == 0) return;

        StartCoroutine(CoolTimeStart_co(index, duration));
    }

    // 변신 스킬 쿨타임 UI 적용 코루틴
    private IEnumerator CoolTimeStart_co(Skill index, float duration)
    {
        Slider skillUI = skillUI_List[(int)index];
        float elapsed = 0f;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            skillUI.value = elapsed / duration;
            yield return null;
        }
    }

    // Skill UI 색상 변경 메소드
    public void ChangeSkillColor(Skill index, bool isChange)
    {
        // 연결된 Image가 없다면 리턴
        if (!skillFillImages.TryGetValue(index, out var img)) return;
        
        // 색상 적용
        img.color = isChange ? changeUIColor : Color.white;
    }
    
    // 선물 CheckLIst 활성화 메소드
    public void ShowCheckList(bool isActive)
    {
        checkList.SetActive(isActive);
    }


}
