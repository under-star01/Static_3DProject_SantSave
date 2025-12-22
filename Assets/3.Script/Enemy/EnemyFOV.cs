using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//스크립트 넣은 오브젝트에게 컴포넌트 강제 추가
[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class EnemyFOV : MonoBehaviour
{
    [Header("설정")]
    public float viewRadius = 10f;      // 시야 거리
    [Range(0, 360)]
    public float viewAngle = 90f;       // 시야 각도
    public LayerMask obstacleMask;      // 벽 레이어 (이것에 닿으면 시야가 잘림)
    public LayerMask playerMask;        // 플레이어 레이어 (감지 해야 될 레이어)
    public Transform player;            // 플레이어 위치

    [Header("퀄리티 설정")]
    public float meshResolution = 1f;   // 1이면 1도당 1번 체크 (높을수록 부드럽지만 성능 무거움)
    public int edgeResolveIterations = 4; // 모서리를 부드럽게 처리하는 반복 횟수
    public float edgeDstThreshold = 0.5f; // 모서리 판정 거리

    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    private void Awake()
    {
        player = FindAnyObjectByType<PlayerInput>().gameObject.transform;
    }

    void Start()
    {
        //빈 메쉬 생성 및 viewMeshFilter의 메쉬에 적용
        viewMeshFilter = GetComponent<MeshFilter>();
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    private void Update()
    {
        //매 프레임 플레이어 감지 여부 체크
        DetectPlayer();
    }

    // 움직임 후 메쉬가 따라와야 하므로 LateUpdate 사용
    void LateUpdate()
    {
        //NPC의 움직임을 Update에서 먼저 처리하고 위치에 맞는 시야를 이동한 위치에서 그려주기 위해 LateUpdate에서 관리
        DrawFieldOfView();
    }

    public bool DetectPlayer(bool isChasing = false)
    {
        if (player == null) return false;

        if (!isChasing && player.CompareTag("Decoy"))
        {
            return false;
        }

        // 1. 거리 체크(오브젝트와 플레이어 간 거리체크)
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < viewRadius)
        {
            // 2. 각도 체크(부채꼴 범위)
            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            // 내 정면(transform.forward)과 플레이어 방향 사이의 각도가 시야각의 절반보다 작은지 확인
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f)
            {
                // 3. 장해물(벽) 체크 (Raycast)
                // 나와 플레이어 사이의 거리에 벽이 있는지 확인
                RaycastHit hit;
                //장애물에 닿지 않았을 때 true
                if (!Physics.Raycast(transform.position, dirToPlayer, out hit, distanceToPlayer, obstacleMask))
                {
                    Debug.Log("플레이어 탐지 성공!");
                    return true;
                }
                else
                {
                    // 탐지 실패 시 무엇에 맞았는지 로그 출력
                    Debug.Log($"시야가 막힘! 방해물: {hit.collider.gameObject.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");

                    // 디버그 라인 그려서 눈으로 확인 (Scene 뷰에서 빨간선이 어디서 끊기는지 보세요)
                    Debug.DrawLine(transform.position, hit.point, Color.red, 0.1f);
                }
            }
        }
        return false; // 감지 실패
    }

    void DrawFieldOfView()
    {
        //스텝 계산
        //총 몇번의 레이를 쏠지 결정(90도에 1도마다 1번씩)
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);   
        float stepAngleSize = viewAngle / stepCount;    //레이와 레이 사이의 각도 간격 계산
        List<Vector3> viewPoints = new List<Vector3>();

        // 1. 부채꼴 모양으로 레이캐스트를 쏴서 정점(Vertex) 위치들을 구함
        for (int i = 0; i <= stepCount; i++)
        {
            //앵글 값 계산(내 캐릭터가 월드에서 바라보고 있는방향 - 시야각의 절반만큼 왼쪽 회전 / for문을 통해 부채꼴을 채움)
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);
            viewPoints.Add(newViewCast.point);
        }

        // 2. 구한 정점들로 메쉬 생성
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        //삼각형 인덱스를 담을 배열(인덱스 개수 = 삼각형 개수 * 3(꼭지점))
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero; // 중심점 (적의 위치, 로컬 좌표계이므로 0,0,0)

        for (int i = 0; i < vertexCount - 1; i++)
        {
            // 월드 좌표인 viewPoints를 로컬 좌표로 변환
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            //삼각형 만들기
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;           //중심 점
                triangles[i * 3 + 1] = i + 1;   //현재 점
                triangles[i * 3 + 2] = i + 2;   //다음 점
            }
        }

        viewMesh.Clear();               //메쉬 정보 초기화
        viewMesh.vertices = vertices;   //새로 계산한 점 추가
        viewMesh.triangles = triangles; //점을 잇는 순서 추가(삼각형)
        viewMesh.RecalculateNormals();  //빛 반사를 계산하기 위한 법선 재계산(법선 : 특정 점의 곡선이나 표면에 수직으로 만나는 선)
    }

    // 특정 각도로 레이를 쏘는 헬퍼 함수
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        // 벽에 부딪히면 그 지점, 아니면 최대 사거리 지점 반환
        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        //앵글값을 디그리 값으로 계산(컴퓨터의 삼각함수 계산은 라디안만 처리가능)
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        //시야를 그려줄 레이캐스트의 변수를 담는 구조체
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }
}

