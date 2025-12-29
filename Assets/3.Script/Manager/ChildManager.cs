using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable] // Random 때문에 겹쳐서 키워드 추가
public class BedData
{
    public GameObject Bed; // 연결할 침대 오브젝트
    public Transform spawnPoint; // 생성 위치
    public Transform summitPos; // 선물을 놓을 위치
    public List<Transform> patrolPoints; // 패트롤할 위치 리스트
    public int spawnIndex; // 생성 순서
}

public class ChildManager : MonoBehaviour
{
    public static ChildManager instance = null;

    [SerializeField] private List<ChildType_SO> childType_List_Ori;
    [SerializeField] private List<GiftType_SO> giftType_List_Ori;
    [SerializeField] private List<BedData> bedData_List_Ori;
    [SerializeField] private List<Transform> giftPos_List_Ori;
    [SerializeField] private List<GameObject> giftPref_List;

    [SerializeField] private List<ChildType_SO> childType_List; // 저장할 아이 타입 리스트
    [SerializeField] private List<GiftType_SO> giftType_List; // 저장할 목표 선물 타입 리스트
    [SerializeField] private List<BedData> bedData_List; // 저장할 침대 데이터 리스트
    [SerializeField] private List<Transform> giftPos_List; // 선물을 둘 위치 리스트

    public List<GameObject> spawnChild_List; // 생성된 Child 리스트

    private int spawnCnt = 0;
    private int spawnIndex = 0;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 제네릭을 통해 ChildType_SO, GiftType_SO 타입 Shuffle을 모두 처리
    private List<T> Shuffle<T>(List<T> list)
    {
        for(int i=0; i<list.Count; i++)
        {
            // 현재 값과 랜덤으로 선택한 값의 위치를 순차적으로 변경 (이미 변경한 위치는 고정!)
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }

        return list;
    } 

    // 데이터 초기화 및 생성 메소드
    public void InitializeData()
    {
        // 데이터 초기화
        spawnCnt = bedData_List_Ori.Count;

        childType_List = Shuffle(childType_List_Ori);
        giftType_List = Shuffle(giftType_List_Ori);
        bedData_List = Shuffle(bedData_List_Ori);
        giftPos_List = Shuffle(giftPos_List_Ori);

        // 저장 데이터에 내용 저장 및 생성
        for (int i = 0; i < spawnCnt; i++)
        {
            ChildType_SO childData = childType_List[i];
            GiftType_SO giftData = giftType_List[i];
            BedData bedData = bedData_List[i];

            // Child 오브젝트 초기화 실행
            GameObject childObj 
                = Instantiate(childData.childPrefab, bedData_List[i].spawnPoint.transform.position, bedData_List[i].spawnPoint.transform.rotation);

            ChildCtrl childCtrl = childObj.GetComponent<ChildCtrl>();

            if (childCtrl != null)
            {
                childCtrl.Init(bedData);
            }
            else
            {
                Debug.Log($"{childData.name} : 해당 오브젝트에 ChildCtrl 스크립트가 없습니다.");
            }

            // Bed 오브젝트 초기화 실행
            BedCtrl bedCtrl = bedData.Bed.GetComponent<BedCtrl>();
            if (bedCtrl != null)
            {
                bedData.spawnIndex = i;
                bedCtrl.Init(childData, giftData, bedData);
            }
            else
            {
                Debug.Log($"{childData.name} : 해당 오브젝트에 대한 bedCtrl 스크립트가 없습니다.");
            }

            // 생성 Child 목록 추가 및 선물 목록 UI 초기화
            spawnChild_List.Add(childObj);
            UIManager.instance.Init(childData, giftData);

            // 생성 순서 갱신
            spawnIndex++;
        }

        // 깨어날 아이 1명 설정
        if( spawnChild_List.Count > 0)
        {
            spawnChild_List[0].GetComponent<EnemyFSM>().isSleeping = false;
        }

        // 선물 위치 설정
        for(int i = 0; i < giftPref_List.Count; i++)
        {
            Instantiate(giftPref_List[i], giftPos_List[i].transform.position, Quaternion.identity);
        }
    }

    // 잠 게이지 도달 시 Wake 메소드
    public void WakeChild()
    {
        foreach(GameObject childObj in spawnChild_List)
        {
            EnemyFSM fsm = childObj.GetComponent<EnemyFSM>();

            if (fsm != null && fsm.isSleeping)
            {
                fsm.isSleeping = false;
                return;
            }
        }

        Debug.Log("더이상 깨울 아이가 없습니다.");
    }
}
