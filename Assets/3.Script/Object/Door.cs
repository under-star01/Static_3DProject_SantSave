using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("투명화할 오브젝트")]
    [SerializeField] private List<Renderer> xrayObject_List;

    [Header("머테리얼 종류")]
    [SerializeField] private Material m_Default; // 기본 머테리얼
    [SerializeField] private Material m_Xray; // 투명화 머테리얼

    [Header("상태 종류")]
    [SerializeField] private bool isXray = false;

    private Renderer render;

    private void Awake()
    {
        TryGetComponent(out render);
    }

    private void OnTriggerEnter(Collider other)
    {
        //isXray ? 

        //foreach (Renderer o in xrayObject_List)
        //{
        //    o.
        //}
    }
}
