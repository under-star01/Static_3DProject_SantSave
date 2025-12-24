using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("투명화할 오브젝트")]
    [SerializeField] private List<Renderer> xrayObject_List;

    [Header("머테리얼 종류")]
    [SerializeField] private Material m_Default; // 기본 머테리얼
    [SerializeField] private Material m_Xray; // 투명화 머테리얼

    private void OnTriggerEnter(Collider other)
    {
        // 투명화할 오브젝트들에 Material 적용
        foreach (Renderer r in xrayObject_List)
        {
            r.material = m_Xray;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        // 투명화할 오브젝트들에 Material 복구
        foreach (Renderer r in xrayObject_List)
        {
            r.material = m_Default;
        }
    }
}
