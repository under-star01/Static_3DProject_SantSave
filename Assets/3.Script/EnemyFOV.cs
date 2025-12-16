using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class EnemyFOV : MonoBehaviour
{
    [Header("설정")]
    public float viewRadius = 10f;      // 시야 거리
    [Range(0, 360)]
    public float viewAngle = 90f;       // 시야 각도
    public LayerMask obstacleMask;      // 벽 레이어 (이것에 닿으면 시야가 잘림)
    public LayerMask playerMask;
    public Transform player;


    [Header("퀄리티 설정")]
    public float meshResolution = 1f;   // 1이면 1도당 1번 체크 (높을수록 부드럽지만 성능 무거움)
    public int edgeResolveIterations = 4; // 모서리를 부드럽게 처리하는 반복 횟수
    public float edgeDstThreshold = 0.5f; // 모서리 판정 거리

    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    void Start()
    {
        viewMeshFilter = GetComponent<MeshFilter>();
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    private void Update()
    {
        DetectPlayer();
    }

    // 움직임 후 메쉬가 따라와야 하므로 LateUpdate 사용
    void LateUpdate()
    {
        DrawFieldOfView();
    }

    public bool DetectPlayer()
    {
        if (player == null) return false;

        // 1. 거리 체크
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < viewRadius)
        {
            // 2. 각도 체크
            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            // 내 정면(transform.forward)과 플레이어 방향 사이의 각도가 시야각의 절반보다 작은지 확인
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f)
            {
                // 3. 장해물(벽) 체크 (Raycast)
                // 나와 플레이어 사이의 거리에 벽이 있는지 확인
                //Vector3 eyePos = transform.position + Vector3.up * 1.0f;
                RaycastHit hit;

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
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();

        // 1. 부채꼴 모양으로 레이캐스트를 쏴서 정점(Vertex) 위치들을 구함
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);
            viewPoints.Add(newViewCast.point);
        }

        // 2. 구한 정점들로 메쉬 생성
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero; // 중심점 (적의 위치, 로컬 좌표계이므로 0,0,0)

        for (int i = 0; i < vertexCount - 1; i++)
        {
            // 월드 좌표인 viewPoints를 로컬 좌표로 변환
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
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
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
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

